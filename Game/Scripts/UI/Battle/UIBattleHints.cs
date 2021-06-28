using GameCore.Data;
using GameCore.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Template;
using Template.Systems.Dialogs;
using Template.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIBattleHints : UIElementAnimated
    {
        [SerializeField] private List<Image> hintStars;

        private Timer hintsTimer;
        private int highlightedHintId = 0;

        private List<UITile> unlockedTiles;

        private bool isHidden = false;
        private int pairsHighlightTimes = 0;
        private int pairsExplodeTimes = 0;
        private (UITile tile1, UITile tile2) matchingPair;

        private void OnEnable()
        {
            isHidden = false;
            pairsHighlightTimes = 0;
            pairsExplodeTimes = 0;
            UIManager.GetElement<UIBattleField>().OnFindMatch += OnFindMatchHandler;

            //Инициализируем таймер для подсказок
            hintsTimer = new Timer();
            hintsTimer.Set(15f);
            hintsTimer.SetOnFinish(HighlightHint);

            //Выключение звезды после анимации
            hintStars.ForEach(star =>
            {
                var tween = star.GetComponent<BroTweener>();
                tween.OnFinish.RemoveAllListeners();
                tween.AddOnFinish((i) => star.gameObject.SetActive(false));
            });
        }

        private void OnDisable()
        {
            UIManager.GetElement<UIBattleField>().OnFindMatch -= OnFindMatchHandler;
            unlockedTiles = null;
        }

        private void Update()
        {
            if (hintsTimer != null) hintsTimer.Update();
        }

        private void HighlightHint()
        {
            for (int i = 0; i < hintStars.Count; i++)
            {
                hintStars[i].gameObject.SetActive(i == highlightedHintId);
            }

            highlightedHintId++;
            if (highlightedHintId == hintStars.Count) highlightedHintId = 0;

            //Обновляем таймер
            hintsTimer.Set(15f);
        }

        /// <summary>
        /// Скрывает плитки которые залочены
        /// </summary>
        public void HideLockedTiles()
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            if (isHidden) return;
            if (!UserHelper.TrySpendCoins(40)) return;

            foreach (var layer in UIManager.GetElement<UIBattleField>().GridLayers)
            {
                foreach (UITile tile in layer.Tiles)
                {
                    tile.SetHidden(State_Battle.IsLocked(tile.Content.TileData, false));
                }
            }

            isHidden = true;
            AudioManager.Play(new PlaySettings("FlashlightClick"));
        }

        /// <summary>
        /// Матчит пары подряд нужное коль-во раз
        /// </summary>
        public void ExplodeMatchingPairs(int repeatTimes)
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            if (pairsExplodeTimes > 0) return;

            if (!UserHelper.TrySpendCoins(60)) return;

            pairsExplodeTimes = repeatTimes;
            ExplodeMatchingPair();
        }

        private void ExplodeMatchingPair()
        {
            matchingPair = GetMatchingPair();

            if (matchingPair == (null, null)) return;

            AudioManager.Play(new PlaySettings("BombClick"));
            UIManager.GetElement<UIBattleField>().MatchTiles(new List<UITile>() { matchingPair.tile1, matchingPair.tile2 });
        }

        /// <summary>
        /// Подсвечивает пары подряд несколько раз
        /// </summary>
        public void HighlightMatchingPair(int repeatTimes)
        {
            AudioManager.Play(new PlaySettings("ButtonClick"));
            if (pairsHighlightTimes > 0) return;

            if (!UserHelper.TrySpendCoins(20)) return;

            pairsHighlightTimes = repeatTimes;
            HighlightMatchingPair();
        }

        private void HighlightMatchingPair()
        {
            matchingPair = GetMatchingPair();

            if (matchingPair == (null, null)) return;

            AudioManager.Play(new PlaySettings("LampClick"));
            matchingPair.tile1.Highlight(true);
            matchingPair.tile2.Highlight(true);
        }

        private void HideMatchingPair()
        {
            if (matchingPair != (null, null))
            {
                matchingPair.tile1.Highlight(false);
                matchingPair.tile2.Highlight(false);

                matchingPair = (null, null);
            }
        }

        private (UITile tile1, UITile tile2) GetMatchingPair()
        {
            if (unlockedTiles == null) GetUnlockedTiles();

            if (unlockedTiles.Count < 2) return (null, null);

            //Соритруем в обратном порядке, чтобы не заруинить прохождение игроку
            unlockedTiles = unlockedTiles.OrderBy(tile => tile.Content.TileData.Id).Reverse().ToList();

            UITile tile1 = null;
            UITile tile2 = null;

            int tries = 0;

            while (tile2 == null)
            {
                tile1 = unlockedTiles[tries];
                tries++;
                if (tries > unlockedTiles.Count - 1) return (null, null);

                foreach (var tile in unlockedTiles)
                {
                    if (LevelsConfig.GetConfig.MatchingTable[tile1.Content.TileData.Id].ImagesId.Contains(tile.Content.TileData.Id) && tile != tile1)
                    {
                        tile2 = tile;
                        break;
                    }
                }

                //int randIndex = Random.Range(0, unlockedTiles.Count);
                //tile1 = unlockedTiles[randIndex];
                //tile2 = unlockedTiles.Find(tile =>
                //{
                //    foreach (var id in LevelsConfig.GetConfig.MatchingTable[tile1.Content.TileData.Id].ImagesId)
                //    {
                //        if (id == tile.Content.TileData.Id) return true;
                //    }
                //    return false;
                //});
            }

            return (tile1, tile2);
        }

        private void OnFindMatchHandler(TileData prevTile, TileData currentTile)
        {
            //Убираем свечение подсказок
            hintsTimer.Set(15f);
            hintStars.ForEach(star => star.gameObject.SetActive(false));

            //анлочим ближайшие тайлы к тем, что сматчили в последний раз
            UnHideAdjacentTiles(prevTile);
            UnHideAdjacentTiles(currentTile);

            if ((unlockedTiles.Count < 2 || GetMatchingPair() == (null, null)) && unlockedTiles.Count > 0)
            {
                GetMatchingPair();
                Dialog.ShowDialog(dialog => DialogPresets.InitAsSimpleDialog(dialog, "Common.Battle.Deadlock.Title".Localized(), "Common.Battle.Deadlock.Text".Localized(), true, "Common.OK".Localized()),
                    result => GameManager.Instance.StateMachine.SetState(new State_Start()));
            }

            pairsHighlightTimes--;
            if (pairsHighlightTimes > 0)
            {
                HighlightMatchingPair();
            }

            pairsExplodeTimes--;
            if (pairsExplodeTimes > 0)
            {
                ExplodeMatchingPair();
            }
        }

        /// <summary>
        /// Разблокирует соседние плитки, если они больше не залочены
        /// </summary>
        private void UnHideAdjacentTiles(TileData tileData)
        {
            if (unlockedTiles == null) GetUnlockedTiles();

            unlockedTiles.Remove(unlockedTiles.Find(tile => tile.Content.TileData == tileData));

            for (int l = tileData.Layer; l >= 0; l--)
            {
                var layer = UIManager.GetElement<UIBattleField>().GridLayers[l];

                for (int y = Mathf.Max(0, tileData.yPos - 1 + layer.yResizeTileOffset); y <= Mathf.Min(layer.Resizer.Height - 1, tileData.yPos + 1 + layer.yResizeTileOffset); y++)
                {
                    for (int x = Mathf.Max(0, tileData.xPos - 1 + layer.xResizeTileOffset); x <= Mathf.Min(layer.Resizer.Width - 1, tileData.xPos + 1 + layer.xResizeTileOffset); x++)
                    {
                        var tile = (UITile)layer.Tiles[y * layer.Resizer.Width + x];
                        //var tile = (UITile)layer.Tiles.Find((ftile) =>
                        //{
                        //    var uiTile = ftile as UITile;
                        //    return (uiTile.Content.TileData.xPos == x && uiTile.Content.TileData.yPos == y);
                        //});

                        if (tile.Content.gameObject.activeSelf && !State_Battle.IsLocked(tile.Content.TileData, false) && !State_Battle.IsEmpty(tile.Content.TileData))
                        {
                            tile.SetHidden(false);

                            if (!unlockedTiles.Contains(tile))
                                unlockedTiles.Add(tile);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получить список доступных плиток (перебор по всем слоям уровня)
        /// </summary>
        private List<UITile> GetUnlockedTiles()
        {
            unlockedTiles = new List<UITile>();

            foreach (var layer in UIManager.GetElement<UIBattleField>().GridLayers)
            {
                foreach (UITile tile in layer.Tiles)
                {
                    if (tile.Content.gameObject.activeSelf && !State_Battle.IsLocked(tile.Content.TileData, false))
                    {
                        unlockedTiles.Add(tile);
                    }
                }
            }

            return unlockedTiles;
        }
    }
}