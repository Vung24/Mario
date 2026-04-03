using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SnailEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speedSnail = 1f;
    [SerializeField] private float behindChaseSpeed = 7f;
    [SerializeField] private float gravityScale = 5f;
    private float behindChaseDuration = 3f;
    private bool faceMovementDirection = true;
    private bool invertFacing = false;
    private SpriteRenderer spriteRenderer;

    private Vector3 target;
    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private Animator animator;
    private Rigidbody2D rb;
    private bool movingToB = true;
    private bool isShellMode = false;
    private bool isChasingPlayer = false;
    private bool isDestroying = false;
    private Transform chaseTarget;
    private Transform lastBehindAttacker;
    private Coroutine chaseDestroyRoutine;
    private float backHitToleranceX = 0.08f;
    private float hitAnimLeadTime = 0.1f;
    private float hitUpDistance = 0.25f;
    private float hitUpDuration = 0.08f;
    private float fallDistance = 2.5f;
    private float fallDuration = 0.5f;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Sẽ set thành gravityScale khi đuổi theo
            rb.freezeRotation = true;
        }

        if (pointA == null || pointB == null)
        {
            enabled = false;
            return;
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        pointAPosition = pointA.position;
        pointBPosition = pointB.position;
        target = pointBPosition;
        UpdateFacing();
    }

    void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isDestroying)
        {
            return;
        }

        if (isChasingPlayer)
        {
            ChasePlayer();
            return;
        }

        if (!isShellMode)
        {
            Move();
        }
    }

    private void ChasePlayer()
    {
        Transform resolvedTarget = ResolveChaseTarget();
        if (resolvedTarget != null)
        {
            chaseTarget = resolvedTarget;
            target = chaseTarget.position;
        }

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 desiredPos = chaseTarget != null
            ? (Vector2)chaseTarget.position
            : currentPos + (IsFacingRight() ? Vector2.right : Vector2.left);
        Vector2 nextPos = Vector2.MoveTowards(currentPos, desiredPos, behindChaseSpeed * Time.fixedDeltaTime);

        if (rb != null)
        {
            // Áp dụng gravity và chỉ di chuyển theo trục X
            Vector2 velocity = rb.velocity;
            velocity.x = (nextPos.x - currentPos.x) / Time.fixedDeltaTime;
            rb.velocity = velocity;
            // Gravity sẽ được áp dụng tự động từ rigidbody
            rb.gravityScale = gravityScale;
        }
        else
        {
            transform.position = nextPos;
        }

        if (faceMovementDirection)
        {
            UpdateFacing();
        }
    }

    private void Move()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f; // Tắt gravity khi đi tuần tra bình thường
        }

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 nextPos = Vector2.MoveTowards(currentPos, (Vector2)target, speedSnail * Time.fixedDeltaTime);

        if (rb != null)
        {
            rb.MovePosition(nextPos);
        }
        else
        {
            transform.position = nextPos;
        }

        if ((nextPos - (Vector2)target).sqrMagnitude <= 0.0001f)
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
        if (animator == null)
        {
            return;
        }

        float currentX = rb != null ? rb.position.x : transform.position.x;
        float value = (isShellMode || isDestroying) ? 0f : Mathf.Abs(target.x - currentX);
        animator.SetFloat("Run", value);
    }
    private void UpdateFacing()
    {
        float currentX = rb != null ? rb.position.x : transform.position.x;
        float directionX = target.x - currentX;

        if (Mathf.Abs(directionX) < 0.0001f)
        {
            return;
        }

        bool movingLeft = directionX < 0f;
        bool shouldFaceRight = invertFacing ? !movingLeft : movingLeft;

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
    private void ShellMode()
    {
        isShellMode = true;
        animator.SetTrigger("OneHit");
    }
    public void OnHitByPlayer()
    {
        if (isDestroying)
        {
            return;
        }

        if (!isShellMode)
        {
            ShellMode();
            return;
        }

        isDestroying = true;
        StartCoroutine(PlayDestroyEffect(false));
    }

    public void OnHitByPlayerFromBehind()
    {
        if (isDestroying)
        {
            return;
        }

        if (!isShellMode)
        {
            ShellMode();
        }

        StartBehindChase();
    }

    private bool IsFacingRight()
    {
        if (spriteRenderer != null)
        {
            return !spriteRenderer.flipX;
        }

        return transform.localScale.x >= 0f;
    }
    public bool CanBeHitFromBehind(Transform attacker)
    {
        if (!isShellMode || isDestroying || attacker == null)
        {
            return false;
        }

        float currentX = rb != null ? rb.position.x : transform.position.x;
        float deltaX = attacker.position.x - currentX;
        if (Mathf.Abs(deltaX) <= backHitToleranceX)
        {
            return false;
        }

        bool attackerOnRight = deltaX > 0f;
        bool snailFacingRight = IsFacingRight();
        bool canHit = snailFacingRight ? attackerOnRight : !attackerOnRight;
        if (canHit)
        {
            lastBehindAttacker = attacker;
        }

        return canHit;
    }

    private void StartBehindChase()
    {
        isChasingPlayer = true;
        chaseTarget = ResolveChaseTarget();

        if (chaseDestroyRoutine != null)
        {
            StopCoroutine(chaseDestroyRoutine);
        }

        chaseDestroyRoutine = StartCoroutine(DestroyAfterBehindChase());
    }

    private Transform ResolveChaseTarget()
    {
        if (lastBehindAttacker != null)
        {
            return lastBehindAttacker;
        }

        if (chaseTarget != null)
        {
            return chaseTarget;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        return playerObj != null ? playerObj.transform : null;
    }

    private IEnumerator DestroyAfterBehindChase()
    {
        if (behindChaseDuration > 0f)
        {
            yield return new WaitForSeconds(behindChaseDuration);
        }

        Destroy(gameObject);
    }

    private IEnumerator PlayDestroyEffect(bool useBehindHit)
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger(useBehindHit ? "BehindHit" : "TwoHit");
        }

        if (hitAnimLeadTime > 0f)
        {
            yield return new WaitForSeconds(hitAnimLeadTime);
        }


        Vector3 startPos = rb != null ? (Vector3)rb.position : transform.position;
        Vector3 upPos = startPos + Vector3.up * hitUpDistance;
        Vector3 downPos = upPos + Vector3.down * fallDistance;

        float t = 0f;
        while (t < hitUpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / hitUpDuration);
            Vector3 pos = Vector3.Lerp(startPos, upPos, p);
            if (rb != null)
            {
                rb.position = pos;
            }
            else
            {
                transform.position = pos;
            }
            yield return null;
        }

        t = 0f;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        while (t < fallDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fallDuration);
            Vector3 pos = Vector3.Lerp(upPos, downPos, p);
            if (rb != null)
            {
                rb.position = pos;
            }
            else
            {
                transform.position = pos;
            }
            if (spriteRenderer != null)
            {
                Color c = originalColor;
                c.a = 1f - p;
                spriteRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}