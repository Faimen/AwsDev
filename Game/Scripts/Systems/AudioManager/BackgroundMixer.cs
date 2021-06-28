using System;
using System.Collections;
using UnityEngine;

namespace GameCore
{
    public class BackgroundMixer : MixerController
    {
        [SerializeField] private MixerSoundSettings backgroundSound;

        [SerializeField] private float transitionTime;

        private MixerSoundSettings currentSound;

        public void PlayBackgroundSound()
        {
            PlaySound(backgroundSound);
        }

        //public void PlayGameSound()
        //{
        //    PlaySound(gameSound);
        //}

        private void PlaySound(MixerSoundSettings mixerSound)
        {


            if (currentSound == mixerSound) return;

            mixerSound.SetRandomSound();
            mixerSound.source.Play();
            StartCoroutine(IChangeSound(mixerSound));
        }

        private IEnumerator IChangeSound(MixerSoundSettings newSound)
        {
            float currentTime = 0;
            float newVolume = 0;

            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (newVolume < 1)
            {
                yield return waitForFixedUpdate;

                currentTime += Time.fixedDeltaTime;
                newVolume = Mathf.MoveTowards(newVolume, 1, transitionTime);

                SetVolume(newVolume, newSound.MixerExposedParam);

                if (currentSound != null)
                    SetVolume(GetSoundVolume(currentSound.MixerExposedParam) - newVolume, currentSound.MixerExposedParam);
            }
            currentSound = newSound;
        }

        public void TurnOffSound()
        {
            StartCoroutine(IChangeSound(currentSound));
        }
    }

    [Serializable]
    public class MixerSoundSettings
    {
        public AudioSource source;
        public string MixerExposedParam;
        public string soundId;
        public void SetRandomSound()
        {
            source.clip = AudioManager.GetAudioClip(soundId);
            //source.clip = AudioManager.SoundsConf.Get(soundId, GameCore.Data.SoundCategory.Music).RandomClip;
        }

    }
}