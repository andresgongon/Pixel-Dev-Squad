using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RioLimpio
{
    public class RecyclingBin : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Bin Configuration")]
        public BinCategory binCategory;
        public string binTitle = "APROVECHABLES";
        public int shortcutKeyNumber = 1;

        [Header("Visual Feedback References (Rol 2 & Rol 3)")]
        public Image binSpriteImage;
        public Image glowEffectImage;
        public Text binLabelText;

        private RectTransform rectTransform;
        private Coroutine glowRoutine;

        // Initializes transform reference and hides the golden glow at the start
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (glowEffectImage != null)
            {
                glowEffectImage.gameObject.SetActive(false);
            }
        }

        // Handles Drag & Drop when the player releases a waste item over this bin
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                WasteItemInteractable droppedItem = eventData.pointerDrag.GetComponent<WasteItemInteractable>();

                // Verify that the dragged object is a valid waste item before validating
                if (droppedItem != null && GameManager.Instance != null)
                {
                    droppedItem.MarkDroppedOnBin();
                    GameManager.Instance.EvaluateClassification(this);
                }
            }
        }

        // Handles Click-to-Click / Tap-to-Tap interaction when the player clicks this bin
        public void OnPointerClick(PointerEventData eventData)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TryClassifyByBinClick(this);
            }
        }

        // Enlarges the bin slightly on hover to reinforce target area visibility (WCAG 2.5.8)
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = new Vector3(1.06f, 1.06f, 1f);
            }
        }

        // Restores normal scale when the cursor leaves the bin area
        public void OnPointerExit(PointerEventData eventData)
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        // Triggers the golden glow and scale bounce animation on correct classification
        public void PlaySuccessGlow()
        {
            if (glowRoutine != null)
            {
                StopCoroutine(glowRoutine);
            }
            glowRoutine = StartCoroutine(AnimateGlowFeedback());
        }

        // Coroutine that fades the golden glow in and out over a short duration
        private IEnumerator AnimateGlowFeedback()
        {
            if (glowEffectImage != null)
            {
                glowEffectImage.gameObject.SetActive(true);
            }

            float duration = 0.75f;
            float elapsed = 0f;

            // Animate the glow opacity and bin bounce while elapsed time is below duration
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float bounce = 1f + 0.1f * Mathf.Sin(progress * Mathf.PI);

                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(bounce, bounce, 1f);
                }

                if (glowEffectImage != null)
                {
                    Color c = glowEffectImage.color;
                    c.a = Mathf.Sin(progress * Mathf.PI);
                    glowEffectImage.color = c;
                }

                yield return null;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }

            if (glowEffectImage != null)
            {
                glowEffectImage.gameObject.SetActive(false);
            }
        }
    }
}
