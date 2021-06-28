using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameCore
{
    [System.Serializable]
    public class LevelsConfig
    {
        private static LevelsConfig _config;

        public static LevelsConfig GetConfig
        {
            get
            {
                if (_config == null)
                {
                    _config = ReadConfigFromJson();
                }
                return _config;
            }
        }

        public static int MatchingSpritesCount => GetConfig.MatchingTable.Count;

        private static string _configPath => Application.dataPath + "/Resources/LevelsConfig.json";

        public List<MatchingData> MatchingTable;
        public List<LevelData> levelList;

        public LevelsConfig()
        {
#if UNITY_EDITOR

#endif
        }

        public void Save()
        {
            MatchingTable = GetConfig.MatchingTable;
            levelList = GetConfig.levelList;

            //levelList[0].grids[0].Fields = new FieldData[] {
            //    new FieldData { PictureId = 1, OffsetData = new OffsetData() },
            //    new FieldData { PictureId = 3, OffsetData = new OffsetData() },
            //    new FieldData { PictureId = 4, OffsetData = new OffsetData() },
            //    new FieldData { PictureId = 2, OffsetData = new OffsetData() } };

            WriteToJson();
        }

        public static LevelsConfig ReadConfigFromJson()
        {
            var text = Resources.Load<TextAsset>("LevelsConfig");
            return JsonUtility.FromJson<LevelsConfig>(text.text);
        }

        private void WriteToJson()
        {
            string str = JsonUtility.ToJson(this, true);
            File.WriteAllText(_configPath, str);
        }
    }

    [System.Serializable]
    public class LevelData
    {
        public string Name;

        public bool HorizontalBlock;
        public bool VerticalBlock;

        public GridData[] grids;

        public int emptyLinesCircle = 0;

        public LevelData()
        {
            Name = "NewLevel";
            grids = new GridData[] { new GridData() };
        }
    }

    [System.Serializable]
    public class MatchingData
    {
        public string Name;
        public int Id;

        public int[] ImagesId;

        public bool isCanRepeating = true;
    }
}