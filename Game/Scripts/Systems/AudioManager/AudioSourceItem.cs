using UnityEngine;
using System.Collections;

namespace GameCore
{
    public class AudioSourceItem : MonoBehaviour
    {
        private AudioManager _AM;
        private AudioSource _source;
        //private float _baseVolume = 1;
        private float _zoneFactor = 1;

        #region Properties

        /// <summary>
        /// Уровень громкости относительно всех множителей
        /// </summary>
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                UpdateVolume();
            }
        }
        private float _volume = 1;

        /// <summary>
        /// ID проигрываемого звука
        /// </summary>
        public string Id
        {
            get { return PlaySettings != null ? PlaySettings.Id : ""; }
        }

        /// <summary>
        /// Настройки звука
        /// </summary>
        public PlaySettings PlaySettings { get; private set; }

        /// <summary>
        /// Проигрывается ли звук
        /// </summary>
        public bool IsPlaying { get { return _source.isPlaying; } }

        /// <summary>
        /// Прогресс проигрывания звука
        /// </summary>
        public float Time { get { return _source.time; } }

        public float Pitch { get => _source.pitch; set { _source.pitch = value; } }

        #endregion


        public void Awake()
        {
            _source = gameObject.GetComponent<AudioSource>();
            if (_source == null)
                _source = gameObject.AddComponent<AudioSource>();

            _source.playOnAwake = false;
        }
        public void OnEnable()
        {
        }
        public void Set(AudioManager am, AudioClip clip, PlaySettings settings)
        {
            StopAllCoroutines();

            PlaySettings = settings;
            _AM = am;

            //_baseVolume = (item.volume / 100f);
            _volume = settings.relVolume;
            _zoneFactor = 1;

            _source.clip = clip;
            _source.volume = _volume/*_baseVolume*/;
            _source.pitch = settings.relPitch;
            _source.loop = settings.loop;

            // Настройки 3д
            _source.spatialBlend = 0/*settings.DDDLevel*/;
            //_source.dopplerLevel = 0;
            //_source.rolloffMode = AudioRolloffMode.Linear;

            //if (settings.dontUseBasicRolloff)
            //{
            //    _source.maxDistance = 1000;
            //    _source.minDistance = 1000;
            //}
            //else
            //{
            //    _source.minDistance = AudioManager.SoundsConf.soundDistance.x;
            //    _source.maxDistance = AudioManager.SoundsConf.soundDistance.y;
            //}

            UpdateVolume();
        }

        /// <summary>
        /// Применить все множители к источнику звука
        /// </summary>
        public void UpdateVolume()
        {
            float globalVolume = GameSettings.Sound ? 1f : 0f;

            _source.volume = Volume * globalVolume /** _baseVolume*/ * _zoneFactor;
        }

        /// <summary>
        /// Назначить родительский объект источника звука
        /// </summary>
        public void SetParent(Transform parent)
        {
            if (parent != null)
            {
                _source.transform.parent = parent;
                _source.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Запустить звук
        /// </summary>
        public void Play()
        {
            _source.PlayDelayed(PlaySettings.delay);

            if (PlaySettings.loop && PlaySettings.duration > 0)
            {
                Invoke("Stop", PlaySettings.duration);
            }
        }


        #region Stoping


        /// <summary>
        /// Остановить звук
        /// </summary>
        public void Stop() { Stop(new StopSettings(Id)); }

        /// <summary>
        /// Остановить звук с заданными настройками
        /// </summary>
        public void Stop(StopSettings stopStngs)
        {
            // Если завершаем плавно
            if (stopStngs.fadeOut > 0)
            {
                StartCoroutine(FadeOut_Process(stopStngs.fadeOut));
                return;
            }
            StopSource();
        }

        private IEnumerator FadeOut_Process(float time)
        {
            float timer = time;
            float lastVolume = Volume;
            while (timer > 0)
            {
                timer -= UnityEngine.Time.deltaTime;
                Volume = Mathf.Lerp(0, lastVolume, timer / time);
                yield return 0;
            }
            StopSource();
            yield break;
        }

        private void StopSource()
        {
            _source.Stop();
        }

        #endregion

        public void OnDestroy()
        {
            AudioManager.Instance.DestroyAudioSourceItem(this);
        }
    }
}