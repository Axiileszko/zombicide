using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SetFPSLimit();
    }
    void SetFPSLimit()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}
