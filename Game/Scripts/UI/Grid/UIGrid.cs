using GameCore.UI;
using RuntimePresets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Template.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Tools
{
    public class UIGrid : UIGridPick
    {
        public GridResizer Resizer;

        public new UITile ElementPrefab;

        public List<UIGridPickElement> Tiles => _elements;

        /// <summary>
        /// Смещение координат плиток по оси X из-за ресайзинга
        /// </summary>
        public int xResizeTileOffset { get; private set; }
        /// <summary>
        /// Смещение координат плиток по оси Y из-за ресайзинга
        /// </summary>
        public int yResizeTileOffset { get; private set; }

        /// <summary>
        /// Анимация изменения и сдвига игрового поля
        /// </summary>
        public bool isResizeAnimation { get; private set; }

        public void Set(List<TileContent> content)
        {
            isResizeAnimation = false;
            xResizeTileOffset = 0;
            yResizeTileOffset = 0;

            Clear();

            var i = 0;
            foreach (var c in content)
            {
                var element = Instantiate(ElementPrefab, Grid.transform);
                element.gameObject.SetActive(true);
                element.SetContent(i++, c);
                element.ElementPressedEvent += OnElementPressed;
                _elements.Add(element);
            }
        }

        public void RemoveDisabledTiles()
        {
            Tiles.RemoveAll(tile => !tile.gameObject.activeSelf);
        }

        public void DisableTileLines(int up, int down, int left, int right)
        {
            DisableUpTileLines(up);
            DisableDownTileLines(down);
            DisableLeftTileLines(left);
            DisableRightTileLines(right);
        }

        public void DisableTileLinesAnimated(int up, int down, int left, int right, int layer)
        {
            isResizeAnimation = true;

            //Запускаем анимацию ресайза
            Resizer.Width -= left + right;
            Resizer.Height -= up + down;
            Resizer.Resize(true);

            var targetAnchored = new Vector2(Resizer.TileSize.x / -10.5f, Resizer.TileSize.y / 15f) * layer;

            //Анимация сдвига
            StartCoroutine(ChangePaddingAnimated(left - right, up - down, layer > 0 ? targetAnchored : Vector2.zero, () =>
           {
                //Возвращаем значения обратно, чтобы правильно убрать лишние плитки
                Resizer.Width += left + right;
               Resizer.Height += up + down;

               DisableUpTileLines(up);
               DisableDownTileLines(down);
               DisableLeftTileLines(left);
               DisableRightTileLines(right);

                //Обнуляем смещение и возвращаем размеры в правильное значение
                Grid.padding.top = 0;
               Grid.padding.left = 0;
               Resizer.Width -= left + right;
               Resizer.Height -= up + down;

               RemoveDisabledTiles();

                //Чтобы плитки не съезжали при анимации ресайза
                Resizer.SetConstraintWidth();
               isResizeAnimation = false;
           }));

        }

        public IEnumerator ChangePaddingAnimated(int xOffset, int yOffset, Vector2 targetAnchored, System.Action OnEndAnim = null)
        {
            float leftTime = 0f;
            float targetTime = 1f;

            float startTopPadding = Grid.padding.top;
            float startLeftPadding = Grid.padding.left;

            var rect = GetComponent<RectTransform>();
            var startAnchored = rect.anchoredPosition;

            while (leftTime < targetTime + Time.deltaTime)
            {
                if (yOffset != 0)
                    Grid.padding.top = (int)Mathf.Lerp(startTopPadding, startTopPadding - yOffset * (Grid.cellSize.y + Grid.spacing.y), leftTime / targetTime);
                if (xOffset != 0)
                    Grid.padding.left = (int)Mathf.Lerp(startLeftPadding, startLeftPadding - xOffset * (Grid.cellSize.x + Grid.spacing.x), leftTime / targetTime);

                if (targetAnchored != Vector2.zero)
                    rect.anchoredPosition = Vector2.Lerp(startAnchored, targetAnchored, leftTime / targetTime);

                LayoutRebuilder.MarkLayoutForRebuild(rect);
                yield return null;
                leftTime += Time.deltaTime;
            }

            OnEndAnim?.Invoke();
            yield break;
        }

        public void DisableUpTileLines(int count)
        {
            for (int i = 0; i < Resizer.Width * count; i++)
            {
                Tiles[i].gameObject.SetActive(false);
            }

            yResizeTileOffset -= count;
        }

        public void DisableDownTileLines(int count)
        {
            for (int i = Tiles.Count - 1; i > Tiles.Count - Resizer.Width * count - 1; i--)
            {
                Tiles[i].gameObject.SetActive(false);
            }
        }

        public void DisableLeftTileLines(int count)
        {
            for (int x = 0; x < count; x++)
            {
                for (int y = 0; y < Resizer.Height; y++)
                {
                    Tiles[y * Resizer.Width + x].gameObject.SetActive(false);
                }
            }

            xResizeTileOffset -= count;
        }

        public void DisableRightTileLines(int count)
        {
            for (int x = Resizer.Width - 1; x > Resizer.Width - count - 1; x--)
            {
                for (int y = 0; y < Resizer.Height; y++)
                {
                    Tiles[y * Resizer.Width + x].gameObject.SetActive(false);
                }
            }
        }

        protected override void OnElementPressed(UIGridPickElement element)
        {
            AudioManager.Play(new PlaySettings("TileClick"));

            var isElementAlreadySelected = Selected.Contains(element);
            if (isElementAlreadySelected)
            {
                DeselectElement(element);
                return;
            }

            if (State_Battle.IsLocked(((UITile)element).Content.TileData, true))
            {
                AudioManager.Play(new PlaySettings("BlockedTilesClick"));
                //TODO: Lock Anim
                return;
            }

            // Isn't already selected
            if (!IsMultiSelect && IsElementSelected)
            {
                DeselectElement(Selected.Single());
            }
            SelectElement(element);
        }

        public void PlayBlockTileAnim(int tileID, bool isHorizontal, bool isInitiator)
        {
            var content = ((UITile)_elements[tileID]).Content;

            var tweener = content.GetComponent<BroTweener>();

            if (tweener == null)
            {
                tweener = content.gameObject.AddComponent<BroTweener>();
            }

            var rect = content.GetComponent<RectTransform>();

            tweener.Duration = 0.75f;
            tweener.Delay = isInitiator ? 0f : 0.15f;
            tweener.PlayOnEnable = false;
            tweener.OnFinish.RemoveAllListeners();

            tweener.Curve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.125f, -1f),
                new Keyframe(0.35f, 1f),
                new Keyframe(0.5f, -1f),
                new Keyframe(0.75f, 1f),
                new Keyframe(1f, 0f)
            });

            tweener.TweenDataList.Clear();
            //tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenPosition)
            //{
            //    fromVector3 = rect.anchoredPosition,
            //    toVector3 = rect.anchoredPosition + (isHorizontal ? new Vector2(5f, 0f) : new Vector2(0f, 5f))
            //});

            tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenRotation)
            {
                fromVector3 = Vector3.zero,
                toVector3 = Vector3.forward * 5f
            });

            tweener.AddTween(new Bro.TweenData(BroTweener.TypeEnum.TweenColor)
            {
                fromColor = Color.white,
                toColor = Color.red,
                Override = true,
                overrideData = new Bro.TweenData.OverrideData()
                {
                    curve = new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(0.5f, 1f),
                        new Keyframe(1f, 0f)
                    })
                }
            });



            tweener.Play();
        }
    }
}