using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.U2D.IK;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Sources")]
    [SerializeField] private AudioClip defaultMusicClip;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------- MUSIC ----------
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlayDefaultMusic()
    {
        if (defaultMusicClip == null) return;

        musicSource.clip = defaultMusicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ---------- SFX ----------
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayRandomClip(AudioClip clipA, AudioClip clipB)
    {
        if (sfxSource == null || clipA == null || clipB == null)
            return;

        AudioClip chosen = (Random.value < 0.5f) ? clipA : clipB;

        sfxSource.clip = chosen;
        sfxSource.Play();
    }
}
