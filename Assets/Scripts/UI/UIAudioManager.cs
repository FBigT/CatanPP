using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    [Header("Settings")]
    public bool allowOverlap = false;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        if (allowOverlap)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
