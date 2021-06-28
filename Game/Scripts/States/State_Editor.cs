using System.Collections;
using System.Collections.Generic;
using Template.Systems.StateMachine;
using Template;
using UnityEngine;
using GameCore.UI;
using GameCore.UI.GameTools;
using System.Linq;

namespace GameCore
{
    public class State_Editor : SMState
    {
        private LevelData levelData = new LevelData();

        public static GridData editableGrid;

        private int loadedIndex = -1;

        public override IEnumerator Enter()
        {
            UIManager.SetBaseStateAndActivate(UIStateCollection.LevelEditor);
            UIManager.GetElement<UIBackground>().Set(UIBackground.BackTypes.Editor);

            editableGrid = levelData.grids[0];
            UIManager.GetElement<UILevelEditor>().SetGrid(levelData.grids[0], 0);

            UIManager.GetElement<UILevelEditor>().OnChangeLayer += OnChangeLayerHandler;
            UIManager.GetElement<UILevelEditor>().OnDeleteLayer += OnDeleteLayerHandler;
            UIManager.GetElement<UILevelEditor>().OnClearAll += OnClearAllHandler;
            UIManager.GetElement<UILevelEditor>().OnChangeGridSize += OnChangeGridSizeHandler;
            UIManager.GetElement<UILevelEditor>().OnChangeTileData += OnChangeTileDataHandler;

            UIManager.GetElement<UILevelEditor>().OnSaveLevel += OnSaveLevelHandler;
            UILoadlLevelItem.LoadEvent += OnLoadLevelHandler;
            yield break;
        }

        public override IEnumerator Exit()
        {
            UIManager.GetElement<UILevelEditor>().OnChangeLayer -= OnChangeLayerHandler;
            UIManager.GetElement<UILevelEditor>().OnDeleteLayer -= OnDeleteLayerHandler;
            UIManager.GetElement<UILevelEditor>().OnClearAll -= OnClearAllHandler;
            UIManager.GetElement<UILevelEditor>().OnChangeGridSize -= OnChangeGridSizeHandler;
            UIManager.GetElement<UILevelEditor>().OnChangeTileData -= OnChangeTileDataHandler;

            UIManager.GetElement<UILevelEditor>().OnSaveLevel -= OnSaveLevelHandler;
            UILoadlLevelItem.LoadEvent -= OnLoadLevelHandler;
            yield break;
        }

        private void OnChangeLayerHandler(int newLayer)
        {
            if (newLayer >= levelData.grids.Length)
            {
                var temp = levelData.grids;
                levelData.grids = Enumerable.Repeat(new GridData(), newLayer + 1).ToArray();
                temp.CopyTo(levelData.grids, 0);

                levelData.grids[newLayer].Width = editableGrid.Width;
                levelData.grids[newLayer].Height = editableGrid.Height;

                levelData.grids[newLayer].Fields = new FieldData[editableGrid.Width * editableGrid.Height];
                for (int i = 0; i < levelData.grids[newLayer].Fields.Length; i++)
                {
                    levelData.grids[newLayer].Fields[i] = newLayer > 0 ? new FieldData(levelData.grids[newLayer - 1].Fields[i]) : new FieldData();
/*                    if (newLayer > 0)
                    {
                        levelData.grids[newLayer].Fields[i].OffsetData = levelData.grids[newLayer - 1].Fields[i].OffsetData;
                        levelData.grids[newLayer].Fields[i].SpriteId = levelData.grids[newLayer - 1].Fields[i].SpriteId;
                    }*/
                }

                levelData.grids[newLayer].Layer = newLayer;
            }

            editableGrid = levelData.grids[newLayer];
            UIManager.GetElement<UILevelEditor>().SetGrid(editableGrid, newLayer);
        }

        private void OnDeleteLayerHandler(int layerId)
        {
            if (levelData.grids.Length == 1)
            {
                levelData.grids = new GridData[] { new GridData() };
                editableGrid = levelData.grids[0];
                UIManager.GetElement<UILevelEditor>().SetGrid(levelData.grids[editableGrid.Layer], editableGrid.Layer);
                return;
            }

            levelData.grids = levelData.grids.Where((data, id) => id != layerId).ToArray();

            for (int i = 0; i < levelData.grids.Length; i++)
            {
                levelData.grids[i].Layer = i;
            }
        }

        private void OnClearAllHandler()
        {
            loadedIndex = -1;
            levelData = new LevelData();
            editableGrid = levelData.grids[0];
            UIManager.GetElement<UILevelEditor>().SetGrid(levelData.grids[editableGrid.Layer], editableGrid.Layer);
        }

        private void OnChangeGridSizeHandler(int x, int y)
        {
            editableGrid.Width = x;
            editableGrid.Height = y;

            var temp = editableGrid.Fields;

            editableGrid.Fields = new FieldData[x * y];
            for(int i=0; i < editableGrid.Fields.Length; i++)
            {
                if (i < temp.Length)
                    editableGrid.Fields[i] = temp[i];
                else
                    editableGrid.Fields[i] = new FieldData();
            }

            levelData.grids[editableGrid.Layer] = editableGrid;
            UIManager.GetElement<UILevelEditor>().SetGrid(levelData.grids[editableGrid.Layer], editableGrid.Layer);
        }

        private void OnChangeTileDataHandler(List<(TileData tileData, FieldData fieldData)> tiles)
        {
            foreach (var tile in tiles)
            {
                editableGrid.Fields[tile.tileData.yPos * editableGrid.Width + tile.tileData.xPos] = tile.fieldData;
            }

            levelData.grids[editableGrid.Layer] = editableGrid;
            UIManager.GetElement<UILevelEditor>().SetGrid(levelData.grids[editableGrid.Layer], editableGrid.Layer);
        }

        private void OnSaveLevelHandler(LevelData saveData)
        {
            levelData.Name = saveData.Name;
            levelData.HorizontalBlock = saveData.HorizontalBlock;
            levelData.VerticalBlock = saveData.VerticalBlock;
            levelData.emptyLinesCircle = saveData.emptyLinesCircle;

            if (loadedIndex < 0)
            {
                LevelsConfig.GetConfig.levelList.Add(levelData);
            }
            else
            {
                LevelsConfig.GetConfig.levelList[loadedIndex] = levelData;
                loadedIndex = -1;
            }

            LevelsConfig.GetConfig.Save();
        }

        public void OnLoadLevelHandler(LevelData loadData)
        {
            levelData = loadData;
            foreach(var grid in levelData.grids)
            {
                UIManager.GetElement<UILevelEditor>().SetGrid(grid, grid.Layer);
            }

            editableGrid = levelData.grids.Last();

            loadedIndex = LevelsConfig.GetConfig.levelList.IndexOf(loadData);
        }
    }
}
