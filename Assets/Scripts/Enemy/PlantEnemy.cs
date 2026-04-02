using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private Transform shootPoint;
    private float detectionRange = 10f;
    private float hitAnimLeadTime = 0.1f;
    private float hitUpDistance = 0.2f;
    private float hitUpDuration = 0.08f;
    private float fallDistance = 2f;
    private float fallDuration = 0.3f;

    private float fireTimer = 0f;
    private GameObject player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isHit;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (Bullet.Instance == null)
        {
            Debug.LogError("PlantEnemy: Bullet singleton is missing. Add one Bullet manager in scene.", this);
        }
        fireTimer = fireRate;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (isHit)
        {
            return;
        }
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && PlayerInRange())
        {
            // WaitForOneSecond();
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                ShootBullet();
                fireTimer = 1f / fireRate;
            }
        }
        else
        {
            fireTimer = 1f / fireRate;
            AnimationIdle();
        }

    }
    public IEnumerator WaitForOneSecond()
    {
        yield return new WaitForSeconds(1f);
    }
    public bool PlayerInRange()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            return false;
        }
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool inDistance = distance <= detectionRange;
        bool playerOnLeft = player.transform.position.x < transform.position.x;

        return inDistance && playerOnLeft;
    }
    private void ShootBullet()
    {
        Bullet bulletInstance = Bullet.Instance;
        if (bulletInstance == null)
        {
            return;
        }

        Vector3 shootDirection = Vector3.left;
        WaitForOneSecond();
        bulletInstance.GetBullet(
            shootPoint != null ? shootPoint.position : transform.position,
            shootDirection
        );
        
        AnimationAttack();
    }
    private void AnimationAttack()
    {
        animator.SetBool("Attack", true);
    }
    private void AnimationIdle()
    {
        animator.SetBool("Attack", false);
    }

    public void OnHitByPlayer()
    {
        if (isHit)
        {
            return;
        }

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

