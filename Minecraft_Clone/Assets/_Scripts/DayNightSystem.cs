using CongTDev.Collection;
using Minecraft;
using Minecraft.Input;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class DayNightSystem : MonoBehaviour
{
    public const string FILE_NAME = "DayNightSystem.dat";

    [SerializeField]
    private float timeMultilier;

    [SerializeField]
    [Range(0.0f, 23.99f)]
    private float startTime = 7f;

    [SerializeField]
    private Light dayLight;

    [SerializeField]
    private Material skyBoxMaterialPrefab;

    [SerializeField]
    private AnimationCurve skyBlendCurve;

    [SerializeField]
    private Gradient horizontalColorGradient;

    [SerializeField]
    private AnimationCurve horizontalBlendCurve;


    public TimeSpan CurrentTimeSpan => _currentTime;

    public float TotalDay => (float)_currentTime.TotalDays;

    public float TotalHour => (float)_currentTime.TotalHours;

    public float TotalMinutes => (float)_currentTime.TotalMinutes;

    public float CurrentHourInDay => math.fmod(TotalHour, 24f);

    public float CurrentMinuteInHour => math.fmod(TotalMinutes, 60f);


    private TimeSpan _currentTime;
    private readonly int _dayNightTimeID = Shader.PropertyToID("_Day_Night_Time");
    private readonly int _lightDirectionID = Shader.PropertyToID("_Light_Direction");
    private readonly int _horizonBlend = Shader.PropertyToID("_Horizon_Blend");
    private readonly int _horizonColor = Shader.PropertyToID("_Horizon_Color");
    private Vector3 _lightEulerAngle;
    private Material _skyBoxMaterial;
    private float _currentDayValue;

    private void Awake()
    {
        _currentTime = TimeSpan.FromHours(startTime);
        _lightEulerAngle = dayLight.transform.eulerAngles;

        _skyBoxMaterial = new Material(skyBoxMaterialPrefab);
        RenderSettings.skybox = _skyBoxMaterial;
        GameManager.Instance.OnGameSave += SaveTime;
        GameManager.Instance.OnGameLoad += LoadTime;
        MInput.InputActions.General.AddAnHour.performed += AddHourHandle;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameSave -= SaveTime;
        GameManager.Instance.OnGameLoad -= LoadTime;
        RenderSettings.skybox = skyBoxMaterialPrefab;
        DestroyImmediate(_skyBoxMaterial);
        MInput.InputActions.General.AddAnHour.performed -= AddHourHandle;
    }

    private void FixedUpdate()
    {
        AddSencondToCurrent(Time.fixedDeltaTime * timeMultilier);
        UpdateDayValue();
        UpdateLightDirection();
        BlendSkyBox();
        UpdateSunDirection();
        DuskAndDawnEffect();
    }

    private void SaveTime(Dictionary<string, ByteString> dictionary)
    {
        ByteString byteString = ByteString.Create();
        byteString.WriteValue(_currentTime);
        dictionary[FILE_NAME] = byteString;
    }

    private void LoadTime(Dictionary<string, ByteString> dictionary)
    {
        try
        {
            if (dictionary.Remove(FILE_NAME, out ByteString byteString))
            {
                ByteString.BytesReader byteReader = byteString.GetBytesReader();
                _currentTime = byteReader.ReadValue<TimeSpan>();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    private void AddHourHandle(InputAction.CallbackContext context)
    {
        AddHours(1);
    }

    public void AddHours(int hours)
    {
        AddSencondToCurrent(hours * 3600f);
    }

    private void AddSencondToCurrent(float seconds)
    {
        _currentTime = _currentTime.Add(TimeSpan.FromSeconds(seconds));
    }

    private void UpdateDayValue()
    {
        _currentDayValue = CurrentHourInDay / 24f;
    }

    private void UpdateLightDirection()
    {
        _lightEulerAngle.x = math.lerp(-90f, 270f, _currentDayValue);
        dayLight.transform.eulerAngles = _lightEulerAngle;
    }

    private void BlendSkyBox()
    {
        var blendValue = skyBlendCurve.Evaluate(_currentDayValue);
        _skyBoxMaterial.SetFloat(_dayNightTimeID, blendValue);
    }

    private void UpdateSunDirection()
    {
        _skyBoxMaterial.SetVector(_lightDirectionID, dayLight.transform.forward);
    }

    private void DuskAndDawnEffect()
    {
        var horizontalColor = horizontalColorGradient.Evaluate(_currentDayValue);
        var horizontalBlend = horizontalBlendCurve.Evaluate(_currentDayValue);
        _skyBoxMaterial.SetColor(_horizonColor, horizontalColor);
        _skyBoxMaterial.SetFloat(_horizonBlend, horizontalBlend);
    }

}