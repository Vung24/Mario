using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private readonly HashSet<int> triggeredBoxIds = new HashSet<int>();

    private float boxBottomNormalThreshold = 0.5f;
    private float upwardHitVelocityThreshold = 0.05f;
    private float boxCenterYOffsetTolerance = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Fruits"))
        {
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Traps"))
        {
            animator.SetTrigger("Hit");
            GameManager.Instance?.GameOver();
        }
        else if(other.gameObject.CompareTag("Enemy")){
            GameManager.Instance?.GameOver();
        }
        else if (other.gameObject.CompareTag("Win"))
        {
            GameManager.Instance?.GameWin();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Win"))
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

        if (!collision.gameObject.CompareTag("Traps"))
        {
            return;
        }

        animator.SetTrigger("Hit");
        GameManager.Instance?.GameOver();
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

}
