using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake instance;
    private CinemachineVirtualCamera camera;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private float shakeTimer;
    private float shakeIntensity;
    private float shakeTimerTime;


    private void Awake()
    {
        instance = this;
        camera = GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannelPerlin = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float streangth = 1f, float frequency = 2f, float duration = 0.5f)
    {
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = streangth;
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = frequency;
        shakeTimer = shakeTimerTime = duration;
        shakeIntensity = streangth;
    }

    private void Update()
    {
        HandleShake();
    }

    private void HandleShake()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(shakeIntensity, 0f, 1f - (shakeTimer / shakeTimerTime));
        }
    }
}
