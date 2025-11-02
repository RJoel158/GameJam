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
        missionManager = MissionManager.Instance;

        if (missionManager == null)
        {
            Debug.LogError("MissionManager not found in scene!");
            return;
        }

        // Subscribe to mission events
        missionManager.OnMissionStarted.AddListener(OnMissionStarted);
        missionManager.OnMissionCompleted.AddListener(OnMissionCompleted);
        missionManager.OnMissionProgressChanged.AddListener(UpdateProgress);

        // Hide panel initially
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
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
        }
        else
        {
            Debug.LogError("<color=red>[MissionUI] progressSlider is NULL!</color>");
        }
    }

    private void OnMissionCompleted(DefeatEnemiesMission mission)
    {
        if (progressFillImage != null)
        {
            progressFillImage.color = completedColor;
        }

        if (progressText != null)
        {
            progressText.text = "COMPLETED!";
        }

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
