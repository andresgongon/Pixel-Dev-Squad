using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RioLimpio
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class WasteItemInteractable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("UI References")]
        public Image itemIconImage;
        public Text itemLabelText;
        public Outline selectionOutline;

        [Header("Current Item State")]
        public WasteItemData currentData;
        public bool isSelectedForClickMode = false;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas parentCanvas;
        private Vector2 homeAnchoredPosition;
        private bool wasDroppedOnBin = false;

        // Caches component references when the object initializes
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            parentCanvas = GetComponentInParent<Canvas>();

            if (itemIconImage == null)
            {
                itemIconImage = GetComponent<Image>();
            }

            if (selectionOutline == null)
            {
                selectionOutline = GetComponent<Outline>();
            }
        }

        // Stores the default spawn position so the item can return if dropped outside a bin
        private void Start()
        {
            homeAnchoredPosition = rectTransform.anchoredPosition;
        }

        // Assigns new waste item data and resets the position and selection state
        public void SetupWasteItem(WasteItemData data, Vector2 spawnPosition)
        {
            currentData = data;
            homeAnchoredPosition = spawnPosition;
            rectTransform.anchoredPosition = spawnPosition;
            rectTransform.localScale = Vector3.one;

            isSelectedForClickMode = false;
            wasDroppedOnBin = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }

            // Update the sprite and accessible text label if valid data was provided
            if (currentData != null)
            {
                if (itemIconImage != null)
                {
                    itemIconImage.sprite = currentData.itemSprite;
                    itemIconImage.preserveAspect = true;
                }

                if (itemLabelText != null)
                {
                    itemLabelText.text = currentData.displayName.ToUpper();
                }
            }
        }

        // Handles Click-to-Click / Tap-to-Tap selection for motor accessibility (WCAG 2.2)
        public void OnPointerClick(PointerEventData eventData)
        {
            // Ignore click if the player was dragging the item
            if (eventData.dragging)
            {
                return;
            }

            isSelectedForClickMode = !isSelectedForClickMode;

            // Highlight the waste item when selected so the player knows to click a bin next
            if (isSelectedForClickMode)
            {
                rectTransform.localScale = new Vector3(1.12f, 1.12f, 1f);
                if (selectionOutline != null)
                {
                    selectionOutline.enabled = true;
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySelectSound();
                }
            }
            else
            {
                rectTransform.localScale = Vector3.one;
                if (selectionOutline != null)
                {
                    selectionOutline.enabled = false;
                }
            }
        }

        // Prepares the waste item when the player starts dragging it
        public void OnBeginDrag(PointerEventData eventData)
        {
            wasDroppedOnBin = false;
            isSelectedForClickMode = false;
            canvasGroup.alpha = 0.85f;
            canvasGroup.blocksRaycasts = false;
            rectTransform.localScale = new Vector3(1.1f, 1.1f, 1f);

            if (selectionOutline != null)
            {
                selectionOutline.enabled = true;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectSound();
            }
        }

        // Moves the waste item along with the mouse cursor or touch input
        public void OnDrag(PointerEventData eventData)
        {
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }

            // Divide delta by canvas scaleFactor so movement matches screen resolution
            if (parentCanvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
            }
            else
            {
                rectTransform.anchoredPosition += eventData.delta;
            }
        }

        // Restores raycast blocking and returns the item to its home position if not dropped on a bin
        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = Vector3.one;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }

            // If the item was released outside any recycling bin, snap it back to the center
            if (!wasDroppedOnBin)
            {
                ResetToHomePosition();
            }
        }

        // Marks the item as received by a bin and resets its selection state
        public void MarkDroppedOnBin()
        {
            wasDroppedOnBin = true;
            isSelectedForClickMode = false;
            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }
        }

        // Returns the waste item back to its presentation position
        public void ResetToHomePosition()
        {
            rectTransform.anchoredPosition = homeAnchoredPosition;
            rectTransform.localScale = Vector3.one;
            isSelectedForClickMode = false;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }
        }
    }
}
