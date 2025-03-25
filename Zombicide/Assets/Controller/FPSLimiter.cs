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
        QualitySettings.vSyncCount = 0;  // VSync kikapcsolása, mert ütközhet az FPS limittel
        Application.targetFrameRate = 60;
    }
}
