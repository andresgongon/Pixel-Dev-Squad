using System.Collections.Generic;
using UnityEngine;

namespace RioLimpio
{
    // Lead Programmer: Germán Andrés González (Andrés) - Rol 1
    // Arcade Speed-Scoring Manager, Dynamic Hall of Fame Leaderboard, Keyboard Name Entry, and Colombian Resolution 2184 rules.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private const string PLAYER_NAME_KEY = "Player_Saved_Name";
        private const string HALL_OF_FAME_KEY = "Arcade_HallOfFame_Json";

        [Header("Level Configuration")]
        public int totalWasteTarget = 12;
        public int maxEcosystemLives = 3;
        public int basePointsPerItem = 100;
        public int penaltyPointsPerError = 150;

        [Header("Runtime State")]
        public int currentClassifiedCount = 0;
        public int currentScore = 0;
        public int currentLives = 3;
        public int currentStreak = 0;
        public bool isGameActive = false;
        public bool isTimeAttackMode = false; // Default: Educational Mode (UDL compliant)

        [Header("Scene References")]
        public UIManager uiManager;
        public WasteItemInteractable activeWasteItem;
        public RecyclingBin[] recyclingBins;
        public Vector2 wasteSpawnPosition = new Vector2(0f, 25f);

        [Header("Fish Life Sprites")]
        public Sprite activeFishSprite;
        public Sprite lostFishSprite;

        [Header("Waste Catalog Loaded From JSON")]
        public List<WasteItemData> wasteCatalog = new List<WasteItemData>();

        // Arcade Leaderboard & Name Entry
        private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
        private LeaderboardEntry currentSessionEntry = null;
        private string currentPlayerName = "";
        private string previousCommittedName = "";
        private bool isWaitingAtStartScreen = true;
        private bool isEditingName = false;
        private bool isSessionEnded = false;
        private bool lastVictory = false;
        private int playerRankPosition = 1;
        private string playerRankGrade = "RANGO S";
        private string playerRankTitle = "Guardián Supremo del Río 🌟🏆";

        private int plasticRecoveredCount = 0;
        private int organicRecoveredCount = 0;
        private List<int> shuffledIndices = new List<int>();
        private int shufflePointer = 0;
        private WasteInspectionMachine inspectionMachine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (activeWasteItem != null)
            {
                inspectionMachine = activeWasteItem.GetComponent<WasteInspectionMachine>();
            }

            // Ensure player always enters their name manually on launch
            currentPlayerName = "";
        }

        private void Start()
        {
            LoadCatalogFromJson();
            isGameActive = false;
            isWaitingAtStartScreen = true;

            if (activeWasteItem != null)
            {
                activeWasteItem.gameObject.SetActive(false);
            }

            if (uiManager != null)
            {
                uiManager.ShowStartMissionPanel(currentPlayerName, isTimeAttackMode, false);
            }
        }

        private void Update()
        {
            // 1. Start Mission Screen Controls (Live Name Entry & Start Game)
            if (isWaitingAtStartScreen)
            {
                HandleStartScreenInput();
                return;
            }

            // 2. End Game Panel Controls (Name Entry & Restart)
            if (!isGameActive && isSessionEnded)
            {
                if (isEditingName)
                {
                    HandleNameInput();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.N))
                {
                    isEditingName = true;
                    RefreshEndGameUI();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.M))
                {
                    ToggleMuteState();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ReturnToStartScreen();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    isTimeAttackMode = !isTimeAttackMode;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySelectSound();
                    }
                    StartNewGame();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    StartNewGame();
                }
                return;
            }

            // Return to main menu anytime during gameplay
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToStartScreen();
                return;
            }

            // Keyboard audio mute accessibility shortcut (WCAG 2.2 Criterion 1.4.2)
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMuteState();
            }

            // Keyboard shortcuts (Keys 1 to 4) for high-speed arcade reaction (WCAG 2.1.1)
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

        // Live keyboard text capture for the initial Start Mission screen
        private void HandleStartScreenInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isTimeAttackMode = !isTimeAttackMode;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySelectSound();
                }
                if (uiManager != null)
                {
                    uiManager.RefreshStartMissionPanel(currentPlayerName, isTimeAttackMode, false);
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnStartMissionConfirmed();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (currentPlayerName.Length > 0)
                {
                    currentPlayerName = currentPlayerName.Substring(0, currentPlayerName.Length - 1);
                    if (uiManager != null) uiManager.RefreshStartMissionPanel(currentPlayerName, isTimeAttackMode, false);
                }
                return;
            }

            foreach (char c in Input.inputString)
            {
                if (c == '\b' || c == '\n' || c == '\r' || c == '\t') continue;
                if (currentPlayerName.Length < 14 && (char.IsLetterOrDigit(c) || c == ' ' || c == '.'))
                {
                    currentPlayerName += char.ToUpper(c);
                    if (uiManager != null) uiManager.RefreshStartMissionPanel(currentPlayerName, isTimeAttackMode, false);
                }
            }
        }

        // Confirms player name (mandatory non-empty) and launches the active recycling session
        public void OnStartMissionConfirmed()
        {
            currentPlayerName = currentPlayerName.Trim();
            if (string.IsNullOrWhiteSpace(currentPlayerName))
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayErrorSound();
                }
                if (uiManager != null)
                {
                    uiManager.RefreshStartMissionPanel(currentPlayerName, isTimeAttackMode, true);
                }
                return;
            }

            previousCommittedName = currentPlayerName;
            PlayerPrefs.SetString(PLAYER_NAME_KEY, currentPlayerName);
            PlayerPrefs.Save();

            isWaitingAtStartScreen = false;
            if (uiManager != null)
            {
                uiManager.HideStartMissionPanel();
            }

            StartNewGame();
        }

        // Live keyboard text capture for classic arcade name entry
        private void HandleNameInput()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ConfirmPlayerName();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (currentPlayerName.Length > 0)
                {
                    currentPlayerName = currentPlayerName.Substring(0, currentPlayerName.Length - 1);
                    RefreshEndGameUI();
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ConfirmPlayerName();
                return;
            }

            foreach (char c in Input.inputString)
            {
                if (c == '\b' || c == '\n' || c == '\r')
                {
                    continue;
                }
                else if (currentPlayerName.Length < 14 && (char.IsLetterOrDigit(c) || c == ' ' || c == '.'))
                {
                    currentPlayerName += char.ToUpper(c);
                    RefreshEndGameUI();
                }
            }
        }

        private void ConfirmPlayerName()
        {
            isEditingName = false;
            currentPlayerName = currentPlayerName.Trim();
            if (string.IsNullOrWhiteSpace(currentPlayerName))
            {
                currentPlayerName = string.IsNullOrWhiteSpace(previousCommittedName) ? "JUGADOR" : previousCommittedName;
            }

            PlayerPrefs.SetString(PLAYER_NAME_KEY, currentPlayerName);
            PlayerPrefs.Save();

            RecomputeLeaderboard(true);
            RefreshEndGameUI();
        }

        // Loads catalog dynamically from JSON asset (Resolution 2184 & Stage 4 feedback)
        public void LoadCatalogFromJson()
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>("waste_catalog");
            if (jsonAsset == null)
            {
                Debug.LogWarning("waste_catalog.json not found in Resources. Using existing catalog.");
                return;
            }

            WasteCatalogData catalogData = JsonUtility.FromJson<WasteCatalogData>(jsonAsset.text);
            if (catalogData != null && catalogData.items != null && catalogData.items.Count > 0)
            {
                Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
                if (wasteCatalog != null)
                {
                    foreach (var oldItem in wasteCatalog)
                    {
                        if (oldItem != null && oldItem.itemSprite != null)
                        {
                            if (!string.IsNullOrEmpty(oldItem.itemId))
                            {
                                spriteCache[oldItem.itemId.ToLower()] = oldItem.itemSprite;
                            }
                            spriteCache[oldItem.itemSprite.name.ToLower()] = oldItem.itemSprite;
                        }
                    }
                }

                wasteCatalog = new List<WasteItemData>();
                foreach (var config in catalogData.items)
                {
                    Sprite sprite = null;
                    string keyId = config.itemId != null ? config.itemId.ToLower() : "";
                    string keySprite = config.spriteName != null ? config.spriteName.ToLower() : "";

                    if (!string.IsNullOrEmpty(keyId) && spriteCache.ContainsKey(keyId))
                    {
                        sprite = spriteCache[keyId];
                    }
                    else if (!string.IsNullOrEmpty(keySprite) && spriteCache.ContainsKey(keySprite))
                    {
                        sprite = spriteCache[keySprite];
                    }
                    else if (!string.IsNullOrEmpty(config.spriteName))
                    {
                        sprite = Resources.Load<Sprite>("Sprites/WasteItems/" + config.spriteName);
                    }

                    BinCategory category = (BinCategory)config.categoryIndex;
                    WasteItemData item = new WasteItemData(
                        config.itemId,
                        config.displayName,
                        category,
                        config.isTrapItem,
                        config.pedagogicalHint,
                        sprite
                    );
                    wasteCatalog.Add(item);
                }

                totalWasteTarget = wasteCatalog.Count;
            }
        }

        // Shuffles catalog indices using Fisher-Yates algorithm
        private void ReshuffleCatalog()
        {
            shuffledIndices.Clear();
            if (wasteCatalog == null || wasteCatalog.Count == 0) return;

            for (int i = 0; i < wasteCatalog.Count; i++)
            {
                shuffledIndices.Add(i);
            }

            for (int i = shuffledIndices.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                int temp = shuffledIndices[i];
                shuffledIndices[i] = shuffledIndices[rnd];
                shuffledIndices[rnd] = temp;
            }
            shufflePointer = 0;
        }

        // Returns cleanly to the Start Mission menu to change mode or player name
        public void ReturnToStartScreen()
        {
            isGameActive = false;
            isSessionEnded = false;
            isEditingName = false;
            isWaitingAtStartScreen = true;

            if (inspectionMachine != null)
            {
                inspectionMachine.StopTimer();
            }
            if (activeWasteItem != null)
            {
                activeWasteItem.gameObject.SetActive(false);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectSound();
            }
            if (uiManager != null)
            {
                if (uiManager.endGamePanel != null)
                {
                    uiManager.endGamePanel.SetActive(false);
                }
                uiManager.ShowStartMissionPanel(currentPlayerName, isTimeAttackMode, false);
            }
        }

        // Starts a fresh game session
        public void StartNewGame()
        {
            currentClassifiedCount = 0;
            currentScore = 0;
            currentLives = maxEcosystemLives;
            currentStreak = 0;
            plasticRecoveredCount = 0;
            organicRecoveredCount = 0;
            isSessionEnded = false;
            isEditingName = false;
            isGameActive = true;

            if (wasteCatalog != null && wasteCatalog.Count > 0)
            {
                totalWasteTarget = wasteCatalog.Count;
            }

            ReshuffleCatalog();

            if (inspectionMachine == null && activeWasteItem != null)
            {
                inspectionMachine = activeWasteItem.GetComponent<WasteInspectionMachine>();
            }
            if (inspectionMachine != null)
            {
                inspectionMachine.SetTimeAttackMode(isTimeAttackMode);
            }

            if (uiManager != null)
            {
                if (uiManager.endGamePanel != null)
                {
                    uiManager.endGamePanel.SetActive(false);
                }

                uiManager.UpdateProgressHUD(currentClassifiedCount, totalWasteTarget);
                uiManager.UpdateScoreHUD(currentScore, currentStreak);
                uiManager.UpdateLivesHUD(currentLives, activeFishSprite, lostFishSprite);

                if (isTimeAttackMode)
                {
                    uiManager.ShowFeedbackBubble("⚡ ¡MODO RETO: Clasifica rápido para multiplicar puntos y lograr el récord! 🏆", true, 4.0f);
                }
                else
                {
                    uiManager.ShowFeedbackBubble("🌱 ¡MODO EDUCATIVO: Tómate tu tiempo para clasificar y aprender sin prisa! 🌎", true, 4.0f);
                }
            }

            SpawnNextWasteItem();
        }

        // Toggles audio mute cleanly via UIManager or direct AudioManager fallback
        private void ToggleMuteState()
        {
            if (uiManager != null)
            {
                uiManager.OnMuteButtonClicked();
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMute();
            }
        }

        // Spawns the next item in randomized order with progressive arcade time pressure
        public void SpawnNextWasteItem()
        {
            if (activeWasteItem == null || wasteCatalog == null || wasteCatalog.Count == 0) return;

            if (shuffledIndices == null || shuffledIndices.Count == 0 || shufflePointer >= shuffledIndices.Count)
            {
                ReshuffleCatalog();
            }

            int nextIndex = shuffledIndices[shufflePointer];
            shufflePointer++;

            // Progressive difficulty ramp: warm-up (6.5s) -> medium (5.5s) -> fast rush (4.5s)
            float stageTimeLimit = 6.5f;
            if (currentClassifiedCount >= 8)
            {
                stageTimeLimit = 4.5f;
            }
            else if (currentClassifiedCount >= 4)
            {
                stageTimeLimit = 5.5f;
            }

            activeWasteItem.gameObject.SetActive(true);
            WasteItemData nextData = wasteCatalog[nextIndex];
            activeWasteItem.SetupWasteItem(nextData, wasteSpawnPosition, stageTimeLimit);

            if (inspectionMachine == null)
            {
                inspectionMachine = activeWasteItem.GetComponent<WasteInspectionMachine>();
            }
            if (inspectionMachine != null)
            {
                inspectionMachine.SetTimeAttackMode(isTimeAttackMode);
            }
        }

        // Called when player clicks on a bin directly (Click-to-Click mode)
        public void TryClassifyByBinClick(RecyclingBin clickedBin)
        {
            if (!isGameActive || activeWasteItem == null || clickedBin == null) return;
            EvaluateClassification(clickedBin, activeWasteItem);
        }

        // Classifies via keyboard shortcut
        private void TriggerBinByIndex(int index)
        {
            if (recyclingBins != null && index >= 0 && index < recyclingBins.Length)
            {
                RecyclingBin targetBin = recyclingBins[index];
                if (targetBin != null && activeWasteItem != null)
                {
                    EvaluateClassification(targetBin, activeWasteItem);
                }
            }
        }

        // Evaluates classification for a specific waste item
        public void EvaluateClassification(RecyclingBin selectedBin, WasteItemInteractable item = null)
        {
            if (!isGameActive || selectedBin == null) return;
            if (item == null) item = activeWasteItem;
            if (item == null || item.currentData == null) return;

            WasteItemData currentItem = item.currentData;

            if (selectedBin.binCategory == currentItem.correctCategory)
            {
                HandleCorrectClassification(selectedBin, currentItem);
            }
            else
            {
                HandleWrongClassification(selectedBin, currentItem);
            }
        }

        // Success handling: calculates time bonus, streak multiplier, updates HUD and checks for victory
        private void HandleCorrectClassification(RecyclingBin selectedBin, WasteItemData item)
        {
            currentStreak++;
            currentClassifiedCount = Mathf.Min(totalWasteTarget, currentClassifiedCount + 1);

            // Calculate Speed Bonus (only in Time-Attack mode; in Educational mode focus on learning accuracy)
            int speedBonus = 0;
            if (isTimeAttackMode && inspectionMachine != null)
            {
                float remainingTime = inspectionMachine.GetRemainingTime();
                speedBonus = Mathf.RoundToInt(remainingTime * 35f);
            }

            // Combo Streak Multiplier
            float streakMultiplier = 1.0f;
            string streakTag = "";
            if (isTimeAttackMode)
            {
                if (currentStreak >= 4)
                {
                    streakMultiplier = 2.0f;
                    streakTag = "ULTRA FRENESÍ x2.0 🔥";
                }
                else if (currentStreak == 3)
                {
                    streakMultiplier = 1.5f;
                    streakTag = "MODO FRENESÍ x1.5 🔥";
                }
                else if (currentStreak == 2)
                {
                    streakMultiplier = 1.25f;
                    streakTag = "Racha x1.25";
                }
            }
            else
            {
                if (currentStreak >= 3)
                {
                    streakTag = $"¡{currentStreak} Aciertos Seguidos! ⭐";
                }
            }

            int earnedPoints = Mathf.RoundToInt((basePointsPerItem + speedBonus) * streakMultiplier);
            currentScore += earnedPoints;

            if (item.correctCategory == BinCategory.WhiteRecyclable)
            {
                plasticRecoveredCount++;
            }
            else if (item.correctCategory == BinCategory.GreenOrganic)
            {
                organicRecoveredCount++;
            }

            selectedBin.PlaySuccessGlow();

            if (AudioManager.Instance != null)
            {
                bool isFinalItem = currentClassifiedCount >= totalWasteTarget;
                if (!isFinalItem)
                {
                    AudioManager.Instance.PlayCorrectSound(currentStreak);
                }
                float ratio = (float)currentClassifiedCount / totalWasteTarget;
                AudioManager.Instance.UpdateMusicByRiverProgress(ratio);
            }

            if (uiManager != null)
            {
                uiManager.UpdateProgressHUD(currentClassifiedCount, totalWasteTarget);
                uiManager.UpdateScoreHUD(currentScore, currentStreak);
                uiManager.SetGuardianReaction(true);

                string popupText = currentStreak >= 3 ? $"+{earnedPoints} 🔥" : $"+{earnedPoints}";
                Vector2 popupPos = selectedBin != null ? selectedBin.GetCanvasPosition() : wasteSpawnPosition;
                uiManager.ShowFloatingScoreVFX(popupText, true, popupPos);

                if (isTimeAttackMode)
                {
                    string bonusDetail = speedBonus > 100 ? "¡Súper Veloz!" : "¡A tiempo!";
                    if (!string.IsNullOrEmpty(streakTag))
                    {
                        uiManager.ShowFeedbackBubble($"¡+{earnedPoints} PTS! ({bonusDetail} | {streakTag}) ✔", true, 2.0f);
                    }
                    else if (item.isTrapItem)
                    {
                        string binName = selectedBin != null ? selectedBin.binNameText : "la caneca correcta";
                        uiManager.ShowFeedbackBubble($"¡+{earnedPoints} PTS! ¡Excelente ojo! Va en {binName} ✔", true, 2.2f);
                    }
                    else
                    {
                        uiManager.ShowFeedbackBubble($"¡+{earnedPoints} PTS! (Velocidad: +{speedBonus}) ✔", true, 1.8f);
                    }
                }
                else
                {
                    // Educational Mode: short, warm, pedagogical confirmation without speed references
                    string binName = selectedBin != null ? selectedBin.binNameText : "la caneca correcta";
                    string positiveMsg = item.isTrapItem
                        ? $"¡Bien pensado! {item.displayName} va en {binName} ✔"
                        : $"¡Muy bien! {item.displayName} va en {binName} ✔";
                    uiManager.ShowFeedbackBubble(positiveMsg, true, 2.5f);
                }
            }

            if (currentClassifiedCount >= totalWasteTarget)
            {
                EndSession(true);
            }
            else
            {
                SpawnNextWasteItem();
            }
        }

        // Error handling: penalty points, breaks streak, reduces life (in Time-Attack), gives pedagogical hint
        private void HandleWrongClassification(RecyclingBin selectedBin, WasteItemData item)
        {
            currentStreak = 0;
            if (isTimeAttackMode)
            {
                currentLives = Mathf.Max(0, currentLives - 1);
                currentScore = Mathf.Max(0, currentScore - penaltyPointsPerError);
            }
            else
            {
                // In Educational mode, do not deduct lives so children can finish all items without stress (UDL)
                currentScore = Mathf.Max(0, currentScore - 25);
            }

            if (shufflePointer > 0 && shufflePointer - 1 < shuffledIndices.Count)
            {
                shuffledIndices.Add(shuffledIndices[shufflePointer - 1]);
            }

            if (selectedBin != null)
            {
                selectedBin.PlayErrorShake();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayErrorSound();
            }

            if (uiManager != null)
            {
                uiManager.UpdateLivesHUD(currentLives, activeFishSprite, lostFishSprite);
                uiManager.UpdateScoreHUD(currentScore, currentStreak);
                uiManager.SetGuardianReaction(false);

                Vector2 popupPos = selectedBin != null ? selectedBin.GetCanvasPosition() : wasteSpawnPosition;
                string penaltyTag = isTimeAttackMode ? $"-{penaltyPointsPerError} PTS ✖" : "-25 PTS ✖";
                uiManager.ShowFloatingScoreVFX(penaltyTag, false, popupPos);

                string penaltyMsg = isTimeAttackMode ? $"¡-{penaltyPointsPerError} PTS! (Error de caneca) ✖\n" : "¡Cuidado! ";
                uiManager.ShowFeedbackBubble($"{penaltyMsg}{item.pedagogicalHint}", false, 4.0f);
            }

            if (isTimeAttackMode && currentLives <= 0)
            {
                EndSession(false);
            }
            else
            {
                SpawnNextWasteItem();
            }
        }

        // Called when inspection timer expires without sorting (Timeout Penalty - only active in Time-Attack mode)
        public void OnInspectionTimeout()
        {
            if (!isGameActive || !isTimeAttackMode) return;

            currentStreak = 0;
            currentLives = Mathf.Max(0, currentLives - 1);
            currentScore = Mathf.Max(0, currentScore - penaltyPointsPerError);

            if (shufflePointer > 0 && shufflePointer - 1 < shuffledIndices.Count)
            {
                shuffledIndices.Add(shuffledIndices[shufflePointer - 1]);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayErrorSound();
            }

            if (uiManager != null)
            {
                uiManager.UpdateLivesHUD(currentLives, activeFishSprite, lostFishSprite);
                uiManager.UpdateScoreHUD(currentScore, currentStreak);
                uiManager.SetGuardianReaction(false);
                uiManager.ShowFloatingScoreVFX($"-{penaltyPointsPerError} PTS ⏱", false, wasteSpawnPosition + new Vector2(0f, 30f));
                uiManager.ShowFeedbackBubble($"¡-{penaltyPointsPerError} PTS! (Tiempo agotado) ⏱\n¡Debes clasificar más rápido para sumar puntos!", false, 3.5f);
            }

            if (currentLives <= 0)
            {
                EndSession(false);
            }
            else
            {
                SpawnNextWasteItem();
            }
        }

        // Recomputes and persists true classic arcade Hall of Fame (every run gets its own ranked entry)
        private void RecomputeLeaderboard(bool isRenameUpdate = false)
        {
            if (isRenameUpdate && currentSessionEntry != null)
            {
                currentSessionEntry.name = currentPlayerName;
                previousCommittedName = currentPlayerName;
            }
            else
            {
                leaderboard.Clear();

                string json = PlayerPrefs.GetString(HALL_OF_FAME_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
                    if (data != null && data.entries != null)
                    {
                        leaderboard.AddRange(data.entries);
                    }
                }

                if (leaderboard.Count == 0)
                {
                    leaderboard.Add(new LeaderboardEntry("SOFÍA (LÍDER)", 2850));
                    leaderboard.Add(new LeaderboardEntry("GUARDIÁN ANDINO", 2100));
                    leaderboard.Add(new LeaderboardEntry("VOLUNTARIO ECO", 1500));
                }

                currentSessionEntry = new LeaderboardEntry(currentPlayerName, currentScore);
                leaderboard.Add(currentSessionEntry);
                previousCommittedName = currentPlayerName;
            }

            // Sort descending (older/existing ties stay ahead unless beaten strictly)
            leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

            // Find exact rank position of THIS run BEFORE trimming
            playerRankPosition = leaderboard.Count;
            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (ReferenceEquals(leaderboard[i], currentSessionEntry))
                {
                    playerRankPosition = i + 1;
                    break;
                }
            }

            // Save top 10 historical records cleanly (discarding runs out of top 10)
            List<LeaderboardEntry> savedTop = new List<LeaderboardEntry>(leaderboard);
            if (savedTop.Count > 10)
            {
                savedTop.RemoveRange(10, savedTop.Count - 10);
            }

            LeaderboardData saveData = new LeaderboardData();
            saveData.entries = savedTop;
            PlayerPrefs.SetString(HALL_OF_FAME_KEY, JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
        }

        // Handles end game ranking calculation, high scores and summary
        private void EndSession(bool victory)
        {
            isGameActive = false;
            isSessionEnded = true;
            lastVictory = victory;
            // Play the final victory sound when all waste items are classified.
            if (victory && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictorySound();
            }

            if (inspectionMachine != null)
            {
                inspectionMachine.StopTimer();
            }
            if (activeWasteItem != null)
            {
                activeWasteItem.gameObject.SetActive(false);
            }

            if (isTimeAttackMode)
            {
                RecomputeLeaderboard(false);

                // Calculate Performance Rank
                if (currentScore >= 2600)
                {
                    playerRankGrade = "RANGO S";
                    playerRankTitle = "Guardián Supremo del Río 🌟🏆";
                }
                else if (currentScore >= 1900)
                {
                    playerRankGrade = "RANGO A";
                    playerRankTitle = "Protector Ecológico Experto 🥇";
                }
                else if (currentScore >= 1300)
                {
                    playerRankGrade = "RANGO B";
                    playerRankTitle = "Guardián en Entrenamiento 🥈";
                }
                else
                {
                    playerRankGrade = "RANGO C";
                    playerRankTitle = "Voluntario Novato 🥉";
                }
            }

            if (uiManager != null)
            {
                uiManager.ShowEndGameSummary(
                    victory,
                    currentClassifiedCount,
                    currentScore,
                    playerRankPosition,
                    playerRankGrade,
                    playerRankTitle,
                    leaderboard,
                    currentPlayerName,
                    isEditingName,
                    plasticRecoveredCount,
                    organicRecoveredCount,
                    isTimeAttackMode
                );
            }
        }

        private void RefreshEndGameUI()
        {
            if (uiManager != null)
            {
                uiManager.RefreshEndGamePanelText(
                    lastVictory,
                    currentClassifiedCount,
                    currentScore,
                    playerRankPosition,
                    playerRankGrade,
                    playerRankTitle,
                    leaderboard,
                    currentPlayerName,
                    isEditingName,
                    plasticRecoveredCount,
                    organicRecoveredCount,
                    isTimeAttackMode
                );
            }
        }

        public void OnItemEscapedRiver()
        {
            OnInspectionTimeout();
        }
    }
}
