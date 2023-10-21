using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInteract playerInteract;

    public bool flying;

    private bool isWaiting;

    private bool _isInUI;

    private void Awake()
    {
        if(mainCamera == null)
            mainCamera = Camera.main;

        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();

        Cursor.lockState = CursorLockMode.Locked;
    }


    private void OnEnable()
    {
        playerInput.OnFlyClick += HandlerFly;
        playerInput.OnMouseLeftClick += PlayerInput_OnMouseLeftClick;
        playerInput.OnMouseRightClick += PlayerInput_OnMouseRightClick;
    }

    private void OnDisable()
    {
        playerInput.OnFlyClick -= HandlerFly;
        playerInput.OnMouseLeftClick -= PlayerInput_OnMouseLeftClick;
        playerInput.OnMouseRightClick -= PlayerInput_OnMouseRightClick;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            _isInUI = !_isInUI;
            UIManager.Instance.IsActive = _isInUI;
            Cursor.lockState = _isInUI ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (!_isInUI)
        {
            HandlerMovement();
        }

        
    }

    private void HandlerMovement()
    {
        if (flying)
        {
            //animator.SetFloat("speed", 0);
            //animator.SetBool("isGrounded", false);
            //animator.ResetTrigger("jump");
            playerMovement.Fly(playerInput.MovementVector, playerInput.IsJumping, playerInput.IsSprinting);
        }
        else
        {
            //animator.SetBool("isGrounded", playerMovement.IsGrounded);
            if (playerMovement.IsGrounded && playerInput.IsJumping && !isWaiting)
            {
                //animator.SetTrigger("jump");
                isWaiting = true;
                StopAllCoroutines();
                StartCoroutine(ResetWaiting());
            }
            //animator.SetFloat("speed", playerInput.MovementVector.magnitude);
            playerMovement.HandleGravity(playerInput.IsJumping);
            playerMovement.Walk(playerInput.MovementVector, playerInput.IsSprinting);
        }
    }
    private IEnumerator ResetWaiting()
    {
        yield return new WaitForSeconds(0.1f);
        //animator.ResetTrigger("jump");
        isWaiting = false;
    }

    private void PlayerInput_OnMouseRightClick()
    {
        Debug.Log("Right clicked");
    }

    private void PlayerInput_OnMouseLeftClick()
    {
        
    }

    private void HandlerFly()
    {
        flying = !flying;
    }
}
