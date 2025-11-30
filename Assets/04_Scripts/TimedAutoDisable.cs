using UnityEngine;

/// <summary>
/// Helper component that deactivates its GameObject after a delay using Invoke.
/// This is used so that calls to StopAllCoroutines() on other components do not cancel the scheduled disable.
/// </summary>
public class TimedAutoDisable : MonoBehaviour
{
    public void ActivateForSeconds(float seconds)
    {
        try
        {
            gameObject.SetActive(true);
            if (seconds > 0f)
            {
                CancelInvoke(nameof(DisableNow));
                Invoke(nameof(DisableNow), seconds);
            }
        }
        catch { }
    }

    public void DisableNow()
    {
        try { gameObject.SetActive(false); }
        catch { }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
