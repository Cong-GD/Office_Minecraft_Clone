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

    [OnValueChanged(nameof(UpdateSeed))]

    public int seed;

    public TerrainGenerator terrainGenerator;

    [SerializeField] private ObjectPool chunkRendererPool;
    public Transform player;
    public Vector3Int playerCoord;

    [Header("Chunk render properties")]
    public int viewDistance = 3;

    [SerializeField, Range(1, 12)]
    int startMaxDegreeOfParallelism = 6;

    [SerializeField, Range(1, 6)]
    int maxDegreeOfParallelism;

    [SerializeField, Range(1, 1000)]
    int chunkRenderPerFrame;

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
    private readonly List<ChunkData> _structuredChunks = new();

    private readonly Dictionary<Vector3Int, ChunkRenderer> _chunkRendererDictionary = new();

    private Coroutine _editingCoroutine;
    private CancellationTokenSource _blockEditToken;
    private CancellationToken _cancellationToken;
    private ParallelOptions _parallelOptions;
    private Camera cam;

    public static World Instance { get; private set; }

    private void Awake()
    {
        BlockDataHelper.Init();
        cam = Camera.main;
        Instance = this;
        _cancellationToken = destroyCancellationToken;
        _parallelOptions = new ParallelOptions()
        {
            CancellationToken = _cancellationToken,
            MaxDegreeOfParallelism = startMaxDegreeOfParallelism
        };
    }

    private async void Start()
    {
        await Task.Delay(100);
        if (!multiThread)
        {
            PrepareChunkData(playerCoord);
            ExcuseModifyQuery();
            BuildStructures();
            PrepareMeshDatas(playerCoord);
        }

        try
        {
            if (multiThread)
            {
                await Task.Run(() => PrepareChunkData(playerCoord), _cancellationToken);
                ExcuseModifyQuery();
                BuildStructures();
                await Task.Run(() => PrepareMeshDatas(playerCoord), _cancellationToken);
            }
            if (continueGenerate)
            {
                chunkRenderPerFrame = 1;
                StartCoroutine(HiddenAnOutOfViewChunk());
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
        playerCoord = Chunk.GetChunkCoord(Vector3Int.FloorToInt(player.transform.position)).X_Z(0);
        int renderCount = 0;
        while (renderCount++ < chunkRenderPerFrame && _preparedMeshs.TryDequeue(out var preparedMesh))
        {
            RenderMesh(preparedMesh.coord, preparedMesh.meshData);
        }
    }

    private void UpdateSeed()
    {
        WorldSeed = seed;
    }


    /// <summary>
    /// Background thread
    /// Method LongtermViewCheckTask() only call once on Start(), Don't call any of these methods from outside of this region
    /// </summary>
    #region Long term world operation method group
    private async Task LongtermViewCheckTask()
    {
        var playerCoord = this.playerCoord - Vector3Int.one;
        while (true)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (playerCoord == this.playerCoord)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }
            playerCoord = this.playerCoord;
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

            if (Chunk.IsPositionInRange(playerCoord, chunkData.chunkCoord, ChunkDataLoadRange + 2))
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
        if (_parallelOptions.MaxDegreeOfParallelism < 2)
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
                        _structuredChunks.Add(chunkData);
                    }
                }
            }
        }
        else
        {
            object _lockObject = new object();

            Parallel.ForEach(Chunk.GetCoordsInRange(playerCoord, ChunkDataLoadRange)
                .Where(chunkCoord => !_chunkDataDictionary.ContainsKey(chunkCoord)),
                _parallelOptions,
                chunkCoord =>
                {
                    var chunkData = terrainGenerator.GenerateChunk(chunkCoord);
                    if (_chunkDataDictionary.TryAdd(chunkCoord, chunkData))
                    {
                        lock (_lockObject)
                        {
                            _activeChunkData.Enqueue(chunkData);
                            if (chunkData.HasStructure())
                            {
                                _structuredChunks.Add(chunkData);
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
        foreach (var chunkData in _structuredChunks)
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
        _structuredChunks.Clear();
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
            mod.blockType);
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

    public void PrepareMeshDatas(Vector3Int playerCoord)
    {
        using var _ = TimeExcute.Start("Prepare mesh datas");
        if (_parallelOptions.MaxDegreeOfParallelism < 2)
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
        else
        {
            if (meshData.HasDataToRender())
            {
                chunkRenderer = (ChunkRenderer)chunkRendererPool.Get();
                chunkRenderer.SetChunkData(chunkData);
                chunkRenderer.transform.SetParent(transform);
                _chunkRendererDictionary[coord] = chunkRenderer;
                chunkRenderer.RenderMesh(meshData);
            }
        }

        ThreadSafePool<MeshData>.Release(meshData);
        chunkData.state = ChunkState.Rendering;
    }

    private IEnumerator HiddenAnOutOfViewChunk()
    {
        var oldPlayerCoord = playerCoord;
        while (true)
        {
            if (oldPlayerCoord == playerCoord)
            {
                yield return Wait.ForSeconds(1f);
                continue;
            }
            oldPlayerCoord = playerCoord;
            foreach (var chunk in _chunkRendererDictionary.ToArray())
            {
                if (Chunk.IsPositionInRange(playerCoord, chunk.Key, HiddenChunkDistance) || !TryGetChunkData(chunk.Key, out var chunkData))
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

    public void EditBlock(Vector3Int worldPosition, BlockType blockType)
    {
        if (_editingCoroutine != null)
        {
            StopCoroutine(_editingCoroutine);
            _blockEditToken.Cancel();
            _blockEditToken.Dispose();
            _editingCoroutine = null;
        }
        StartCoroutine(EditBlockCoroutine(worldPosition, blockType));
    }

    private IEnumerator EditBlockCoroutine(Vector3Int worldPosition, BlockType blockType)
    {
        Chunk.SetBlock(worldPosition.x, worldPosition.y, worldPosition.z, blockType);
        var chunkToUpdate = Chunk.GetAdjacentChunkCoords(worldPosition);
        _blockEditToken = new CancellationTokenSource();
        var task = Task.Run(() =>
        {
            Dictionary<Vector3Int, MeshData> meshDatas = new();
            foreach (var chunkCoord in chunkToUpdate)
            {
                if (TryGetChunkData(chunkCoord, out var chunkData) && chunkData.state == ChunkState.Rendering)
                {
                    _blockEditToken.Token.ThrowIfCancellationRequested();
                    meshDatas.TryAdd(chunkCoord, Chunk.GetMeshData(chunkData));
                }
            }
            return meshDatas;
        }, _blockEditToken.Token);

        while (!task.IsCompleted)
        {
            yield return null;
        }

        foreach (var meshData in task.Result)
        {
            RenderMesh(meshData.Key, meshData.Value);
        }
    }

}
