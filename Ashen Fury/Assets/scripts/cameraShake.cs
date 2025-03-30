using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class cameraShake : MonoBehaviour
{
    private GameObject freeLookCamera;
    private CinemachineBasicMultiChannelPerlin noiseComponent;

    private void Awake()
    {
        freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCam");
        
        /* if (freeLookCamera == null)
        {
            Debug.LogError("FreeLook Camera component not found!");
            return;
        } */

        noiseComponent = freeLookCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        /* if (noiseComponent == null)
        {
            Debug.LogError("Noise component not found! Add CinemachineBasicMultiChannelPerlin to the FreeLook camera.");
            return;
        } */
        ResetIntensity();
    }

    public void ShakeCamera(float intensity, float time)
    {
        if (noiseComponent == null) return;
        
        noiseComponent.AmplitudeGain = intensity;
        StartCoroutine(Shake(time));
    }

    private IEnumerator Shake(float time)
    {
        yield return new WaitForSeconds(time);
        ResetIntensity();
    }

    private void ResetIntensity()
    {
        if (noiseComponent != null)
        {
            noiseComponent.AmplitudeGain = 0f;
        }
    }
}
