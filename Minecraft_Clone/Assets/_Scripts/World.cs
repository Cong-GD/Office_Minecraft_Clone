using NaughtyAttributes;
using ObjectPooling;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    [ShowNonSerializedField]
    public static int WorldSeed = 1;

    public int seed;

    [SerializeField]
    private TerrainGenerator terrainGenerator;

    [SerializeField] 
    private ObjectPool chunkRendererPool;

    [field: SerializeField]
    public Rigidbody Player { get; private set; }

    [field: SerializeField]
    public Vector3Int PlayerCoord { get; private set; }

    [SerializeField ,Header("Chunk render properties")]
    private int viewDistance = 3;

    [SerializeField, Range(1, 12)]
    private int startMaxDegreeOfParallelism = 6;

    [SerializeField, Range(1, 6)]
    private int maxDegreeOfParallelism;

    [SerializeField, Range(1, 1000)]
    private int chunkRenderPerFrame;

    [Header("Testing Purpose")]
    public bool multiThread;
    public bool continueGenerate;

    [ShowNativeProperty]
    public int ChunkCount => _chunkDataDictionary.Count;

    [ShowNativeProperty]
    public int ChunkRendering => _chunkRendererDictionary.Count;

    public int ChunkDataLoadRange => viewDistance + 1;
    public int HiddenChunkDistance => viewDistance + 2;


    private readonly ConcurrentDictionary<Vector3Int, ChunkData> _chunkDataDictionary = new();
    private readonly ConcurrentQueue<(Vector3Int coord, MeshData meshData)> _preparedMeshs = new();
    private readonly Queue<ChunkData> _activeChunkData = new();
    private readonly ConcurrentDictionary<Vector3Int, ChunkData> _hasModifierChunks = new();
    private readonly List<ChunkData> _haveStructuresChunks = new();

    private readonly Dictionary<Vector3Int, ChunkRenderer> _chunkRendererDictionary = new();

    private CancellationToken _cancellationToken;
    private ParallelOptions _parallelOptions;
    private bool _isEditing;

    public static World Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
            Destroy(gameObject);

        Instance = this;
        WorldSeed = seed;

        _cancellationToken = destroyCancellationToken;
        _parallelOptions = new ParallelOptions()
        {
            CancellationToken = _cancellationToken,
            MaxDegreeOfParallelism = startMaxDegreeOfParallelism
        };
    }

    private async void Start()
    {
        await Task.Delay(50);
        
        if (!multiThread)
        {
            _parallelOptions.MaxDegreeOfParallelism = 1;
            PrepareChunkData(PlayerCoord);
            BuildStructures();
            PrepareMeshDatas(PlayerCoord);
        }

        try
        {
            if (multiThread)
            {
                await Task.Run(() =>
                {
                    PrepareChunkData(PlayerCoord);
                    BuildStructures();
                    PrepareMeshDatas(PlayerCoord);
                }, _cancellationToken);
            }
            SpawnPlayer();
            if (continueGenerate)
            {
                chunkRenderPerFrame = 1;
                StartCoroutine(HiddenOutOfViewChunks());
                await Task.Run(LongtermViewCheckTask, _cancellationToken);
            }

        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    private void Update()
    {
        PlayerCoord = Chunk.GetChunkCoord(Vector3Int.FloorToInt(Player.position).X_Z(0));

        int renderCount = 0;
        while (renderCount++ < chunkRenderPerFrame && _preparedMeshs.TryDequeue(out var preparedMesh))
        {
            RenderMesh(preparedMesh.coord, preparedMesh.meshData);
        }
    }

    private void SpawnPlayer()
    {
        if(Physics.Raycast(new Vector3(0, 250,0), Vector3.down, out var hit, 250, LayerMask.GetMask("Ground")))
        {
            Player.position = hit.point + Vector3.up * 5;
            Player.velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Background thread
    /// Method LongtermViewCheckTask() only call once on Start(), Don't call any of these methods from outside of this region
    /// </summary>
    #region Long term world operation method group
    private async Task LongtermViewCheckTask()
    {
        var playerCoord = this.PlayerCoord - Vector3Int.one;
        while (true)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (playerCoord == this.PlayerCoord)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }
            playerCoord = this.PlayerCoord;
            _parallelOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            RemoveOutOfRangeChunkData(playerCoord);
            PrepareChunkData(playerCoord);
            ExcuseModifyQuery();
            BuildStructures();
            PrepareMeshDatas(playerCoord);

            await Task.Delay(100).ConfigureAwait(false);
        }
    }

    private void RemoveOutOfRangeChunkData(Vector3Int playerCoord)
    {
        bool IsThisChunkNeedToReturn(ChunkData chunkData)
        {
            if (chunkData.state != ChunkState.Generated)
                return false;

            if (Chunk.IsPositionInRange(playerCoord, chunkData.chunkCoord, HiddenChunkDistance + 2))
                return false;

            return true;
        }

        int count = _activeChunkData.Count;
        for (int i = 0; i < count; i++)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!_activeChunkData.TryDequeue(out var chunkData))
            {
                throw new Exception("Error when dequeue chunk data to remove");
            }
            if (IsThisChunkNeedToReturn(chunkData) && _chunkDataDictionary.TryRemove(chunkData.chunkCoord, out _))
            {
                terrainGenerator.ReleaseChunk(chunkData);
                _hasModifierChunks.TryRemove(chunkData.chunkCoord, out _);
                continue;
            }

            _activeChunkData.Enqueue(chunkData);
        }
    }

    private void PrepareChunkData(Vector3Int playerCoord)
    {
        using var _ = TimeExcute.Start("Prepare chunk datas");
        terrainGenerator.CalculateBiomeCenter(playerCoord.x * WorldSettings.CHUNK_WIDTH, playerCoord.z * WorldSettings.CHUNK_WIDTH);
        if (_parallelOptions.MaxDegreeOfParallelism == 1)
        {
            foreach (var chunkCoord in Chunk.GetCoordsInRange(playerCoord, ChunkDataLoadRange))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (_chunkDataDictionary.ContainsKey(chunkCoord))
                    continue;

                var chunkData = terrainGenerator.GenerateChunk(chunkCoord);
                if (_chunkDataDictionary.TryAdd(chunkCoord, chunkData))
                {
                    _activeChunkData.Enqueue(chunkData);
                    if (chunkData.HasStructure())
                    {
                        _haveStructuresChunks.Add(chunkData);
                    }
                }
            }
        }
        else
        {
            object lockObject = new object();

            Parallel.ForEach(Chunk.GetCoordsInRange(playerCoord, ChunkDataLoadRange)
                .Where(chunkCoord => !_chunkDataDictionary.ContainsKey(chunkCoord)),
                _parallelOptions,
                chunkCoord =>
                {
                    var chunkData = terrainGenerator.GenerateChunk(chunkCoord);
                    if (_chunkDataDictionary.TryAdd(chunkCoord, chunkData))
                    {
                        lock (lockObject)
                        {
                            _activeChunkData.Enqueue(chunkData);
                            if (chunkData.HasStructure())
                            {
                                _haveStructuresChunks.Add(chunkData);
                            }
                        }
                    }
                });
        }
    }

    private void ExcuseModifyQuery()
    {
        foreach (var chunkData in _hasModifierChunks.Values.ToArray())
        {
            _cancellationToken.ThrowIfCancellationRequested();
            int count = chunkData.modifierQueue.Count;
            for (int j = 0; j < count; j++)
            {
                ApplyModification(chunkData, chunkData.modifierQueue.Dequeue());
            }
            if (!chunkData.HasModifier())
            {
                _hasModifierChunks.TryRemove(chunkData.chunkCoord, out _);
            }
        }
    }

    private void BuildStructures()
    {
        Queue<ModifierUnit> modifiers = new Queue<ModifierUnit>();
        foreach (var chunkData in _haveStructuresChunks)
        {
            foreach (var (position, structure) in chunkData.structures)
            {
                structure.GetModifications(modifiers, position);
                while (modifiers.TryDequeue(out var modifier))
                {
                    ApplyModification(chunkData, modifier);
                }
            }

            if (chunkData.HasModifier())
            {
                _hasModifierChunks.TryAdd(chunkData.chunkCoord, chunkData);
            }
        }
        _haveStructuresChunks.Clear();
    }

    private void ApplyModification(ChunkData chunkData, ModifierUnit mod)
    {
        if (!Chunk.IsValidWorldY(mod.y))
            return;

        var chunkCoord = Chunk.GetChunkCoord(mod.x, mod.y, mod.z);

        if (!_chunkDataDictionary.TryGetValue(chunkCoord, out var modifyChunk))
        {
            chunkData.modifierQueue.Enqueue(mod);
            return;
        }

        if (modifyChunk.modifiedByPlayer)
            return;

        if (modifyChunk.state == ChunkState.Rendering)
            modifyChunk.isDirty = true;

        modifyChunk.SetBlock(
            mod.x - modifyChunk.worldPosition.x,
            mod.y - modifyChunk.worldPosition.y,
            mod.z - modifyChunk.worldPosition.z,
            mod.blockType,
            mod.direction);
    }

    private IEnumerable<ChunkData> GetChunkNeedToPrepareMesh(Vector3Int playerCoord)
    {
        foreach (var chunkCoord in Chunk.GetCoordsInRange(playerCoord, viewDistance))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!_chunkDataDictionary.TryGetValue(chunkCoord, out var chunkData))
                continue;

            if (chunkData.state != ChunkState.Generated || (chunkData.state == ChunkState.Rendering && !chunkData.isDirty))
                continue;

            chunkData.state = ChunkState.PreparingMesh;
            chunkData.isDirty = false;
            yield return chunkData;
        }
    }

    private void PrepareMeshDatas(Vector3Int playerCoord)
    {
        using var timer = TimeExcute.Start("Prepare mesh datas");
        if (_parallelOptions.MaxDegreeOfParallelism == 1)
        {
            var chunkNeedToPrepareMesh = GetChunkNeedToPrepareMesh(playerCoord).ToArray();
            foreach (var chunkData in chunkNeedToPrepareMesh)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var item = (chunkData.chunkCoord, Chunk.GetMeshData(chunkData));
                chunkData.state = ChunkState.MeshPrepared;
                _preparedMeshs.Enqueue(item);
            }
        }
        else
        {
            Parallel.ForEach(GetChunkNeedToPrepareMesh(playerCoord),
            _parallelOptions,
            (chunkData) =>
            {
                var item = (chunkData.chunkCoord, Chunk.GetMeshData(chunkData));
                chunkData.state = ChunkState.MeshPrepared;
                _preparedMeshs.Enqueue(item);
            });
        }
    }
    #endregion

    private void RenderMesh(Vector3Int coord, MeshData meshData)
    {
        if (!_chunkDataDictionary.TryGetValue(coord, out var chunkData))
            return;

        if (_chunkRendererDictionary.TryGetValue(coord, out var chunkRenderer))
        {
            if (meshData.HasDataToRender())
            {
                chunkRenderer.RenderMesh(meshData);
            }
            else
            {
                _chunkRendererDictionary.Remove(chunkData.chunkCoord);
                chunkRenderer.ReturnToPool();
            }
        }
        else if(meshData.HasDataToRender())
        {
            chunkRenderer = (ChunkRenderer)chunkRendererPool.Get();
            chunkRenderer.SetChunkData(chunkData);
            chunkRenderer.transform.SetParent(transform);
            _chunkRendererDictionary[coord] = chunkRenderer;
            chunkRenderer.RenderMesh(meshData);
        }

        chunkData.ValidateBlockState();
        ThreadSafePool<MeshData>.Release(meshData);
        chunkData.state = ChunkState.Rendering;
    }

    private IEnumerator HiddenOutOfViewChunks()
    {
        var oldPlayerCoord = PlayerCoord;
        while (true)
        {
            if (oldPlayerCoord == PlayerCoord)
            {
                yield return Wait.ForSeconds(1f);
                continue;
            }
            oldPlayerCoord = PlayerCoord;
            foreach (var chunk in _chunkRendererDictionary.ToArray())
            {
                if (Chunk.IsPositionInRange(PlayerCoord, chunk.Key, HiddenChunkDistance) || !TryGetChunkData(chunk.Key, out var chunkData))
                    continue;

                if (chunkData.state != ChunkState.Rendering)
                    continue;

                _chunkRendererDictionary.Remove(chunkData.chunkCoord);
                chunk.Value.ReturnToPool();
                chunkData.state = ChunkState.Generated;
            }
            yield return Wait.ForSeconds(1f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetChunkData(Vector3Int chunkCoord, out ChunkData chunkData)
    {
        if (!Chunk.IsValidChunkCoordY(chunkCoord.y))
        {
            chunkData = null;
            return false;
        }
        return _chunkDataDictionary.TryGetValue(chunkCoord, out chunkData);
    }

    public bool CanEdit()
    {
        return !_isEditing;
    }

    public async Task<bool> EditBlockAsync(Vector3Int worldPosition, BlockType blockType, Direction direction)
    {
        if(!CanEdit()) 
            return false;

        _isEditing = true;
        Chunk.SetBlock(worldPosition, blockType, direction);
        var chunkToUpdate = Chunk.GetAdjacentChunkCoords(worldPosition);

        var meshDatas = await Task.Run(() =>
        {
            List<(Vector3Int coord, MeshData data)> meshDatas = new();
            foreach (var chunkCoord in chunkToUpdate)
            {
                if (TryGetChunkData(chunkCoord, out var chunkData) && chunkData.state == ChunkState.Rendering)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    meshDatas.Add((chunkCoord, Chunk.GetMeshData(chunkData)));
                }
            }
            return meshDatas;

        }, _cancellationToken);

        foreach (var meshData in meshDatas)
        {
            RenderMesh(meshData.coord, meshData.data);
        }
        _isEditing = false;

        return true;
    }
}
