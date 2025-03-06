using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private float minimum= 0.015f;
    private int maximum=43;
    private int current;
    [SerializeField] private Image mask;

    void Update()
    {
        GetCurrentFill();
    }
    void GetCurrentFill()
    {
        float currentOffset=current-minimum;
        float maximumOffset=maximum-minimum;
        float fillAmount=(float)current/ (float)maximumOffset;
        mask.fillAmount = fillAmount;
    }
}
