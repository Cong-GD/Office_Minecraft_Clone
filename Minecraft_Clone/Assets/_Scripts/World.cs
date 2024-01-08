using CongTDev.Collection;
using Cysharp.Threading.Tasks;
using Minecraft;
using Minecraft.ProceduralTerrain.Structures;
using NaughtyAttributes;
using ObjectPooling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class World : MonoBehaviour
{
    public const string BLOCK_STATE_FILE_NAME = "BlockStates.dat";
    public const string CHUNK_MODIFICATIONS_FILE_NAME = "ChunkModifications.dat";

    [SerializeField]
    private TerrainGenerator terrainGenerator;

    [SerializeField]
    private ObjectPool chunkRendererPool;

    [SerializeField]
    private PlayerData_SO playerData;

    [field: ShowNonSerializedField]
    public Vector3Int PlayerCoord { get; private set; }

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
    private Dictionary<Vector3Int, MyNativeList<LocalBlock>> _playerModifications = new();
    private Dictionary<Vector3Int, List<IBlockState>> _blockStates = new();
    private CancellationToken _cancellationToken;
    private ParallelOptions _parallelOptions;
    private bool _isEditing;

    public static World Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ThreadSafePool<MeshData>.Capacity = 50;

        _cancellationToken = destroyCancellationToken;
        _parallelOptions = new ParallelOptions()
        {
            CancellationToken = _cancellationToken,
            MaxDegreeOfParallelism = startMaxDegreeOfParallelism
        };
        GameSettings.Instance.Apply();
        GameManager.Instance.OnGameSave += SaveWorldDatas;
        GameManager.Instance.OnGameLoad += LoadWorldDatas;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameSave -= SaveWorldDatas;
        GameManager.Instance.OnGameLoad -= LoadWorldDatas;
        Instance = null;
    }

    private async void Start()
    {
        await UniTask.DelayFrame(10);
        Debug.Log("World: Start generate world");
        int renderDistance = GameSettings.Instance.RenderDistance;
        Vector3 playerPosition = playerData.PlayerBody.position;
        Vector3Int playerCoord = Chunk.GetChunkCoord(playerPosition.With(y: 0));
        try
        {
            if (multiThread)
            {
                await UniTask.SwitchToThreadPool();
            }
            else
            {
                _parallelOptions.MaxDegreeOfParallelism = 1;
            }

            PrepareChunkData(playerCoord, renderDistance);
            BuildStructures();
            OveridePlayerModifications();
            PrepareMeshDatas(playerCoord, renderDistance);
            await UniTask.SwitchToMainThread();
            while (_preparedMeshs.TryDequeue(out MeshData meshData))
            {
                RenderMesh(meshData);
            }
            PlayerController.Instance.SpawnPlayer();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        if (continueGenerate)
        {
            LongtermViewCheckTask().Forget();
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

    private int ChunkLoadRange(int renderDistance)
    {
        return renderDistance + 1;
    }

    private int ChunkHideRange(int renderDistance)
    {
        return renderDistance + 3;
    }

    private void SaveWorldDatas(Dictionary<string, ByteString> dictionary)
    {
        try
        {
            lock (_playerModifications)
            {
                ByteString byteString = ByteString.Create(5000);
                byteString.WriteValue(_playerModifications.Count);
                foreach ((Vector3Int coord, MyNativeList<LocalBlock> localBlocks) in _playerModifications)
                {
                    byteString.WriteValue(coord);
                    byteString.WriteValue(localBlocks.Count);
                    byteString.WriteValues<LocalBlock>(localBlocks.AsSpan());
                }
                dictionary[CHUNK_MODIFICATIONS_FILE_NAME] = byteString;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error while save {CHUNK_MODIFICATIONS_FILE_NAME}: {e}");
        }

        try
        {
            ByteString blockStateData = ByteString.Create(5000);
            blockStateData.WriteValue(_blockStates.Count);
            foreach ((Vector3Int coord, List<IBlockState> blockStates) in _blockStates)
            {
                blockStateData.WriteValue(coord);
                blockStateData.WriteValue(blockStates.Count);
                foreach (IBlockState blockState in blockStates)
                {
                    BlockStateSerializeHelper.GetSerializedData(blockStateData, blockState);
                }
            }
            dictionary[BLOCK_STATE_FILE_NAME] = blockStateData;
        }
        catch (Exception e)
        {

            Debug.LogWarning($"Error while save {BLOCK_STATE_FILE_NAME}: {e}");
        }

    }

    private void LoadWorldDatas(Dictionary<string, ByteString> dictionary)
    {
        try
        {
            if (dictionary.Remove(CHUNK_MODIFICATIONS_FILE_NAME, out ByteString byteString))
            {
                ByteString.BytesReader byteReader = byteString.GetBytesReader();
                int count = byteReader.ReadValue<int>();

                Dictionary<Vector3Int, MyNativeList<LocalBlock>> chunksModifiedByPlayer = new Dictionary<Vector3Int, MyNativeList<LocalBlock>>(count);

                for (int i = 0; i < count; i++)
                {
                    Vector3Int coord = byteReader.ReadValue<Vector3Int>();
                    int localBlockCount = byteReader.ReadValue<int>();
                    MyNativeList<LocalBlock> localBlocks = new(byteReader.ReadValues<LocalBlock>(localBlockCount));
                    chunksModifiedByPlayer[coord] = localBlocks;
                }

                lock (_playerModifications)
                {
                    _playerModifications = chunksModifiedByPlayer;
                }
                byteString.Dispose();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error while load {CHUNK_MODIFICATIONS_FILE_NAME}: {e}");
        }

        try
        {

            if (dictionary.Remove(BLOCK_STATE_FILE_NAME, out ByteString byteString))
            {
                ByteString.BytesReader byteReader = byteString.GetBytesReader();
                int chunkCount = byteReader.ReadValue<int>();
                Dictionary<Vector3Int, List<IBlockState>> blockStates = new Dictionary<Vector3Int, List<IBlockState>>(chunkCount);
                for (int i = 0; i < chunkCount; i++)
                {
                    Vector3Int coord = byteReader.ReadValue<Vector3Int>();
                    int blockStateCount = byteReader.ReadValue<int>();
                    List<IBlockState> blockStateList = new List<IBlockState>(blockStateCount);
                    for (int j = 0; j < blockStateCount; j++)
                    {
                        IBlockState blockState = BlockStateSerializeHelper.GetBlockState(ref byteReader);
                        if (blockState != null)
                        {
                            blockStateList.Add(blockState);
                        }
                        else
                        {
                            Debug.LogError($"Can't deserialize block state at {coord}");
                        }
                    }
                    blockStates[coord] = blockStateList;
                }
                _blockStates = blockStates;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error while load {BLOCK_STATE_FILE_NAME}: {e}");
        }
    }

    public List<IBlockState> GetOrAddBlockStates(Vector3Int chunkCoord)
    {
        if (!_blockStates.TryGetValue(chunkCoord, out List<IBlockState> blockStates))
        {
            blockStates = new List<IBlockState>();
            _blockStates[chunkCoord] = blockStates;
        }
        return blockStates;
    }

    public void ValidateBlockState(Vector3Int chunkCoord)
    {
        if (!_blockStates.TryGetValue(chunkCoord, out List<IBlockState> blockStates))
            return;

        for (int i = blockStates.Count - 1; i >= 0; i--)
        {
            if (!blockStates[i].ValidateBlockState())
            {
                blockStates.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Background thread
    /// Method LongtermViewCheckTask() only call once on Start(), Don't call any of these methods from outside of this region
    /// </summary>
    #region Long term world operation method group
    private async UniTaskVoid LongtermViewCheckTask()
    {
        Vector3Int playerCoord = PlayerCoord;
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (playerCoord == PlayerCoord)
                {
                    await UniTask.Delay(200, cancellationToken: _cancellationToken);
                    continue;
                }
                playerCoord = PlayerCoord;
                _parallelOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
                int renderDistance = GameSettings.Instance.RenderDistance;
                await UniTask.SwitchToThreadPool();

                RemoveOutOfRangeChunkData(playerCoord, renderDistance);
                PrepareChunkData(playerCoord, renderDistance);
                ExcuseModifyQuery();
                BuildStructures();
                OveridePlayerModifications();
                PrepareMeshDatas(playerCoord, renderDistance);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LongtermViewCheckTask canceled");
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void RemoveOutOfRangeChunkData(Vector3Int playerCoord, int renderDistance)
    {
        bool IsThisChunkNeedToReturn(ChunkData chunkData)
        {
            if (chunkData.state is not (ChunkState.Generated or ChunkState.Rendering))
                return false;

            if (Chunk.IsPositionInRange(playerCoord, chunkData.chunkCoord, ChunkHideRange(renderDistance)))
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

    private void PrepareChunkData(Vector3Int playerCoord, int renderDistance)
    {
        using TimeExcute timer = TimeExcute.Start("Prepare chunk datas");
        terrainGenerator.CalculateBiomeCenter(playerCoord.x, playerCoord.z);
        if (_parallelOptions.MaxDegreeOfParallelism == 1)
        {
            foreach (Vector3Int chunkCoord in Chunk.GetCoordsInRange(playerCoord, ChunkLoadRange(renderDistance)))
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

            Parallel.ForEach(Chunk.GetCoordsInRange(playerCoord, ChunkLoadRange(renderDistance))
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
        List<ChunkData> chunkDatas = ThreadSafePool<List<ChunkData>>.Get();
        chunkDatas.Clear();
        chunkDatas.AddRange(_hasModifierChunks.Values);
        foreach (ChunkData chunkData in chunkDatas)
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
        chunkDatas.Clear();
        ThreadSafePool<List<ChunkData>>.Release(chunkDatas);
    }

    private void BuildStructures()
    {
        MyNativeList<ModifierUnit> modifiers = ThreadSafePool<MyNativeList<ModifierUnit>>.Get();
        foreach (ChunkData chunkData in _haveStructuresChunks)
        {
            foreach ((Vector3Int position, IStructure structure) in chunkData.structures)
            {
                modifiers.Clear();
                structure.GetModifications(modifiers, position);
                foreach (ref ModifierUnit modifier in modifiers.AsSpan())
                {
                    ApplyModification(chunkData, modifier);
                }
            }

            if (chunkData.HasModifier())
            {
                _hasModifierChunks.TryAdd(chunkData.chunkCoord, chunkData);
            }
        }
        modifiers.Clear();
        ThreadSafePool<MyNativeList<ModifierUnit>>.Release(modifiers);
        _haveStructuresChunks.Clear();
    }

    private void ApplyModification(ChunkData chunkData, in ModifierUnit mod)
    {
        if (!Chunk.IsValidWorldY(mod.y))
            return;

        Vector3Int chunkCoord = Chunk.GetChunkCoord(mod.x, mod.y, mod.z);

        if (!TryGetChunkData(chunkCoord, out var modifyChunk))
        {
            chunkData.modifierQueue.Enqueue(mod);
            return;
        }

        if (modifyChunk.state == ChunkState.Rendering)
            modifyChunk.isDirty = true;

        modifyChunk.SetBlock(
            mod.x - modifyChunk.worldPosition.x,
            mod.y - modifyChunk.worldPosition.y,
            mod.z - modifyChunk.worldPosition.z,
            mod.blockType,
            mod.direction);
    }

    private void OveridePlayerModifications()
    {
        using TimeExcute timer = TimeExcute.Start("Overide player modifications");
        foreach(ChunkData chunkData in _activeChunkData)
        {
            if(chunkData.state != ChunkState.Rendering || chunkData.isDirty)
            {
                EnsurePlayerModifications(chunkData);
            }
        }
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
        //using TimeExcute timer = TimeExcute.Start("Prepare mesh datas");
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
                EnsurePlayerModifications(chunkData);
                MeshData meshData = Chunk.GetMeshData(chunkData);
                chunkData.state = ChunkState.MeshPrepared;
                _preparedMeshs.Enqueue(meshData);
            });
        }
    }

    private void EnsurePlayerModifications(ChunkData chunkData)
    {
        MyNativeList<LocalBlock> localBlocks;
        lock (_playerModifications)
        {
            if (!_playerModifications.TryGetValue(chunkData.chunkCoord, out localBlocks))
            {
                return;
            }
        }

        lock (localBlocks)
        {
            try
            {
                foreach (LocalBlock localBlock in localBlocks.AsSpan())
                {
                    chunkData.SetBlock(localBlock.x, localBlock.y, localBlock.z, localBlock.blockType, localBlock.direction);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error when apply player modification to chunk {chunkData.chunkCoord}: {e}");
            } 
        }
    }
    #endregion

    private void RenderMesh(MeshData meshData)
    {
        Vector3Int coord = meshData.position;
        if (!TryGetChunkData(coord, out ChunkData chunkData))
            return;

        ValidateBlockState(coord);
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
        else if (meshData.HasDataToRender())
        {
            chunkRenderer = (ChunkRenderer)chunkRendererPool.Get(transform);
            chunkRenderer.SetChunkData(chunkData);
            _chunkRendererDictionary[coord] = chunkRenderer;
            chunkRenderer.RenderMesh(meshData);
        }

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

    public async UniTask<bool> EditBlockAsync(Vector3Int worldPosition, BlockType blockType, Direction direction)
    {
        if (_isEditing)
            return false;

        Vector3Int coord = Chunk.GetChunkCoord(worldPosition);

        if (!TryGetChunkData(coord, out ChunkData chunkData))
        {
            return false;
        }
        _isEditing = true;
        await UniTask.SwitchToThreadPool();
        Vector3Int localPos = worldPosition - coord * WorldSettings.ChunkSizeVector;
        chunkData.SetBlock(localPos.x, localPos.y, localPos.z, blockType, direction);

        MyNativeList<LocalBlock> localBlocks;
        lock (_playerModifications)
        {
            if (!_playerModifications.TryGetValue(coord, out localBlocks))
            {
                localBlocks = new MyNativeList<LocalBlock>();
                _playerModifications[coord] = localBlocks;
            }
        }

        LocalBlock localBlock = new LocalBlock()
        {
            x = (byte)localPos.x,
            y = (byte)localPos.y,
            z = (byte)localPos.z,
            blockType = blockType,
            direction = direction
        };

        lock (localBlocks)
        {
            int index = localBlocks.IndexOf(localBlock);
            if (index == -1)
            {
                localBlocks.Add(localBlock);
            }
            else
            {
                localBlocks[index] = localBlock;
            }
        }

        foreach (Vector3Int chunkCoord in Chunk.GetAdjacentChunkCoords(worldPosition))
        {
            if (TryGetChunkData(chunkCoord, out chunkData) && chunkData.state == ChunkState.Rendering)
            {
                _priorityMeshToRenders.Enqueue(Chunk.GetMeshData(chunkData));
            }
        }
        await UniTask.SwitchToMainThread();
        _isEditing = false;
        return true;
    }
}
