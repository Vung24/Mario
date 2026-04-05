using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public static PlayerCollision Instance { get; private set; }
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerSkill playerSkill;
    private readonly HashSet<int> triggeredBoxIds = new HashSet<int>();
    private readonly HashSet<int> triggeredEnemyIds = new HashSet<int>();

    private float boxBottomNormalThreshold = 0.5f;
    private float upwardHitVelocityThreshold = 0.05f;
    private float downwardHitVelocityThreshold = -0.05f;
    private float boxCenterYOffsetTolerance = 0.02f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerSkill = GetComponent<PlayerSkill>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Fruits"))
        {
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Traps"))
        {
            TriggerPlayer();
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            TriggerPlayer();
        }
        else if (other.gameObject.CompareTag("Finish"))
        {
            GameManager.Instance?.GameWin();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Finish"))
        {
            GameManager.Instance?.GameWin();
            return;
        }

        BoxController box = collision.gameObject.GetComponent<BoxController>();
        if (box != null || collision.gameObject.CompareTag("Box"))
        {
            TryTriggerBoxHit(collision, box);
            return;
        }

        IEnemy enemy = collision.gameObject.GetComponentInParent<IEnemy>();
        if (enemy != null)
        {
            HitEnemy(collision, enemy);
            return;
        }

        if (!collision.gameObject.CompareTag("Traps"))
        {
            return;
        }

        TriggerPlayer();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        BoxController box = collision.gameObject.GetComponent<BoxController>();
        if (box == null && !collision.gameObject.CompareTag("Box"))
        {
            return;
        }

        TryTriggerBoxHit(collision, box);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        triggeredBoxIds.Remove(collision.gameObject.GetInstanceID());
        triggeredEnemyIds.Remove(collision.gameObject.GetInstanceID());
    }

    private void TryTriggerBoxHit(Collision2D collision, BoxController box)
    {
        if (box == null)
            return;

        int boxId = collision.gameObject.GetInstanceID();
        if (triggeredBoxIds.Contains(boxId))
        {
            return;
        }

        if (!IsBottomHitOnBox(collision))
        {
            return;
        }

        triggeredBoxIds.Add(boxId);
        box.OnHitBox();
    }

    private bool IsBottomHitOnBox(Collision2D collision)
    {
        bool playerIsBelowBox = transform.position.y < (collision.transform.position.y - boxCenterYOffsetTolerance);
        if (!playerIsBelowBox)
        {
            return false;
        }

        bool isMovingUp = collision.relativeVelocity.y > upwardHitVelocityThreshold ||
                          (rb != null && rb.velocity.y > upwardHitVelocityThreshold);

        if (isMovingUp)
        {
            return true;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -boxBottomNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }

public void HitEnemy(Collision2D collision, IEnemy enemy)
{
    MonoBehaviour enemyBehaviour = enemy as MonoBehaviour;
    int enemyId = enemyBehaviour != null
        ? enemyBehaviour.gameObject.GetInstanceID()
        : collision.gameObject.GetInstanceID();

    if (triggeredEnemyIds.Contains(enemyId))
        return;

    bool hitTop = HitTopEnemy(collision);
    SnailEnemy snailEnemy = enemy as SnailEnemy;

    if (hitTop)
    {
        triggeredEnemyIds.Add(enemyId);

        if (snailEnemy != null)
        {
            snailEnemy.OnHitByPlayer();
        }
        else
        {
            enemy.OnHitByPlayer();
        }
        return;
    }

    TriggerPlayer();
}
    private bool HitTopEnemy(Collision2D collision)
    {
        bool playerIsAboveEnemy = transform.position.y > (collision.transform.position.y + boxCenterYOffsetTolerance);
        if (!playerIsAboveEnemy)
        {
            return false;
        }

        bool isMovingDown = collision.relativeVelocity.y < downwardHitVelocityThreshold ||
                            (rb != null && rb.velocity.y < downwardHitVelocityThreshold);

        if (isMovingDown)
        {
            return true;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > boxBottomNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void TriggerPlayer()
    {
        if (playerSkill != null && playerSkill.GuyImmortal())
        {
            return;
        }
        animator.SetTrigger("Hit");
        GameManager.Instance?.GameOver();
    }

    public void OnHitByEnemy()
    {
        TriggerPlayer();
    }

}
