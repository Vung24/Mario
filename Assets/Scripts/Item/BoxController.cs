using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public enum BoxType
    {
        Normal_Box,
        Secret_Box,
        Lock_Box
    }
    [SerializeField] private BoxType myType;
    [SerializeField] private Sprite normalBoxSprite;
    [SerializeField] private Sprite secretBoxSprite;
    [SerializeField] private Sprite lockBoxSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject objCoinEarn;
    [SerializeField] private float bumpDistance = 0.2f;
    [SerializeField] private float bumpDuration = 0.08f;
    private Animator animator;
    private bool isBumping;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Disable animator transform driving to prevent all boxes snapping to clip-authored positions.
        if (animator != null)
        {
            animator.enabled = false;
        }

        switch (myType)
        {
            case BoxType.Normal_Box:
                spriteRenderer.sprite = normalBoxSprite;
                break;
            case BoxType.Secret_Box:
                spriteRenderer.sprite = secretBoxSprite;
                break;
            case BoxType.Lock_Box:
                spriteRenderer.sprite = lockBoxSprite;
                break;
        }
    }

    public void OnHitBox()
    {
        switch (myType)
        {
            case BoxType.Normal_Box:
                if (!isBumping)
                {
                    StartCoroutine(BumpBox());
                }
                break;
            case BoxType.Secret_Box:
                if (!isBumping)
                {
                    StartCoroutine(BumpBox());
                }
                objCoinEarn.SetActive(true);
                spriteRenderer.sprite = lockBoxSprite;
                StartCoroutine(IEDeativeCoin());
                myType = BoxType.Lock_Box;
                IEnumerator IEDeativeCoin()
                {
                    yield return new WaitForSeconds(0.83f);
                    objCoinEarn.SetActive(false); 
                }
                break;
            case BoxType.Lock_Box:
                break;
        }
    }

    private IEnumerator BumpBox()
    {
        isBumping = true;

        Vector3 startPos = transform.localPosition;
        Vector3 upPos = startPos + Vector3.up * bumpDistance;

        float t = 0f;
        while (t < bumpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / bumpDuration);
            transform.localPosition = Vector3.Lerp(startPos, upPos, p);
            yield return null;
        }

        t = 0f;
        while (t < bumpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / bumpDuration);
            transform.localPosition = Vector3.Lerp(upPos, startPos, p);
            yield return null;
        }

        transform.localPosition = startPos;
        isBumping = false;
    }
}
