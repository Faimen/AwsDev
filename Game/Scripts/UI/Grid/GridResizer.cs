using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Tools
{
    public class GridResizer : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup grid;

        [SerializeField] private RectTransform tilePrefab;
        [SerializeField] private RectTransform parentTransform;

        [Tooltip("Can X resize?")]
        [SerializeField] private bool xResize;
        [Tooltip("Can Y resize?")]
        [SerializeField] private bool yResize;

        public int Width;
        public int Height;

        /// <summary>
        /// Размер плитки
        /// </summary>
        public Vector2 TileSize => grid.cellSize;

        private Vector2 defaultSize;
        private Vector2 defaultSpacing;

        private bool isResize = false;

        private void OnEnable()
        {
            if (!isResize) Resize();

            //Чтобы плитки не съезжали при анимации ресайза
            SetConstraintWidth();
            ResetPadding();
        }

        public void SetConstraintWidth()
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Width;
        }

        private void OnDisable()
        {
            isResize = false;
        }

        /// <summary>
        /// Manual resize
        /// </summary>
        public void Resize(bool isAnimated = false)
        {
            isResize = true;

            var targetCellSize = grid.cellSize;
            var targetSpacing = grid.spacing;

            if (defaultSize == Vector2.zero)
            {
                defaultSize = grid.cellSize;
                defaultSpacing = grid.spacing;
            }
            else
            {
                targetCellSize = defaultSize;
                targetSpacing = defaultSpacing;
            }

            if (parentTransform == null)
                parentTransform = transform.parent.GetComponentInParent<RectTransform>();

            var gridRect = grid.GetComponent<RectTransform>();
            Vector2 newGridSize = gridRect.sizeDelta;

            if (xResize)
            {
                newGridSize.x = Width * (defaultSize.x + defaultSpacing.x) - defaultSpacing.x;
            }
            if (yResize)
            {
                newGridSize.y = Height * (defaultSize.y + defaultSpacing.y) - defaultSpacing.y;
            }

            //If we got out from parent sizes 
            if (newGridSize.x > parentTransform.rect.width || newGridSize.y > parentTransform.rect.height)
            {
                float resizeRatio = Mathf.Min(parentTransform.rect.width / newGridSize.x, parentTransform.rect.height / newGridSize.y);

                targetCellSize *= resizeRatio;
                targetSpacing *= resizeRatio;
                newGridSize *= resizeRatio;
            }

            if (!isAnimated)
            {
                gridRect.sizeDelta = newGridSize;
                grid.cellSize = targetCellSize;
                grid.spacing = targetSpacing;
            }
            else StartCoroutine(AnimateResize(targetCellSize, targetSpacing, newGridSize));
        }

        private IEnumerator AnimateResize(Vector2 targetCellSize, Vector2 targetSpacing, Vector2 targetSizeDelta)
        {
            float leftTime = 0f;
            float targetTime = 1f;

            var gridRect = grid.GetComponent<RectTransform>();

            var startCellSize = grid.cellSize;
            var startSpacing = grid.spacing;
            var startSizeDelta = gridRect.sizeDelta;

            while (leftTime < targetTime + Time.deltaTime)
            {
                gridRect.sizeDelta = Vector2.Lerp(startSizeDelta, targetSizeDelta, leftTime / targetTime);
                grid.cellSize = Vector2.Lerp(startCellSize, targetCellSize, leftTime / targetTime);
                grid.spacing = Vector2.Lerp(startSpacing, targetSpacing, leftTime / targetTime);
                yield return null;
                leftTime += Time.deltaTime;
            }

            yield break;
        }

        public void ResetPadding()
        {
            grid.padding = new RectOffset(0, 0, 0, 0);
        }
    }
}