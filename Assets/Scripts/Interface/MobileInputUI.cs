using System;
using UnityEngine;
using UnityEngine.UI;

public class MobileInputUI : MonoBehaviour
{
    private PlayerController cachedPlayer;
    private PlayerSkill cachedSkill;
    [Header("UI")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private CanvasGroup skillCanvasGroup;
    [SerializeField] private Image coolDownSkillFill;

    private bool controlsVisible = true;
    private bool skillVisible = true;

    private void Awake()
    {
        if (controlsCanvasGroup == null)
        {
            controlsCanvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        RefreshPlayerReferences();

        bool hasPlayer = GetPlayer() != null;
        bool shouldShow = Time.timeScale > 0f && hasPlayer;

        if (shouldShow == controlsVisible)
        {
            UpdateSkillVisibility(shouldShow);
            CoolDownSkill();
            return;
        }

        SetControlsVisible(shouldShow);
        UpdateSkillVisibility(shouldShow);
        CoolDownSkill();
    }

    private void LateUpdate()
    {
        CoolDownSkill();
    }

    private void RefreshPlayerReferences()
    {
        PlayerController instancePlayer = PlayerController.Instance;

        if (instancePlayer != null && cachedPlayer != instancePlayer)
        {
            cachedPlayer = instancePlayer;
            cachedSkill = cachedPlayer.GetComponent<PlayerSkill>();
            return;
        }

        if (cachedPlayer == null)
        {
            cachedPlayer = FindObjectOfType<PlayerController>();
            cachedSkill = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerSkill>() : null;
            return;
        }

        if (cachedSkill == null)
        {
            cachedSkill = cachedPlayer.GetComponent<PlayerSkill>();
        }
    }

    private void CoolDownSkill()
    {
        if (coolDownSkillFill == null)
        {
            return;
        }

        PlayerSkill skill = GetSkill();
        if (skill == null || !skill.HasActiveSkillButton())
        {
            coolDownSkillFill.fillAmount = 0f;
            return;
        }

        coolDownSkillFill.fillAmount = skill.GetSkillCooldownNormalized();
    }

    private PlayerSkill GetSkill()
    {
        if (cachedSkill != null)
        {
            return cachedSkill;
        }

        PlayerController player = GetPlayer();
        if (player == null)
        {
            return null;
        }

        cachedSkill = player.GetComponent<PlayerSkill>();
        return cachedSkill;
    }

    private void SetSkillVisible(bool visible)
    {
        skillVisible = visible;

        if (!visible)
        {
            PlayerSkill skill = GetSkill();
            if (skill != null)
            {
                skill.MobileSkillUp();
            }
        }

        if (skillCanvasGroup == null)
        {
            return;
        }

        skillCanvasGroup.alpha = visible ? 1f : 0f;
        skillCanvasGroup.interactable = visible;
        skillCanvasGroup.blocksRaycasts = visible;
    }

    private void UpdateSkillVisibility(bool controlsAreVisible)
    {
        PlayerSkill skill = GetSkill();
        bool shouldShowSkill = controlsAreVisible && skill != null && skill.HasActiveSkillButton();

        if (shouldShowSkill == skillVisible)
        {
            return;
        }

        SetSkillVisible(shouldShowSkill);
    }

    private void SetControlsVisible(bool visible)
    {
        controlsVisible = visible;

        if (!visible && cachedPlayer != null)
        {
            cachedPlayer.MobileMoveRelease();
        }

        if (controlsCanvasGroup == null)
        {
            return;
        }

        controlsCanvasGroup.alpha = visible ? 1f : 0f;
        controlsCanvasGroup.interactable = visible;
        controlsCanvasGroup.blocksRaycasts = visible;
    }

    private void OnDisable()
    {
        if (cachedPlayer != null)
        {
            cachedPlayer.MobileMoveRelease();
        }

        if (cachedSkill != null)
        {
            cachedSkill.MobileSkillUp();
        }
    }

    private PlayerController GetPlayer()
    {
        RefreshPlayerReferences();

        if (cachedPlayer != null)
        {
            return cachedPlayer;
        }

        return cachedPlayer;
    }

    private void InvokeOnPlayer(Action<PlayerController> action)
    {
        PlayerController player = GetPlayer();
        if (player == null)
        {
            return;
        }

        action(player);
    }

    public void OnLeftDown()
    {
        InvokeOnPlayer(player => player.MobileMoveLeftDown());
    }

    public void OnRightDown()
    {
        InvokeOnPlayer(player => player.MobileMoveRightDown());
    }

    public void OnMoveRelease()
    {
        InvokeOnPlayer(player => player.MobileMoveRelease());
    }

    public void OnJump()
    {
        InvokeOnPlayer(player => player.MobileJump());
    }

    public void OnSkillDown()
    {
        PlayerSkill skill = GetSkill();
        if (skill == null)
        {
            return;
        }

        skill.MobileSkillDown();

        if (!skill.SkillUsesHoldInput())
        {
            skill.MobileSkillUp();
        }
    }

    public void OnSkillClick()
    {
        PlayerSkill skill = GetSkill();
        if (skill == null)
        {
            return;
        }

        skill.MobileSkillTap();
    }

    public void OnSkillUp()
    {
        PlayerSkill skill = GetSkill();
        if (skill == null)
        {
            return;
        }

        skill.MobileSkillUp();
    }
}
