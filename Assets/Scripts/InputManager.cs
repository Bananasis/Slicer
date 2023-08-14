using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public delegate void OnAim(Vector2 screenPos);
    public static event OnAim onAim;
    
    public delegate void OnThrow();
    public static event OnThrow onThrow;
    
    private NewControls controls;
    private Vector2 inputPosition;
    private bool aiming;
    
    private void Awake()
    {
        controls = new NewControls();
        controls.Enable();
        controls.ScreenInput.Aim.performed += _ =>
        {
            if(aiming) return;
            aiming = true;
            onAim?.Invoke(inputPosition);
            StartCoroutine(Aim());
        };
        controls.ScreenInput.Aim.canceled += _ =>
        {
            aiming = false;
        };
        controls.ScreenInput.Position.performed += context =>
        {
            inputPosition = context.ReadValue<Vector2>();
            if (!aiming) return;
            onAim?.Invoke(inputPosition);
        };
    }

    IEnumerator Aim()
    {
        while (aiming)
        {
            yield return null;
        }
        onThrow?.Invoke();
       
    }
}
