using UnityEngine;

public class AudioManager : MonoBehaviour 
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource speedupSound;
    public GameObject GameoverSound;
    public GameObject PointIncreaseSound;
    public GameObject DeliveryIncreaseSound;
    public GameObject GoombaShooedSound;
    private AudioSource goodbyeJojo;
    private AudioSource playerPickupTrash;
    private AudioSource goombaPickupTrash;
    private AudioSource goombaGetShooed;
    private void Start()
    {
        goodbyeJojo = GameoverSound.GetComponent<AudioSource>();
        playerPickupTrash = PointIncreaseSound.GetComponent<AudioSource>();
        goombaPickupTrash = DeliveryIncreaseSound.GetComponent<AudioSource>();
        goombaGetShooed = GoombaShooedSound.GetComponent<AudioSource>();
    }
    public void PlayGameoverSound()
    {
        if (backgroundMusic != null)
            backgroundMusic.Stop();
        goodbyeJojo.Play();
    }
    public void PlayPointIncreaseSound()
    {
        playerPickupTrash.Play();
    }
    public void PlayDeliverIncreaseSound()
    {
        goombaPickupTrash.Play();
    }
    public void PlayGoombaShooedSound()
    {
        goombaGetShooed.Play();
    }
    public void PlaySpeedupSound()
    {
        if (speedupSound != null)
            speedupSound.Play();
    }
}
