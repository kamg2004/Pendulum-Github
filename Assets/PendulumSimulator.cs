using UnityEngine;

public class PendulumSimulator : MonoBehaviour
{
    // Public variables for user manipulation (like in your concept [cite: 26])
    public float length = 1.0f;           // L: Length of the string (m)
    public float mass = 1.0f;             // m: Mass of the bob (kg)
    public float gravity = 9.81f;         // g: Gravitational acceleration (m/s^2)
    public float initialAngle = 45f;      // Initial displacement angle (degrees)
    public float dampingFactor = 0.05f;   // For simulating air resistance [cite: 6]

    // Private variables for simulation
    private float angularVelocity;        // ω (omega)
    private float currentAngleRadians;    // θ (theta)
    private Vector3 initialPosition;      // Starting position relative to pivot

    // Event/Delegate to broadcast data to the graphing scripts
    public delegate void PendulumDataUpdate(float angle, float velocity, float time);
    public static event PendulumDataUpdate OnDataUpdate;

    void Start()
    {
        // Convert initial angle to radians for physics calculations
        currentAngleRadians = initialAngle * Mathf.Deg2Rad;
        
        // Store the initial local position relative to the pivot
        initialPosition = transform.localPosition;
    }

    void FixedUpdate()
    {
        // 1. Calculate Angular Acceleration (α)
        // For a simple pendulum (approximated for small angles: α ≈ -(g/L) * θ)
        // For accurate, non-linear motion: α = -(g/L) * sin(θ)
        float angularAcceleration = -(gravity / length) * Mathf.Sin(currentAngleRadians);

        // 2. Apply Damping (reduces the velocity over time)
        angularAcceleration -= dampingFactor * angularVelocity;

        // 3. Update Angular Velocity (ω)
        // Use a time-step (Time.fixedDeltaTime) for stable physics integration
        angularVelocity += angularAcceleration * Time.fixedDeltaTime;

        // 4. Update Angle (θ)
        currentAngleRadians += angularVelocity * Time.fixedDeltaTime;

        // 5. Update Visual Position (Digital Twin movement [cite: 18])
        float angleDegrees = currentAngleRadians * Mathf.Rad2Deg;
        
        // Rotate the bob around the pivot point (assuming the pivot is at the parent's origin)
        // Use a Quaternion to apply the rotation around the Z-axis
        transform.localRotation = Quaternion.Euler(0, 0, angleDegrees);

        // 6. Broadcast Data for Graphs and LLM Assistant [cite: 20, 27]
        OnDataUpdate?.Invoke(angleDegrees, angularVelocity, Time.time);
    }
}