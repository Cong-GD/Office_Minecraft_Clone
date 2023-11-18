using System;
using UnityEngine;
using UnityEngine.UI;

public class FillImageProgressDisplayer : ProgressDisplayer
{
    [SerializeField] 
    private Image[] images = Array.Empty<Image>();
    public override void Disable()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(false);
        }
    }

    public override void Enable()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(true);
        }
    }

    public override void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].fillAmount = value;
        }
    }
}