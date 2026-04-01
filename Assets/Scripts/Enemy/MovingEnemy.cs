using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speedSlime = 3f;
    private float hitAnimLeadTime = 0.1f;
    private float hitUpDistance = 0.25f;
    private float hitUpDuration = 0.08f;
    private float fallDistance = 2.5f;
    private float fallDuration = 0.5f;
    
    private bool faceMovementDirection = true;
    private bool invertFacing = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isHit = false;

    private Vector3 target;
    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private bool movingToB = true;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            enabled = false;
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        pointAPosition = pointA.position;
        pointBPosition = pointB.position;
        target = pointBPosition;
        UpdateFacing();
    }

    void Update()
    {
        if (isHit)
            return;

        Move();
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speedSlime * Time.deltaTime);

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

    public void OnHitByPlayer()
    {
        if (isHit)
            return;

        isHit = true;
        StartCoroutine(PlayDestroyEffect());
    }

    private IEnumerator PlayDestroyEffect()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        if (animator != null && !string.IsNullOrEmpty("Hit"))
        {
            animator.SetTrigger("Hit");
            if (hitAnimLeadTime > 0f)
            {
                yield return new WaitForSeconds(hitAnimLeadTime);
            }
        }

        Vector3 startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * hitUpDistance;
        Vector3 downPos = upPos + Vector3.down * fallDistance;

        float t = 0f;
        while (t < hitUpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / hitUpDuration);
            transform.position = Vector3.Lerp(startPos, upPos, p);
            yield return null;
        }

        t = 0f;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        while (t < fallDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fallDuration);
            transform.position = Vector3.Lerp(upPos, downPos, p);
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
