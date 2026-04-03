using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public float speed = 8f;
    [Header("Jumping")]
    public float jumpForce = 17f;
    private int jumpCount = 0;
    public int maxJump = 2;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private int facingDirection = 1;
    private bool isFacingRight = true;
    private float dashEndTime;
    private bool isWallSliding;
    private float wallSlideSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    [Header("Input")]
    [SerializeField] private bool forceMobileInput;

    private float mobileMoveInput;
    private bool jumpQueued;
    private float currentMoveInput;
    private Coroutine mushroomBuffRoutine;
    private bool isMushroomBuffActive;
    private float baseSpeed;
    private float baseJumpForce;
    private Vector3 baseScale;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    public Rigidbody2D Body => rb;
    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public int FacingDirection => facingDirection;
    public bool IsFacingRight => isFacingRight;
    private bool UseMobileInput => forceMobileInput || Application.isMobilePlatform;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseSpeed = speed;
        baseJumpForce = jumpForce;
        baseScale = transform.localScale;

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CaptureJumpInput();
        CaptureMovementInput();
        WallSlide();
        WallJump();
        HandleJump();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void CaptureJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
        }
    }

    private bool ConsumeJump()
    {
        if (!jumpQueued)
        {
            return false;
        }

        jumpQueued = false;
        return true;
    }

    private void CaptureMovementInput()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Prefer virtual button direction while it is being held.
        if (Mathf.Abs(mobileMoveInput) > 0.01f)
        {
            moveInput = mobileMoveInput;
        }

        currentMoveInput = moveInput;
    }

    private void HandleMovement()
    {
        if (Time.time < dashEndTime)
        {
            return;
        }

        rb.velocity = new Vector2(currentMoveInput * speed, rb.velocity.y);
        if (currentMoveInput > 0)
        {
            SetFacingDirection(1);
        }
        else if (currentMoveInput < 0)
        {
            SetFacingDirection(-1);
        }
    }
    private void SetFacingDirection(int direction)
    {
        facingDirection = direction >= 0 ? 1 : -1;
        isFacingRight = facingDirection > 0;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }
    private bool IsWall()
    {
        if (wallCheck == null)
        {
            return isTouchingWall;
        }

        if (wallLayer == 0)
        {
            return isTouchingWall || Physics2D.OverlapCircle(wallCheck.position, 0.2f);
        }

        return isTouchingWall || Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
    private void WallSlide()
    {
        if (IsWall() && !isGrounded && rb.velocity.y < 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = isFacingRight ? -1f : 1f;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
        if ((isWallSliding || wallJumpingCounter > 0f) && ConsumeJump())
        {
            isWallJumping = true;
            wallJumpingCounter = 0f;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            if (wallJumpingDirection > 0f && !isFacingRight)
            {
                SetFacingDirection(1);
            }
            else if (wallJumpingDirection < 0f && isFacingRight)
            {
                SetFacingDirection(-1);
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }
    private void StopWallJumping()
    {
        isWallJumping = false;
    }
    private void HandleJump()
    {
        if (isWallSliding || isWallJumping)
        {
            return;
        }

        if (!ConsumeJump())
        {
            return;
        }

        if (jumpCount < maxJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            jumpCount++;
            AudioManager.Instance?.PlayJumpSound();
            if (jumpCount == 2)
            {
                animator.SetBool("DoubleJump", true);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        EvaluateCollisionContacts(other);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        EvaluateCollisionContacts(other);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        isGrounded = false;
        isTouchingWall = false;
    }

    private void EvaluateCollisionContacts(Collision2D collision)
    {
        bool wasGrounded = isGrounded;
        bool foundGround = false;
        bool foundWall = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            bool isFootContact = contact.point.y < transform.position.y - 0.05f;
            bool isLandingOrStanding = rb == null || rb.velocity.y <= 0.1f;

            if (normal.y > 0.6f && isFootContact && isLandingOrStanding)
            {
                foundGround = true;
            }

            if (Mathf.Abs(normal.x) > 0.6f)
            {
                foundWall = true;
            }
        }

        isGrounded = foundGround;
        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
            animator.SetBool("DoubleJump", false);

            if (isWallJumping)
            {
                isWallJumping = false;
                CancelInvoke(nameof(StopWallJumping));
            }
        }

        isTouchingWall = foundWall && !isGrounded;
    }

    private void UpdateAnimation()
    {
        float runValue = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Run", runValue);

        float jumpValue = isGrounded ? 0f : 1f;
        animator.SetFloat("Jump", jumpValue);
    }

    public void ConfigureStats(float newSpeed, float newJumpForce, int newMaxJump)
    {
        speed = newSpeed;
        jumpForce = newJumpForce;
        maxJump = Mathf.Clamp(newMaxJump, 1, 3);

        if (!isMushroomBuffActive)
        {
            baseSpeed = speed;
            baseJumpForce = jumpForce;
        }
    }

    public void ApplyMushroomBuff(float duration, float speedMultiplier, float jumpMultiplier, float scaleMultiplier)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (isMushroomBuffActive)
        {
            if (mushroomBuffRoutine != null)
            {
                StopCoroutine(mushroomBuffRoutine);
            }
        }
        else
        {
            baseSpeed = speed;
            baseJumpForce = jumpForce;
            baseScale = transform.localScale;
        }

        isMushroomBuffActive = true;
        speed = baseSpeed * Mathf.Max(0.1f, speedMultiplier);
        jumpForce = baseJumpForce * Mathf.Max(0.1f, jumpMultiplier);
        transform.localScale = baseScale * Mathf.Max(0.1f, scaleMultiplier);

        mushroomBuffRoutine = StartCoroutine(MushroomBuffCountdown(duration));
    }

    private IEnumerator MushroomBuffCountdown(float duration)
    {
        yield return new WaitForSeconds(duration);

        speed = baseSpeed;
        jumpForce = baseJumpForce;
        transform.localScale = baseScale;
        isMushroomBuffActive = false;
        mushroomBuffRoutine = null;
    }

    public void SetHorizontalVelocity(float xVelocity)
    {
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    public void AddImpulse(Vector2 impulse)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    public void BeginDash(float xVelocity, float duration)
    {
        dashEndTime = Mathf.Max(dashEndTime, Time.time + Mathf.Max(0f, duration));
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    public void SetStartCheckpoint(Transform checkpoint)
    {
        bool wasKinematic = rb.isKinematic;
        rb.isKinematic = true;
        transform.position = checkpoint.position;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = wasKinematic;
    }

    public void SetMobileInputEnabled(bool enabled)
    {
        forceMobileInput = enabled;
    }

    public void MobileMoveLeftDown()
    {
        mobileMoveInput = -1f;
    }

    public void MobileMoveRightDown()
    {
        mobileMoveInput = 1f;
    }

    public void MobileMoveRelease()
    {
        mobileMoveInput = 0f;
    }

    public void MobileJump()
    {
        jumpQueued = true;
    }
}
