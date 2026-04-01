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

    [Header("MaskDude")]
    private float dashSpeed = 18f;
    private float dashDuration = 0.18f;
    private float dashCooldown = 5f;

    [Header("PinkMan")]
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
        if (!Input.GetKeyDown(KeyCode.E) || Time.time < nextdashTime)
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

        bool canGlide = !playerController.IsGrounded && rb.velocity.y < 0f && Input.GetKey(pinkManGlideKey) && Time.time >= nextGlideTime;
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
        if (rb == null)
        {
            return;
        }

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
}
