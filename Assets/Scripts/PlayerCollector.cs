using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private SnowboardControlV2 snowboardController;
    [SerializeField] private GameObject GameOverScreen;

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Collectible"))
        {
            // Collect the item
            snowboardController.AddScore(10f);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            // Handle endgame logic
            snowboardController.AddScore(20f);
            GameOverScreen.SetActive(true);
            Destroy(collision.gameObject);
        }
    }
}
