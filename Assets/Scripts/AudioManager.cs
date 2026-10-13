using UnityEngine;

public class AudioManager : MonoBehaviour 
{
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

        GameManager.instance.gameOver.AddListener(PlayGameoverSound);
        GameManager.instance.scoreIncreased.AddListener(PlayPointIncreaseSound);
        GameManager.instance.goombaDeliveredTrash.AddListener(PlayDeliverIncreaseSound);
        GameManager.instance.goombaShooed.AddListener(PlayGoombaShooedSound);
    }

    public void PlayGameoverSound()
    {
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
}
