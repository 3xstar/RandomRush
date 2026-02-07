using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Collision Settings")]
    public UnityEvent onPlayerDeath;
    public LayerMask obstacleLayer;
    public float collisionCheckRadius = 0.5f;

    [Header("Movement Settings")]
    public float laneDistance = 2.5f;
    public float forwardSpeed = 10f;
    public float laneChangeSpeed = 15f;
    private int currentLane = 1;
    private float targetXPosition;

    [Header("Jump/Slide Settings")]
    public float jumpHeight = 1.5f;
    public float slideDuration = 1f;
    public float jumpCooldown = 0.5f;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.3f;
    public Transform groundCheckPoint;
    private bool isSliding;
    private bool isGrounded;
    private Vector3 originalScale;
    private float lastJumpTime;

    [Header("Physics Settings")]
    public float gravityScale = 2f;
    private float originalGravity;

    [Header("Animation Settings")]
    public string jumpAnimParam = "IsJumping";
    public string slideAnimParam = "IsSliding";
    public string runAnimParam = "IsRunning";
    public string dieAnimParam = "Die";

    private Rigidbody rb;
    private Animator anim;
    private bool controlsInverted = false;
    private bool isDead = false;
    private bool isJumping = false;

    // Input variables
    private bool moveLeftInput;
    private bool moveRightInput;
    private bool jumpInput;
    private bool slideInput;

    // Хэши параметров анимаций
    private int jumpAnimHash;
    private int slideAnimHash;
    private int runAnimHash;
    private int dieAnimHash;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        originalGravity = Physics.gravity.y;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        lastJumpTime = -jumpCooldown;

        // Инициализация хэшей анимаций
        jumpAnimHash = Animator.StringToHash(jumpAnimParam);
        slideAnimHash = Animator.StringToHash(slideAnimParam);
        runAnimHash = Animator.StringToHash(runAnimParam);
        dieAnimHash = Animator.StringToHash(dieAnimParam);

        anim.applyRootMotion = false;
        
        // Инициализация целевой позиции
        UpdateTargetPosition();
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        CheckGrounded();
        HandleMovement();
        ApplyCustomGravity();

        // Обновляем анимацию бега
        anim.SetBool(runAnimHash, isGrounded && !isSliding && !isJumping);
    }

    void HandleInput()
    {
        // Обработка движения влево (одиночное нажатие)
        if (moveLeftInput)
        {
            MoveLeft();
            moveLeftInput = false;
        }

        // Обработка движения вправо (одиночное нажатие)
        if (moveRightInput)
        {
            MoveRight();
            moveRightInput = false;
        }

        // Обработка прыжка
        if (jumpInput)
        {
            Jump();
            jumpInput = false;
        }

        // Обработка слайда
        if (slideInput)
        {
            Slide();
            slideInput = false;
        }
    }

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        // Сбрасываем состояние прыжка когда касаемся земли
        if (isGrounded && !wasGrounded)
        {
            if (isJumping)
            {
                anim.SetBool(jumpAnimHash, false);
                isJumping = false;
            }
        }
        
        // Дополнительная проверка: если мы на земле и velocity.y близок к 0, сбрасываем прыжок
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            anim.SetBool(jumpAnimHash, false);
            isJumping = false;
        }
    }

    void HandleMovement()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.z = forwardSpeed;
        
        // Плавное движение к целевой позиции по X
        float targetX = (currentLane - 1) * laneDistance;
        velocity.x = (targetX - transform.position.x) * laneChangeSpeed;
        
        rb.linearVelocity = velocity;
    }

    void ApplyCustomGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * (originalGravity * gravityScale), ForceMode.Acceleration);
        }
    }

    // Input System методы - используем started вместо performed
    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        if (context.started && !isDead)
        {
            moveLeftInput = true;
        }
    }

    public void OnMoveRight(InputAction.CallbackContext context)
    {
        if (context.started && !isDead)
        {
            moveRightInput = true;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && !isDead)
        {
            jumpInput = true;
        }
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.started && !isDead)
        {
            slideInput = true;
        }
    }

    void MoveLeft()
    {
        if (isSliding || isJumping) return;
        
        if (controlsInverted)
            MoveRightAction();
        else
            MoveLeftAction();
    }

    void MoveRight()
    {
        if (isSliding || isJumping) return;
        
        if (controlsInverted)
            MoveLeftAction();
        else
            MoveRightAction();
    }

    void MoveLeftAction()
    {
        if (currentLane > 0)
        {
            currentLane--;
            UpdateTargetPosition();
        }
    }

    void MoveRightAction()
    {
        if (currentLane < 2)
        {
            currentLane++;
            UpdateTargetPosition();
        }
    }

    void Jump()
    {
        if (controlsInverted)
            SlideAction();
        else if (CanJump())
            JumpAction();
    }

    void Slide()
    {
        if (controlsInverted)
            JumpAction();
        else if (CanSlide())
            SlideAction();
    }

    void JumpAction()
    {
        if (!CanJump()) return;
        
        float jumpForce = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y * gravityScale));
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        lastJumpTime = Time.time;
        isJumping = true;
        anim.SetBool(jumpAnimHash, true);
    }

    void SlideAction()
    {
        if (!CanSlide()) return;
        
        isSliding = true;
        transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
        anim.SetBool(slideAnimHash, true);
        Invoke(nameof(ResetSlide), slideDuration);
    }

    void ResetSlide()
    {
        transform.localScale = originalScale;
        isSliding = false;
        anim.SetBool(slideAnimHash, false);
    }

    bool CanJump()
    {
        return !isDead && isGrounded && !isSliding && !isJumping && (Time.time - lastJumpTime) >= jumpCooldown;
    }

    bool CanSlide()
    {
        return !isDead && isGrounded && !isSliding && !isJumping;
    }

    void UpdateTargetPosition()
    {
        targetXPosition = (currentLane - 1) * laneDistance;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.isKinematic = true;
        anim.SetTrigger(dieAnimHash);
        onPlayerDeath.Invoke();
        this.enabled = false;
    }

    public void SetControlsInverted(bool inverted)
    {
        controlsInverted = inverted;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}