using NaughtyAttributes;
using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    [SerializeField]
    private Camera renderCamera;

    [SerializeField]
    private RenderTexture renderTexture;

    [Button("Capture", EButtonEnableMode.Playmode)]

    public Texture2D Capture()
    {
        RenderTexture current = RenderTexture.active;
        renderCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        renderCamera.Render();
        Texture2D renderedTexture = new Texture2D(renderTexture.width, renderTexture.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = current;
        renderCamera.targetTexture = null;
        return renderedTexture;
    }
}