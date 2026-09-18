using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource rwrWarningSource;
    [SerializeField] private AudioSource radioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateEngineAudio(float throttle, bool afterburner)
    {
        if (engineSource == null) return;
        engineSource.pitch = Mathf.Lerp(0.75f, 1.45f, throttle);
        engineSource.volume = afterburner ? 1.0f : Mathf.Lerp(0.3f, 0.75f, throttle);
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void SetRWRActive(bool active)
    {
        if (rwrWarningSource == null) return;
        if (active && !rwrWarningSource.isPlaying)
            rwrWarningSource.Play();
        else if (!active && rwrWarningSource.isPlaying)
            rwrWarningSource.Stop();
    }
}
