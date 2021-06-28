using GameCore.Tools;
using GameCore.UI;
using System.Collections;
using System.Collections.Generic;
using Template;
using Template.Systems.Dialogs;
using Template.Systems.StateMachine;
using Template.UIBase;
using UnityEngine;

namespace GameCore
{
    public class State_Battle : SMState
    {
        public static int level = GameManager.Instance.GameUser.Data.GameLevel;

        //[L][Y, X]
        private static FieldData[][,] levelArray;

        public static bool IsEmpty(TileData tileData) => levelArray[tileData.Layer][tileData.yPos, tileData.xPos].SpriteId == 0;
        public static bool IsLocked(TileData tileData, bool isPlayAnim)
        {
            LevelData levelData = LevelsConfig.GetConfig.levelList[level];

            List<(int l, int y, int x)> blockTiles = new List<(int l, int y, int x)>();
            bool isVertical = false;

            ///смещение проверяемой плитки
            var tileOffset = levelArray[tileData.Layer][tileData.yPos, tileData.xPos].OffsetData;

            if (tileData.Layer < levelArray.Length - 1)
            {
                //3x3 upCheck
                for (int y = Mathf.Max(tileData.yPos - 1, 0); y <= Mathf.Min(tileData.yPos + 1, levelData.grids[tileData.Layer + 1].Height - 1); y++)
                {
                    for (int x = Mathf.Max(tileData.xPos - 1, 0); x <= Mathf.Min(tileData.xPos + 1, levelData.grids[tileData.Layer + 1].Width - 1); x++)
                    {
                        if (levelArray[tileData.Layer + 1][y, x].SpriteId != 0)
                        {
                            var offsetData = levelArray[tileData.Layer + 1][y, x].OffsetData;

                            if (tileData.yPos == y && tileData.xPos == x)
                            {
                                //TODO: Упростить
                                if (offsetData.Down && tileOffset.Up || offsetData.Up && tileOffset.Down) continue;
                                if (offsetData.Left && tileOffset.Right || offsetData.Right && tileOffset.Left) continue;


                                if (!isPlayAnim) return true;

                                blockTiles.Add((tileData.Layer + 1, y, x));
                                continue;
                            }

                            if ((x < tileData.xPos && (offsetData.Right && tileOffset.Right != offsetData.Right || tileOffset.Left && tileOffset.Left != offsetData.Left))
                                || (x > tileData.xPos && (tileOffset.Right && tileOffset.Right != offsetData.Right || offsetData.Left && tileOffset.Left != offsetData.Left)))
                            {
                                if ((y < tileData.yPos && (offsetData.Down && tileOffset.Down != offsetData.Down || tileOffset.Up && tileOffset.Up != offsetData.Up))
                                 || (y > tileData.yPos && (offsetData.Up && tileOffset.Up != offsetData.Up || tileOffset.Down && tileOffset.Down != offsetData.Down))
                                 || (y == tileData.yPos && (offsetData.Down != tileOffset.Up || offsetData.Up != tileOffset.Down || !offsetData.Down && !offsetData.Up)))
                                {
                                    if (!isPlayAnim) return true;

                                    blockTiles.Add((tileData.Layer + 1, y, x));
                                    continue;
                                }
                            }
                            else if (x == tileData.xPos)
                            {
                                if ((y < tileData.yPos && (offsetData.Down && tileOffset.Down != offsetData.Down || tileOffset.Up && tileOffset.Up != offsetData.Up))
                                 || (y > tileData.yPos && (offsetData.Up && tileOffset.Up != offsetData.Up || tileOffset.Down && tileOffset.Down != offsetData.Down)))
                                {
                                    if (!isPlayAnim) return true;

                                    blockTiles.Add((tileData.Layer + 1, y, x));
                                    continue;
                                }
                            }
                        }
                    }
                }

                /* if (blockTiles.Count > 0)
                     Debug.LogError("IS blocked TOP");*/
            }

            var leftBlockers = new List<(int l, int y, int x)>();
            var rightBlockers = new List<(int l, int y, int x)>();
            var upBlockers = new List<(int l, int y, int x)>();
            var downBlockers = new List<(int l, int y, int x)>();

            //3x3 tileLayerCheck
            for (int y = Mathf.Max(tileData.yPos - 1, 0); y <= Mathf.Min(tileData.yPos + 1, levelData.grids[tileData.Layer].Height - 1); y++)
            {
                for (int x = Mathf.Max(tileData.xPos - 1, 0); x <= Mathf.Min(tileData.xPos + 1, levelData.grids[tileData.Layer].Width - 1); x++)
                {
                    if (levelArray[tileData.Layer][y, x].SpriteId != 0)
                    {
                        var offsetData = levelArray[tileData.Layer][y, x].OffsetData;

                        if (levelData.HorizontalBlock)
                        {
                            if (y == tileData.yPos || (y < tileData.yPos && offsetData.Down && tileOffset.Down != offsetData.Down) || (y > tileData.yPos && offsetData.Up && tileOffset.Up != offsetData.Up))
                            {
                                //Check left Block
                                if (x < tileData.xPos && (!offsetData.Left && !tileOffset.Right || offsetData.Right && tileOffset.Right && offsetData.Right || offsetData.Left == tileOffset.Left && offsetData.Left)) 
                                    leftBlockers.Add((tileData.Layer, y, x));

                                //Check right Block
                                if (x > tileData.xPos && (!offsetData.Right && !tileOffset.Left || offsetData.Left == tileOffset.Left && offsetData.Left || offsetData.Right == tileOffset.Right && offsetData.Right)) 
                                    rightBlockers.Add((tileData.Layer, y, x));
                            }
                        }

                        if (levelData.VerticalBlock)
                        {
                            if (x == tileData.xPos || (x < tileData.xPos && offsetData.Right && tileOffset.Right != offsetData.Right) || (x > tileData.xPos && offsetData.Left && tileOffset.Left != offsetData.Left))
                            {
                                //Check top Block
                                if (y < tileData.yPos && !offsetData.Up) 
                                    upBlockers.Add((tileData.Layer, y, x));

                                //Check down Block
                                if (y > tileData.yPos && !offsetData.Down) 
                                    downBlockers.Add((tileData.Layer, y, x));
                            }
                        }
                    }
                }
            }

            if (leftBlockers.Count > 0 && rightBlockers.Count > 0)
            {
                //Debug.LogError("IS blocked HORIZONTAL");
                if (!isPlayAnim) return true;

                blockTiles.AddRange(leftBlockers);
                blockTiles.AddRange(rightBlockers);
            }

            if (upBlockers.Count > 0 && downBlockers.Count > 0)
            {
                //Debug.LogError("IS blocked VERTICAL");
                if (!isPlayAnim) return true;

                blockTiles.AddRange(upBlockers);
                blockTiles.AddRange(downBlockers);
                isVertical = true;
            }

            if (blockTiles.Count > 0)
            {
                blockTiles.Add((tileData.Layer, tileData.yPos, tileData.xPos));

                if (isPlayAnim)
                    UIManager.GetElement<UIBattleField>().PlayBlockAnims(blockTiles.ToArray(), !isVertical);

                return true;
            }

            //Debug.LogError("Is NOT blocked");

            return false;
        }

        public override IEnumerator Enter()
        {
            UIManager.SetBaseState(UIStateCollection.Battle);
            UIManager.GetElement<UIBackground>().Set(UIBackground.BackTypes.Game);

            LoadLevelData();

            UIManager.GetElement<UIBattleField>().OnFindMatch += OnFindMatchHandler;

            UIManager.GetElement<UIBattleField>().SetLevel(levelArray);
            UIManager.SetState(UIStateCollection.Battle);

            if (level == 0)
            {
                yield return new WaitForSeconds(0.6f);
                Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "", "<size=57>" + "Common.FirstLevel.Goal".Localized(), true, "Common.OK".Localized()), result =>
                {
                    Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "", "<size=57>" + "Classic/Common.FirstLevel.Description".Localized(), true, "Common.OK".Localized()), nextResult => { });
                });
            }

            if(level == 3)
            {
                yield return new WaitForSeconds(1.8f);
                Dialog.ShowDialog(dialog => MahjongDialogPresets.InitAsSeasonTiles(dialog), result => 
                {
                    Dialog.ShowDialog(dialog => MahjongDialogPresets.InitAsFlowerTiles(dialog), result2 => { });
                });
            }

            if (level == 4)
            {
                yield return new WaitForSeconds(1.8f);
                Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "", "<size=57>" + "Common.Hints.UseFlashlight".Localized(), true, "Common.OK".Localized()), result => { });
            }

            if (level == 8 || level == 11 || level == 19)
            {
                if (level == 8)
                {
                    Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "Common.BattleTimer.FirstLevel.Title".Localized(), "<size=57>" + "Common.BattleTimer.FirstLevel".Localized(), true, "Common.OK".Localized()),
                        result => UIManager.GetElement<UIBattleHeader>().SetTimer(270f));

                    yield break;
                }

                if (level == 11)
                {
                    Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "Common.BattleTimer.FirstLevel.Title".Localized(), "<size=57>" + "Common.BattleTimer.SecondLevel".Localized(), true, "Common.OK".Localized()),
                        result => UIManager.GetElement<UIBattleHeader>().SetTimer(390f));

                    yield break;
                }

                UIManager.GetElement<UIBattleHeader>().SetTimer(390f);
            }

            yield break;
        }

        public override IEnumerator Exit()
        {
            UIManager.GetElement<UIBattleField>().OnFindMatch -= OnFindMatchHandler;
            yield break;
        }

        private void OnFindMatchHandler(TileData prevTile, TileData currentTile)
        {
            levelArray[prevTile.Layer][prevTile.yPos, prevTile.xPos].SpriteId = 0;
            levelArray[currentTile.Layer][currentTile.yPos, currentTile.xPos].SpriteId = 0;
        }

        /// <summary>
        /// Create 3d array from levelDataConfig
        /// </summary>
        private void LoadLevelData()
        {
            int tiles = 0;
            int pairsToGenerate = 0;
            LevelData levelData = LevelsConfig.GetConfig.levelList[level];

            levelArray = new FieldData[levelData.grids.Length][,];

            for (int l = 0; l < levelData.grids.Length; l++)
            {
                levelArray[l] = new FieldData[levelData.grids[l].Height, levelData.grids[l].Width];

                for (int h = 0; h <= (levelData.grids[l].Height * levelData.grids[l].Width - levelData.grids[l].Width); h += levelData.grids[l].Width)
                {
                    for (int w = 0; w < levelData.grids[l].Width; w++)
                    {
                        int spriteId = levelData.grids[l].Fields[h + w].SpriteId;

                        levelArray[l][h / levelData.grids[l].Width, w] = new FieldData()
                        {
                            SpriteId = spriteId,
                            OffsetData = levelData.grids[l].Fields[h + w].OffsetData
                        };

                        if (spriteId != 0) tiles++;
                        if (spriteId == -1) pairsToGenerate++;
                    }
                }
            }

            UIManager.GetElement<UIBattleHeader>().Init(0, tiles);

            pairsToGenerate /= 2;
            LevelGenerator.NewLevelGenerator(ref levelArray, pairsToGenerate);
        }
    }
}