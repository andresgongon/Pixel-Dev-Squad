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
        public string binNameText => $"[{shortcutKeyNumber}] {binTitle}";

        [Header("Visual Feedback References (Rol 2 & Rol 3)")]
        public Image binSpriteImage;
        public Image glowEffectImage;
        public Text binLabelText;

        private RectTransform rectTransform;
        private BoxCollider2D binCollider;
        private Coroutine glowRoutine;
        private bool isShowingTriggerHighlight = false;

        // Initializes transform, 2D trigger collider, and hides golden glow at start
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ConfigureTriggerCollider();

            if (glowEffectImage != null)
            {
                glowEffectImage.gameObject.SetActive(false);
            }
        }

        // Configures a 2D BoxCollider trigger so dragged waste items trigger proximity feedback
        private void ConfigureTriggerCollider()
        {
            binCollider = GetComponent<BoxCollider2D>();
            if (binCollider == null)
            {
                binCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            binCollider.isTrigger = true;
            binCollider.size = new Vector2(165f, 225f);
        }

        // Reacts via Unity 2D Physics Trigger when a waste item collider enters the bin zone
        private void OnTriggerEnter2D(Collider2D other)
        {
            WasteItemInteractable waste = other.GetComponent<WasteItemInteractable>();
            if (waste != null && waste.isBeingDragged)
            {
                isShowingTriggerHighlight = true;
                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(1.08f, 1.08f, 1f);
                }
                if (glowEffectImage != null && glowRoutine == null)
                {
                    glowEffectImage.gameObject.SetActive(true);
                    Color c = glowEffectImage.color;
                    c.a = 0.45f;
                    glowEffectImage.color = c;
                }
            }
        }

        // Restores visual state when dragged waste item exits the bin trigger zone
        private void OnTriggerExit2D(Collider2D other)
        {
            WasteItemInteractable waste = other.GetComponent<WasteItemInteractable>();
            if (waste != null && isShowingTriggerHighlight)
            {
                isShowingTriggerHighlight = false;
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                }
                if (glowEffectImage != null && glowRoutine == null)
                {
                    glowEffectImage.gameObject.SetActive(false);
                }
            }
        }

        // Returns canvas position for floating VFX popups above the bin
        public Vector2 GetCanvasPosition()
        {
            return rectTransform != null ? rectTransform.anchoredPosition + new Vector2(110f, -90f) : Vector2.zero;
        }

        // Shakes the bin sideways with red tint on incorrect classification
        public void PlayErrorShake()
        {
            StartCoroutine(AnimateErrorShakeRoutine());
        }

        private IEnumerator AnimateErrorShakeRoutine()
        {
            if (rectTransform == null) yield break;

            Vector2 originalPos = rectTransform.anchoredPosition;
            float duration = 0.35f;
            float elapsed = 0f;

            if (binSpriteImage != null)
            {
                binSpriteImage.color = new Color(1f, 0.5f, 0.5f, 1f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float xOffset = Mathf.Sin(elapsed * 45f) * 7f;
                rectTransform.anchoredPosition = new Vector2(originalPos.x + xOffset, originalPos.y);
                yield return null;
            }

            rectTransform.anchoredPosition = originalPos;

            if (binSpriteImage != null)
            {
                binSpriteImage.color = Color.white;
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
                    GameManager.Instance.EvaluateClassification(this, droppedItem);
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
            isShowingTriggerHighlight = false;
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

            glowRoutine = null;
        }
    }
}
