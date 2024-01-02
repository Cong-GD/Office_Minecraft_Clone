using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minecraft
{
    public class SettingUI : MonoBehaviour
    {
        [SerializeField]
        private Canvas rootCanvas;

        [SerializeField]
        private Behaviour[] enableWhenOpen;

        [SerializeField]
        private Behaviour[] disableWhenClose;

        [Header("Field of View")]
        [SerializeField]
        private TextMeshProUGUI fieldOfViewText;
        [SerializeField]
        private Slider fieldOfViewSlider;
        [SerializeField]
        private int fieldOfViewMin = 30;
        [SerializeField]
        private int fieldOfViewMax = 110;

        [Header("Render Distance")]
        [SerializeField]
        private TextMeshProUGUI renderDistanceText;
        [SerializeField]
        private Slider renderDistanceSlider;
        [SerializeField]
        private int renderDistanceMin = 2;
        [SerializeField]
        private int renderDistanceMax = 32;

        [Header("Shadow Quality")]
        [SerializeField]
        private TextMeshProUGUI shadowQualityText;

        [Header("Full Screen")]
        [SerializeField]
        private TextMeshProUGUI fullScreenText;

        [Header("Max Frame Rate")]
        [SerializeField]
        private TextMeshProUGUI maxFrameRateText;
        [SerializeField]
        private Slider maxFrameRateSlider;
        [SerializeField]
        private int maxFrameRateMin = 10;
        [SerializeField]
        private int maxFrameRateMax = 260;

        [Header("Mouse Sensitivity")]
        [SerializeField]
        private TextMeshProUGUI mouseSensitivityText;
        [SerializeField]
        private Slider mouseSensitivitySlider;
        [SerializeField]
        private int mouseSensitivityRange = 200;

        private void Start()
        {
            UpdateAllUI();
        }

        public void Open()
        {
            rootCanvas.enabled = true;
            foreach (Behaviour behaviour in enableWhenOpen)
            {
                behaviour.enabled = true;
            }
            UpdateAllUI();
        }

        public void Close()
        {
            rootCanvas.enabled = false;
            foreach (Behaviour behaviour in disableWhenClose)
            {
                behaviour.enabled = false;
            }
            GameSettings.Instance.Apply();
        }

        public void UpdateAllUI()
        {
            UpdateFullscreenToggleUI();
            UpdateShadowSettingUI();
            UpdateViewOfViewUI(updateSlider: true);
            UpdateRenderDistanceUI(updateSlider: true);
            //UpdateMouseSensityUI(updateSlider: true);
            UpdateMaxFrameRateUI(updateSlider: true);
        }

        public void OnFieldOfViewSliderChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value * (fieldOfViewMax - fieldOfViewMin) + fieldOfViewMin);
            GameSettings.Instance.FieldOfView = intValue;
            UpdateViewOfViewUI();
        }

        public void OnRenderDistanceSliderChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value * (renderDistanceMax - renderDistanceMin) + renderDistanceMin);
            GameSettings.Instance.RenderDistance = intValue;
            UpdateRenderDistanceUI();
        }

        public void OnShadowButtonClicked()
        {
            ShadowQuality shadowQuality = (ShadowQuality)(((int)GameSettings.Instance.ShadowQuality + 1) % 3);
            GameSettings.Instance.ShadowQuality = shadowQuality;
            UpdateShadowSettingUI();
        }

        public void OnFullScreenButtonClick()
        {
            GameSettings.Instance.IsFullScreen = !GameSettings.Instance.IsFullScreen;
            UpdateFullscreenToggleUI();
        }

        public void OnMouseSensitivitySliderChanged(float value)
        {
            GameSettings.Instance.MouseSensitivity = Mathf.RoundToInt(value * mouseSensitivityRange);
            UpdateMouseSensityUI();
        }

        public void OnMaxFrameRateSliderChanged(float value)
        {
            int maxFrameRate = Mathf.RoundToInt(value * (maxFrameRateMax - maxFrameRateMin) + maxFrameRateMin);
            GameSettings.Instance.MaxFrameRate = maxFrameRate;
            UpdateMaxFrameRateUI();
        }

        private void UpdateViewOfViewUI(bool updateSlider = false)
        {
            string valueText;
            if (GameSettings.Instance.FieldOfView == 70)
            {
                valueText = "Normal";
            }
            else if (GameSettings.Instance.FieldOfView == fieldOfViewMax)
            {
                valueText = "Quake Pro";
            }
            else
            {
                valueText = GameSettings.Instance.FieldOfView.ToString();
            }
            fieldOfViewText.text = $"FOV: {valueText}";

            if (updateSlider)
            {
                fieldOfViewSlider.value = (float)(GameSettings.Instance.FieldOfView - fieldOfViewMin) / (fieldOfViewMax - fieldOfViewMin);
            }
        }

        private void UpdateRenderDistanceUI(bool updateSlider = false)
        {
            renderDistanceText.text = $"Render Distance: {GameSettings.Instance.RenderDistance} chunks";

            if (updateSlider)
            {
                renderDistanceSlider.value = (float)(GameSettings.Instance.RenderDistance - renderDistanceMin) / (renderDistanceMax - renderDistanceMin);
            }
        }

        private void UpdateShadowSettingUI()
        {
            shadowQualityText.text = $"Shadow Quality: {GameSettings.Instance.ShadowQuality}";
        }

        private void UpdateFullscreenToggleUI()
        {
            fullScreenText.text = $"Full Screen: {(GameSettings.Instance.IsFullScreen ? "On" : "Off")}";
        }

        private void UpdateMouseSensityUI(bool updateSlider = false)
        {
            int mouseSensitivity = GameSettings.Instance.MouseSensitivity;
            string valueText;
            if (mouseSensitivity == 0)
            {
                valueText = "*yawn*";
            }
            else if (mouseSensitivity == mouseSensitivityRange)
            {
                valueText = "HYPERSPEED!!";
            }
            else
            {
                valueText = mouseSensitivity.ToString() + "%";
            }
            mouseSensitivityText.text = $"Mouse Sensitivity: {valueText}";

            if (updateSlider)
            {
                mouseSensitivitySlider.value = (float)mouseSensitivity / (mouseSensitivityRange * 100);
            }
        }

        private void UpdateMaxFrameRateUI(bool updateSlider = false)
        {
            int maxFrameRate = GameSettings.Instance.MaxFrameRate;
            if (maxFrameRate == maxFrameRateMax)
            {
                maxFrameRate = int.MaxValue;
                maxFrameRateText.text = $"Max Frame Rate: Unlimited";
            }
            else
            {
                maxFrameRateText.text = $"Max Frame Rate: {maxFrameRate} fps";
            }

            if (updateSlider)
            {
                maxFrameRateSlider.value = (float)(maxFrameRate - maxFrameRateMin) / (maxFrameRateMax - maxFrameRateMin);
            }
        }
    }
}
