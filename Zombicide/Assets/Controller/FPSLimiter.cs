using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public static FPSLimiter Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetFPSLimit();
    }
    void SetFPSLimit()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}
