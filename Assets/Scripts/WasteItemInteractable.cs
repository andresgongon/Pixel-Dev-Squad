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
        public bool isBeingDragged = false;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas parentCanvas;
        private Vector2 homeAnchoredPosition;
        private bool wasDroppedOnBin = false;
        private WasteInspectionMachine inspectionMachine;
        private BoxCollider2D physicsCollider;
        private Rigidbody2D physicsBody;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            parentCanvas = GetComponentInParent<Canvas>();
            inspectionMachine = GetComponent<WasteInspectionMachine>();

            ConfigurePhysicsCollider();

            if (itemIconImage == null)
            {
                itemIconImage = GetComponent<Image>();
            }

            if (selectionOutline == null)
            {
                selectionOutline = GetComponent<Outline>();
            }
        }

        // Ensures kinematic 2D physics collider is attached so OnTriggerEnter2D fires over bins
        private void ConfigurePhysicsCollider()
        {
            physicsBody = GetComponent<Rigidbody2D>();
            if (physicsBody == null)
            {
                physicsBody = gameObject.AddComponent<Rigidbody2D>();
            }
            physicsBody.bodyType = RigidbodyType2D.Kinematic;
            physicsBody.gravityScale = 0f;
            physicsBody.useFullKinematicContacts = true;

            physicsCollider = GetComponent<BoxCollider2D>();
            if (physicsCollider == null)
            {
                physicsCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            physicsCollider.isTrigger = true;
            physicsCollider.size = new Vector2(115f, 115f);
        }

        private void Start()
        {
            homeAnchoredPosition = rectTransform.anchoredPosition;
        }

        // Sets up data, sprite, label, and home position on the inspection tray
        public void SetupWasteItem(WasteItemData data, Vector2 spawnPosition, float? customTimeLimit = null)
        {
            currentData = data;
            homeAnchoredPosition = spawnPosition;

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            rectTransform.anchoredPosition = spawnPosition;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            isBeingDragged = false;
            isSelectedForClickMode = false;
            wasDroppedOnBin = false;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }

            if (currentData != null)
            {
                if (itemIconImage != null)
                {
                    itemIconImage.sprite = currentData.itemSprite;
                    itemIconImage.preserveAspect = true;
                    itemIconImage.color = Color.white;
                }

                if (itemLabelText != null)
                {
                    itemLabelText.text = currentData.displayName.ToUpper();
                }
            }

            if (inspectionMachine == null)
            {
                inspectionMachine = GetComponent<WasteInspectionMachine>();
            }
            if (inspectionMachine != null)
            {
                inspectionMachine.StartInspectionCycle(customTimeLimit);
            }
        }

        // Click-to-Click selection (WCAG 2.2 accessibility)
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;

            isSelectedForClickMode = !isSelectedForClickMode;

            if (isSelectedForClickMode)
            {
                rectTransform.localScale = new Vector3(1.12f, 1.12f, 1f);
                if (selectionOutline != null)
                {
                    selectionOutline.enabled = true;
                    selectionOutline.effectColor = new Color(0.2f, 0.9f, 0.3f, 1f);
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

        // Begins dragging the item from the tray
        public void OnBeginDrag(PointerEventData eventData)
        {
            isBeingDragged = true;
            wasDroppedOnBin = false;
            isSelectedForClickMode = false;
            canvasGroup.alpha = 0.88f;
            canvasGroup.blocksRaycasts = false;
            rectTransform.localScale = new Vector3(1.12f, 1.12f, 1f);

            // Bring to front while dragging
            transform.SetAsLastSibling();

            if (selectionOutline != null)
            {
                selectionOutline.enabled = true;
                selectionOutline.effectColor = new Color(1f, 0.9f, 0.2f, 1f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectSound();
            }
        }

        // Moves the waste item with pointer
        public void OnDrag(PointerEventData eventData)
        {
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }

            if (parentCanvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
            }
            else
            {
                rectTransform.anchoredPosition += eventData.delta;
            }
        }

        // Restores interaction state and resets to home tray if dropped outside a bin
        public void OnEndDrag(PointerEventData eventData)
        {
            isBeingDragged = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = Vector3.one;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }

            if (!wasDroppedOnBin)
            {
                ResetToHomePosition();
            }
        }

        // Marks item as received by a bin
        public void MarkDroppedOnBin()
        {
            wasDroppedOnBin = true;
            isBeingDragged = false;
            isSelectedForClickMode = false;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }
        }

        // Returns item to its home tray position
        public void ResetToHomePosition()
        {
            rectTransform.anchoredPosition = homeAnchoredPosition;
            rectTransform.localScale = Vector3.one;
            isSelectedForClickMode = false;
            isBeingDragged = false;

            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }
        }
    }
}
