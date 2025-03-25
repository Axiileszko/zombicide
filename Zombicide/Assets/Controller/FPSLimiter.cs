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
        QualitySettings.vSyncCount = 0;  // VSync kikapcsol�sa, mert �tk�zhet az FPS limittel
        Application.targetFrameRate = 60;
    }
}
