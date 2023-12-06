using UnityEngine;

public static class TransformHelper
{
    public static void CopyRectTransform(RectTransform source, RectTransform destination)
    {
        destination.anchoredPosition = source.anchoredPosition;
        destination.sizeDelta = source.sizeDelta;
        destination.anchorMin = source.anchorMin;
        destination.anchorMax = source.anchorMax;
        destination.pivot = source.pivot;
        destination.rotation = source.rotation;
        destination.localScale = source.localScale;
    }
}
