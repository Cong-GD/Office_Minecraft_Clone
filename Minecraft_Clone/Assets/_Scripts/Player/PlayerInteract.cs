using DG.Tweening.Core.Easing;
using Minecraft;
using Minecraft.Input;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField, Required]
    private Transform eye;

    [SerializeField]
    private PlayerData_SO playerData;

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
    private int destroyInputReceiveDelayInMilisecond;

    [SerializeField]
    private LayerMask entityLayer;

    [SerializeField]
    private LayerMask groundLayer;

    private bool isCastHit;

    private Vector3Int hitPosition;
    private Vector3Int adjacentHitPosition;

    private bool _isDigging;
    private float _allowReceiveBuildInputTime;

    private Vector3 _halfOne = new Vector3(0.5f, 0.5f, 0.5f);

    private bool AllowReceiveBuildInput => Time.time > _allowReceiveBuildInputTime;

    public bool IsCastHit => isCastHit;
    public Vector3Int HitPosition => hitPosition;
    public Vector3Int AdjacentHitPosition => adjacentHitPosition;

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
        if (MInput.Destroy.IsPressed() && !_isDigging && isCastHit)
        {
            StartCoroutine(DiggingCoroutine());
        }
    }

    private void ProcessBuildInput()
    {
        if (!MInput.Build.IsPressed() || !AllowReceiveBuildInput)
            return;

        StartBuildInputCoolDown();

        if (!isCastHit)
            return;

        if (!HasInteractedWithBlock())
        {
            CheckForPlaceBlock();
        }
    }

    private void StartBuildInputCoolDown()
    {
        _allowReceiveBuildInputTime = Time.time + buildInputCoolTime;
    }

    private void CheckForPlaceBlock()
    {
        if (!IsSafeForPlaceBlock(adjacentHitPosition))
            return;

        if (!World.Instance.CanEdit())
            return;

        var rightHand = InventorySystem.Instance.RightHand;
        if (rightHand.IsNullOrEmpty())
            return;

        if (rightHand.RootItem is not BlockData_SO blockData)
            return;

        rightHand.TakeAmount(1);
        var direction = GetDirectionWithPlayer(hitPosition + _halfOne);
        var _ = World.Instance.EditBlockAsync(adjacentHitPosition, blockData.BlockType, direction);
    }

    private IEnumerator DiggingCoroutine()
    {
        var hitPosition = this.hitPosition;
        var block = Chunk.GetBlock(hitPosition).Data();
        if (block.BlockType == BlockType.Air)
            yield break;

        _isDigging = true;
        ITool toolInHand = InventorySystem.Instance.RightHand.GetTool();
        blockBreakingProgress.Enable();
        blockBreakingProgress.SetMeshAndPosition(block.GetMeshWithoutUvAtlas(), hitPosition);
        float progress = 0f;
        while (MInput.Destroy.IsPressed())
        {
            DiggingCalculation(toolInHand, block, ref progress);
            animator.Play(AnimID.Attack, 1);
            if (progress >= 1f || hitPosition != this.hitPosition)
                break;

            blockBreakingProgress.SetValue(progress);
            yield return null;
        }
        blockBreakingProgress.Disable();
        _isDigging = false;
        if (progress < 1f)
            yield break;

        var task = World.Instance.EditBlockAsync(hitPosition, BlockType.Air, Direction.Backward);
        yield return Wait.ForTask(task);

        var isSuccess = task.Result;
        if (isSuccess)
        {
            Vector3 randomForce = Vector3.up + Random.insideUnitSphere * dropForce;
            PickupManager.Instance.ThrowItem(block.GetHarvestResult(toolInHand), hitPosition + _halfOne, randomForce);
        }  
    }

    private Direction GetDirectionWithPlayer(Vector3 pos)
    {
        var direction = Vector2Int.RoundToInt((eye.position - pos).XZ().normalized);

        if (direction.x == 0)
            return direction.y >= 0 ? Direction.Forward : Direction.Backward;

        return direction.x >= 0 ? Direction.Right : Direction.Left;
    }

    private void ProcessThrowInput(InputAction.CallbackContext obj)
    {
        var rightHand = InventorySystem.Instance.RightHand;
        if (rightHand.IsNullOrEmpty())
            return;

        var itemPacked = rightHand.TakeAmount(1);
        PickupManager.Instance.ThrowItem(itemPacked, eye.position + eye.forward, eye.forward * 1.5f);
    }

    private bool HasInteractedWithBlock()
    {
        if (MInput.Shift.IsPressed())
            return false;

        var blockHit = Chunk.GetBlock(hitPosition).Data();

        if (blockHit is IInteractable interactable)
        {
            interactable.Interact(hitPosition);
            return true;
        }
        return false;
    }


    private void RayCast()
    {
        var ray = new Ray(eye.transform.position, eye.transform.forward);

        if (Physics.Raycast(ray, out var hit, checkDistance, groundLayer))
        {
            hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
            adjacentHitPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);

            isCastHit = true;
        }
        else
        {
            isCastHit = false;
        }
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
        if(isBestTool && canHarvest)
            speedMultilier = tool.GetToolMultilier();

        if (!playerData.isGrounded)
            speedMultilier /= 5f;

        float damage = speedMultilier / block.Hardness;
        damage *= canHarvest ? 1f : 0.3f;
        damage *= Time.deltaTime;

        progress = Mathf.Clamp01(progress + damage);
    }

    private void OnDrawGizmosSelected()
    {
        if (isCastHit)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(adjacentHitPosition + _halfOne, Vector3.one);
        } 
    }
}
