using ObjectPooling;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{

    public TerrainGenerator terrainGenerator;

    [SerializeField] private ObjectPool chunkRendererPool;

    public Transform player;
    public Vector3Int playerCoord;

    [Header("Chunk render properties")]
    public int viewDistance = 3;
    [SerializeField, Range(1, 12)] 
    int startMaxDegreeOfParallelism = 6;
    [SerializeField, Range(1, 6)] 
    int maxDegreeOfParallelism = 2;
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
        //PrepareChunkData(playerCoord);
        //ExcuseModifyQuery();
        //BuildStructures();
        //PrepareMeshDatas(playerCoord);
        try
        {
            await Task.Run(() => PrepareChunkData(playerCoord), _cancellationToken);
            ExcuseModifyQuery();
            BuildStructures();
            await Task.Run(() => PrepareMeshDatas(playerCoord), _cancellationToken);
            StartCoroutine(HiddenAnOutOfViewChunk());
            await Task.Run(LongtermViewCheckTask, _cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public int chunkCount;
    public int chunkRendering;

    private void Update()
    {
        playerCoord = Chunk.GetChunkCoord(Vector3Int.FloorToInt(player.transform.position)).X_Z(0);
        chunkCount = _chunkDataDictionary.Count;
        chunkRendering = _chunkRendererDictionary.Count;
        if(_preparedMeshs.TryDequeue(out var preparedMesh))
        {
            RenderMesh(preparedMesh.coord, preparedMesh.meshData);
        }
        
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
        Benmark timer = new Benmark();
        timer.Start();
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

        timer.Stop();
        timer.PrintResult(nameof(PrepareChunkData));
    }

    private void ExcuseModifyQuery()
    {
        //Benmark timer = new Benmark();
        //timer.Start();
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
        //timer.Stop();
        //timer.PrintResult(nameof(ExcuseModifyQuery));
    }

    private void BuildStructures()
    {
        //Benmark timer = new Benmark();
        //timer.Start();
        foreach (var chunkData in _structuredChunks)
        {
            foreach (var (position, structure) in chunkData.structures)
            {
                foreach (var modifier in structure.GetModifications(position))
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
        //timer.Stop();
        //timer.PrintResult(nameof(BuildStructures));
    }

    private void ApplyModification(ChunkData chunkData, ModifierUnit modifier)
    {
        var position = modifier.position;
        var chunkCoord = Chunk.GetChunkCoord(position);
        if (!Chunk.IsValidChunkCoordY(chunkCoord.y))
            return;

        if (!_chunkDataDictionary.TryGetValue(chunkCoord, out var modifyChunk))
        {
            chunkData.modifierQueue.Enqueue(modifier);
            return;
        }

        if (modifyChunk.modifiedByPlayer)
            return;

        if (modifyChunk.state == ChunkState.Rendering)
            modifyChunk.isDirty = true;

        modifyChunk.SetBlock(position - modifyChunk.worldPosition, modifier.blockType);
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
        Benmark timer = new Benmark();
        timer.Start();
        Parallel.ForEach(GetChunkNeedToPrepareMesh(playerCoord),
            _parallelOptions,
            (chunkData) =>
        {
            var item = (chunkData.chunkCoord, Chunk.GetMeshData(chunkData));
            chunkData.state = ChunkState.MeshPrepared;
            _preparedMeshs.Enqueue(item);
        });
        timer.Stop();
        timer.PrintResult(nameof(PrepareMeshDatas));
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
            if(oldPlayerCoord == playerCoord)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlock(Vector3Int worldPosition)
    {
        var chunkCoord = Chunk.GetChunkCoord(worldPosition);
        if (!Chunk.IsValidChunkCoordY(chunkCoord.y))
        {
            return BlockType.Air;
        }
        if (_chunkDataDictionary.TryGetValue(chunkCoord, out var chunkData))
        {
            return chunkData.GetBlock(worldPosition - chunkData.worldPosition);
        }
        return BlockType.Air;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(Vector3Int worldPosition, BlockType blockType)
    {
        var chunkCoord = Chunk.GetChunkCoord(worldPosition);
        if (_chunkDataDictionary.TryGetValue(chunkCoord, out var chunkData))
        {
            chunkData.SetBlock(worldPosition - chunkData.worldPosition, blockType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockData_SO GetBlockData(Vector3Int worldPosition)
    {
        return BlockDataManager.GetData(GetBlock(worldPosition));
    }

    public void EditBlock(Vector3Int worldPosition, BlockType blockType)
    {
        //SetBlock(worldPosition, blockType);
        //foreach (var chunk in Chunk.GetAdjacentChunkCoords(worldPosition))
        //{
        //    if(_chunkRendererDictionary.TryGetValue(chunk, out var chunkRenderer))
        //    {
        //        chunkRenderer.UpdateMesh();
        //    }
        //};

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
        SetBlock(worldPosition, blockType);
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
            //if (_chunkRendererDictionary.TryGetValue(meshData.Key, out var chunkRenderer))
            //{
            //    chunkRenderer.RenderMesh(meshData.Value);
            //}
            RenderMesh(meshData.Key, meshData.Value);
        }
    }

}
