using GameCore.Tools;
using System.Collections;
using System.Collections.Generic;
using Template;
using Template.Systems.Dialogs;
using Template.Tools;
using Template.Tools.Pool;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIBattleField : UIElementAnimated
    {
        [SerializeField] private UIGrid gridPrefab;

        public PoolList<UIGrid> GridLayers { get; private set; } = new PoolList<UIGrid>();

        [SerializeField] private TileContent contentPrefab;

        [SerializeField] private ParticleSystem matchParticles;

        private int prevTileId = 0;
        private int prevLayer = 0;

        private Vector2 defaultSize;

        public event System.Action<TileData, TileData> OnFindMatch;

        private int animsCount = 0;
        public bool isMatchingAnimPlay => animsCount != 0;

        public bool isBlockSelection = false;

        private void Awake()
        {
            defaultSize = GetComponent<RectTransform>().sizeDelta;
        }

        private void OnDisable()
        {
            foreach (var grid in GridLayers)
            {
                grid.ElementSelectedEvent -= OnSelectTile;
                grid.ElementDeselectedEvent -= OnDeselectTile;
            }

            GridLayers.Clear();
            animsCount = 0;
            isBlockSelection = false;
        }

        public void SetLevel(FieldData[][,] levelData)
        {
            TileContent.SpritesId.Clear();
            GetComponent<RectTransform>().sizeDelta = defaultSize;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            prevTileId = 0;

            for (int layer = 0; layer < levelData.Length; layer++)
            {
                GridData gridData = LevelsConfig.GetConfig.levelList[State_Battle.level].grids[layer];
                var newGrid = GridLayers.Add(gridPrefab, transform);

                newGrid.ElementSelectedEvent += OnSelectTile;
                newGrid.ElementDeselectedEvent += OnDeselectTile;

                newGrid.Resizer.Width = gridData.Width;
                newGrid.Resizer.Height = gridData.Height;
                newGrid.Resizer.Resize();

                //Layer offset
                if (levelData.Length > 1)
                {
                    if (layer == 0)
                    {
                        var rect = GetComponent<RectTransform>();
                        var offset = new Vector2(newGrid.Resizer.TileSize.x / -12.5f, 0f/*newGrid.Resizer.TileSize.y / 17f*/) * (levelData.Length - 1);
                        rect.sizeDelta = defaultSize + offset;
                        rect.anchoredPosition = -offset / 2f;
                    }

                    newGrid.Resizer.Resize();
                    newGrid.GetComponent<RectTransform>().anchoredPosition = new Vector2(newGrid.Resizer.TileSize.x / -10.5f, newGrid.Resizer.TileSize.y / 15f) * gridData.Layer;
                }

                List<TileContent> content = new List<TileContent>();

                for (int i = 0; i <= (gridData.Height * gridData.Width - gridData.Width); i += gridData.Width)
                {
                    for (int j = 0; j < gridData.Width; j++)
                    {
                        TileContent tile = Instantiate(contentPrefab);

                        int spriteId = levelData[layer][i / gridData.Width, j].SpriteId;

                        tile.SetOffset(gridData.Fields[i + j].OffsetData, newGrid.Resizer.TileSize.x / 2.5f, newGrid.Resizer.TileSize.y / 2.5f);

                        tile.Set(new TileData()
                        {
                            Id = spriteId,
                            Layer = gridData.Layer,
                            xPos = j,
                            yPos = i / gridData.Width
                        });

                        tile.gameObject.SetActive(spriteId != 0);
                        content.Add(tile);
                    }
                }

                newGrid.Set(content);
                newGrid.gameObject.SetActive(true);
            }
        }

        public void PlayBlockAnims((int l, int y, int x)[] tilesCoords, bool isHorizontal = true)
        {
            if (!PlayerPrefs.HasKey("ShowBlockTilesAnim"))
            {
                Dialog.ShowDialog(dialog => MahjongDialogPresets.InitAsBlockedTiles(dialog), result => { });
                PlayerPrefs.SetInt("ShowBlockTilesAnim", 1);
                PlayerPrefs.Save();
            }

            for (int i = 0; i < tilesCoords.Length; i++)
            {
                var tile = tilesCoords[i];

                GridLayers[tile.l].PlayBlockTileAnim((tile.y + GridLayers[tile.l].yResizeTileOffset) * GridLayers[tile.l].Resizer.Width + tile.x + GridLayers[tile.l].xResizeTileOffset, isHorizontal, i == tilesCoords.Length - 1);
            }
        }

        private void OnSelectTile(UIGridPickElement tile)
        {
            if (isBlockSelection)
            {
                tile.SetSelected(false);
                return;
            }

            var content = ((UITile)tile).Content;

            if (prevTileId == 0)
            {
                prevTileId = content.TileData.Id;
                prevLayer = content.TileData.Layer;
                return;
            }

            foreach (var id in LevelsConfig.GetConfig.MatchingTable[prevTileId].ImagesId)
            {
                if (id == content.TileData.Id)
                {
                    List<UITile> tiles = new List<UITile>();

                    foreach (var t in GridLayers[prevLayer].Selected)
                    {
                        tiles.Add((UITile)t);
                        t.SetSelected(false);
                    }

                    foreach (var t in GridLayers[content.TileData.Layer].Selected)
                    {
                        tiles.Add((UITile)t);
                        t.SetSelected(false);
                    }

                    OnFindMatch?.Invoke(tiles[0].Content.TileData, tiles[1].Content.TileData);
                    prevTileId = 0;

                    PlayMatchAnim(tiles);
                    return;
                }
            }

            //prevTileId = 0;
            GridLayers[prevLayer].CancelSelection();
            //GridLayers[content.TileData.Layer].CancelSelection();
            tile.SetSelected(true);
            prevTileId = content.TileData.Id;
            prevLayer = content.TileData.Layer;

            #region OLD
            //if (prevTile == TileCategory.Empty)
            //{
            //    prevTile = content.TileCategory;
            //    return;
            //}

            //if(prevTile != content.TileCategory)
            //{
            //    prevTile = TileCategory.Empty;
            //    grid.CancelSelection();
            //    return;
            //}

            //foreach(var t in grid.Selected)
            //{
            //    t.SetSelected(false);
            //    t.Content.SetActive(false);
            //}

            //OnFindMatch?.Invoke();
            //prevTile = TileCategory.Empty;
            #endregion
        }

        private void OnDeselectTile(UIGridPickElement tile)
        {
            prevTileId = 0;
            //prevTile = TileCategory.Empty;
        }

        public void MatchTiles(List<UITile> tiles)
        {
            GridLayers[prevLayer].CancelSelection();

            isBlockSelection = true;

            PlayMatchAnim(tiles, () =>
            {
                isBlockSelection = false;
                OnFindMatch?.Invoke(tiles[0].Content.TileData, tiles[1].Content.TileData);
                prevTileId = 0;
            });
        }

        private void PlayMatchAnim(List<UITile> tiles, System.Action onEndAnim = null)
        {
            animsCount++;

            List<RectTransform> rectTransforms = new List<RectTransform>();

            //Extract content GO from grid
            foreach (var tile in tiles)
            {
                tile.Content.transform.SetParent(transform);
                tile.Highlight(false);
                rectTransforms.Add(tile.Content.GetComponent<RectTransform>());
            }

            bool isLeftSide = (rectTransforms[1].anchoredPosition.x - rectTransforms[0].anchoredPosition.x) >= 0;

            float targetY = (rectTransforms[0].anchoredPosition.y + rectTransforms[1].anchoredPosition.y) / 2f;
            float targetX = (rectTransforms[0].anchoredPosition.x + rectTransforms[1].anchoredPosition.x) / 2f;

            for (int i = 0; i < tiles.Count; i++)
            {
                //Calculate target one level coords 
                float xOffset = GetComponent<RectTransform>().rect.width / 4f;
                Vector3 targetVector = new Vector2(rectTransforms[i].anchoredPosition.x + ((i == 0 ? xOffset : -xOffset) * (isLeftSide ? -1f : 1f)), targetY);

                //Add new tweener
                BroTweener tweener = tiles[i].Content.gameObject.AddComponent<BroTweener>();

                //1 Vertical + Horizontal move one level
                tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenPosition)
                {
                    fromVector3 = rectTransforms[i].anchoredPosition,
                    toVector3 = targetVector,
                    rightEdge = 0.5f
                });

                //2 Horizontal move
                tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenPosition)
                {
                    fromVector3 = targetVector,
                    toVector3 = new Vector3(targetX, targetY),
                    leftEdge = 0.5f
                });

                //On end Events
                tweener.SetOnFinish((t) => tweener.gameObject.SetActive(false));

                if (i == 0) tweener.AddOnFinish((t) => StartCoroutine(ParticlesAnim(onEndAnim)));
                tweener.AddOnFinish((t) =>
                {
                    matchParticles.GetComponent<RectTransform>().anchoredPosition = new Vector2(targetX, targetY);
                    matchParticles.Play();

                    AudioManager.Play(new PlaySettings("Match") { relVolume = 0.65f });
                });
                tweener.AddOnFinish((t) => Destroy(tweener));

                tweener.Duration = 0.5f;
                tweener.Play();

                AudioManager.Play(new PlaySettings("MatchMove") { relVolume = 0.75f });
            }
        }

        private IEnumerator ParticlesAnim(System.Action onEndAnim = null)
        {
            yield return null;

            while (matchParticles.IsAlive())
            {
                yield return null;
            }

            onEndAnim?.Invoke();
            animsCount--;

            ResizeIfNeeded();
            isBlockSelection = true;

            yield return new WaitWhile(() => GridLayers[0].isResizeAnimation);

            isBlockSelection = false;

            yield break;
        }

        public void ResizeIfNeeded()
        {
            var downGrid = GridLayers[0];

            if (downGrid.Resizer.Width == 4 && downGrid.Resizer.Width >= downGrid.Resizer.Height) return;

            ///Вычитаем пустые линии чтобы сразу не ресайзить это
            var emptyLines = LevelsConfig.GetConfig.levelList[State_Battle.level].emptyLinesCircle;

            int emptyLineUp = -emptyLines;
            int emptyLineDown = -emptyLines;
            int emptyLineLeft = -emptyLines;
            int emptyLineRight = -emptyLines;

            var tilesToRemove = new List<UIGridPickElement>();

            //Count top empty lines
            foreach (var tile in downGrid.Tiles)
            {
                if (((UITile)tile).Content.isActiveAndEnabled)
                {
                    tilesToRemove.Clear();
                    //if (emptyLineUp == 0) return;
                    goto Down;
                }

                tilesToRemove.Add(tile);
                if (tilesToRemove.Count == downGrid.Resizer.Width)
                {
                    tilesToRemove.Clear();
                    emptyLineUp++;
                }
            }

        Down:
            //Count down empty lines
            for (int i = downGrid.Tiles.Count - 1; i > 0; i--)
            {
                var tile = downGrid.Tiles[i];

                if (((UITile)tile).Content.isActiveAndEnabled)
                {
                    tilesToRemove.Clear();
                    //if (emptyLineDown == 0) return;
                    goto Left;
                }

                tilesToRemove.Add(tile);
                if (tilesToRemove.Count == downGrid.Resizer.Width)
                {
                    tilesToRemove.Clear();
                    emptyLineDown++;
                }
            }

        Left:
            //Count left empty lines
            for (int x = 0; x < downGrid.Resizer.Width; x++)
            {
                for (int y = 0; y < downGrid.Resizer.Height; y++)
                {
                    var tile = downGrid.Tiles[y * downGrid.Resizer.Width + x];

                    if (((UITile)tile).Content.isActiveAndEnabled)
                    {
                        tilesToRemove.Clear();
                        //if (emptyLineLeft == 0) return;
                        goto Right;
                    }

                    tilesToRemove.Add(tile);
                    if (tilesToRemove.Count == downGrid.Resizer.Height)
                    {
                        tilesToRemove.Clear();
                        emptyLineLeft++;
                    }
                }
            }

        Right:
            //Count right empty lines
            for (int x = downGrid.Resizer.Width - 1; x > 0; x--)
            {
                for (int y = 0; y < downGrid.Resizer.Height; y++)
                {
                    var tile = downGrid.Tiles[y * downGrid.Resizer.Width + x];

                    if (((UITile)tile).Content.isActiveAndEnabled)
                    {
                        tilesToRemove.Clear();
                        //if (emptyLineRight == 0) return;
                        goto Resize;
                    }

                    tilesToRemove.Add(tile);
                    if (tilesToRemove.Count == downGrid.Resizer.Height)
                    {
                        tilesToRemove.Clear();
                        emptyLineRight++;
                    }
                }
            }


        Resize:

            //Если новый ресайз меньше 5 плиток
            if (downGrid.Resizer.Width - emptyLineLeft - emptyLineRight < 4)
            {
                if (emptyLineLeft + emptyLineRight > 2 && downGrid.Resizer.Width > 5)
                {
                    emptyLineLeft = 1;
                    emptyLineRight = 1;
                }
                else if (downGrid.Resizer.Width < downGrid.Resizer.Height)
                {
                    emptyLineLeft = 0;
                    emptyLineRight = 0;
                }
                else return;
            }

            if (downGrid.Resizer.Width - emptyLineLeft - emptyLineRight >= downGrid.Resizer.Height)
            {
                emptyLineDown = 0;
                emptyLineUp = 0;
            }

            //Если нечего ресайзить выходим
            if (emptyLineDown + emptyLineUp + emptyLineRight + emptyLineLeft <= 0) return;

            //Мы не вышли из метода, значит нужно ресайзить
            for (int layer = 0; layer < GridLayers.Count; layer++)
            {
                var grid = GridLayers[layer];


                //if (grid.Resizer.Height - emptyLineUp - emptyLineDown < 5 && (emptyLineDown + emptyLineUp > 2 && grid.Resizer.Height > 6))
                //{
                //    emptyLineDown = 1;
                //    emptyLineUp = 1;
                //}    

                if (!grid.isResizeAnimation)
                    grid.DisableTileLinesAnimated(emptyLineUp, emptyLineDown, emptyLineLeft, emptyLineRight, layer);

                //grid.Resizer.Width -= emptyLineLeft + emptyLineRight;
                //grid.Resizer.Height -= emptyLineUp + emptyLineDown;
                //grid.RemoveDisabledTiles();

                //grid.Resizer.Resize(true);
            }
        }
    }
}
