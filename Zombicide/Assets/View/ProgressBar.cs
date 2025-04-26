using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private static float current= 0.015f;
    private static Image mask;

    private void Awake()
    {
        mask = transform.GetChild(0).GetComponent<Image>();
    }
    public static void UpdateFill(int amount)
    {
        if(amount==0)
            current = 0.015f;
        else if (amount > 42)
            current = 1f;
        else
        { 
            if(amount<=9)
                current = amount * 0.015f+0.015f;
            else
                current = (amount-9) * 0.024f + 0.150f;
        }
        float fillAmount = current;
        mask.fillAmount = fillAmount;
    }
}
