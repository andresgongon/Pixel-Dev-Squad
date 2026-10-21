using System.Collections.Generic;
using UnityEngine;

namespace RioLimpio
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level Configuration")]
        public int totalWasteTarget = 80;
        public int maxEcosystemLives = 3;
        public int pointsPerItem = 8;

        [Header("Runtime State")]
        public int currentClassifiedCount = 0;
        public int currentLives = 3;
        public int currentStreak = 0;
        public bool isGameActive = true;

        [Header("Scene References")]
        public UIManager uiManager;
        public WasteItemInteractable activeWasteItem;
        public RecyclingBin[] recyclingBins;
        public Vector2 wasteSpawnPosition = new Vector2(-180f, -10f);

        [Header("Fish Life Sprites (Optional - Rol 2)")]
        public Sprite activeFishSprite;
        public Sprite lostFishSprite;

        [Header("Waste Catalog")]
        public List<WasteItemData> wasteCatalog = new List<WasteItemData>();

        private int currentCatalogIndex = 0;
        private int plasticRecoveredCount = 0;
        private int organicRecoveredCount = 0;

        // Initializes the singleton instance
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // Sets up the level, initializes default catalog data, and spawns the first waste item
        private void Start()
        {
            InitializeDefaultCatalogIfEmpty();
            StartNewGame();
        }

        // Listens for keyboard shortcuts (keys 1, 2, 3, 4) for motor accessibility (WCAG 2.1.1)
        private void Update()
        {
            if (!isGameActive)
            {
                // Allow pressing R to restart the level after completion
                if (Input.GetKeyDown(KeyCode.R))
                {
                    StartNewGame();
                }
                return;
            }

            // Check number keys 1 to 4 on main keyboard or numpad to classify without a mouse
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                TriggerBinByIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                TriggerBinByIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                TriggerBinByIndex(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                TriggerBinByIndex(3);
            }
        }

        // Populates the waste list with everyday Colombian items and trap items if none were set
        private void InitializeDefaultCatalogIfEmpty()
        {
            if (wasteCatalog != null && wasteCatalog.Count > 0)
            {
                return;
            }

            wasteCatalog = new List<WasteItemData>
            {
                new WasteItemData(
                    "pet_bottle",
                    "Botella Plástica PET (Limpia)",
                    BinCategory.BlueRecyclable,
                    false,
                    "¡Pista! Las botellas plásticas limpias y secas van en la caneca AZUL [1] de aprovechables."
                ),
                new WasteItemData(
                    "banana_peel",
                    "Cáscara de Banano",
                    BinCategory.GreenOrganic,
                    false,
                    "¡Pista! Los restos de fruta y cáscaras son biodegradables y van en la caneca VERDE [3]."
                ),
                new WasteItemData(
                    "greasy_pizza",
                    "Caja de Pizza Engrasada (Trampa)",
                    BinCategory.GreyNonRecyclable,
                    true,
                    "¡Cuidado! Aunque es cartón, tiene grasa de comida y NO se puede reciclar. Va en la GRIS [4]."
                ),
                new WasteItemData(
                    "used_battery",
                    "Pila Alcalina Usada",
                    BinCategory.RedSpecial,
                    false,
                    "¡Pista! Las pilas contienen metales pesados y requieren manejo especial en la caneca ROJA [2]."
                ),
                new WasteItemData(
                    "clean_cardboard",
                    "Caja de Cartón Limpia",
                    BinCategory.BlueRecyclable,
                    false,
                    "¡Pista! El cartón limpio y sin manchas de grasa es aprovechable y va en la caneca AZUL [1]."
                ),
                new WasteItemData(
                    "apple_core",
                    "Restos de Manzana",
                    BinCategory.GreenOrganic,
                    false,
                    "¡Pista! Los restos de manzana sirven para abono orgánico en la caneca VERDE [3]."
                ),
                new WasteItemData(
                    "chip_bag",
                    "Empaque Metalizado de Papas (Trampa)",
                    BinCategory.GreyNonRecyclable,
                    true,
                    "¡Ojo! Los paquetes de mecato con interior metalizado y restos de sal van en la caneca GRIS [4]."
                ),
                new WasteItemData(
                    "glass_jar",
                    "Frasco de Vidrio Limpio",
                    BinCategory.BlueRecyclable,
                    false,
                    "¡Pista! Los envases de vidrio enjuagados son 100% reciclables en la caneca AZUL [1]."
                )
            };
        }

        // Resets counters, lives, and UI panels to start a new game session
        public void StartNewGame()
        {
            currentClassifiedCount = 0;
            currentLives = maxEcosystemLives;
            currentStreak = 0;
            plasticRecoveredCount = 0;
            organicRecoveredCount = 0;
            currentCatalogIndex = 0;
            isGameActive = true;

            if (uiManager != null)
            {
                if (uiManager.endGamePanel != null)
                {
                    uiManager.endGamePanel.SetActive(false);
                }

                uiManager.UpdateProgressHUD(currentClassifiedCount, totalWasteTarget);
                uiManager.UpdateLivesHUD(currentLives, activeFishSprite, lostFishSprite);
                uiManager.ShowFeedbackBubble("¡CLASIFICA EL RESIDUO! (ARRASTRA, CLIC O TECLAS 1-4)", true, 3.5f);
            }

            SpawnNextWasteItem();
        }

        // Selects the next waste item from the catalog and presents it on the screen
        public void SpawnNextWasteItem()
        {
            if (activeWasteItem == null || wasteCatalog == null || wasteCatalog.Count == 0)
            {
                return;
            }

            activeWasteItem.gameObject.SetActive(true);
            WasteItemData nextData = wasteCatalog[currentCatalogIndex % wasteCatalog.Count];
            currentCatalogIndex++;

            activeWasteItem.SetupWasteItem(nextData, wasteSpawnPosition);
        }

        // Handles classification when a player clicks on a bin in Click-to-Click mode
        public void TryClassifyByBinClick(RecyclingBin clickedBin)
        {
            if (!isGameActive || activeWasteItem == null || clickedBin == null)
            {
                return;
            }

            EvaluateClassification(clickedBin);
        }

        // Triggers classification using keyboard keys 1, 2, 3, or 4
        private void TriggerBinByIndex(int index)
        {
            if (recyclingBins != null && index >= 0 && index < recyclingBins.Length)
            {
                RecyclingBin targetBin = recyclingBins[index];
                if (targetBin != null)
                {
                    EvaluateClassification(targetBin);
                }
            }
        }

        // Compares the selected bin category with the current waste item category
        public void EvaluateClassification(RecyclingBin selectedBin)
        {
            if (!isGameActive || activeWasteItem == null || activeWasteItem.currentData == null || selectedBin == null)
            {
                return;
            }

            WasteItemData currentItem = activeWasteItem.currentData;

            // Check if the player chose the correct recycling bin
            if (selectedBin.binCategory == currentItem.correctCategory)
            {
                HandleCorrectClassification(selectedBin, currentItem);
            }
            else
            {
                HandleWrongClassification(currentItem);
            }
        }

        // Applies positive feedback, increases river progress, and checks for victory
        private void HandleCorrectClassification(RecyclingBin selectedBin, WasteItemData item)
        {
            currentStreak++;
            currentClassifiedCount = Mathf.Min(totalWasteTarget, currentClassifiedCount + pointsPerItem);

            // Track material categories for the final pedagogical impact summary
            if (item.correctCategory == BinCategory.BlueRecyclable)
            {
                plasticRecoveredCount += pointsPerItem;
            }
            else if (item.correctCategory == BinCategory.GreenOrganic)
            {
                organicRecoveredCount += pointsPerItem;
            }

            selectedBin.PlaySuccessGlow();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCorrectSound();
                float ratio = (float)currentClassifiedCount / totalWasteTarget;
                AudioManager.Instance.UpdateMusicByRiverProgress(ratio);
            }

            if (uiManager != null)
            {
                uiManager.UpdateProgressHUD(currentClassifiedCount, totalWasteTarget);
                uiManager.SetGuardianReaction(true);
                uiManager.ShowFeedbackBubble("¡CLASIFICACIÓN CORRECTA! ✔", true, 2.0f);
            }

            // Check if the player reached 100% of the cleaning progress bar
            if (currentClassifiedCount >= totalWasteTarget)
            {
                isGameActive = false;
                activeWasteItem.gameObject.SetActive(false);

                if (uiManager != null)
                {
                    uiManager.ShowEndGameSummary(true, currentClassifiedCount, plasticRecoveredCount, organicRecoveredCount);
                }
            }
            else
            {
                SpawnNextWasteItem();
            }
        }

        // Applies gentle negative feedback, subtracts 1 fish life, and shows a pedagogical hint
        private void HandleWrongClassification(WasteItemData item)
        {
            currentStreak = 0;
            currentLives = Mathf.Max(0, currentLives - 1);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayErrorSound();
            }

            if (uiManager != null)
            {
                uiManager.UpdateLivesHUD(currentLives, activeFishSprite, lostFishSprite);
                uiManager.SetGuardianReaction(false);
                uiManager.ShowFeedbackBubble(item.pedagogicalHint, false, 3.8f);
            }

            activeWasteItem.ResetToHomePosition();

            // Check if all 3 native fish lives were lost
            if (currentLives <= 0)
            {
                isGameActive = false;
                activeWasteItem.gameObject.SetActive(false);

                if (uiManager != null)
                {
                    uiManager.ShowEndGameSummary(false, currentClassifiedCount, plasticRecoveredCount, organicRecoveredCount);
                }
            }
        }
    }
}
