using NaughtyAttributes;
using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
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

    }

    private void OnDestroy()
    {
        RenderSettings.skybox = skyBoxMaterialPrefab;
        DestroyImmediate(_skyBoxMaterial);
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