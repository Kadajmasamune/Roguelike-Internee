using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioClip BoltSFX;
    public AudioClip DarkBoltSFX;
    public AudioClip Select;
    public AudioClip Open;
    public AudioClip Denied;
    public AudioClip Attack;
    public AudioClip Kick;
    public AudioClip SpinAttack;



    private AudioSource audioSource;

    void Start() { audioSource = GetComponent<AudioSource>(); }

    public void PlaySFX(AudioClip SFX, float Pitch)
    {
        audioSource.pitch = Pitch;
        audioSource.PlayOneShot(SFX);
    }
}
