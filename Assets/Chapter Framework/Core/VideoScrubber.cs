using UnityEngine;
using UnityEngine.Video;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

namespace VRG.ChapterFramework.Core
{

    public class VideoScrubber : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer _videoPlayer;

        [Header("Knob Movement")]
        [SerializeField] private Transform _target;
        [SerializeField] protected float _minX = 0.01f;
        [SerializeField] protected float _maxX = -0.0719f;
        [Range(0, 1)] public float _slider;

        private bool _scrubbing = false;
        private bool _prepared = false;
        private double _lastAppliedTime = -999;

        protected virtual void Start()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.playOnAwake = false;
                _videoPlayer.prepareCompleted += OnPrepared;
            }

        }

        protected void OnEnable()
        {

        }

       

        protected void OnDestroy()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.prepareCompleted -= OnPrepared;
            }
        }

        protected void Update()
        {
            if (!_scrubbing || !_prepared)
                return;

            float tNorm = 0;
            float x = 0;

        //#if !UNITY_EDITOR
            if (_target == null || _videoPlayer == null || _videoPlayer.clip == null)
                return;
                   x = _target.localPosition.x;

             tNorm = Mathf.InverseLerp(_maxX, _minX, x);
        //#endif

        //#if UNITY_EDITOR
        //    tNorm = Mathf.InverseLerp(_maxX, _minX, _slider);
        //#endif

            double targetTime = tNorm * _videoPlayer.length;

            if (Mathf.Abs((float)(targetTime - _lastAppliedTime)) < 0.03f)
                return;

            _lastAppliedTime = targetTime;

            //Debug.Log("[Scrub] x=" + x + "  t=" + tNorm + "  time=" + targetTime);

            _videoPlayer.time = targetTime;
        }

        private void OnPrepared(VideoPlayer vp)
        {
            _prepared = true;

            // Start playback engine but freeze time.
            vp.playbackSpeed = 0f;
            vp.isLooping = false;
            vp.Play();        // MUST CALL: otherwise frames don’t update on Quest

            _scrubbing = true;

            Debug.Log("[Video] Prepared. Ready for scrubbing.");
        }


        [Button]
        public async void Scrub(VideoClip clip)
        {
            await Task.Delay(1000);

            if (clip == null)
            {
                Debug.LogError("[Video] Clip is null!");
                return;
            }

            _scrubbing = false;
            _prepared = false;
            _lastAppliedTime = -999;

            _videoPlayer.Stop();
            _videoPlayer.clip = clip;


            Debug.Log("[Video] Preparing…");
            _videoPlayer.Prepare();
        }

        public void StopScrub()
        {
            _scrubbing = false;
        }


        public void Stop()
        {
            StopScrub();
            _videoPlayer.Stop();
        }
    }
}