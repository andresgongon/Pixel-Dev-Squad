using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RioLimpio
{
    public class UIManager : MonoBehaviour
    {
        [Header("Top Left HUD - Cleaning Progress")]
        public Image progressBarFill;
        public Text progressPercentText;
        public Text wasteCounterText;

        [Header("Top Right HUD - Ecosystem Lives")]
        public Image[] fishLifeIcons;
        public Text livesCountText;
        public Text levelStatusText;

        [Header("Background & River States (Sprites assigned by Rol 2)")]
        public Image stationBackgroundImage;
        public Sprite riverPollutedSprite;
        public Sprite riverRecoveringSprite;
        public Sprite riverCleanSprite;

        [Header("Guardian Character (Sprites assigned by Rol 2)")]
        public Image guardianCharacterImage;
        public Sprite guardianIdleSprite;
        public Sprite guardianHappySprite;
        public Sprite guardianEmpathySprite;

        [Header("Feedback Speech Bubble (Sensory Redundancy - Rol 3)")]
        public GameObject feedbackBubbleObject;
        public Text feedbackBubbleText;

        [Header("End Game Impact Panel")]
        public GameObject endGamePanel;
        public Text endGameTitleText;
        public Text endGameImpactSummaryText;

        private Coroutine feedbackCoroutine;
        private Coroutine guardianPoseCoroutine;

        // Updates the cleaning progress bar fill, percentage text, and item counter
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

        // Updates the 3 native red fish icons and numeric life counter in the top-right HUD
        public void UpdateLivesHUD(int remainingLives, Sprite activeFishSprite, Sprite lostFishSprite)
        {
            if (livesCountText != null)
            {
                livesCountText.text = remainingLives.ToString();
            }

            // Loop through each fish icon and update its visual state based on remaining lives
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
                            fishLifeIcons[i].color = new Color(0.91f, 0.30f, 0.24f, 1f);
                        }
                        else
                        {
                            if (lostFishSprite != null)
                            {
                                fishLifeIcons[i].sprite = lostFishSprite;
                            }
                            fishLifeIcons[i].color = new Color(0.35f, 0.35f, 0.35f, 0.55f);
                        }
                    }
                }
            }
        }

        // Transitions the river background across the 3 ecological states (0%, 50%, 100%)
        public void UpdateRiverVisualState(float normalizedProgress)
        {
            if (stationBackgroundImage == null)
            {
                return;
            }

            // Switch the background sprite or fallback color depending on the cleaning percentage
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

        // Displays the visual speech bubble with high-contrast text for auditory redundancy (WCAG 1.2.2)
        public void ShowFeedbackBubble(string message, bool isPositive, float displaySeconds = 2.4f)
        {
            if (feedbackBubbleObject == null || feedbackBubbleText == null)
            {
                return;
            }

            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
            }

            feedbackCoroutine = StartCoroutine(DisplayBubbleRoutine(message, isPositive, displaySeconds));
        }

        // Coroutine that activates the speech bubble and hides it after the timer finishes
        private IEnumerator DisplayBubbleRoutine(string message, bool isPositive, float duration)
        {
            feedbackBubbleObject.SetActive(true);
            feedbackBubbleText.text = message;

            // Set high-contrast text colors (dark green for success, dark red/brown for hints)
            if (isPositive)
            {
                feedbackBubbleText.color = new Color(0.11f, 0.42f, 0.18f, 1f);
            }
            else
            {
                feedbackBubbleText.color = new Color(0.55f, 0.16f, 0.12f, 1f);
            }

            yield return new WaitForSeconds(duration);
            feedbackBubbleObject.SetActive(false);
        }

        // Changes the Guardian Girl expression temporarily on success or error
        public void SetGuardianReaction(bool isSuccess)
        {
            if (guardianCharacterImage == null)
            {
                return;
            }

            if (guardianPoseCoroutine != null)
            {
                StopCoroutine(guardianPoseCoroutine);
            }

            guardianPoseCoroutine = StartCoroutine(GuardianReactionRoutine(isSuccess));
        }

        // Restores the Guardian Girl back to her idle sprite after showing her reaction
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

        // Shows the final screen translating classified items into real-world ecological impact
        public void ShowEndGameSummary(bool victory, int totalClassified, int plasticCount, int organicCount)
        {
            if (endGamePanel == null)
            {
                return;
            }

            endGamePanel.SetActive(true);

            if (endGameTitleText != null)
            {
                endGameTitleText.text = victory
                    ? "¡RÍO ANDINO 100% RECUPERADO!"
                    : "¡INTÉNTALO DE NUEVO, GUARDIÁN!";
            }

            if (endGameImpactSummaryText != null)
            {
                int litersProtected = totalClassified * 15;
                endGameImpactSummaryText.text =
                    "IMPACTO AMBIENTAL REAL LOGRADO:\n\n" +
                    "• Residuos separados en la fuente: " + totalClassified + "\n" +
                    "• Agua del río protegida: ~" + litersProtected + " litros\n" +
                    "• Plásticos y envases recuperados: " + plasticCount + "\n" +
                    "• Orgánicos para compostaje escolar: " + organicCount + "\n\n" +
                    "Presiona la tecla R o el botón inferior para jugar de nuevo.";
            }
        }
    }
}
