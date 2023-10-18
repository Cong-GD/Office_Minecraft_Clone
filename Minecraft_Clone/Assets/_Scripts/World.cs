using ObjectPooling;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static GameSettings;
using static UnityEngine.Mesh;

public class World : MonoBehaviour
{
    public string seed;

    [Space(1)]
    public TerrainGenerator terrainGenerator;


    [SerializeField] private ObjectPool chunkRendererPool;

    public Transform player;

    [Header("Chunk render properties")]
    public int chunkDataLoadRange = 7;
    public int viewDistance = 3;
    public int hiddenChunkDistance = 5;


    private readonly ConcurrentDictionary<Vector3Int, ChunkData> _chunkDataDictionary = new();
    private readonly Dictionary<Vector3Int, ChunkRenderer> _chunkRendererDictionary = new();

    private readonly ConcurrentQueue<ChunkData> _meshNeedToPrepare = new();
    private readonly ConcurrentQueue<(Vector3Int coord, MeshData meshData)> _preparedMeshs = new();
    private readonly ConcurrentQueue<ChunkData> _chunkNeedToHidden = new();

    public Vector3Int _playerCoord;

    private Coroutine _editingCoroutine;
    private CancellationTokenSource _blockEditToken;
    private CancellationToken _cancellationToken;
    private ParallelOptions _parallelOptions;

    private void Awake()
    {
        UnityEngine.Random.InitState(seed.GetHashCode());
        _cancellationToken = destroyCancellationToken;
        _parallelOptions = new ParallelOptions()
        {
            CancellationToken = _cancellationToken
        };
    }

    private async void Start()
    {
        try
        {
            DateTime startTime = DateTime.Now;
            await Task.Run(() => PrepareChunkData(_playerCoord), _cancellationToken);
            DateTime mark1 = DateTime.Now;
            var count1 = mark1 - startTime;
            Debug.Log($"Done generate chunk data in  {count1.Seconds}:{count1.Milliseconds}");
            var chunkToPrepare = await Task.Run(() => GetChunkNeedToPrepareMesh(_playerCoord), _cancellationToken);
            await Task.Run(() => PrepareMeshDatas(_chunkDataDictionary.Values.ToList()), _cancellationToken);
            //GenerateMeshDatas(_chunkDataDictionary.Values.ToList());
            DateTime mark2 = DateTime.Now;
            var count2 = mark2 - mark1;
            Debug.Log($"Done generate mesh data in {count2.Seconds}:{count2.Milliseconds}");
        }
        catch ( Exception ex )
        {
            Debug.LogError( ex.Message );
        }
        
        LongtermViewCheckTask();
    }

    private void Update()
    {
        _playerCoord = Chunk.GetChunkCoord(player.transform.position);

        
    }

    private void FixedUpdate()
    {
        RenderAPreparedMesh();
        HiddenAnOutOfViewChunk();
    }

    private async void LongtermViewCheckTask()
    {
        try
        {
            await Task.Run(() =>
            {
                var playerCoord = _playerCoord - Vector3Int.one;
                while (!_cancellationToken.IsCancellationRequested)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    if (playerCoord == _playerCoord)
                    {
                        continue;
                    }
                    playerCoord = _playerCoord; 
                    PrepareChunkData(playerCoord);
                    var chunks = GetChunkNeedToPrepareMesh(playerCoord);
                    if (chunks.Count > 0)
                    {
                        Task.Run(() =>
                        {
                            PrepareMeshDatas(chunks);
                        }, _cancellationToken);
                    }
                    Thread.Sleep(100);
                }
            }, _cancellationToken);
        }
        catch(System.Exception ex)
        {
            Debug.LogWarning($"{ex.Message}");
        }
    }

    // This method run in multitheard enviroment
    private void PrepareChunkData(Vector3Int playerCoord)
    {
        foreach(var chunkCoord in Chunk.GetCoordsInRange(playerCoord, chunkDataLoadRange))
        {
            if(!_chunkDataDictionary.ContainsKey(chunkCoord))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                _chunkDataDictionary.TryAdd(chunkCoord, terrainGenerator.GenerateChunk(this, chunkCoord));
            }
        }
    }

    // This method run in multitheard enviroment
    private List<ChunkData> GetChunkNeedToPrepareMesh(Vector3Int playerCoord)
    {
        List<ChunkData> chunkDatas = new ();
        foreach (var chunkCoord in Chunk.GetCoordsInRange(playerCoord, viewDistance))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (_chunkDataDictionary.TryGetValue(chunkCoord, out var chunkData))
            {
                if (chunkData.state == ChunkState.Generated)
                {
                    chunkData.state = ChunkState.PreparingMesh;
                    chunkDatas.Add(chunkData);
                }
            }
        }
        return chunkDatas;
    }

    // This method run in multitheard enviroment
    public void PrepareMeshDatas(List<ChunkData> chunkDatas)
    {
        //foreach (var chunkData in chunkDatas)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    if (chunkData.state == ChunkState.Generated)
        //    {
        //        chunkData.state = ChunkState.PreparingMesh;
        //        var item = (chunkData.chunkCoord, Chunk.GetMeshData(chunkData));
        //        _preparedMeshs.Enqueue(item);
        //        chunkData.state = ChunkState.MeshPrepared;
        //    }
        //}

        Parallel.ForEach(chunkDatas, _parallelOptions, (chunkData) =>
        {
            if (chunkData.state == ChunkState.PreparingMesh)
            {
                var item = (chunkData.chunkCoord, Chunk.GetMeshData(chunkData));
                chunkData.state = ChunkState.MeshPrepared;
                _preparedMeshs.Enqueue(item);
            }
        });
    }

    private void RenderAPreparedMesh()
    {
        if (!_preparedMeshs.TryDequeue(out var preparedMesh))
            return;

        if (!_chunkDataDictionary.TryGetValue(preparedMesh.coord, out var chunkData))
            return;

        if (chunkData.state != ChunkState.MeshPrepared)
            return;

        if(!_chunkRendererDictionary.TryGetValue(chunkData.chunkCoord, out var chunkRenderer))
        {
            chunkRenderer = (ChunkRenderer)chunkRendererPool.Get();
            chunkRenderer.SetChunkData(chunkData);
            chunkRenderer.transform.SetParent(transform);
            _chunkRendererDictionary[chunkData.chunkCoord] = chunkRenderer;
        }
        chunkRenderer.RenderMesh(preparedMesh.meshData);
        chunkData.state = ChunkState.Rendering;
    }

    private void HiddenAnOutOfViewChunk()
    {
        foreach (var chunk in _chunkRendererDictionary)
        {
            if (Chunk.IsPositionInRange(_playerCoord, chunk.Key, hiddenChunkDistance) || !TryGetChunkData(chunk.Key, out var chunkData))
                continue;

            if (chunkData.state != ChunkState.Rendering)
                continue;

            _chunkRendererDictionary.Remove(chunkData.chunkCoord);
            chunk.Value.ReturnToPool();
            chunkData.state = ChunkState.Generated;
            return;
        }
    }

    public bool TryGetChunkData(Vector3Int chunkCoord, out ChunkData chunkData)
    {
        if(!Chunk.IsValidChunkCoordY(chunkCoord.y))
        {
            chunkData = null;
            return false;
        }
        return _chunkDataDictionary.TryGetValue(chunkCoord, out chunkData);
    }

    public BlockType GetBlock(Vector3Int worldPosition)
    {
        var chunkCoord = Chunk.GetChunkCoord(worldPosition);
        if(!Chunk.IsValidChunkCoordY(chunkCoord.y))
        {
            return BlockType.Air;
        }
        if (_chunkDataDictionary.TryGetValue(chunkCoord, out var chunkData))
        {
            var localPos = worldPosition - chunkData.worldPosition;
            return chunkData.GetBlock(localPos);
        }
        return BlockType.Air;
    }

    public void SetBlock(Vector3Int worldPosition, BlockType blockType)
    {
        var chunkCoord = Chunk.GetChunkCoord(worldPosition);
        if (_chunkDataDictionary.TryGetValue(chunkCoord, out var chunkData))
        {
            var localPos = worldPosition - chunkData.worldPosition;
            chunkData.SetBlock(localPos, blockType);
        }
    }

    public BlockDataSO GetBlockData(Vector3Int worldPosition)
    {
        return BlockDataManager.GetData(GetBlock(worldPosition));
    }

    public void EditBlock(Vector3Int worldPosition, BlockType blockType)
    {
        if (_editingCoroutine != null)
        {
            StopCoroutine( _editingCoroutine );
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
                if(TryGetChunkData(chunkCoord, out var chunkData) && chunkData.state == ChunkState.Rendering)
                {
                    _blockEditToken.Token.ThrowIfCancellationRequested();
                    meshDatas.TryAdd(chunkCoord, Chunk.GetMeshData(chunkData));
                }
            }
            return meshDatas;
        }, _blockEditToken.Token);
        while(!task.IsCompleted)
        {
            yield return null;
        }
        foreach(var meshData in task.Result)
        {
            if(_chunkRendererDictionary.TryGetValue(meshData.Key, out var chunkRenderer))
            {
                chunkRenderer.RenderMesh(meshData.Value);
            }
        }
    }

}
