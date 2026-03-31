using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailEnemy : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speedSnail = 1f;
    private bool faceMovementDirection = true;
    private bool invertFacing = false;
    private SpriteRenderer spriteRenderer;

    private Vector3 target;
    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private Animator animator;
    private Rigidbody2D rb;
    private bool movingToB = true;
    private bool isHit = false; // Trạng thái bị đạp

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Disable gravity để snail không rơi
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }
        
        if (pointA == null || pointB == null)
        {
            enabled = false;
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        pointAPosition = pointA.position;
        pointBPosition = pointB.position;
        target = pointBPosition;
        UpdateFacing();
    }

    void Update()
    {
        if (!isHit) // Chỉ di chuyển khi chưa bị đạp
        {
            Move();
        }
        UpdateAnimation();
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speedSnail * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude <= 0.0001f)
        {
            movingToB = !movingToB;
            target = movingToB ? pointBPosition : pointAPosition;
        }

        if (faceMovementDirection)
        {
            UpdateFacing();
        }
    }
    private void UpdateAnimation()
    {
        if (isHit)
        {
            // Khi bị đạp, chơi animation "Hit"
            animator.SetTrigger("Hit");
        }
        else
        {
            // Khi đang chạy bình thường
            float Value = Mathf.Abs(target.x - transform.position.x);
            animator.SetFloat("Run", Value);
        }
    }
    private void UpdateFacing()
    {
        float directionX = target.x - transform.position.x;
        
        if (Mathf.Abs(directionX) < 0.0001f)
        {
            return;
        }

        bool movingRight = directionX < 0f;
        bool shouldFaceRight = invertFacing ? !movingRight : movingRight;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !shouldFaceRight;
            return;
        }

        Vector3 scale = transform.localScale;
        float absX = Mathf.Abs(scale.x);
        scale.x = shouldFaceRight ? absX : -absX;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (playerRb != null && playerRb.velocity.y < 0)
            {
                if (collision.gameObject.transform.position.y > transform.position.y)
                {
                    HitByPlayer();
                }
            }
        }
    }

    private void HitByPlayer()
    {
        isHit = true;
        Debug.Log("Snail hit by player from above!");
        enabled = false;
    }
}