using UnityEngine;
using System.Collections;

namespace GameCore
{
    [System.Serializable]
    public class StopSettings
    {

        public string Id;

        /// <summary>
        /// Время затухания звука
        /// </summary>
        public float fadeOut = 0;


        public StopSettings(string id)
        {
            Id = id;
        }
    }
}