using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioClip BoltSFX;
    public AudioClip DarkBoltSFX;
    public AudioClip Select;
    public AudioClip Open;
    public AudioClip Denied;
    
    private AudioSource audioSource;

    void Start() { audioSource = GetComponent<AudioSource>(); }

    public void PlaySFX(AudioClip SFX)
    {
        audioSource.PlayOneShot(SFX);
    }
}
