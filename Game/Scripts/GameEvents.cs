using Template.Tools.Events;
using UnityEngine;

namespace Template
{
    public static class GameEvents
    {
        /// <summary>
        /// Зашли в игру и получили юзера
        /// </summary>
        public static GlobalEvent OnGameEnter = new GlobalEvent();
    }
}