using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Template.Tools;
using Template;

namespace GameCore.Tools
{
    public class LevelGenerator
    {
        private static Dictionary<int, int> matchPairs = new Dictionary<int, int>();
        private static Dictionary<int, int> spawnPairs = new Dictionary<int, int>();

        /// <summary>
        /// Массив массивов доступных спрайтов для пар
        /// </summary>
        private static List<int> spritesToPairs = new List<int>();

        private static List<TileData> unlockedTiles;

        private static FieldData[][,] levelDataToGenerate;

        private static int spawnedPairs;

        public static void FillEmptyLevelPairs(ref FieldData[][,] levelData, int emptyPairs)
        {
            matchPairs = new Dictionary<int, int>();
            InitSpritesList(emptyPairs);

            for (int layer = levelData.Length - 1; layer >= 0; layer--)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[layer];

                int middleX = Mathf.CeilToInt(gridData.Width / 2f);
                int middleY = Mathf.CeilToInt(gridData.Height / 2f);

                spawnedPairs = 0;
                int tilesToSpawn = levelData[layer].Cast<FieldData>().Where(tile => tile.SpriteId == -1).Count();

                for (int yOffset = 0; yOffset <= Mathf.Max(middleY, middleX); yOffset++)
                {
                    for (int xOffset = -yOffset; xOffset <= yOffset; xOffset++)
                    {
                        int x = xOffset >= 0 ? Mathf.Min(middleX + xOffset, gridData.Width - 1) : Mathf.Max(middleX + xOffset, 0);

                        #region Horizontal
                        if (levelData[layer][Mathf.Max(middleY - yOffset, 0), x].SpriteId == -1)
                        {
                            levelData[layer][Mathf.Max(middleY - yOffset, 0), x].SpriteId = GetSpriteId(tilesToSpawn);
                        }

                        if (yOffset == 0) continue;

                        if (levelData[layer][Mathf.Min(middleY + yOffset, gridData.Height - 1), x].SpriteId == -1)
                        {
                            levelData[layer][Mathf.Min(middleY + yOffset, gridData.Height - 1), x].SpriteId = GetSpriteId(tilesToSpawn);
                        }
                        #endregion

                        #region Vertical
                        if (xOffset == -yOffset || xOffset == yOffset)
                        {
                            for (int y = Mathf.Max(middleY - yOffset, 0) + 1; y <= Mathf.Min(middleY + yOffset, gridData.Height - 1) - 1; y++)
                            {
                                if (levelData[layer][y, x].SpriteId == -1)
                                {
                                    levelData[layer][y, x].SpriteId = GetSpriteId(tilesToSpawn);
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        public static void NewLevelGenerator(ref FieldData[][,] levelData, int emptyPairs)
        {
            matchPairs = new Dictionary<int, int>();
            spawnPairs = new Dictionary<int, int>();
            InitSpritesList(emptyPairs);

            GetUnlockedTiles(levelData);

            //Запоминаем новую карту, чтобы начать генерить
            levelDataToGenerate = new FieldData[levelData.Length][,];
            for (int l = 0; l < levelData.Length; l++)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[l];
                levelDataToGenerate[l] = new FieldData[gridData.Height, gridData.Width];
            }

            int leftPairs = emptyPairs;

            while (leftPairs > 0)
            {
                var tilesPair = GetPairToMatching();

                if (tilesPair == null)
                {
                    ResetGenerator(ref levelData, emptyPairs);
                    return;
                }

                tilesPair[0].Id = GetSpriteId(2);
                tilesPair[1].Id = GetSpriteId(1);

                levelDataToGenerate[tilesPair[0].Layer][tilesPair[0].yPos, tilesPair[0].xPos] = new FieldData();
                levelDataToGenerate[tilesPair[0].Layer][tilesPair[0].yPos, tilesPair[0].xPos].SpriteId = tilesPair[0].Id;

                levelDataToGenerate[tilesPair[1].Layer][tilesPair[1].yPos, tilesPair[1].xPos] = new FieldData();
                levelDataToGenerate[tilesPair[1].Layer][tilesPair[1].yPos, tilesPair[1].xPos].SpriteId = tilesPair[1].Id;

                //Убираем тайлы в массиве левела
                levelData[tilesPair[0].Layer][tilesPair[0].yPos, tilesPair[0].xPos].SpriteId = 0;
                levelData[tilesPair[1].Layer][tilesPair[1].yPos, tilesPair[1].xPos].SpriteId = 0;

                UnHideAdjacentTiles(tilesPair[0], ref levelData);
                UnHideAdjacentTiles(tilesPair[1], ref levelData);

                leftPairs--;
            }

            for (int l = 0; l < levelData.Length; l++)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[l];

                for (int i = 0; i <= (gridData.Height * gridData.Width - gridData.Width); i += gridData.Width)
                {
                    for (int j = 0; j < gridData.Width; j++)
                    {
                        if (levelDataToGenerate[l][i / gridData.Width, j] != null)
                        {
                            levelData[l][i / gridData.Width, j].SpriteId = levelDataToGenerate[l][i / gridData.Width, j].SpriteId;
                        }
                    }
                }

            }
        }

        //TODO: надо это заменить на дерево решений 
        private static void ResetGenerator(ref FieldData[][,] levelData, int emptyPairs)
        {
            for (int l = 0; l < levelData.Length; l++)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[l];

                for (int i = 0; i <= (gridData.Height * gridData.Width - gridData.Width); i += gridData.Width)
                {
                    for (int j = 0; j < gridData.Width; j++)
                    {
                        if (levelDataToGenerate[l][i / gridData.Width, j] != null)
                        {
                            levelData[l][i / gridData.Width, j].SpriteId = -1;
                        }
                    }
                }
            }

            NewLevelGenerator(ref levelData, emptyPairs);
        }

        private static int GetSpriteId(int tilesToSpawn)
        {
            int spriteId = 0;

            if (spawnedPairs < tilesToSpawn / 2 || matchPairs.Keys.Count == 0)
            {
                spawnedPairs++;

                spriteId = spritesToPairs.Random();
                var keys = LevelsConfig.GetConfig.MatchingTable[spriteId].ImagesId;

                var key = keys[Random.Range(0, keys.Length)];
                while ((spawnPairs.ContainsKey(key) || spawnPairs.ContainsValue(key)) && keys.Length > 1)
                {
                    key = keys[Random.Range(0, keys.Length)];
                }

                //if (matchPairs.ContainsKey(spriteId))
                //{
                //    var value = keys[Random.Range(0, keys.Length)];
                //    matchPairs.Add(value, spriteId);

                //    spritesToPairs.Remove(spriteId);
                //    spritesToPairs.Remove(value);

                //    return value;
                //}

                matchPairs.Add(spriteId, key);

                if (!spawnPairs.ContainsKey(spriteId))
                    spawnPairs.Add(spriteId, key);

                spritesToPairs.Remove(spriteId);
                spritesToPairs.Remove(key);
                return spriteId;
            }
            else
            {
                int pairIndex = Random.Range(0, matchPairs.Count);
                var key = matchPairs.Keys.ToList()[pairIndex];

                spriteId = matchPairs[key];
                matchPairs.Remove(key);
                return spriteId;
            }
        }

        private static void InitSpritesList(int emptyPairs)
        {
            spritesToPairs = new List<int>();

            for (int i = 1; i < LevelsConfig.MatchingSpritesCount; i++)
            {
                spritesToPairs.Add(i);
            }

            //Коль-во оставшихся спрайтов, за пределом matchingTable
            int spritesLeft = emptyPairs * 2 - LevelsConfig.MatchingSpritesCount;

            var temp = spritesToPairs.Select(item => item).ToList();

            //Убираем из доп рандома все что не может повторяться (цветы/времена года)
            foreach (var sprite in LevelsConfig.GetConfig.MatchingTable.FindAll(data => !data.isCanRepeating))
            {
                temp.Remove(sprite.Id);
            }

            while (spritesLeft > 0)
            {
                var spritesToRandom = spritesLeft > temp.Count ? temp.Count : spritesLeft;
                spritesToPairs.AddRange(temp.Random(spritesToRandom));
                spritesLeft -= spritesToRandom;
            }
        }

        /// <summary>
        /// Разблокирует соседние плитки, если они больше не залочены
        /// </summary>
        private static void UnHideAdjacentTiles(TileData tileData, ref FieldData[][,] levelData)
        {
            //Убираем из разлоченных тайлов
            unlockedTiles.Remove(unlockedTiles.Find(data => data == tileData));

            for (int l = tileData.Layer; l >= 0; l--)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[l];

                var layer = levelData[l];

                for (int y = Mathf.Max(0, tileData.yPos - 1); y <= Mathf.Min(gridData.Height - 1, tileData.yPos + 1); y++)
                {
                    for (int x = Mathf.Max(0, tileData.xPos - 1); x <= Mathf.Min(gridData.Width - 1, tileData.xPos + 1); x++)
                    {
                        var tile = new TileData()
                        {
                            Layer = l,
                            yPos = y,
                            xPos = x
                        };

                        if (!State_Battle.IsLocked(tile, false) && !State_Battle.IsEmpty(tile))
                        {
                            var array = unlockedTiles.FindAll(data => data.Layer == tile.Layer && data.xPos == tile.xPos && data.yPos == tile.yPos);

                            if (array.Count == 0)
                                unlockedTiles.Add(tile);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получить список доступных плиток (перебор по всем слоям уровня)
        /// </summary>
        private static List<TileData> GetUnlockedTiles(FieldData[][,] levelData)
        {
            unlockedTiles = new List<TileData>();

            for (int layer = levelData.Length - 1; layer >= 0; layer--)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[layer];

                for (int i = 0; i <= (gridData.Height * gridData.Width - gridData.Width); i += gridData.Width)
                {
                    for (int j = 0; j < gridData.Width; j++)
                    {
                        var tileData = new TileData()
                        {
                            Layer = layer,
                            yPos = i / gridData.Width,
                            xPos = j
                        };

                        if (!State_Battle.IsLocked(tileData, false) && !State_Battle.IsEmpty(tileData))
                        {
                            unlockedTiles.Add(tileData);
                        }
                    }
                }
            }

            return unlockedTiles;
        }

        private static List<TileData> GetPairToMatching()
        {
            if (unlockedTiles.Count < 2)
            {
                Debug.LogError("LevelGenerator Error: no tiles to pair");
                //Debug.LogError($"Tile: [{unlockedTiles[0].Layer}]({unlockedTiles[0].xPos}, {unlockedTiles[0].yPos})");
                return null;
            }

            var tilesPair = unlockedTiles.Random(2);

            Debug.Log($"Pair: ({tilesPair.First().xPos}, {tilesPair.First().yPos}) || ({tilesPair.Last().xPos}, {tilesPair.Last().yPos})");

            return tilesPair.ToList();
        }
    }
}