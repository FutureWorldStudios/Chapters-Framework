using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Cathlab POV Data", menuName = "Scriptable Objects/POVData")]
public class POVData : ScriptableObject
{
    public List<CathlabPOV> POVS;

    [Button]
    private void RecordPOV(POV pov)
    {
        Transform player = FindObjectOfType<FWSPlayerController>().transform;

        CathlabPOV newPOV = new CathlabPOV
        {
            View = pov,
            Position = player.position,
            Rotation = player.rotation
        };

        POVS.Add(newPOV);
    }
}

[Serializable]
public class CathlabPOV
{
    public POV View;
    public Vector3 Position;
    public Quaternion Rotation; 
}