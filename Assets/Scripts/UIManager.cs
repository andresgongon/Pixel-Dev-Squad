using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RioLimpio
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;

        public LeaderboardEntry(string n, int s)
        {
            name = n;
            score = s;
        }
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    public class UIManager : MonoBehaviour
    {
        [Header("Top Left HUD - Cleaning Progress")]
        public Image progressBarFill;
        public Text progressPercentText;
        public Text wasteCounterText;

        [Header("Top Right HUD - Ecosystem Lives & Score")]
        public Image[] fishLifeIcons;
        public Text livesCountText;
        public Text levelStatusText;

        [Header("Background & River States")]
        public Image stationBackgroundImage;
        public Sprite riverPollutedSprite;
        public Sprite riverRecoveringSprite;
        public Sprite riverCleanSprite;

        [Header("Guardian Character")]
        public Image guardianCharacterImage;
        public Sprite guardianIdleSprite;
        public Sprite guardianHappySprite;
        public Sprite guardianEmpathySprite;

        [Header("Feedback Speech Bubble")]
        public GameObject feedbackBubbleObject;
        public Text feedbackBubbleText;

        [Header("Audio Accessibility Control (WCAG 1.4.2)")]
        public Button muteToggleButton;
        public Text muteButtonLabel;

        [Header("Start Game Instructions Panel")]
        public GameObject startMissionPanel;
        public Text startMissionTitleText;
        public Text startMissionBodyText;
        public Button startMissionButton;

        [Header("End Game Impact Panel")]
        public GameObject endGamePanel;
        public Text endGameTitleText;
        public Text endGameImpactSummaryText;

        private Coroutine feedbackCoroutine;
        private Coroutine guardianPoseCoroutine;
        private Canvas cachedCanvas;

        private void Awake()
        {
            cachedCanvas = GetComponentInParent<Canvas>();
            if (cachedCanvas == null)
            {
                cachedCanvas = FindFirstObjectByType<Canvas>();
            }

            if (startMissionButton != null)
            {
                startMissionButton.onClick.AddListener(OnStartMissionButtonClicked);
            }

            if (muteToggleButton != null)
            {
                muteToggleButton.onClick.AddListener(OnMuteButtonClicked);
            }
        }

        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                RefreshMuteButtonVisual(AudioManager.Instance.isMuted);
            }
        }

        private void OnStartMissionButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStartMissionConfirmed();
            }
        }

        // Toggles audio mute independently for accessibility (WCAG 2.2 Criterion 1.4.2)
        public void OnMuteButtonClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMute();
                RefreshMuteButtonVisual(AudioManager.Instance.isMuted);
            }
        }

        // Updates visual state of the mute button so feedback is not color-only
        public void RefreshMuteButtonVisual(bool isMuted)
        {
            if (muteButtonLabel != null)
            {
                muteButtonLabel.text = isMuted ? "🔇 [M]" : "🔊 [M]";
                muteButtonLabel.color = isMuted
                    ? new Color(1f, 0.45f, 0.45f, 1f)
                    : new Color(0.75f, 1f, 0.78f, 1f);
            }
        }

        // Spawns an animated floating score popup VFX above the bin or waste item
        public void ShowFloatingScoreVFX(string message, bool isPositive, Vector2 startAnchoredPosition)
        {
            if (cachedCanvas == null)
            {
                cachedCanvas = FindFirstObjectByType<Canvas>();
            }
            if (cachedCanvas == null) return;

            GameObject popupObj = new GameObject("FloatingScoreVFX");
            popupObj.transform.SetParent(cachedCanvas.transform, false);

            RectTransform rect = popupObj.AddComponent<RectTransform>();
            rect.anchoredPosition = startAnchoredPosition;
            rect.sizeDelta = new Vector2(280f, 60f);

            Text txt = popupObj.AddComponent<Text>();
            txt.font = levelStatusText != null
                ? levelStatusText.font
                : (wasteCounterText != null ? wasteCounterText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            txt.text = message;
            txt.color = isPositive ? new Color(1f, 0.9f, 0.25f, 1f) : new Color(1f, 0.28f, 0.22f, 1f);

            Outline outline = popupObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            StartCoroutine(AnimateFloatingPopupRoutine(rect, txt, startAnchoredPosition));
        }

        private IEnumerator AnimateFloatingPopupRoutine(RectTransform rect, Text txt, Vector2 startPos)
        {
            float duration = 0.9f;
            float elapsed = 0f;
            Color baseColor = txt.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Smooth upward drift + elastic punch at start
                float yOffset = Mathf.Sin(t * Mathf.PI * 0.5f) * 85f;
                rect.anchoredPosition = startPos + new Vector2(0f, yOffset);

                float punch = t < 0.2f ? Mathf.Lerp(0.6f, 1.25f, t / 0.2f) : Mathf.Lerp(1.25f, 1.0f, (t - 0.2f) / 0.8f);
                rect.localScale = new Vector3(punch, punch, 1f);

                float alpha = t > 0.6f ? Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f) : 1f;
                txt.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                yield return null;
            }

            if (rect != null)
            {
                Destroy(rect.gameObject);
            }
        }

        // Displays the starting mission intro and instructions modal
        public void ShowStartMissionPanel(string playerName, bool isTimeAttackMode = false, bool showNameError = false)
        {
            if (feedbackBubbleObject != null)
            {
                feedbackBubbleObject.SetActive(false);
            }
            if (startMissionPanel != null)
            {
                startMissionPanel.SetActive(true);
            }
            RefreshStartMissionPanel(playerName, isTimeAttackMode, showNameError);
        }

        // Hides start mission panel
        public void HideStartMissionPanel()
        {
            if (startMissionPanel != null)
            {
                startMissionPanel.SetActive(false);
            }
        }

        // Live text refresh for the start mission panel
        public void RefreshStartMissionPanel(string playerName, bool isTimeAttackMode = false, bool showNameError = false)
        {
            if (startMissionTitleText != null)
            {
                startMissionTitleText.text = "🏆 RÍO LIMPIO: MISIÓN RECICLAJE 🏆";
            }

            if (startMissionBodyText != null)
            {
                string nameDisplay = string.IsNullOrEmpty(playerName)
                    ? "<color=#FFD966>[ Escribe tu nombre con el teclado_ ]</color>"
                    : $"<color=#6EFF8B>[ {playerName}_ ]</color>";

                string modeDisplay = isTimeAttackMode
                    ? "<color=#FFAA33>⚡ Modo Reto (Contrarreloj)</color>"
                    : "<color=#55FF77>🌱 Modo Libre (Sin tiempo)</color>";

                string modeDescription = isTimeAttackMode
                    ? "<color=#FFD280>\"Pon a prueba tus conocimientos contra el reloj: ¡gana bonos de velocidad, encadena rachas multiplicadoras y compite por el récord en el Hall of Fame!\"</color>"
                    : "<color=#99FFB3>\"Aprende la clasificación ambiental a tu propio ritmo: sin límite de tiempo, sin pérdida de vidas y con explicaciones de la guardiana para cada residuo.\"</color>";

                string promptLine = showNameError
                    ? "<color=#FF5555>⚠ ¡OBLIGATORIO: ESCRIBE TU NOMBRE PARA COMENZAR! ⚠</color>"
                    : "Presiona [TAB] para alternar modo  |  [ENTER] o clic en ¡COMENZAR MISIÓN!";

                startMissionBodyText.text =
                    $"¡BIENVENIDO, GUARDIÁN AMBIENTAL!\n\n" +
                    $"TU NOMBRE:  {nameDisplay}\n" +
                    $"MODO: {modeDisplay}  <color=#CCCCCC>[Presiona TAB para cambiar]</color>\n" +
                    $"{modeDescription}\n\n" +
                    $"────────────────────────────────────────\n" +
                    $"CÓDIGO DE COLORES (RESOLUCIÓN 2184 DE 2019):\n" +
                    $"  [1] BLANCA: Aprovechables (botellas limpias, cartón seco, papel, Tetra Pak, latas)\n" +
                    $"  [2] VERDE:  Orgánicos (cáscaras de fruta, té, comida cruda, poda)\n" +
                    $"  [3] NEGRA:  No aprovechables (servilletas, cajas con grasa, vasos encerados, recibos)\n" +
                    $"  [4] ROJA:   Peligrosos y especiales (pilas, medicamentos, bombillos, RAEE, aerosoles)\n" +
                    $"────────────────────────────────────────\n" +
                    $"CONTROLES: Arrastra o usa teclas [1]-[4]  |  🔊 Tecla [M]: Silenciar audio\n\n" +
                    $"{promptLine}";
            }
        }

        // Updates progress HUD
        public void UpdateProgressHUD(int classifiedCount, int targetTotal)
        {
            float ratio = targetTotal > 0 ? Mathf.Clamp01((float)classifiedCount / targetTotal) : 0f;
            int percentage = Mathf.RoundToInt(ratio * 100f);

            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = ratio;
            }

            if (progressPercentText != null)
            {
                progressPercentText.text = percentage + "%";
            }

            if (wasteCounterText != null)
            {
                wasteCounterText.text = classifiedCount + " / " + targetTotal;
            }

            UpdateRiverVisualState(ratio);
        }

        // Updates arcade score and combo streak in the top right board
        public void UpdateScoreHUD(int currentScore, int currentStreak)
        {
            if (levelStatusText != null)
            {
                string streakTag = currentStreak > 1 ? $"  |  🔥 x{currentStreak}" : "";
                levelStatusText.text = $"PUNTAJE: {currentScore:N0} pts{streakTag}";
                levelStatusText.color = currentStreak >= 3 ? new Color(1f, 0.88f, 0.2f, 1f) : new Color(1f, 1f, 1f, 1f);
            }
        }

        // Updates ecosystem fish lives
        public void UpdateLivesHUD(int remainingLives, Sprite activeFishSprite, Sprite lostFishSprite)
        {
            if (livesCountText != null)
            {
                livesCountText.text = remainingLives.ToString();
            }

            if (fishLifeIcons != null)
            {
                for (int i = 0; i < fishLifeIcons.Length; i++)
                {
                    if (fishLifeIcons[i] != null)
                    {
                        if (i < remainingLives)
                        {
                            if (activeFishSprite != null)
                            {
                                fishLifeIcons[i].sprite = activeFishSprite;
                            }
                            fishLifeIcons[i].color = Color.white;
                        }
                        else
                        {
                            if (lostFishSprite != null)
                            {
                                fishLifeIcons[i].sprite = lostFishSprite;
                            }
                            fishLifeIcons[i].color = new Color(1f, 1f, 1f, 0.28f);
                        }
                    }
                }
            }
        }

        // Updates river background sprite
        public void UpdateRiverVisualState(float normalizedProgress)
        {
            if (stationBackgroundImage == null) return;

            if (normalizedProgress >= 1.0f)
            {
                if (riverCleanSprite != null)
                {
                    stationBackgroundImage.sprite = riverCleanSprite;
                    stationBackgroundImage.color = Color.white;
                }
                else
                {
                    stationBackgroundImage.color = new Color(0.18f, 0.55f, 0.78f, 1f);
                }
            }
            else if (normalizedProgress >= 0.5f)
            {
                if (riverRecoveringSprite != null)
                {
                    stationBackgroundImage.sprite = riverRecoveringSprite;
                    stationBackgroundImage.color = Color.white;
                }
                else
                {
                    stationBackgroundImage.color = new Color(0.24f, 0.48f, 0.46f, 1f);
                }
            }
            else
            {
                if (riverPollutedSprite != null)
                {
                    stationBackgroundImage.sprite = riverPollutedSprite;
                    stationBackgroundImage.color = Color.white;
                }
                else
                {
                    stationBackgroundImage.color = new Color(0.32f, 0.28f, 0.24f, 1f);
                }
            }
        }

        // Displays feedback speech bubble
        public void ShowFeedbackBubble(string message, bool isPositive, float displaySeconds = 2.4f)
        {
            if (feedbackBubbleObject == null || feedbackBubbleText == null) return;

            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
            }

            feedbackCoroutine = StartCoroutine(DisplayBubbleRoutine(message, isPositive, displaySeconds));
        }

        private IEnumerator DisplayBubbleRoutine(string message, bool isPositive, float duration)
        {
            feedbackBubbleObject.SetActive(true);
            feedbackBubbleText.text = message;

            if (isPositive)
            {
                feedbackBubbleText.color = new Color(0.08f, 0.42f, 0.15f, 1f);
            }
            else
            {
                feedbackBubbleText.color = new Color(0.65f, 0.12f, 0.08f, 1f);
            }

            yield return new WaitForSeconds(duration);
            feedbackBubbleObject.SetActive(false);
        }

        // Sets guardian reaction
        public void SetGuardianReaction(bool isSuccess)
        {
            if (guardianCharacterImage == null) return;

            if (guardianPoseCoroutine != null)
            {
                StopCoroutine(guardianPoseCoroutine);
            }

            guardianPoseCoroutine = StartCoroutine(GuardianReactionRoutine(isSuccess));
        }

        private IEnumerator GuardianReactionRoutine(bool isSuccess)
        {
            if (isSuccess && guardianHappySprite != null)
            {
                guardianCharacterImage.sprite = guardianHappySprite;
            }
            else if (!isSuccess && guardianEmpathySprite != null)
            {
                guardianCharacterImage.sprite = guardianEmpathySprite;
            }

            yield return new WaitForSeconds(1.5f);

            if (guardianIdleSprite != null)
            {
                guardianCharacterImage.sprite = guardianIdleSprite;
            }
        }

        // Shows arcade leaderboard and summary
        public void ShowEndGameSummary(
            bool victory,
            int totalClassified,
            int finalScore,
            int rankPosition,
            string rankGrade,
            string rankTitle,
            List<LeaderboardEntry> topEntries,
            string currentPlayerName,
            bool isEditingName,
            int plasticCount,
            int organicCount,
            bool isTimeAttackMode = false)
        {
            if (endGamePanel == null) return;

            StartCoroutine(EndGameSequenceRoutine(
                victory, totalClassified, finalScore, rankPosition, rankGrade, rankTitle, topEntries, currentPlayerName, isEditingName, plasticCount, organicCount, isTimeAttackMode));
        }

        private IEnumerator EndGameSequenceRoutine(
            bool victory,
            int totalClassified,
            int finalScore,
            int rankPosition,
            string rankGrade,
            string rankTitle,
            List<LeaderboardEntry> topEntries,
            string currentPlayerName,
            bool isEditingName,
            int plasticCount,
            int organicCount,
            bool isTimeAttackMode)
        {
            if (victory)
            {
                UpdateRiverVisualState(1.0f);
                string victoryMsg = isTimeAttackMode
                    ? $"🎉 ¡MISIÓN CUMPLIDA! Puntaje final: {finalScore:N0} pts 💧🐟"
                    : $"🎉 ¡MISIÓN EDUCATIVA COMPLETADA! Has limpiado el río 💧🐟";
                ShowFeedbackBubble(victoryMsg, true, 4.0f);
                yield return new WaitForSeconds(3.0f);
            }

            endGamePanel.SetActive(true);
            RefreshEndGamePanelText(
                victory, totalClassified, finalScore, rankPosition, rankGrade, rankTitle, topEntries, currentPlayerName, isEditingName, plasticCount, organicCount, isTimeAttackMode);
        }

        // Live refresh of end game text (used when player is typing their name)
        public void RefreshEndGamePanelText(
            bool victory,
            int totalClassified,
            int finalScore,
            int rankPosition,
            string rankGrade,
            string rankTitle,
            List<LeaderboardEntry> topEntries,
            string currentPlayerName,
            bool isEditingName,
            int plasticCount,
            int organicCount,
            bool isTimeAttackMode = false)
        {
            if (endGameTitleText != null)
            {
                if (!isTimeAttackMode)
                {
                    endGameTitleText.text = "🌿 ¡MISIÓN DE APRENDIZAJE COMPLETADA! 🌿";
                }
                else if (rankPosition == 1 && finalScore > 0)
                {
                    endGameTitleText.text = "🏆 ¡NUEVO RÉCORD #1 DE LA ESTACIÓN! 🏆";
                }
                else
                {
                    endGameTitleText.text = victory ? "¡RÍO ANDINO 100% RECUPERADO! 🐟" : "¡INTÉNTALO DE NUEVO, GUARDIÁN!";
                }
            }

            if (endGameImpactSummaryText != null)
            {
                int liters = totalClassified * 120;

                if (!isTimeAttackMode)
                {
                    // Educational Mode: clean learning certificate without competitive Hall of Fame
                    endGameImpactSummaryText.text =
                        $"¡FELICITACIONES, {currentPlayerName.ToUpper()}!\n\n" +
                        $"Has clasificado con éxito los {totalClassified} residuos bajo la Resolución 2184 de 2019:\n\n" +
                        $"  • [1] BLANCA: Plásticos limpios, cartón seco, papel, Tetra Pak y latas.\n" +
                        $"  • [2] VERDE:  Cáscaras de frutas, té, comida cruda y restos de poda.\n" +
                        $"  • [3] NEGRA:  Servilletas sucias, cajas con grasa, recibos y vasos encerados.\n" +
                        $"  • [4] ROJA:   Peligrosos y especiales (pilas, bombillos, medicamentos, RAEE, aerosoles).\n\n" +
                        $"────────────────────────────────────────\n" +
                        $"💧 Impacto ecológico: Has protegido ~{liters:N0} litros de agua en el río.\n" +
                        $"────────────────────────────────────────\n" +
                        $"¿Quieres retar tu velocidad? Presiona [R] para reiniciar y activa el MODO RETO con [TAB].";
                }
                else
                {
                    // Reto Arcade Mode: full competitive Hall of Fame
                    string lbStr = "";
                    for (int i = 0; i < topEntries.Count && i < 3; i++)
                    {
                        string medal = i == 0 ? "🥇" : (i == 1 ? "🥈" : "🥉");
                        bool isThisSession = ((i + 1) == rankPosition);
                        string youTag = isThisSession ? " ★ (Esta partida)" : "";
                        string entryLabel = $"{topEntries[i].name}{youTag}";
                        lbStr += $"  #{i + 1}  {entryLabel,-28}  —  {topEntries[i].score:N0} pts  {medal}\n";
                    }

                    string namePrompt = isEditingName
                        ? $"▶ EDITANDO NOMBRE: [ {currentPlayerName}_ ]  (Escribe y presiona ENTER)\n"
                        : $"TU NOMBRE REGISTRADO: [ {currentPlayerName} ]  (Presiona [N] para cambiar)\n";

                    endGameImpactSummaryText.text =
                        $"⭐ TU PUNTAJE FINAL:  {finalScore:N0} pts  (Puesto #{rankPosition} en el Ranking)\n" +
                        $"🎖 CALIFICACIÓN:      {rankGrade} - {rankTitle}\n\n" +
                        $"────────────────────────────────────────\n" +
                        $"     TABLA DE MEJORES PUNTAJES (HALL OF FAME)\n" +
                        $"────────────────────────────────────────\n" +
                        lbStr +
                        $"────────────────────────────────────────\n" +
                        namePrompt +
                        $"────────────────────────────────────────\n" +
                        $"• Residuos separados en fuente: {totalClassified} de 12  |  Agua protegida: ~{liters:N0} L\n\n" +
                        $"Presiona [R] para reiniciar el nivel.";
                }
            }
        }
    }
}
