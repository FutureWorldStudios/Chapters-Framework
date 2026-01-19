using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class FramerateManager : MonoBehaviour
{
    [SerializeField] private int targetFramerate = 90;
    [SerializeField] private MsaaQuality quality = MsaaQuality._4x;

    public static FramerateManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        XRSettings.eyeTextureResolutionScale = 1.1f;

        Application.targetFrameRate = targetFramerate;
        OVRPlugin.systemDisplayFrequency = targetFramerate;

        if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Meta_Quest_3S || OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Meta_Quest_3)
        {
            OVRPlugin.suggestedCpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.SustainedHigh;
        }
    }
}
