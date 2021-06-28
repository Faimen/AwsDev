using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace GameCore
{
    public class AudioManager : MonoBehaviour
    {

        public static AudioManager Instance;
        public static BackgroundMixer BacgroundSoundController;

        [SerializeField] private AudioMixer backgroundMixer;

        private List<AudioSourceItem> sourcesList = new List<AudioSourceItem>();

        private static Dictionary<string, AudioClip> audioClipsCollection = new Dictionary<string, AudioClip>();

        public delegate void PlayEventHandler(AudioSourceItem source);
        public static event PlayEventHandler OnPlay;

        void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            BacgroundSoundController = Instance.GetComponentInChildren<BackgroundMixer>();

            AudioListener.volume = 1f;
        }

        private void Start()
        {
            BacgroundSoundController.PlayBackgroundSound();
            //BacgroundSoundController.PlayMenuSound();
            ChangeMusicOn(GameSettings.Music);
            ChangeSoundOn(GameSettings.Sound);
        }

        public void DestroyAudioSourceItem(AudioSourceItem audioSource)
        {
            if (sourcesList.Contains(audioSource))
                sourcesList.Remove(audioSource);
        }

        #region PauseFocus

        bool is_paused = false;
        bool is_focused = true;

        void OnApplicationPause(bool pauseStatus)
        {
            is_paused = pauseStatus;

            if (pauseStatus)
            {
                AudioListener.pause = true;
                AudioListener.volume = 0;
            }
            else
            {
                if (is_focused)
                {
                    AudioListener.pause = false;
                    AudioListener.volume = 1f;
                }
            }
        }

        private void OnApplicationFocus(bool has_focus)
        {
            is_focused = has_focus;

            if (!is_paused && has_focus)
            {
                AudioListener.pause = false;
                AudioListener.volume = 1f;
            }
        }

        #endregion


        /// <summary>
        /// Проигрывание звука с настройками
        /// </summary>
        public static AudioSourceItem Play(PlaySettings settings)
        {
            if (Instance == null) return null;
            return Instance.PlayInternal(settings);
        }


        private AudioSourceItem PlayInternal(PlaySettings settings)
        {
            //if (!GameSettings.Sound) return null;

            // Ищем звук с таким айдишником
            AudioClip clip = GetAudioClip(settings.Id);

            if (clip == null)
            {
                Debug.LogError($"Такого звука нет: {settings.Id}");
                return null;
            }

            // Если нам нужно выждать интервал после такого же звука
            if (settings.intervalSame > 0)
            {
                foreach (var s in sourcesList)
                {
                    if (s.IsPlaying &&
                        s.Id == settings.Id &&
                        s.Time < settings.intervalSame)
                    {
                        return null;
                    }
                }
            }

            // Если нельзя запускать пока проигрывается такой же
            if (settings.synchSame)
            {
                foreach (var s in sourcesList)
                {
                    bool equals = s.Id == settings.Id;

                    //Для одинаковых звуковых ID, но они в разных вариациях
                    //Смотрим есть ли "_" в 1 id
                    int index = s.Id.IndexOf("_");

                    if (index != -1)
                    {
                        var s1 = s.Id.Remove(index);
                        //Смотрим есть ли "_" в 2 id
                        index = settings.Id.IndexOf("_");

                        if (index != -1)
                        {
                            var s2 = settings.Id.Remove(index);
                            equals = s1 == s2;
                        }
                    }

                    if (s.IsPlaying && equals)
                    {
                        return null;
                    }
                }
            }


            //подбираем AudioSourceItem для проигрывания звука
            AudioSourceItem source = GetFreeAudioSource(settings.initiator);
            source.Set(this, clip, settings);

            source.Play();

            // Оповещаем всех, что запустился звук
            OnPlay?.Invoke(source);

            return source;
        }

        public static void Stop(StopSettings settings)
        {
            Instance?.StopInternal(settings);
        }

        private void StopInternal(StopSettings settings)
        {
            foreach (var s in sourcesList)
            {
                if (s.IsPlaying && s.Id == settings.Id)
                {
                    s.Stop(settings);
                }
            }
        }

        public static void StopAll()
        {
            Instance?.StopAllInternal();
        }


        private void StopAllInternal()
        {
            StopSettings stopSettings = new StopSettings("")
            {
                fadeOut = 0.4f
            };

            foreach (var s in sourcesList)
            {
                if (s.isActiveAndEnabled)
                    s.Stop(stopSettings);
            }
        }

        /// <summary>
        /// Возвращаем свободный AudioSourceItem или создаём новый
        /// </summary>
        private AudioSourceItem GetFreeAudioSource(Transform initiator = null)
        {
            AudioSourceItem source = null;

            for (int i = sourcesList.Count - 1; i >= 0; i--)
            {
                if (sourcesList[i] == null)
                {
                    sourcesList.RemoveAt(i);
                    continue;
                }
                if (!sourcesList[i].IsPlaying)
                {
                    source = sourcesList[i];
                    break;
                }
            }

            if (source == null)
            {
                GameObject go = new GameObject("sound#" + sourcesList.Count);
                source = go.AddComponent<AudioSourceItem>();
                sourcesList.Add(source);
            }

            // Запихиваем источник в родителя
            source.SetParent(initiator ?? transform);

            return source;
        }

        /// <summary>
        /// Возвращает загруженный звук, либо загружает его
        /// </summary>
        public static AudioClip GetAudioClip(string id)
        {
            if(audioClipsCollection.ContainsKey(id))
            {
                return audioClipsCollection[id];
            }
            else
            {
                var newClip = Resources.Load<AudioClip>($"Audio/{id}");
                audioClipsCollection.Add(id, newClip);
                return newClip;
            }
        }

        /// <summary>
        /// Вкл/выкл звука
        /// </summary>
        public void ChangeSoundOn(bool state)
        {
            foreach (var source in sourcesList)
            {
                source.UpdateVolume();
            }
        }

        /// <summary>
        /// Вкл/выкл музыки
        /// </summary>
        public void ChangeMusicOn(bool state)
        {
            backgroundMixer.SetFloat("music_volume", state ? 0 : -80);
        }
    }
}
