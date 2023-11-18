using UnityEngine;

public class RectranformCopier : MonoBehaviour
{
    [SerializeField]
    private RectTransform source;

    [SerializeField]
    private RectTransform target;


    private void OnEnable()
    {
        if (source == null || target == null)
            return;

        TransformHelper.CopyRectTransform(source, target);
        TransformHelper.CopyRectTransform(source, target);
    }
}