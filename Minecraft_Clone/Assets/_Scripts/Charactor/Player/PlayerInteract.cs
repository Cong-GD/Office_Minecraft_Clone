using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using Minecraft.Input;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minecraft
{
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField]
        private Transform eye;

        [SerializeField]
        private PlayerData_SO playerData;

        [SerializeField]
        private PlayerAttack attacker;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private BlockBreakingProgress blockBreakingProgress;

        [Min(1f)]
        public float checkDistance;

        public float dropForce = 1f;

        [SerializeField]
        private float buildInputCoolTime = 0.3f;

        [SerializeField]
        private LayerMask entityLayer;

        [SerializeField]
        private LayerMask groundLayer;

        [SerializeField]
        private EventReference diggingSoundEvent;

        public bool IsCastHit => _isCastHitGround;

        public Vector3Int HitPosition => _hitPosition;

        public Vector3Int AdjacentHitPosition => _adjacentHitPosition;

        private bool AllowReceiveBuildInput => Time.time > _allowReceiveBuildInputTime;

        private bool _isCastHitGround;
        private bool _isCastHitEntity;
        private Vector3Int _hitPosition;
        private Vector3Int _adjacentHitPosition;
        private bool _isDigging;
        private float _allowReceiveBuildInputTime;
        private Vector3 _halfOne = new Vector3(0.5f, 0.5f, 0.5f);
        private float _destroyAnimationTriggedTime;
        private EventInstance _diggingSoundInstance;
        private PARAMETER_ID _blockMaterialParameterId;

        [SerializeField, ReadOnly]
        private GameObject _hitEntity;

        private void Start()
        {
            _allowReceiveBuildInputTime = 0f;
            _destroyAnimationTriggedTime = 0f;
            _diggingSoundInstance = RuntimeManager.CreateInstance(diggingSoundEvent);
            Audio.AudioManager.GetParameterID(_diggingSoundInstance, "BlockMaterial", out _blockMaterialParameterId);
            _diggingSoundInstance.set3DAttributes(Vector3.zero.To3DAttributes());
        }

        private void OnDestroy()
        {
            _diggingSoundInstance.release();
        }

        private void OnEnable()
        {
            MInput.Throw.performed += ProcessThrowInput;
        }

        private void OnDisable()
        {
            MInput.Throw.performed -= ProcessThrowInput;
        }

        private void Update()
        {
            RayCast();
            ProcessDestroyInput();
            ProcessBuildInput();
        }

        private void ProcessDestroyInput()
        {
            if (MInput.Destroy.IsPressed())
            {
                if (Time.time > _destroyAnimationTriggedTime)
                {
                    _destroyAnimationTriggedTime = Time.time + 0.1f;
                    animator.Play(AnimID.Attack, 1);
                }
                if (!_isDigging && _isCastHitGround)
                {
                    StartCoroutine(DiggingCoroutine());
                }
                else if (_isCastHitEntity)
                {
                    attacker.TryAttack(InventorySystem.Instance.RightHand, _hitEntity);
                }
            }
        }

        private void ProcessBuildInput()
        {
            if (!MInput.Build.IsPressed() || !AllowReceiveBuildInput)
                return;

            StartBuildInputCoolDown();

            if (_isCastHitGround)
            {
                if (CheckAndInteractWithBlock())
                {
                    return;
                }
                CheckAndPlaceBlock();
            }
            else if (_isCastHitEntity)
            {
                // TODO: Interact with entity
            }
        }

        private void StartBuildInputCoolDown()
        {
            _allowReceiveBuildInputTime = Time.time + buildInputCoolTime;
        }

        private bool CheckAndPlaceBlock()
        {
            if (!IsSafeForPlaceBlock(_adjacentHitPosition))
                return false;

            if (!World.Instance.CanEdit())
                return false;

            var rightHand = InventorySystem.Instance.RightHand;
            if (rightHand.IsNullOrEmpty())
                return false;

            if (rightHand.RootItem is not BlockData_SO blockData)
                return false;

            Direction direction = GetDirectionWithPlayer(_hitPosition + _halfOne);
            World.Instance.EditBlockAsync(_adjacentHitPosition, blockData.BlockType, direction).Forget();
            rightHand.TakeAmount(1);
            _diggingSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(_adjacentHitPosition));
            _diggingSoundInstance.setParameterByID(_blockMaterialParameterId, (float)blockData.BlockMaterial);
            _diggingSoundInstance.start();
            return true;
        }

        private IEnumerator DiggingCoroutine()
        {
            Vector3Int hitPosition = _hitPosition;
            BlockData_SO block = Chunk.GetBlock(hitPosition).Data();
            if (block.BlockType == BlockType.Air)
                yield break;

            _isDigging = true;
            ITool toolInHand = InventorySystem.Instance.RightHand.GetTool();
            blockBreakingProgress.Enable();
            blockBreakingProgress.SetMeshAndPosition(block.GetMeshFlattenUV(), hitPosition);
            float progress = 0f;
            while (MInput.Destroy.IsPressed())
            {
                DiggingCalculation(toolInHand, block, ref progress);
                if (progress >= 1f || !_isCastHitGround || hitPosition != _hitPosition)
                    break;

                blockBreakingProgress.SetValue(progress);
                yield return null;
            }
            blockBreakingProgress.Disable();
            _isDigging = false;
            if (progress < 1f)
                yield break;

            _diggingSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(hitPosition));
            _diggingSoundInstance.setParameterByID(_blockMaterialParameterId, (float)block.BlockMaterial);
            _diggingSoundInstance.start();
            BreakBlockAsync(hitPosition, block, toolInHand).Forget();
        }

        private async UniTaskVoid BreakBlockAsync(Vector3Int position, BlockData_SO block, ITool toolInHand)
        {
            bool isSuccess = await World.Instance.EditBlockAsync(position, BlockType.Air, Direction.Backward);
            if (isSuccess)
            {
                Vector3 randomForce = Vector3.up + Random.insideUnitSphere * dropForce;
                PickupManager.Instance.ThrowItem(block.GetHarvestResult(toolInHand), position + _halfOne, randomForce);
            }
        }

        private Direction GetDirectionWithPlayer(Vector3 pos)
        {
            Vector2Int direction = Vector2Int.RoundToInt((eye.position - pos).XZ().normalized);

            if (direction.x == 0)
                return direction.y >= 0 ? Direction.Forward : Direction.Backward;

            return direction.x > 0 ? Direction.Right : Direction.Left;
        }

        private void ProcessThrowInput(InputAction.CallbackContext _)
        {
            ItemSlot rightHand = InventorySystem.Instance.RightHand;
            if (rightHand.IsNullOrEmpty())
                return;

            ItemPacked itemPacked = rightHand.TakeAmount(1);
            PickupManager.Instance.ThrowItem(itemPacked, eye.position + eye.forward, eye.forward * 1.5f);
        }

        private bool CheckAndInteractWithBlock()
        {
            if (MInput.Shift.IsPressed())
                return false;

            BlockData_SO blockHit = Chunk.GetBlock(_hitPosition).Data();

            if (blockHit is IInteractableBlock interactable)
            {
                interactable.Interact(_hitPosition);
                return true;
            }
            return false;
        }


        private void RayCast()
        {
            Ray ray = new Ray(eye.transform.position, eye.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, checkDistance, groundLayer | entityLayer))
            {
                _isCastHitGround = false;
                _isCastHitEntity = false;
                _hitEntity = null;
                return;
            }

            GameObject hitObject = hit.collider.gameObject;

            _isCastHitGround = IsGround(hitObject.layer);
            _isCastHitEntity = !_isCastHitGround;
            if (_isCastHitEntity)
            {
                _hitEntity = hitObject;
            }
            else
            {
                _hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
                _adjacentHitPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);
            }
        }

        private bool IsGround(int layer)
        {
            return (groundLayer.value & 1 << layer) != 0;
        }

        private bool IsSafeForPlaceBlock(Vector3Int position)
        {
            return !Physics.CheckBox(position + _halfOne, _halfOne, Quaternion.identity, entityLayer);
        }


        private void DiggingCalculation(ITool tool, BlockData_SO block, ref float progress)
        {
            bool isBestTool = block.BestTool == tool.ToolType;
            bool canHarvest = block.CanHarvestBy(tool);

            float speedMultilier = 1f;
            if (isBestTool && canHarvest)
                speedMultilier = tool.GetToolMultilier();

            //if (!playerData.isGrounded)
                //speedMultilier /= 5f;

            float damage = speedMultilier / block.Hardness;
            damage *= canHarvest ? 1f : 0.3f;
            damage *= Time.deltaTime;

            progress = Mathf.Clamp01(progress + damage);
        }

        private void OnDrawGizmosSelected()
        {
            if (_isCastHitGround)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_adjacentHitPosition + _halfOne, Vector3.one);
            }
        }
    }
}