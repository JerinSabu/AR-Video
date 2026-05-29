using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ARInputReader : MonoBehaviour
{
    // Event exposed to other scripts
    public event Action<Vector2> OnScreenTapped;

    private void Update()
    {
        DetectTouch();
    }

    private void DetectTouch()
    {
        // Check if a touchscreen device is currently active
        if (Touchscreen.current == null) return;

        // Verify if the primary touch contact was initiated 
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            
            OnScreenTapped?.Invoke(touchPosition);
        }
    }
}