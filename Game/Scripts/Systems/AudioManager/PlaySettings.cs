using UnityEngine;

namespace GameCore
{

    [System.Serializable]
    /// <summary>
    /// Настройки запуска звука
    /// </summary>
    public class PlaySettings
    {
        public string Id;

        public Transform initiator = null;
        /// <summary>
        /// Не запускать, пока играется аналогичный
        /// </summary>
        public bool synchSame = false;
        /// <summary>
        /// Не запускать в течение этого интервала после аналогичного
        /// </summary>
        public float intervalSame = 0;
        /// <summary>
        /// Задержка перед запуском
        /// </summary>
        public float delay = 0;
        /// <summary>
        /// Зацикливание
        /// </summary>
        public bool loop = false;
        /// <summary>
        /// Долгота зацыкленного звука
        /// </summary>
        public float duration = 0;
        /// <summary>
        /// Относительная громкость от 0 до 1
        /// </summary>
        public float relVolume = 1;
        /// <summary>
        /// Относительная высота/скорость от 0 до 1
        /// </summary>
        public float relPitch = 1;

        /// <summary>
        /// Уровень объёма звука: 0 - 2D, 1 - 3D
        /// </summary>
        public float DDDLevel = 0;

        /// <summary>
        /// Если true, устанавливает минимальную дистанцию очень большой,
        /// чтобы контролировать громкость звука отдельно
        /// </summary>
        [System.NonSerialized]
        public bool dontUseBasicRolloff = false;
        //public CustomDDD customDDD = null;

        public string soundZone = "";

        public PlaySettings(string id)
        {
            Id = id;
        }


        //public class CustomDDD
        //{
        //    public System.Action<float> distanceAction;
        //    public float minDistance = 1;
        //    public float maxDistance = 20;
        //}
    }
}