using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private static float current= 0.015f;
    [SerializeField] private Image mask;

    //void Update()
    //{
    //    GetCurrentFill();
    //}
    void GetCurrentFill()
    {
        float fillAmount = Math.Max(1, current);
        mask.fillAmount = fillAmount;
    }

    public static void IncreaseFill(int amount)
    {
        current += amount * 0.015f;
    }
}
