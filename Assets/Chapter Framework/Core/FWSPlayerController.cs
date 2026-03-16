using UnityEngine;
using System.Threading.Tasks;
using System;
using VRG.ChapterFramework;

public enum POV
{

}

public class FWSPlayerController : MonoBehaviour
{
    [SerializeField] private POVData _povData;

    private Transform _playerRoot;
    private POV _currentPOV;

    public POV CurrentPOV => _currentPOV;

    public static FWSPlayerController Instance;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);   

        _playerRoot = GetComponent<Transform>();
    }

    public async void SetPOV(POV pov, bool blink = true)
    {
        CathlabPOV povEntry = null;

        Debug.Log("[vivek] pov" +  pov.ToString());    

        foreach (CathlabPOV p in _povData.POVS)
        {
            if (p.View == pov)
            {
                povEntry = p;

                _currentPOV = p.View;
                break; // stop searching once we find the first match
            }
        }

        if (povEntry == null)
        {
            Debug.LogWarning($"[FWS] POV '{pov}' not found in data.");
            return;
        }

        if(blink)
            await FWS_OVRScreenFade.Instance.Blink();
       

        _playerRoot.position = new Vector3(povEntry.Position.x, _playerRoot.position.y, povEntry.Position.z);
        _playerRoot.rotation = povEntry.Rotation;

        if(!blink)
           FWS_OVRScreenFade.Instance.FadeIn();

#if !UNITY_EDITOR
        OVRManager.display.RecenterPose();
#endif
    }

}
