using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class PendulumSimulator : MonoBehaviour
{
    // Public variables for user manipulation (like in your concept [cite: 26])
    public float length_in_cm = 100.0f; 			// L: Length of the string (cm)
    public float mass = 1.0f;             // m: Mass of the bob (kg)
    public float gravity = 9.81f;         // g: Gravitational acceleration (m/s^2)
    public float initialAngle = 45f;      // Initial displacement angle (degrees)
    public float dampingFactor = 0.05f; 	// For simulating air resistance [cite: 6]

    public LineRenderer ropeLine;
    public Transform bobTransform;
    
    // Private variables for simulation
    private float angularVelocity; 		// ω (omega)
    private float currentAngleRadians; 	// θ (theta)
    private Vector3 initialPosition; 	// Starting position relative to pivot
    private float lengthInMeters;
    
    // Existing variables for grab/simulation...
    public SphereCollider bobCollider; 
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool simulationRunning = false; 


    // Event/Delegate to broadcast data to the graphing scripts
    public delegate void PendulumDataUpdate(float angle, float velocity, float time);
    public static event PendulumDataUpdate OnDataUpdate;

    void Start()
    {
        // 1. Get the Bob's transform (Assuming the bob is the first child of the Pivot)
        if (bobTransform == null)
        {
            bobTransform = transform.GetChild(0);
        }

        // 2. Setup the Line Renderer (Rope)
        if (ropeLine == null)
        {
            ropeLine = GetComponent<LineRenderer>();
        }
        if (ropeLine != null)
        {
            ropeLine.positionCount = 2; // Start point (Pivot) and end point (Bob)
            ropeLine.useWorldSpace = false; // Positions are relative to the Pivot
        }
        
        // 3. Setup Grab Interaction (Existing Logic)
        if (bobCollider != null)
        {
            grabInteractable = bobCollider.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.AddListener(OnGrabStart);
                grabInteractable.selectExited.AddListener(OnGrabEnd);
            }
        }

        // Conversion from CM to Meters
        lengthInMeters = length_in_cm / 100f;

        // Convert initial angle to radians for physics calculations
        currentAngleRadians = initialAngle * Mathf.Deg2Rad;
        
        // Store the initial local position relative to the pivot (Not strictly needed for rotation-based physics)
        initialPosition = transform.localPosition;
        
        // Ensure simulation starts running after setup
        simulationRunning = true;
    }
    
    // --- CORRECTED PUBLIC FUNCTION TO SET GRAVITY ---
    public void SetGravity(float newGravityValue)
    {
        gravity = newGravityValue;
        
        // --- REMOVED: Do NOT reset angularVelocity or currentAngleRadians ---
        // The pendulum will immediately start swinging according to the new gravity
        // from its current position and velocity, correctly simulating the change.
        
        // If the simulation was stopped (e.g., if it was released at the bottom center
        // before the gravity change), give it a small push to restart movement.
        if (simulationRunning == false)
        {
             // Optional: Give a tiny starting angle if it was at rest (0,0) before the change
             currentAngleRadians = 5f * Mathf.Deg2Rad;
             simulationRunning = true;
        }
    }
    // ------------------------------------------

    void FixedUpdate()
    {
        if (!simulationRunning) 
        {
             // Still run the Line Renderer update even when stopped, 
             // to show the current rope position when the user is holding it.
            UpdateRopeVisuals();
            return; 
        }

        // --- CONVERSION: Convert the user's input (cm) to meters for physics ---
        // Must update this every frame in case the ParameterController changed the value
        lengthInMeters = length_in_cm / 100f;

        // 1. Calculate Angular Acceleration (α) - MUST use meters
        // α = -(g/L) * sin(θ)
        float angularAcceleration = -(gravity / lengthInMeters) * Mathf.Sin(currentAngleRadians);

        // 2. Apply Damping (reduces the velocity over time)
        angularAcceleration -= dampingFactor * angularVelocity; 

        // 3. Update Angular Velocity (ω)
        angularVelocity += angularAcceleration * Time.fixedDeltaTime;

        // 4. Update Angle (θ)
        currentAngleRadians += angularVelocity * Time.fixedDeltaTime;

        // 5. Update Visual Position (Rotate the Pivot)
        float angleDegrees = currentAngleRadians * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, angleDegrees); // Assuming Z-axis swing

        // 6. Broadcast Data for Graphs and LLM Assistant
        OnDataUpdate?.Invoke(angleDegrees, angularVelocity, Time.time);
        
        // Update rope visuals during simulation
        UpdateRopeVisuals();
    }
    
    // --- NEW FUNCTION TO UPDATE ROPE VISUALS ---
    void UpdateRopeVisuals()
    {
        if (ropeLine != null && bobTransform != null)
        {
            // Position 0 is the Pivot (local position 0,0,0)
            ropeLine.SetPosition(0, Vector3.zero);
            // Position 1 is the Bob (use its local position since ropeLine is not in World Space)
            ropeLine.SetPosition(1, bobTransform.localPosition);
        }
    }
    
    // ... OnGrabStart and OnGrabEnd functions (Existing Logic) ...
    private void OnGrabStart(SelectEnterEventArgs args)
    {
        simulationRunning = false; 
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        // 1. Get the new angle based on the bob's release position
        Vector3 bobLocalPos = grabInteractable.transform.localPosition;
        
        // Atan2 gives the angle in radians between the X-axis and the vector (x, y)
        // Note: Using -y because the bob hangs down along the negative Y-axis.
        currentAngleRadians = Mathf.Atan2(bobLocalPos.x, -bobLocalPos.y);

        angularVelocity = 0; // Starts from rest
        simulationRunning = true; // Start the physics simulation
    }
}