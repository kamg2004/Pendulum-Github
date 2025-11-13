using UnityEngine;
using UnityEngine.UI; // Required for Slider, Button, and Text components
using TMPro; // Assuming you are using TextMeshPro

public class ParameterController : MonoBehaviour
{
    // --- ASSIGNMENTS IN INSPECTOR ---
    
    // Reference the Pivot/Simulator script to modify its physics variables
    public PendulumSimulator simulator; 
    
    // Mass UI Elements
    public Slider Mass_slider;
    public TMP_Text Mass_value; // The Text object that displays the current mass number
    
    // Length UI Elements
    public Slider Length_slider;
    public TMP_Text Length_value; // The Text object that displays the current length number

    // Damping UI Elements
    public Slider Damping_slider;
    public TMP_Text Damping_value; // The Text object that displays the current damping value

    // --- NEW UI ELEMENT FOR GRAVITY ---
    public TMP_Dropdown Gravity_dropdown;
    
    // Map of gravity values (must match the order in the Dropdown UI in Unity Editor)
    private float[] gravityValues = { 9.81f, 1.62f, 3.71f, 24.79f }; // Earth, Moon, Mars, Jupiter
    private string[] planetNames = { "Earth (9.81 m/s²)", "Moon (1.62 m/s²)", "Mars (3.71 m/s²)", "Jupiter (24.79 m/s²)" };
    // --------------------------------

    // --- SETUP ---

    void Start()
    {
        // 1. Safety Check
        if (simulator == null)
        {
            Debug.LogError("Simulator reference missing on ParameterController. Please drag the Pivot object into the Simulator slot.");
            return;
        }

        // 2. Configure Mass Slider
        Mass_slider.minValue = 0.1f;
        Mass_slider.maxValue = 10f; // Max mass of 10kg
        Mass_slider.value = simulator.mass;
        Mass_slider.onValueChanged.AddListener(UpdateMass);

        // 3. Configure Length Slider
        Length_slider.minValue = 1f;
        Length_slider.maxValue = 50f; // Max length of 50cm
        Length_slider.value = simulator.length_in_cm;
        Length_slider.onValueChanged.AddListener(UpdateLength);

        // 4. Configure Damping Slider
        Damping_slider.minValue = 0f;
        Damping_slider.maxValue = 0.5f; // Max damping ratio of 0.5
        Damping_slider.value = simulator.dampingFactor;
        Damping_slider.onValueChanged.AddListener(UpdateDamping);
        
        // --- NEW: Configure Gravity Dropdown ---
        ConfigureGravityDropdown();
        Gravity_dropdown.onValueChanged.AddListener(UpdateGravity);
        
        // 5. Initial Display Update (Shows the starting value on the screen)
        UpdateMass(Mass_slider.value);
        UpdateLength(Length_slider.value);
        UpdateDamping(Damping_slider.value);
        // Initial setup of gravity value
        UpdateGravity(Gravity_dropdown.value);
    }
    
    // --- NEW FUNCTION: Populates and sets default ---
    private void ConfigureGravityDropdown()
    {
        // Clear any placeholder options in the Unity Editor
        Gravity_dropdown.ClearOptions();
        
        // Add the defined planet names as options
        Gravity_dropdown.AddOptions(new System.Collections.Generic.List<string>(planetNames));
        
        // Find the index that matches the simulator's current gravity (default 9.81)
        int defaultIndex = System.Array.IndexOf(gravityValues, simulator.gravity);
        if (defaultIndex != -1)
        {
            Gravity_dropdown.value = defaultIndex;
        } else {
            // Default to Earth (index 0) if current gravity is unknown
            Gravity_dropdown.value = 0;
        }
        // Manually refresh to show the selected value immediately
        Gravity_dropdown.RefreshShownValue();
    }
    
    // --- NEW FUNCTION TO HANDLE GRAVITY CHANGE ---
    private void UpdateGravity(int selectionIndex)
    {
        // 1. Get the gravity value based on the index selected in the dropdown
        float newGravity = gravityValues[selectionIndex];
        
        // 2. Update the simulator's gravity using the public method
        simulator.SetGravity(newGravity);
        
        // DEBUG: Log the selected planet and its gravity
        Debug.Log($"Gravity set to: {planetNames[selectionIndex]} ({newGravity} m/s²)");
    }
    // ---------------------------------------------


    // --- UPDATE HANDLERS ---

    // Called when the mass slider is moved
    private void UpdateMass(float newValue)
    {
        // Update the simulator's variable
        simulator.mass = newValue;
        
        // Update the Text component display (e.g., "1.5 kg")
        Mass_value.text = newValue.ToString("F1") + " kg"; 
    }

    // Called when the length slider is moved
    private void UpdateLength(float newLengthInCm)
    {
        // Update the simulator's variable
        simulator.length_in_cm = newLengthInCm;
        
        // Update the Text component display (e.g., "2.5 m")
        Length_value.text = newLengthInCm.ToString("F0") + " cm";
        
        // Update the visual position of the bob to reflect the new length
        float newLengthInMeters = newLengthInCm / 100f;
        // The bob is assumed to be the first child [0] of the Pivot
        Transform bobTransform = simulator.transform.GetChild(0);
        
        // We set the local Y position (downwards) to be the negative of the length
        bobTransform.localPosition = new Vector3(0, -newLengthInMeters*100f, 0); 
    }
    
    // Called when the damping slider is moved
    private void UpdateDamping(float newValue)
    {
        // Update the simulator's variable
        simulator.dampingFactor = newValue;
        
        // Update the Text component display
        Damping_value.text = newValue.ToString("F3"); // Showing more precision for damping
    }
}