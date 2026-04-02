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

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    public Rigidbody2D Body => rb;
    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public int FacingDirection => facingDirection;
    public bool IsFacingRight => isFacingRight;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleJump();
        UpdateAnimation();
        WallSlide();
        WallJump();
    }
    private void HandleMovement()
    {
        if (Time.time < dashEndTime)
        {
            return;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
        if (moveInput > 0)
        {
            SetFacingDirection(1);
        }
        else if (moveInput < 0)
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
        if (Input.GetKeyDown(KeyCode.Space) && (isWallSliding || wallJumpingCounter > 0f))
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
        if (!Input.GetKeyDown(KeyCode.Space))
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
        EvaluateCollisionContacts(other, true);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        EvaluateCollisionContacts(other, false);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        isGrounded = false;
        isTouchingWall = false;
    }

    private void EvaluateCollisionContacts(Collision2D collision, bool isEnter)
    {
        bool foundGround = false;
        bool foundWall = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            if (normal.y > 0.6f)
            {
                foundGround = true;
            }

            if (Mathf.Abs(normal.x) > 0.6f)
            {
                foundWall = true;
            }
        }

        isGrounded = foundGround;
        if (isGrounded && isEnter)
        {
            jumpCount = 0;
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
    }

    public void SetHorizontalVelocity(float xVelocity)
    {
        if (rb == null)
        {
            return;
        }

        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    public void AddImpulse(Vector2 impulse)
    {
        if (rb == null)
        {
            return;
        }

        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    public void BeginDash(float xVelocity, float duration)
    {
        if (rb == null)
        {
            return;
        }

        dashEndTime = Mathf.Max(dashEndTime, Time.time + Mathf.Max(0f, duration));
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    public void SetStartCheckpoint(Transform checkpoint)
    {
        rb.isKinematic = true;
        transform.position = checkpoint.position;
        //rb.isKinematic = false;
    }
}
