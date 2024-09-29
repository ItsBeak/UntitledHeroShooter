using UnityEngine;

using Cinemachine;

using FishNet.Object;
using FishNet.Component.Animating;
using FishNet.Component.Transforming;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Ability")]
    public bool canMove = true;
    public bool canJump;

    [Header("Movement Variables")]
    public float moveSpeed;
    public float jumpForce;
    public float gravityForce;
    public float speedMultiplier = 1;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera playerCamera;
    public Transform lookDirectionBase;

    public float heightOffset;

    [HideInInspector] public Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    bool isDead;

    //public float moveSpeed = 5f;
    public float lookSpeed = 5f;

    public CharacterController characterController;
    public Animator playerAnimator;
    public NetworkAnimator playerNetworkAnimator;
    PlayerHealth health;

    public float minLookAngle, maxLookAngle;

    Vector2 inputDir;

    [Header("Debug")]
    [SerializeField] bool drawHeightCheck;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            playerCamera.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            playerCamera.enabled = false;
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (!IsOwner || characterController == null) return;

        GetInput();
        Move();
        Rotate();
        Animate();
    }

    void GetInput()
    {
        inputDir.x = Input.GetAxis("Horizontal");
        inputDir.y = Input.GetAxis("Vertical");

        inputDir.Normalize();
    }

    void Move()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        moveDirection.x = canMove && health.currentHealthState.Value != PlayerHealthState.Dead ? Input.GetAxis("Horizontal") : 0;
        moveDirection.z = canMove && health.currentHealthState.Value != PlayerHealthState.Dead ? Input.GetAxis("Vertical") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection.y = 0;

        moveDirection.Normalize();

        moveDirection.x *= moveSpeed;
        moveDirection.z *= moveSpeed;

        moveDirection = (forward * (moveDirection.z * speedMultiplier)) + (right * (moveDirection.x * speedMultiplier));

        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded && canJump && health.currentHealthState.Value != PlayerHealthState.Dead)
        {
            moveDirection.y = jumpForce;
            playerNetworkAnimator.SetTrigger("Jump");
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravityForce * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void Rotate()
    {
        //float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        //float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
        //
        //transform.Rotate(Vector3.up * mouseX);
        //lookDirectionBase.Rotate(Vector3.left * mouseY);

        if (canMove && health.currentHealthState.Value != PlayerHealthState.Dead)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -75, 75);
            lookDirectionBase.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    void Animate()
    {
        //playerAnimator.SetFloat("MoveX", inputDir.x);
        //playerAnimator.SetFloat("MoveY", inputDir.y);

        playerAnimator.SetFloat("MoveX", Mathf.Lerp(playerAnimator.GetFloat("MoveX"), inputDir.x, Time.deltaTime * 10));
        playerAnimator.SetFloat("MoveY", Mathf.Lerp(playerAnimator.GetFloat("MoveY"), inputDir.y, Time.deltaTime * 10));

        playerAnimator.SetBool("isGrounded", characterController.isGrounded);
    }
}
