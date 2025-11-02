using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private MissionManager missionManager;

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

        // Hide panel initially
        if (missionPanel != null)
        {
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

        // Update fill image color based on progress
        if (progressFillImage != null && current < total)
        {
            progressFillImage.color = inProgressColor;
        }
    }
    private void OnMissionCompleted(DefeatEnemiesMission mission)
    {
        Debug.Log("<color=green>[MissionUI] OnMissionCompleted called!</color>");

        // Update to show final progress FIRST
        UpdateProgress(mission.enemiesDefeated, mission.enemiesRequired);

        if (progressFillImage != null)
        {
            progressFillImage.color = completedColor;
        }

        if (progressText != null)
        {
            progressText.text = $"{mission.enemiesDefeated}/{mission.enemiesRequired} - COMPLETADO!";
        }

        Debug.Log("<color=green>[MissionUI] Mission panel will hide in 3 seconds</color>");

        // Hide panel after 3 seconds
        if (missionPanel != null)
        {
            Invoke(nameof(HideMissionPanel), 3f);
        }
    }

    private void HideMissionPanel()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
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
