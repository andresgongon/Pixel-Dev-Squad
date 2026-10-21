using UnityEngine;

namespace RioLimpio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips (To be assigned by Rol 3)")]
        public AudioClip backgroundMusic;
        public AudioClip correctChordClip;
        public AudioClip errorWaterDropClip;
        public AudioClip selectClickClip;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;
        public bool isMuted = false;

        // Initializes the singleton instance and sets up audio sources on start
        private void Awake()
        {
            // Check if an AudioManager instance already exists to avoid duplicates in the scene
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SetupAudioSources();
        }

        // Starts playing the background music track once the scene loads
        private void Start()
        {
            PlayBackgroundMusic();
        }

        // Configures the AudioSource components if they were not assigned in the Inspector
        private void SetupAudioSources()
        {
            if (musicSource != null)
            {
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }
        }

        // Plays the acoustic background music loop if a clip is assigned
        public void PlayBackgroundMusic()
        {
            if (musicSource != null && backgroundMusic != null && !musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusic;
                musicSource.volume = isMuted ? 0f : musicVolume;
                musicSource.Play();
            }
        }

        // Plays the major chord sound effect when the player classifies an item correctly
        public void PlayCorrectSound()
        {
            if (!isMuted && sfxSource != null && correctChordClip != null)
            {
                sfxSource.PlayOneShot(correctChordClip, sfxVolume);
            }
        }

        // Plays a soft water drop sound effect when the player selects the wrong bin
        public void PlayErrorSound()
        {
            if (!isMuted && sfxSource != null && errorWaterDropClip != null)
            {
                sfxSource.PlayOneShot(errorWaterDropClip, sfxVolume);
            }
        }

        // Plays a short click sound when selecting an item with Click-to-Click mode
        public void PlaySelectSound()
        {
            if (!isMuted && sfxSource != null && selectClickClip != null)
            {
                sfxSource.PlayOneShot(selectClickClip, sfxVolume * 0.7f);
            }
        }

        // Adjusts the background music pitch slightly as the river becomes cleaner
        public void UpdateMusicByRiverProgress(float normalizedProgress)
        {
            if (musicSource != null)
            {
                // Increase music pitch subtly when the river crosses 50% and 100%
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

        // Toggles audio mute state for accessibility in quiet classroom environments
        public void ToggleMute()
        {
            isMuted = !isMuted;
            if (musicSource != null)
            {
                musicSource.volume = isMuted ? 0f : musicVolume;
            }
            if (sfxSource != null)
            {
                sfxSource.volume = isMuted ? 0f : sfxVolume;
            }
        }
    }
}
