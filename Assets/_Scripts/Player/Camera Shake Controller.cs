using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShakeController : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCam;
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Awake()
    {
        virtualCam = GetComponent<CinemachineVirtualCamera>();
        perlinNoise = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        ResetIntensity();
    }

    public void ShakeCamera(float intensity, float shakeTime)
    {
        perlinNoise.m_AmplitudeGain = intensity;
        intensity = 1;
        StartCoroutine(WaitTime(shakeTime));
    }

    IEnumerator WaitTime(float shakeTime)
    {
        //yield return new WaitForSeconds(shakeTime);
        yield return new WaitForSeconds(0.5f);
        ResetIntensity();
    }

    void ResetIntensity()
    {
        perlinNoise.m_AmplitudeGain = 0f;
    }
}
