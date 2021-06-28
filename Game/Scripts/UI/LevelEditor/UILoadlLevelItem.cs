using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameCore.UI.GameTools
{
    public class UILoadlLevelItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelName;
        private LevelData levelData;

        public static event System.Action<LevelData> LoadEvent;

        public void SetData(LevelData levelData)
        {
            this.levelData = levelData;
            levelName.text = levelData.Name;
        }

        public void DeleteLevel()
        {            
            gameObject.SetActive(false);
            LevelsConfig.GetConfig.levelList.Remove(levelData);
            LevelsConfig.GetConfig.Save();
        }

        public void LoadLevel()
        {
            LoadEvent?.Invoke(levelData);
        }
    }
}