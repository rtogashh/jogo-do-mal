using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
public class DayAndNightManager : MonoBehaviour
{
    [Header("Time")]
    [Range(0f, 24f)] public float currentTime;
    public float timeSpeed = 1f;
    public string currentTimeString;

    [Header("Sun Properties")]
    public Light sun;
    public float sunPosition = 1f;
    public float sunIntensity = 1f;
    public AnimationCurve sunIntensityMultiplier;
    //roughly around 0.5f there is noon(12am), 0.25f there is dawn, 0.75f there is dusk
    public AnimationCurve sunlightTemperatureMultiplier;
    public bool isDay = true;
    public bool sunActive = true;
    public bool moonActive = true;

    [Header("Moon Properties")]
    public Light moon;
    public float moonIntensity = 1f;
    public AnimationCurve moonIntensityMultiplier;
    public AnimationCurve moonlightTemperatureMultiplier;

    void Start()
    {
        UpdateTimeText();
        CheckDayShadows();
    }
    void Update()
    {
        currentTime += Time.deltaTime * timeSpeed;
        if (currentTime >= 24f)
        {
            currentTime = 0f;
        }
        UpdateTimeText();
        UpdateLight();
        CheckDayShadows();
    }
    void UpdateTimeText()
    {
        currentTimeString = Mathf.Floor(currentTime).ToString("00") + ":" + ((currentTime % 1) * 60f).ToString("00");
    }
    void UpdateLight()
    {
        if (sun == null)
        {
            return;
        }
        float normalizedTime = currentTime / 24f;

        float sunRotation = currentTime / 24f * 360f;
        float sunIntensityCurve = sunIntensityMultiplier.Evaluate(normalizedTime);
        float sunTemperatureMultiplier = sunlightTemperatureMultiplier.Evaluate(normalizedTime);

        //TO DO: alterar a rotação da lua para ficar mais condizente com a rotação correta
        sun.transform.rotation = Quaternion.Euler(sunRotation - 90f, sunPosition, 0f);

        sun.intensity = sunIntensityCurve * sunIntensity;
        sun.colorTemperature = sunTemperatureMultiplier * 10000f;//temperatura é de corpo celeste e medida em kelvin
                                                                 //multiplicador acima de 1 a luz esfria
                                                                 //multiplicador abaixo de 1 a luz aquece
        if (moon == null)
        {
            return;
        }
        float moonIntensityCurve = moonIntensityMultiplier.Evaluate(normalizedTime);
        float moonTemperatureMultiplier = moonlightTemperatureMultiplier.Evaluate(normalizedTime);

        moon.transform.rotation = Quaternion.Euler(sunRotation + 90f, sunPosition, 0f);

        moon.intensity = moonIntensityCurve * moonIntensity;
        moon.colorTemperature = moonTemperatureMultiplier * 10000f;
    }
    void CheckDayShadows()
    {
        float currentSunRotation = currentTime;
        if (currentSunRotation >= 6f && currentSunRotation <= 18f)
        {
            sun.shadows = LightShadows.Soft;
            moon.shadows = LightShadows.None;
            isDay = true;
        }
        else
        {
            sun.shadows = LightShadows.None;
            moon.shadows = LightShadows.Soft;
            isDay = false;
        }

        if (currentSunRotation >= 5.5f && currentSunRotation <= 18.5f)
        {
            sun.gameObject.SetActive(true);
            sunActive = true;
        }
        else
        {
            sun.gameObject.SetActive(false);
            sunActive = false;
        }
        if (currentSunRotation >= 6f && currentSunRotation <= 18f)
        {
            moon.gameObject.SetActive(false);
            moonActive = false;
        }
        else
        {
            moon.gameObject.SetActive(true);
            moonActive = true;
        }
    }

    private void OnValidate()
    {
        UpdateLight();
        CheckDayShadows();
        UpdateTimeText();
    }
}