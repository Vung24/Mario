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
    [SerializeField] private float groundNormalThreshold = 0.6f;
    [SerializeField] private float wallNormal = 0.6f;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private int facingDirection = 1;
    private float dashEndTime;

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
        if (Time.time < dashEndTime)
        {
            return;
        }

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

        if(jumpCount < maxJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            jumpCount++;
            AudioManager.Instance?.PlayJumpSound();
            if(jumpCount == 2){
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
            }
        }

        isGrounded = foundGround;
        if(isGrounded && isEnter)
        {
            jumpCount = 0;
        }

        isTouchingWall = foundWall && !isGrounded;
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
