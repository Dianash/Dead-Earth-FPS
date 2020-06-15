using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    // Inspector Assigned UI References
    [SerializeField] private GameObject crosshair = null;
    [SerializeField] private Text healthText = null;
    [SerializeField] private Text staminaText = null;
    [SerializeField] private Text interactionText = null;
    [SerializeField] private Image screenFade = null;
    [SerializeField] private Text aim = null;
    [SerializeField] private Text missionText = null;
    [SerializeField] private float missionTextDisplayTime = 3.0f;

    private float currentFadeLevel = 1.0f;
    private IEnumerator coroutine = null;

    public void Start()
    {
        if (screenFade)
        {
            Color color = screenFade.color;
            color.a = currentFadeLevel;
            screenFade.color = color;
        }

        if (missionText)
        {
            Invoke("HideMissionText", missionTextDisplayTime);
        }
    }

    /// <summary>
    /// Refreshes the values of UI elements
    /// </summary>
    public void Invalidate(CharacterManager charManager)
    {
        if (charManager == null) return;
        if (healthText)
            healthText.text = "Health " + ((int)charManager.Health).ToString();

        if (staminaText)
            staminaText.text = "Stamina " + ((int)charManager.Stamina).ToString();
    }

    public void SetInteractionText(string text)
    {
        if (interactionText)
        {
            if (text == null)
            {
                interactionText.text = null;
                interactionText.gameObject.SetActive(false);
            }
            else
            {
                interactionText.text = text;
                interactionText.gameObject.SetActive(true);
            }
        }
    }

    public void ShowMissionText(string text)
    {
        if (missionText)
        {
            missionText.text = text;
            missionText.gameObject.SetActive(true);
        }
    }

    public void HideMissionText()
    {
        if (missionText)
        {
            missionText.gameObject.SetActive(false);
        }
    }

    public void Fade(float seconds, ScreenFadeType direction)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        float targetFade = 0.0f;

        switch (direction)
        {
            case ScreenFadeType.FadeIn:
                targetFade = 0.0f;
                break;

            case ScreenFadeType.FadeOut:
                targetFade = 1.0f;
                break;
        }

        coroutine = FadeInternal(seconds, targetFade);
        StartCoroutine(coroutine);
    }

    private IEnumerator FadeInternal(float seconds, float targetFade)
    {
        if (!screenFade) yield break;

        float timer = 0;
        float srcFade = currentFadeLevel;

        Color oldColor = screenFade.color;

        if (seconds < 0.1f)
            seconds = 0.1f;

        while (timer < seconds)
        {
            timer += Time.deltaTime;
            currentFadeLevel = Mathf.Lerp(srcFade, targetFade, timer / seconds);
            oldColor.a = currentFadeLevel;
            screenFade.color = oldColor;
            yield return null;
        }

        oldColor.a = currentFadeLevel = targetFade;
        screenFade.color = oldColor;
    }

    public void SetAim(bool setActive, AimType aimType = AimType.Use)
    {
        aim.gameObject.SetActive(setActive);

        if (setActive)
        {
            if (aimType == AimType.Use)
            {
                aim.color = new Color(200, 0, 0);
            }
            else if (aimType == AimType.Damage)
            {
                aim.color = new Color(0, 200, 0);
            }
        }
    }
}
