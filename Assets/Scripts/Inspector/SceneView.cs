using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace NeoEditor.Inspector
{
    public class SceneView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform rectTransform;
        public RawImage image;

        private bool isPointerHover;
        private Vector2 prevMousePos;

        public void OnDrag(PointerEventData eventData)
        {
            NeoEditor editor = NeoEditor.Instance;

            //editor.CopyCamObj.transform.position -= (Vector3)scaledMousePosition;
            //Vector3 textureToWorld = new Vector3(
            //    -2 * scaledMousePosition.x,
            //    0,
            //    -2 * scaledMousePosition.y
            //);
            //textureToWorld;
        }

        void Update()
        {
            NeoEditor editor = NeoEditor.Instance;

            if (isPointerHover)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)) { }

                if (Input.GetMouseButton(2))
                {
                    Vector2 delta =
                        GetWorldPoint(Input.mousePosition) - GetWorldPoint(prevMousePos);
                    editor.mainCamera.transform.position -= (Vector3)delta;
                }

                Vector2 mouseScrollDelta = RDInput.mouseScrollDelta;
                if (Mathf.Abs(mouseScrollDelta.y) > 0.05f)
                    ZoomCamera(mouseScrollDelta.y, !Persistence.editorUseLegacyZoom);
            }
            prevMousePos = Input.mousePosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerHover = false;
        }

        public void ZoomCamera(float delta, bool anchorAtPointer = true)
        {
            NeoEditor editor = NeoEditor.Instance;
            float value = editor.camUserSizeTarget - delta * editor.scrollSpeed;
            editor.camUserSizeTarget = Mathf.Clamp(value, 0.5f, 15f);
            if (!anchorAtPointer)
            {
                DOTween
                    .To(
                        () => editor.camUserSize,
                        delegate(float x)
                        {
                            editor.camUserSize = x;
                        },
                        editor.camUserSizeTarget,
                        0.1f
                    )
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(isIndependentUpdate: true);
                return;
            }

            float transitionValue = 0f;
            Vector3 startMousePos = Input.mousePosition;
            float startSizeMultiplier = editor.camUserSize;
            editor.anchorZoomTween?.Kill();

            editor.anchorZoomTween = DOTween
                .To(() => transitionValue, UpdateZoom, 1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(isIndependentUpdate: true);
            void UpdateZoom(float progress)
            {
                Vector3 vector = GetWorldPoint(startMousePos);
                transitionValue = progress;
                editor.camUserSize =
                    (editor.camUserSizeTarget - startSizeMultiplier) * progress
                    + startSizeMultiplier;
                editor.mainCamera.orthographicSize = 5f * editor.camUserSize;
                Transform transform = editor.mainCamera.transform;
                Vector3 position = transform.position;
                position += vector - (Vector3)GetWorldPoint(startMousePos);
                startMousePos = Input.mousePosition;
                transform.position = position;
            }
        }

        private Vector2 GetWorldPoint(Vector2 mousePosition)
        {
            NeoEditor editor = NeoEditor.Instance;
            Vector2 localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                mousePosition,
                null,
                out localPosition
            );
            Main.Entry.Logger.Log(
                (
                    editor.mainCamera.ViewportToWorldPoint(localPosition) / rectTransform.rect.size
                ).ToString()
            );
            return editor.mainCamera.ViewportToWorldPoint(localPosition) / rectTransform.rect.size;
        }
    }
}
