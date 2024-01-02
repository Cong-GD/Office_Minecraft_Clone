using Cysharp.Threading.Tasks;
using Minecraft;
using NaughtyAttributes;
using System.IO;
using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    [SerializeField]
    new private Camera camera;

    [SerializeField]
    private RenderTexture renderTexture;

    [Button("Capture", EButtonEnableMode.Playmode)]

    public Texture2D Capture()
    {
        RenderTexture current = RenderTexture.active;
        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        camera.Render();
        Texture2D renderedTexture = new Texture2D(renderTexture.width, renderTexture.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = current;
        camera.targetTexture = null;
        return renderedTexture;
    }
}