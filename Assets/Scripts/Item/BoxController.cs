using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public enum BoxType
    {
        Normal_Box,
        Scret_Box,
        Lock_Box
    }
    [SerializeField] private BoxType myType;
    [SerializeField] private Sprite normalBoxSprite;
    [SerializeField] private Sprite secretBoxSprite;
    [SerializeField] private Sprite lockBoxSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        GetRenderChild();
        VisualTypeBox();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetRenderChild();
        VisualTypeBox();
    }

    private void OnValidate()
    {
        GetRenderChild();
        VisualTypeBox();
    }

    private void GetRenderChild()
    {        
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void VisualTypeBox()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite targetSprite = normalBoxSprite;
        switch (myType)
        {
            case BoxType.Normal_Box:
                targetSprite = normalBoxSprite;
                break;
            case BoxType.Scret_Box:
                targetSprite = secretBoxSprite;
                break;
            case BoxType.Lock_Box:
                targetSprite = lockBoxSprite;
                break;
        }

        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }

        if (animator != null)
        {
            animator.enabled = myType == BoxType.Normal_Box;
        }
    }

    public void OnHitBox()
    {
        if (animator == null)
        {
            return;
        }

        switch (myType)
        {
            case BoxType.Normal_Box:
                animator.SetTrigger("HitBox");
                break;
            case BoxType.Scret_Box:
                break;
            case BoxType.Lock_Box:
                break;
        }
    }
}
