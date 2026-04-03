using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    private const string SelectedCharacterKey = "SelectedCharacterIndex";
    private const int DefaultMaxJump = 2;
    private const int NinjaFrogMaxJump = 3;

    private enum CharacterType
    {
        MaskDude = 0,
        NinjaFrog = 1,
        PinkMan = 2,
        VitualGuy = 3
    }

    [Header("Character")]
    [SerializeField] private CharacterType activeCharacter = CharacterType.MaskDude;

    private float dashSpeed = 18f;
    private float dashDuration = 0.18f;
    private float dashCooldown = 5f;

    private float pinkManGlideGravityScale = 0.35f;
    private KeyCode pinkManGlideKey = KeyCode.LeftShift;
    private float pinkCooldown = 10f;
    private float nextGlideTime;
    private bool isGlidingLastFrame;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private float defaultGravityScale;
    private float nextdashTime;
    private bool guyIsImmortal;
    private bool mobileDashQueued;
    private bool mobileGlidePressed;
    private float mobileGlideTapUntil;
    private float mobileGlideTapDuration = 0.3f;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            defaultGravityScale = rb.gravityScale;
        }

        int savedCharacter = PlayerPrefs.GetInt(SelectedCharacterKey, (int)activeCharacter);
        if (savedCharacter < (int)CharacterType.MaskDude || savedCharacter > (int)CharacterType.VitualGuy)
        {
            savedCharacter = (int)activeCharacter;
        }

        activeCharacter = (CharacterType)savedCharacter;
        ApplyCharacterProfile(activeCharacter);
    }

    void Update()
    {
        HandleCharacterSkill();
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
    }
    private void HandleMaskDudeSkill()
    {
        bool dashRequested = Input.GetKeyDown(KeyCode.E) || mobileDashQueued;
        mobileDashQueued = false;

        if (!dashRequested || Time.time < nextdashTime)
        {
            return;
        }

        float dashVelocity = playerController.FacingDirection * dashSpeed;
        playerController.BeginDash(dashVelocity, dashDuration);
        nextdashTime = Time.time + dashCooldown;
    }

    private void HandlePinkManSkill()
    {
        if (rb == null)
        {
            return;
        }

        bool glideRequested = Input.GetKey(pinkManGlideKey) || mobileGlidePressed || Time.time < mobileGlideTapUntil;
        bool canGlide = !playerController.IsGrounded && rb.velocity.y < 0f && glideRequested && Time.time >= nextGlideTime;
        rb.gravityScale = canGlide ? pinkManGlideGravityScale : defaultGravityScale;

        if (isGlidingLastFrame && !canGlide)
        {
            nextGlideTime = Time.time + pinkCooldown;
        }

        isGlidingLastFrame = canGlide;
    }

    private void HandleVitualGuySkill()
    {
    }

    public bool GuyImmortal()
    {
        if (activeCharacter != CharacterType.VitualGuy || !guyIsImmortal)
        {
            return false;
        }
        Debug.Log("Immortal");
        guyIsImmortal = false;
        return true;
    }

    private void ResetGravity()
    {
        rb.gravityScale = defaultGravityScale;
    }

    private void ApplyCharacterProfile(CharacterType characterType)
    {
        activeCharacter = characterType;
        PlayerPrefs.SetInt(SelectedCharacterKey, (int)characterType);
        PlayerPrefs.Save();

        if (playerController == null)
        {
            return;
        }
        ResetGravity();

        if (characterType == CharacterType.MaskDude)
        {
            guyIsImmortal = false;
            playerController.ConfigureStats(playerController.speed, playerController.jumpForce, DefaultMaxJump);
            return;
        }

        if (characterType == CharacterType.NinjaFrog)
        {
            guyIsImmortal = false;
            playerController.ConfigureStats(playerController.speed, playerController.jumpForce, NinjaFrogMaxJump);
            return;
        }

        if (characterType == CharacterType.PinkMan)
        {
            guyIsImmortal = false;
            playerController.ConfigureStats(playerController.speed, playerController.jumpForce, DefaultMaxJump);
            return;
        }

        if (characterType == CharacterType.VitualGuy)
        {
            guyIsImmortal = true;
            playerController.ConfigureStats(playerController.speed, playerController.jumpForce, DefaultMaxJump);
            return;
        }

        guyIsImmortal = false;
        playerController.ConfigureStats(playerController.speed, playerController.jumpForce, DefaultMaxJump);
    }

    public bool HasActiveSkillButton()
    {
        return activeCharacter == CharacterType.MaskDude || activeCharacter == CharacterType.PinkMan;
    }

    public bool SkillUsesHoldInput()
    {
        return activeCharacter == CharacterType.PinkMan;
    }

    public void MobileSkillDown()
    {
        if (activeCharacter == CharacterType.MaskDude)
        {
            mobileDashQueued = true;
            return;
        }

        if (activeCharacter == CharacterType.PinkMan)
        {
            mobileGlidePressed = true;
            mobileGlideTapUntil = Time.time + mobileGlideTapDuration;
        }
    }

    public void MobileSkillUp()
    {
        mobileGlidePressed = false;
    }

    public void MobileSkillTap()
    {
        if (activeCharacter == CharacterType.MaskDude)
        {
            mobileDashQueued = true;
            return;
        }

        if (activeCharacter == CharacterType.PinkMan)
        {
            mobileGlideTapUntil = Time.time + mobileGlideTapDuration;
        }
    }

    public float GetSkillCooldownNormalized()
    {
        if (activeCharacter == CharacterType.MaskDude)
        {
            if (dashCooldown <= 0f)
            {
                return 0f;
            }

            float remaining = Mathf.Max(0f, nextdashTime - Time.time);
            return Mathf.Clamp01(remaining / dashCooldown);
        }

        if (activeCharacter == CharacterType.PinkMan)
        {
            if (pinkCooldown <= 0f)
            {
                return 0f;
            }

            float remaining = Mathf.Max(0f, nextGlideTime - Time.time);
            return Mathf.Clamp01(remaining / pinkCooldown);
        }

        return 0f;
    }
}
