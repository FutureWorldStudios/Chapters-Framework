using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSheetScrubber : MonoBehaviour
{
    public List<Sprite> _spriteList; // Drag your sprites here in the Inspector
    public Image _displayImage;      // Drag the UI Image component here

    [SerializeField] protected float _minX = 0.01f;
    [SerializeField] protected float _maxX = -0.0719f;

    [SerializeField] private Transform _target;


    protected virtual void Start()
    {

    }

    private void Update()
    {
        float x = 0;
        float tNorm = 0;

        x = _target.localPosition.x;

        tNorm = Mathf.InverseLerp(_maxX, _minX, x);

        SetSpriteByValue(tNorm);
    }

    public void SetSpriteByValue(float normalizedValue)
    {
        if (_spriteList.Count == 0) return;

        float t = Mathf.Clamp01(normalizedValue);

        int index = Mathf.FloorToInt(t * (_spriteList.Count - 1));

        _displayImage.sprite = _spriteList[index];
    }

    public void SetDefaultImage(Sprite sprite)
    {
        _displayImage.sprite = sprite;  
    }
}
