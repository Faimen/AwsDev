using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public static class GameSettings
    {
        public static bool Sound
        {
            get
            {
                return PlayerPrefs.GetInt("sound", 1) == 1;
            }
            set
            {
                AudioManager.Instance.ChangeSoundOn(value);
                PlayerPrefs.SetInt("sound", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool Music
        {
            get
            {
                return PlayerPrefs.GetInt("music", 1) == 1;
            }
            set
            {
                AudioManager.Instance.ChangeMusicOn(value);
                PlayerPrefs.SetInt("music", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool IsDebugMode => GameManager.Environment != PluginsRoot.Network.Environment.Production;

        #region Version

        static BuildConfig buildConfig = null;
        public static BuildConfig BuildConfig
        {
            get
            {
                return buildConfig ?? (buildConfig = Resources.Load<BuildConfig>("BuildConfig"));
            }
        }
        #endregion
    }
}
