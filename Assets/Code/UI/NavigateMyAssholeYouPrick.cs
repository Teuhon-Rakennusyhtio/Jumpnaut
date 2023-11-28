/*using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NavigateMyAssholeYouPrick : MonoBehaviour
{
    [SerializeField] private Selectable[] navigatables;

    private int highlightedIndex = 0;
    private float switchCooldown = 0.2f; // Cooldown between switching elements
    private float switchCooldownTimer = 0f; // Timer for the cooldown
    private bool settingsMenuActive = false;

    void Start()
    {
        // Set the starting highlighted element
        highlightedIndex = 0;
        HighlightElement(highlightedIndex);
    }

    void Update()
    {
        HandleInput(); // Handle input for navigating elements
    }

    private void HandleInput()
    {
        // Update the switch cooldown timer
        switchCooldownTimer -= Time.deltaTime;

        // Check if the vertical axis is used to navigate through elements
        float verticalInput = Input.GetAxis("Vertical"); // Change to the appropriate axis for your input

        // Change the highlighted element based on vertical input
        if (Mathf.Abs(verticalInput) > 0.1f && switchCooldownTimer <= 0f)
        {
            int direction = (int)Mathf.Sign(verticalInput);
            ChangeHighlightedElement(direction);
        }

        // If any element is highlighted, adjust its value based on horizontal input
        if (highlightedIndex >= 0 && highlightedIndex < navigatables.Length)
        {
            float horizontalInput = Input.GetAxis("Horizontal"); // Change to the appropriate axis for your input

            if (navigatables[highlightedIndex] is Slider slider)
            {
                slider.value += horizontalInput * Time.deltaTime;
                // Adjust the value within the valid range
                slider.value = Mathf.Clamp01(slider.value);
            }
            else if (navigatables[highlightedIndex] is Toggle toggle)
            {
                if (horizontalInput > 0.1f)
                    toggle.isOn = true;
                else if (horizontalInput < -0.1f)
                    toggle.isOn = false;
            }

            // Handle button press for "Back to Main Menu" separately
            if (Input.GetButtonDown("Submit") && navigatables[highlightedIndex] is Button backButton)
            {
                backButton.onClick.Invoke(); // Trigger the back button click
            }
        }
    }

    private void ChangeHighlightedElement(int direction)
    {
        // Change the highlighted element index based on the specified direction
        highlightedIndex += direction;

        // Wrap around if going beyond the array bounds
        if (highlightedIndex < 0)
        {
            highlightedIndex = navigatables.Length - 1;
        }
        else if (highlightedIndex >= navigatables.Length)
        {
            highlightedIndex = 0;
        }

        // Highlight the selected element
        HighlightElement(highlightedIndex);

        // Set the cooldown timer to prevent rapid switching
        switchCooldownTimer = switchCooldown;
    }

    private void HighlightElement(int index)
    {
        if (!settingsMenuActive)
            return;

        // Ensure that the EventSystem is not null
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem is null. Make sure you have an EventSystem in your scene.");
            return;
        }

        // Manually select the highlighted element
        if (index >= 0 && index < navigatables.Length)
        {
            EventSystem.current.SetSelectedGameObject(null); // Deselect the current selection
            EventSystem.current.SetSelectedGameObject(navigatables[index].gameObject);
            navigatables[index].Select(); // Ensure the Selectable is selected (might help with highlighting)
        }
    }

    public void SetSettingsMenuActive(bool active)
    {
        settingsMenuActive = active;
        // When the settings menu becomes active, reset the highlighted index and highlight the first element
        if (settingsMenuActive)
        {
            highlightedIndex = 0;
            HighlightElement(highlightedIndex);
        }
    }
}*/
