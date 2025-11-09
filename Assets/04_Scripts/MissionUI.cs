using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MissionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject missionPanel;
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI missionDescriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressSlider;
    public Image progressFillImage;

    [Header("Colors")]
    public Color inProgressColor = Color.yellow;
    public Color completedColor = Color.green;

    [Header("Animation Settings")]
    public float completionScalePulse = 1.3f;
    public float completionAnimationDuration = 0.5f;
    public float displayCompletedTime = 3f;
    public AnimationCurve completionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private MissionManager missionManager;
    private Vector3 originalPanelScale;

    private void Start()
    {
        Debug.Log("<color=orange>[MissionUI] Start() called - Initializing...</color>");

        missionManager = MissionManager.Instance;

        if (missionManager == null)
        {
            Debug.LogError("<color=red>[MissionUI] MissionManager not found in scene!</color>");
            return;
        }

        Debug.Log("<color=orange>[MissionUI] Found MissionManager, subscribing to events...</color>");

        // Subscribe to mission events
        missionManager.OnMissionStarted.AddListener(OnMissionStarted);
        missionManager.OnMissionCompleted.AddListener(OnMissionCompleted);
        missionManager.OnMissionProgressChanged.AddListener(UpdateProgress);

        Debug.Log("<color=orange>[MissionUI] Subscribed to all events</color>");

        // Store original scale for animations
        if (missionPanel != null)
        {
            originalPanelScale = missionPanel.transform.localScale;
            missionPanel.SetActive(false);
            Debug.Log("<color=orange>[MissionUI] Panel hidden initially</color>");
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] missionPanel is NULL!</color>");
        }

        // If there's already an active mission, show it
        if (missionManager.HasActiveMission())
        {
            Debug.Log("<color=orange>[MissionUI] Active mission detected, showing it now</color>");
            ShowCurrentMission();
        }
    }
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (missionManager != null)
        {
            missionManager.OnMissionStarted.RemoveListener(OnMissionStarted);
            missionManager.OnMissionCompleted.RemoveListener(OnMissionCompleted);
            missionManager.OnMissionProgressChanged.RemoveListener(UpdateProgress);
        }
    }

    private void OnMissionStarted(DefeatEnemiesMission mission)
    {
        Debug.Log($"<color=orange>[MissionUI] OnMissionStarted called for: {mission.missionName}</color>");

        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
            Debug.Log("<color=orange>[MissionUI] Panel activated</color>");
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] missionPanel is NULL!</color>");
        }

        if (missionNameText != null)
        {
            missionNameText.text = mission.missionName;
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] missionNameText is NULL!</color>");
        }

        if (missionDescriptionText != null)
        {
            missionDescriptionText.text = mission.description;
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] missionDescriptionText is NULL!</color>");
        }

        if (progressFillImage != null)
        {
            progressFillImage.color = inProgressColor;
        }

        UpdateProgress(0, mission.enemiesRequired);
    }

    private void UpdateProgress(int current, int total)
    {
        Debug.Log($"<color=cyan>[MissionUI] UpdateProgress called: {current}/{total}</color>");

        if (progressText != null)
        {
            progressText.text = $"{current} / {total}";
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] progressText is NULL!</color>");
        }

        if (progressSlider != null)
        {
            progressSlider.maxValue = total;
            progressSlider.value = current;

            Debug.Log($"<color=cyan>[MissionUI] Slider updated: value={current}, max={total}</color>");
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] progressSlider is NULL!</color>");
        }

        // Update fill image color based on progress (only if not completed yet)
        if (progressFillImage != null)
        {
            // Keep the current color if already set to completed color
            if (progressFillImage.color != completedColor)
            {
                progressFillImage.color = inProgressColor;
            }
        }
    }

    private void OnMissionCompleted(DefeatEnemiesMission mission)
    {
        Debug.Log("<color=green>[MissionUI] OnMissionCompleted called! Starting completion animation...</color>");

        // Start completion animation coroutine
        StartCoroutine(PlayCompletionAnimation(mission));
    }
    private IEnumerator PlayCompletionAnimation(DefeatEnemiesMission mission)
    {
        Debug.Log("<color=green>[MissionUI] PlayCompletionAnimation started!</color>");

        // Update to show final progress FIRST
        UpdateProgress(mission.enemiesDefeated, mission.enemiesRequired);

        if (progressFillImage != null)
        {
            progressFillImage.color = completedColor;
            Debug.Log($"<color=green>[MissionUI] Progress bar color changed to: {completedColor}</color>");
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] progressFillImage is NULL! Can't change color!</color>");
        }

        if (progressText != null)
        {
            progressText.text = $"¡MISIÓN COMPLETADA!";
            Debug.Log("<color=green>[MissionUI] Progress text updated to: ¡MISIÓN COMPLETADA!</color>");
        }

        // Change mission name to celebration text
        if (missionNameText != null)
        {
            missionNameText.text = "¡ÉXITO!";
            missionNameText.color = completedColor;
            Debug.Log("<color=green>[MissionUI] Mission name changed to celebration text</color>");
        }

        // Scale pulse animation
        float elapsedTime = 0f;
        while (elapsedTime < completionAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / completionAnimationDuration;
            float curveValue = completionCurve.Evaluate(normalizedTime);

            // Pulse scale
            float scale = Mathf.Lerp(1f, completionScalePulse, curveValue);
            if (missionPanel != null)
            {
                missionPanel.transform.localScale = originalPanelScale * scale;
            }

            yield return null;
        }

        // Return to normal scale
        if (missionPanel != null)
        {
            missionPanel.transform.localScale = originalPanelScale;
        }

        Debug.Log($"<color=green>[MissionUI] Completion animation done. Waiting {displayCompletedTime}s before hiding...</color>");

        // Wait before hiding
        yield return new WaitForSeconds(displayCompletedTime);

        // Fade out animation
        yield return StartCoroutine(FadeOutPanel());

        // Hide panel
        HideMissionPanel();

        // Check if there's a next mission in queue
        if (missionManager != null)
        {
            missionManager.CheckAndStartNextMission();
        }
    }

    private IEnumerator FadeOutPanel()
    {
        if (missionPanel == null) yield break;

        CanvasGroup canvasGroup = missionPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = missionPanel.AddComponent<CanvasGroup>();
        }

        float fadeTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
    private void HideMissionPanel()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);

            // Reset alpha for next mission
            CanvasGroup canvasGroup = missionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            // Reset scale
            missionPanel.transform.localScale = originalPanelScale;

            Debug.Log("<color=orange>[MissionUI] Panel hidden</color>");
        }
    }

    // Public method to manually show mission info (useful for debugging)
    public void ShowCurrentMission()
    {
        if (missionManager != null && missionManager.HasActiveMission())
        {
            var mission = missionManager.currentMission;
            OnMissionStarted(mission);
            UpdateProgress(mission.enemiesDefeated, mission.enemiesRequired);
        }
    }
}
