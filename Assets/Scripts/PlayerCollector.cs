using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    private SnowboardController snowboardController;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Collectible"))
        {
            // Collect the item
            //snowboardController.score += 10;
            Destroy(collision.gameObject);
        }
    }
}
