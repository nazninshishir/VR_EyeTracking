using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float speed = 0.5f; // Speed of movement
    public float distance = 5.0f; // Distance object moves
    public Color targetColor = Color.red; // Color to change to after 30 seconds

    private Vector3 startPosition;
    private float direction = 1.0f; // Initial direction, 1.0f means right, -1.0f means left
    private bool colorChanged = false;
    private bool isGrabbed = false;

    void Start()
    {
        startPosition = transform.position; // Store initial position
    }

    void Update()
    {
        if (!isGrabbed)
        {
            // Move the object
            transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

            // Check if the object has moved the desired distance
            if (Vector3.Distance(startPosition, transform.position) >= distance)
            {
                // Change direction
                direction *= -1.0f;

                // Reset start position to current position
                startPosition = transform.position;
            }

            // Check if 30 seconds have passed and color hasn't changed yet
            if (!colorChanged && Time.timeSinceLevelLoad >= 30f)
            {
                // Change color to red
                GetComponent<Renderer>().material.color = targetColor;
                colorChanged = true;
            }
        }
    }

    // Method to handle when the object is grabbed
    public void Grabbed()
    {
        isGrabbed = true;
        // Optionally, you can stop the object's movement here as well.
    }

    // Method to handle when the object is released
    public void Released()
    {
        isGrabbed = false;
    }
}