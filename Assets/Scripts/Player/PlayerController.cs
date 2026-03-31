using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    private float speed = 9f;
    [Header("Jumping")]
    private float jumpForce = 18f;
    private int jumpCount = 0;
    private int maxJump = 2;
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpHorizontalForce = 12f;
    [SerializeField] private float wallJumpVerticalForce = 16f;
    [SerializeField] private float groundNormalThreshold = 0.6f;
    [SerializeField] private float wallNormal = 0.6f;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private int wallSide = 0;
    private int facingDirection = 1;

    public Rigidbody2D Body => rb;
    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public int FacingDirection => facingDirection;
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
    }
    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
        if(moveInput > 0)
        {
            facingDirection = 1;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
        }
        else if(moveInput < 0)
        {
            facingDirection = -1;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
    private void HandleJump()
    {
        if(!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        if(isTouchingWall && !isGrounded && jumpCount < maxJump)
        {
            float jumpDirection = wallSide == 1 ? -1f : 1f;
            rb.velocity = new Vector2(jumpDirection * wallJumpHorizontalForce, wallJumpVerticalForce);
            jumpCount = maxJump;
            isTouchingWall = false;
            return;
        }

        if(jumpCount < maxJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            jumpCount++;
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
        wallSide = 0;
    }

    private void EvaluateCollisionContacts(Collision2D collision)
    {
        bool foundGround = false;
        bool foundWall = false;
        int detectedWallSide = 0;

        foreach(ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            if(normal.y > groundNormalThreshold)
            {
                foundGround = true;
            }

            if(Mathf.Abs(normal.x) > wallNormal)
            {
                foundWall = true;
                detectedWallSide = normal.x > 0f ? -1 : 1;
            }
        }

        isGrounded = foundGround;
        if(isGrounded)
        {
            jumpCount = 0;
        }

        isTouchingWall = foundWall && !isGrounded;
        wallSide = isTouchingWall ? detectedWallSide : 0;
    }

    private void UpdateAnimation(){
        float runValue = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Run", runValue);

        float jumpValue = isGrounded ? 0f : 1f;
        animator.SetFloat("Jump", jumpValue);
    }

    public void ConfigureStats(float newSpeed, float newJumpForce, int newMaxJump)
    {
        speed = newSpeed;
        jumpForce = newJumpForce;
        maxJump = Mathf.Max(1, newMaxJump);
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
}
