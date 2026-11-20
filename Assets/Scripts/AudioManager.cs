
using UnityEngine;

namespace RioLimpio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        private AudioSource comboSource;

        [Header("Audio Clips")]
        public AudioClip backgroundMusic;
        public AudioClip correctChordClip;
        public AudioClip errorWaterDropClip;
        public AudioClip selectClickClip;
        public AudioClip timeWarningClip;
        public AudioClip victoryClip;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;
        public bool isMuted = false;

        [Header("Combo Pitch Settings")]
        public float comboPitchStep = 0.08f;
        public float maxComboPitch = 1.4f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SetupAudioSources();
        }

        private void Start()
        {
            PlayBackgroundMusic();
        }

        private void SetupAudioSources()
        {
            if (musicSource != null)
            {
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = isMuted ? 0f : musicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = isMuted ? 0f : sfxVolume;
            }

            // Create an independent source for combo pitch effects.
            comboSource = gameObject.AddComponent<AudioSource>();
            comboSource.playOnAwake = false;
            comboSource.loop = false;
            comboSource.volume = isMuted ? 0f : sfxVolume;
        }

        public void PlayBackgroundMusic()
        {
            if (musicSource != null && backgroundMusic != null && !musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusic;
                musicSource.volume = isMuted ? 0f : musicVolume;
                musicSource.Play();
            }
        }

        public void PlayCorrectSound()
        {
            PlayCorrectSound(1);
        }

        // Plays the correct sound with progressive pitch based on the current streak.
        public void PlayCorrectSound(int currentStreak)
        {
            if (isMuted || comboSource == null || correctChordClip == null)
                return;

            float pitch = Mathf.Clamp(
                1f + Mathf.Max(0, currentStreak - 1) * comboPitchStep,
                1f,
                maxComboPitch
            );

            comboSource.pitch = pitch;
            comboSource.PlayOneShot(correctChordClip, sfxVolume);
        }

        public void PlaySuccessSound()
        {
            PlayCorrectSound();
        }

        public void PlayErrorSound()
        {
            if (!isMuted && sfxSource != null && errorWaterDropClip != null)
            {
                sfxSource.PlayOneShot(errorWaterDropClip, sfxVolume);
            }
        }

        public void PlaySelectSound()
        {
            if (!isMuted && sfxSource != null && selectClickClip != null)
            {
                sfxSource.PlayOneShot(selectClickClip, sfxVolume * 0.7f);
            }
        }

        // Plays a warning sound when less than 25% of the timer remains.
        public void PlayTimeWarningSound()
        {
            if (!isMuted && sfxSource != null && timeWarningClip != null)
            {
                sfxSource.PlayOneShot(timeWarningClip, sfxVolume);
            }
        }

        // Plays the final victory sound when the player completes the game.
        public void PlayVictorySound()
        {
            if (!isMuted && sfxSource != null && victoryClip != null)
            {
                sfxSource.PlayOneShot(victoryClip, sfxVolume);
            }
        }

        public void UpdateMusicByRiverProgress(float normalizedProgress)
        {
            if (musicSource != null)
            {
                if (normalizedProgress >= 1f)
                {
                    musicSource.pitch = 1.05f;
                }
                else if (normalizedProgress >= 0.5f)
                {
                    musicSource.pitch = 1.02f;
                }
                else
                {
                    musicSource.pitch = 1.0f;
                }
            }
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;

            if (musicSource != null)
                musicSource.volume = isMuted ? 0f : musicVolume;

            if (sfxSource != null)
                sfxSource.volume = isMuted ? 0f : sfxVolume;

            if (comboSource != null)
                comboSource.volume = isMuted ? 0f : sfxVolume;
        }
    }
}
