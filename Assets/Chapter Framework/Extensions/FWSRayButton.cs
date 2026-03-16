using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRG.ChapterFramework;
using VRG.ChapterFramework.Core;
using Oculus.Interaction;
using DG.Tweening;

public class FWSRayButton : PointableCanvasUnityEventWrapper
{
    [SerializeField] PokeInteractable _pokeInteractable;
    [SerializeField] RayInteractable _rayInteractable;
    [SerializeField] Button _button;

    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;
    [SerializeField] private float _duration = 1f;
    public Button ButtonElement => _button;

    public bool IsInteractable
    {
        get => _pokeInteractable.enabled && _rayInteractable.enabled;

        set
        {
            _pokeInteractable.enabled = value;
            _rayInteractable.enabled = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (_pokeInteractable == null)
        {
            _pokeInteractable = GetComponent<PokeInteractable>();
        }
        if (_rayInteractable == null)
        {
            _rayInteractable = GetComponent<RayInteractable>();
        }

        if(_button == null)
        {
            _button = transform.GetComponentInChildren<Button>();   
        }
         ChaptersManager.OnChapterBegunHideButtons += HideButton;

    }

    private void OnDestroy()
    {
        ChaptersManager.OnChapterBegunHideButtons -= HideButton;
    }

    private void HandleMenuVisibility(bool active)
    {
        IsInteractable = !active;
    }

    [Button]
    public void ShowButton(bool animate = true)
    {
        DisableInteraction();

        if (animate)
            transform.DOScale(1f, _duration).SetEase(showEase).OnComplete(EnableInteraction);
        else
            transform.DOScale(1f, 0f);
    }

    [Button]
    public void HideButton(bool animate = true)
    {
        DisableInteraction();

        if(animate)
            transform.DOScale(0f, _duration).SetEase(hideEase).OnComplete(EnableInteraction);
        else
            transform.DOScale(0f, 0f);
    }

    private void EnableInteraction()
    {
        IsInteractable = true;
    }

    private void DisableInteraction()
    {
        IsInteractable = false;
    }

    private void Test()
    {
        Debug.Log("Clicked");
    }
}
