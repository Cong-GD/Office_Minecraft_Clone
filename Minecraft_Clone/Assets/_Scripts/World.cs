using Minecraft.ProceduralTerrain.Structures;
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

    [SerializeField]
    private PlayerData_SO playerData;

    [SerializeField]
    private Vector3 SpawnOffset;

    [field: ShowNonSerializedField]
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

    [ShowNativeProperty]
    public int MeshDataInPool => ThreadSafePool<MeshData>.Count;


    private readonly ConcurrentDictionary<Vector3Int, ChunkData> _chunkDataDictionary = new();
    private readonly ConcurrentQueue<MeshData> _preparedMeshs = new();
    private readonly ConcurrentQueue<MeshData> _priorityMeshToRenders = new();
    private readonly Queue<ChunkData> _activeChunkData = new();
    private readonly ConcurrentDictionary<Vector3Int, ChunkData> _hasModifierChunks = new();
    private readonly ConcurrentStack<Vector3Int> _chunkNeedToHide = new();
    private readonly List<ChunkData> _haveStructuresChunks = new();

    private readonly Dictionary<Vector3Int, ChunkRenderer> _chunkRendererDictionary = new();

    private CancellationToken _cancellationToken;
    private ParallelOptions _parallelOptions;
    private bool _isEditing;

    public static World Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
            
        Instance = this;
        WorldSeed = seed;
        ThreadSafePool<MeshData>.Capacity = 50;

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
        int viewDistance = this.viewDistance;
        Vector3Int playerCoord = Chunk.GetChunkCoord(playerData.PlayerBody.position.With(y: 0));
        if (!multiThread)
        {
            _parallelOptions.MaxDegreeOfParallelism = 1;
            PrepareChunkData(playerCoord, viewDistance);
            BuildStructures();
            PrepareMeshDatas(playerCoord, viewDistance);
        }

        try
        {
            if (multiThread)
            {
                await Task.Run(() =>
                {
                    PrepareChunkData(playerCoord, viewDistance);
                    BuildStructures();
                    PrepareMeshDatas(playerCoord, viewDistance);
                }, _cancellationToken);
            }
            while (_preparedMeshs.TryDequeue(out MeshData meshData))
            {
                RenderMesh(meshData);
            }
            SpawnPlayer();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        if (continueGenerate)
        {
            Task _ = Task.Run(LongtermViewCheckTask, _cancellationToken);
        }
    }

    private void Update()
    {
        PlayerCoord = Chunk.GetChunkCoord(playerData.PlayerBody.position.With(y: 0));

        while (_chunkNeedToHide.TryPop(out Vector3Int chunkToHide))
        {
            if (_chunkRendererDictionary.Remove(chunkToHide, out ChunkRenderer chunkRenderer))
            {
                chunkRenderer.ReturnToPool();
            }
        }

        int renderedCount = 0;
        while (_priorityMeshToRenders.TryDequeue(out MeshData meshData))
        {
            RenderMesh(meshData);
            renderedCount++;
        }  
        while (renderedCount++ < chunkRenderPerFrame && _preparedMeshs.TryDequeue(out MeshData preparedMesh))
        {
            RenderMesh(preparedMesh);
        }
    }

    private void SpawnPlayer()
    {
        if(Physics.Raycast(new Vector3(SpawnOffset.x, 250,SpawnOffset.z), Vector3.down, 
            out RaycastHit hit, 250, LayerMask.GetMask("Ground")))
        {
            playerData.PlayerBody.position = hit.point.Add(y: SpawnOffset.y);
            playerData.PlayerBody.velocity = Vector3.zero;
        }
    }

    private int ChunkLoadRange(int viewDistance)
    {
        return viewDistance + 1;
    }

    private int ChunkHideRange(int viewDistance)
    {
        return viewDistance + 3;
    }

    /// <summary>
    /// Background thread
    /// Method LongtermViewCheckTask() only call once on Start(), Don't call any of these methods from outside of this region
    /// </summary>
    #region Long term world operation method group
    private async void LongtermViewCheckTask()
    {
        var playerCoord = PlayerCoord;
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (playerCoord == PlayerCoord)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }
                playerCoord = PlayerCoord;
                _parallelOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
                int viewDistance = this.viewDistance;

                RemoveOutOfRangeChunkData(playerCoord, viewDistance);
                PrepareChunkData(playerCoord, viewDistance);
                ExcuseModifyQuery();
                BuildStructures();
                PrepareMeshDatas(playerCoord, viewDistance);

                await Task.Delay(100).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    private void RemoveOutOfRangeChunkData(Vector3Int playerCoord, int viewDistance)
    {
        bool IsThisChunkNeedToReturn(ChunkData chunkData)
        {
            if (chunkData.state != ChunkState.Generated && chunkData.state != ChunkState.Rendering)
                return false;

            if (Chunk.IsPositionInRange(playerCoord, chunkData.chunkCoord, ChunkHideRange(viewDistance)))
                return false;

            return true;
        }

        int count = _activeChunkData.Count;
        for (int i = 0; i < count; i++)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!_activeChunkData.TryDequeue(out ChunkData chunkData))
            {
                throw new Exception("Can't dequeue chunk data");
            }
            if (IsThisChunkNeedToReturn(chunkData) && _chunkDataDictionary.TryRemove(chunkData.chunkCoord, out _))
            {
                terrainGenerator.ReleaseChunk(chunkData);
                _hasModifierChunks.TryRemove(chunkData.chunkCoord, out _);
                _chunkNeedToHide.Push(chunkData.chunkCoord);
                continue;
            }

            _activeChunkData.Enqueue(chunkData);
        }
    }

    private void PrepareChunkData(Vector3Int playerCoord, int viewDistance)
    {
        using TimeExcute _ = TimeExcute.Start("Prepare chunk datas");
        terrainGenerator.CalculateBiomeCenter(playerCoord.x, playerCoord.z);
        if (_parallelOptions.MaxDegreeOfParallelism == 1)
        {
            foreach (Vector3Int chunkCoord in Chunk.GetCoordsInRange(playerCoord, ChunkLoadRange(viewDistance)))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (_chunkDataDictionary.ContainsKey(chunkCoord))
                    continue;

                ChunkData chunkData = terrainGenerator.GenerateChunk(chunkCoord);
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

            Parallel.ForEach(Chunk.GetCoordsInRange(playerCoord, ChunkLoadRange(viewDistance))
                .Where(chunkCoord => !_chunkDataDictionary.ContainsKey(chunkCoord)),
                _parallelOptions,
                chunkCoord =>
                {
                    ChunkData chunkData = terrainGenerator.GenerateChunk(chunkCoord);
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
        using var pooledArray = ArrayPoolHelper.Rent<ChunkData>(_hasModifierChunks.Count, true);
        _hasModifierChunks.Values.CopyTo(pooledArray.Value, 0);
        Span<ChunkData> hasModifierChunks = pooledArray.Value.AsSpan(0, _hasModifierChunks.Count);
        foreach (ChunkData chunkData in hasModifierChunks)
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
        foreach (ChunkData chunkData in _haveStructuresChunks)
        {
            foreach ((Vector3Int position, IStructure structure) in chunkData.structures)
            {
                structure.GetModifications(modifiers, position);
                while (modifiers.TryDequeue(out ModifierUnit modifier))
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

    private void ApplyModification(ChunkData chunkData,in ModifierUnit mod)
    {
        if (!Chunk.IsValidWorldY(mod.y))
            return;

        Vector3Int chunkCoord = Chunk.GetChunkCoord(mod.x, mod.y, mod.z);

        if (!TryGetChunkData(chunkCoord, out var modifyChunk))
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

    private IEnumerable<ChunkData> GetChunkNeedToPrepareMesh(Vector3Int playerCoord, int viewDistance)
    {
        foreach (Vector3Int chunkCoord in Chunk.GetCoordsInRange(playerCoord, viewDistance))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!TryGetChunkData(chunkCoord, out ChunkData chunkData))
                continue;

            if ((chunkData.state == ChunkState.Rendering && !chunkData.isDirty) || chunkData.state != ChunkState.Generated)
                continue;

            chunkData.state = ChunkState.PreparingMesh;
            chunkData.isDirty = false;
            yield return chunkData;
        }
    }

    private void PrepareMeshDatas(Vector3Int playerCoord, int viewDistance)
    {
        using TimeExcute _ = TimeExcute.Start("Prepare mesh datas");
        if (_parallelOptions.MaxDegreeOfParallelism == 1)
        {
            foreach (ChunkData chunkData in GetChunkNeedToPrepareMesh(playerCoord, viewDistance))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                MeshData meshData = Chunk.GetMeshData(chunkData);
                chunkData.state = ChunkState.MeshPrepared;
                _preparedMeshs.Enqueue(meshData);
            }
        }
        else
        {
            Parallel.ForEach(GetChunkNeedToPrepareMesh(playerCoord, viewDistance),
            _parallelOptions,
            (chunkData) =>
            {
                MeshData meshData = Chunk.GetMeshData(chunkData);
                chunkData.state = ChunkState.MeshPrepared;
                _preparedMeshs.Enqueue(meshData);
            });
        }
    }
    #endregion

    private void RenderMesh(MeshData meshData)
    {
        Vector3Int coord = meshData.position;
        if (!TryGetChunkData(coord, out ChunkData chunkData))
            return;

        if (_chunkRendererDictionary.TryGetValue(coord, out ChunkRenderer chunkRenderer))
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
        await Task.Run(() =>
        {
            foreach (var chunkCoord in Chunk.GetAdjacentChunkCoords(worldPosition))
            {
                if (TryGetChunkData(chunkCoord, out ChunkData chunkData) && chunkData.state == ChunkState.Rendering)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    _priorityMeshToRenders.Enqueue(Chunk.GetMeshData(chunkData));
                }
            }

        }, _cancellationToken);
        _isEditing = false;

        return true;
    }
}
