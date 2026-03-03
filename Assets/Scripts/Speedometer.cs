using UnityEngine;
using TMPro;
public class Speedometer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedLabel;
    [SerializeField] private string current_speed_label="Current Speed: ";
    [SerializeField] private bool useKmh = true;
    [SerializeField] private int decimals = 1;

    public Rigidbody rb;

    private void Update()
    {
        if (speedLabel == null) return;

        float speed = rb.linearVelocity.magnitude; // m/s

        if (useKmh)
            speed *= 3.6f; // convert m/s → km/h

        speedLabel.text = current_speed_label+speed.ToString($"F{decimals}");
    }
}