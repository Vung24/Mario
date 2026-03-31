using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    private enum CharacterType
    {
        MaskDude = 0,
        NinjaFrog = 1,
        PinkMan = 2,
        VitualGuy = 3
    }

    [Header("Character")]
    [SerializeField] private CharacterType activeCharacter = CharacterType.MaskDude;
    [SerializeField] private bool allowSwitchByNumberKeys = true;

    [Header("MaskDude")]
    [SerializeField] private float maskDudeSpeed = 8f;
    [SerializeField] private float maskDudeJumpForce = 16f;
    [SerializeField] private int maskDudeMaxJump = 2;
    [SerializeField] private float maskDudeDashSpeed = 18f;
    [SerializeField] private float maskDudeDashCooldown = 1f;

    [Header("NinjaFrog")]
    [SerializeField] private float ninjaFrogSpeed = 11f;
    [SerializeField] private float ninjaFrogJumpForce = 15f;
    [SerializeField] private int ninjaFrogMaxJump = 3;

    [Header("PinkMan")]
    [SerializeField] private float pinkManSpeed = 7f;
    [SerializeField] private float pinkManJumpForce = 18f;
    [SerializeField] private int pinkManMaxJump = 2;
    [SerializeField] private float pinkManGlideGravityScale = 0.35f;
    [SerializeField] private KeyCode pinkManGlideKey = KeyCode.LeftShift;

    [Header("VitualGuy")]
    [SerializeField] private float vitualGuySpeed = 10f;
    [SerializeField] private float vitualGuyJumpForce = 16f;
    [SerializeField] private int vitualGuyMaxJump = 2;
    [SerializeField] private float vitualGuyWallSlideSpeed = 3f;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private float defaultGravityScale;
    private float nextMaskDudeDashTime;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            defaultGravityScale = rb.gravityScale;
        }

        ApplyCharacterProfile(activeCharacter);
    }

    void Update()
    {
        HandleCharacterSwitch();
        HandleCharacterSkill();
    }

    private void HandleCharacterSwitch()
    {
        if (!allowSwitchByNumberKeys)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ApplyCharacterProfile(CharacterType.MaskDude);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ApplyCharacterProfile(CharacterType.NinjaFrog);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ApplyCharacterProfile(CharacterType.PinkMan);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ApplyCharacterProfile(CharacterType.VitualGuy);
        }
    }

    private void HandleCharacterSkill()
    {
        if (playerController == null)
        {
            return;
        }

        if (activeCharacter == CharacterType.MaskDude)
        {
            HandleMaskDudeSkill();
            return;
        }

        if (activeCharacter == CharacterType.PinkMan)
        {
            HandlePinkManSkill();
            return;
        }

        if (activeCharacter == CharacterType.VitualGuy)
        {
            HandleVitualGuySkill();
            return;
        }

        ResetGravityToDefault();
    }

    private void HandleMaskDudeSkill()
    {
        ResetGravityToDefault();

        if (!Input.GetKeyDown(KeyCode.E) || Time.time < nextMaskDudeDashTime)
        {
            return;
        }

        float dashVelocity = playerController.FacingDirection * maskDudeDashSpeed;
        playerController.SetHorizontalVelocity(dashVelocity);
        nextMaskDudeDashTime = Time.time + maskDudeDashCooldown;
    }

    private void HandlePinkManSkill()
    {
        if (rb == null)
        {
            return;
        }

        bool canGlide = !playerController.IsGrounded && rb.velocity.y < 0f && Input.GetKey(pinkManGlideKey);
        rb.gravityScale = canGlide ? pinkManGlideGravityScale : defaultGravityScale;
    }

    private void HandleVitualGuySkill()
    {
        if (rb == null)
        {
            return;
        }

        if (playerController.IsTouchingWall && !playerController.IsGrounded && rb.velocity.y < 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -vitualGuyWallSlideSpeed));
        }
        else
        {
            ResetGravityToDefault();
        }
    }

    private void ResetGravityToDefault()
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = defaultGravityScale;
    }

    private void ApplyCharacterProfile(CharacterType characterType)
    {
        activeCharacter = characterType;

        if (playerController == null)
        {
            return;
        }

        if (characterType == CharacterType.MaskDude)
        {
            playerController.ConfigureStats(maskDudeSpeed, maskDudeJumpForce, maskDudeMaxJump);
            ResetGravityToDefault();
            return;
        }

        if (characterType == CharacterType.NinjaFrog)
        {
            playerController.ConfigureStats(ninjaFrogSpeed, ninjaFrogJumpForce, ninjaFrogMaxJump);
            ResetGravityToDefault();
            return;
        }

        if (characterType == CharacterType.PinkMan)
        {
            playerController.ConfigureStats(pinkManSpeed, pinkManJumpForce, pinkManMaxJump);
            ResetGravityToDefault();
            return;
        }

        if (characterType == CharacterType.VitualGuy)
        {
            playerController.ConfigureStats(vitualGuySpeed, vitualGuyJumpForce, vitualGuyMaxJump);
            ResetGravityToDefault();
            return;
        }

        playerController.ConfigureStats(vitualGuySpeed, vitualGuyJumpForce, vitualGuyMaxJump);
        ResetGravityToDefault();
    }
}
