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
    private GameManager manager;
    private void Start()
    {
        goodbyeJojo = GameoverSound.GetComponent<AudioSource>();
        playerPickupTrash = PointIncreaseSound.GetComponent<AudioSource>();
        goombaPickupTrash = DeliveryIncreaseSound.GetComponent<AudioSource>();
        goombaGetShooed = GoombaShooedSound.GetComponent<AudioSource>();

        manager = GameManager.instance;
        manager.gameOver.AddListener(PlayGameoverSound);
        manager.scoreIncreased.AddListener(PlayPointIncreaseSound);
        manager.goombaDeliveredTrash.AddListener(PlayDeliverIncreaseSound);
        manager.goombaShooed.AddListener(PlayGoombaShooedSound);
        manager.playerSpedup.AddListener(PlaySpeedupSound);
    }

    private void OnDestroy()
    {
        if (manager == null)
            return;
        manager.gameOver.RemoveListener(PlayGameoverSound);
        manager.scoreIncreased.RemoveListener(PlayPointIncreaseSound);
        manager.goombaDeliveredTrash.RemoveListener(PlayDeliverIncreaseSound);
        manager.goombaShooed.RemoveListener(PlayGoombaShooedSound);
        manager.playerSpedup.RemoveListener(PlaySpeedupSound);
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

    private void PlaySpeedupSound()
    {
        if (speedupSound != null)
            speedupSound.Play();
    }
}
