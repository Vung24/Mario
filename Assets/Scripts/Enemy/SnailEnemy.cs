using UnityEngine;

public class SnailEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float shellSlideSpeed = 25f;
    [SerializeField] private float secondStompSlideSpeed = 20f;
    [SerializeField] private float shellRecoverTime = 15f;
    [SerializeField] private float wallNormalThreshold = 0.5f;
    [SerializeField] private float shellDirectionFlipCooldown = 0.05f;
    [SerializeField] private float maxSlideDistance = 50f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 targetPosA;
    private Vector3 targetPosB;
    private Vector3 currentTargetPos;
    private bool movingToB = true;

    private bool isInsideShell = false;
    private float shellExitTime = 0f;

    private bool isShellSliding = false;
    private float shellSlideDirection = 1f;
    private float currentShellSlideSpeed = 0f;
    private float lastShellDirectionFlipTime = -999f;
    private float accumulativeSlideDistance = 0f;
    private int hitCount = 0;

    private void Start()
    {
        shellSlideSpeed = 25f;
        secondStompSlideSpeed = 20f;
        
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        targetPosA = pointA.position;
        targetPosB = pointB.position;
        currentTargetPos = targetPosB;
        currentShellSlideSpeed = shellSlideSpeed;
    }

    private void Update()
    {
        AnimationUpdate();

        if (isShellSliding)
        {
            rb.velocity = new Vector2(shellSlideDirection * currentShellSlideSpeed, rb.velocity.y);

            accumulativeSlideDistance += Mathf.Abs(currentShellSlideSpeed * Time.deltaTime);
            if (accumulativeSlideDistance >= maxSlideDistance)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            return;
        }

        if (isInsideShell)
        {
            shellExitTime -= Time.deltaTime;
            if (shellExitTime <= 0)
            {
                isInsideShell = false;
            }
            return;
        }

        Patrol();
    }

    private void Patrol()
    {
        float distance = Vector2.Distance(transform.position, currentTargetPos);
        if (distance < 0.5f)
        {
            ChangeTarget();
        }

        Vector2 direction = (currentTargetPos - transform.position).normalized;

        transform.Translate(new Vector2(direction.x * patrolSpeed * Time.deltaTime, 0), Space.World);

        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void ChangeTarget()
    {
        if (movingToB)
        {
            currentTargetPos = targetPosA;
            movingToB = false;
        }
        else
        {
            currentTargetPos = targetPosB;
            movingToB = true;
        }
        // Reset hit count khi đổi mục tiêu
        hitCount = 0;
    }

    public void OnHitByPlayer()
    {
        hitCount++;

        if (hitCount == 1)
        {
            isInsideShell = true;
            shellExitTime = shellRecoverTime;
            currentShellSlideSpeed = shellSlideSpeed;
            accumulativeSlideDistance = 0f;
            rb.velocity = Vector2.zero;
            animator.SetTrigger("OneHit");
        }
        else if (hitCount >= 2)
        {
            isInsideShell = true;
            isShellSliding = true;
            shellSlideDirection = 1f;
            currentShellSlideSpeed = secondStompSlideSpeed;
            shellExitTime = shellRecoverTime;
            hitCount = 0;
            currentTargetPos = targetPosB;
            movingToB = true;
            rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isShellSliding)
        {
            IEnemy enemy = collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null && !(enemy is SnailEnemy snailEnemy && snailEnemy == this))
            {
                MonoBehaviour enemyBehaviour = enemy as MonoBehaviour;
                if (enemyBehaviour != null)
                {
                    Destroy(enemyBehaviour.gameObject);
                }
                return;
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                if (IsPlayerStompingFromAbove(collision))
                {
                    return;
                }

                PlayerCollision playerCollision = collision.gameObject.GetComponent<PlayerCollision>();
                if (playerCollision != null)
                {
                    playerCollision.OnHitByEnemy();
                }
                return;
            }

            if (ShouldReverseShellDirection(collision) && Time.time - lastShellDirectionFlipTime >= shellDirectionFlipCooldown)
            {
                shellSlideDirection *= -1f;
                lastShellDirectionFlipTime = Time.time;
            }
        }
    }

    private bool ShouldReverseShellDirection(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            float horizontal = Mathf.Abs(contact.normal.x);
            float vertical = Mathf.Abs(contact.normal.y);
            if (horizontal >= wallNormalThreshold && horizontal > vertical)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPlayerStompingFromAbove(Collision2D collision)
    {
        const float yTolerance = 0.02f;
        const float downVelocityThreshold = -0.05f;

        bool playerIsAbove = collision.transform.position.y > (transform.position.y + yTolerance);
        if (!playerIsAbove)
        {
            return false;
        }

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        return playerRb != null && playerRb.velocity.y < downVelocityThreshold;
    }

    private void AnimationUpdate()
    {
        float currentX = rb != null ? rb.position.x : transform.position.x;
        float value = (isInsideShell || isShellSliding) ? 0f : Mathf.Abs(currentTargetPos.x - currentX);
        animator.SetFloat("Run", value);
    }
}
