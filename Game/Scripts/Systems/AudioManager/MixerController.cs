using System;
using UnityEngine;
using UnityEngine.Audio;

namespace GameCore
{
    public abstract class MixerController : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;

        /// <summary>
        /// Задаем значение звука
        /// </summary>
        /// <param name="volume"> диапазон допустимых значений от 0 до 1 </param>
        public float SetVolume(float volume, string soundId)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            mixer.SetFloat(soundId, Mathf.Log(volume) * 20);
            return volume;
        }

        protected float GetSoundVolume(string id)
        {
            float pow;
            mixer.GetFloat(id, out pow);
            float result = (float)Math.Pow(10, pow / 20);
            return result;
        }

        protected void SetSoundVolume(string id, float volume)
        {
            mixer.SetFloat(id, volume);
        }
    }
}