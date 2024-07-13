using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MovementManager : MonoBehaviour
{
    public float zoomSpeed;
    public float minOrthoSize = 1f; // Minimum orthographic size of the camera
    public float maxOrthoSize = 10f; // Maximum orthographic size of the camera

    
    private PlayerInput playerInput;
    private TouchControl control;
    private Coroutine zoomCoroutine;
    private InputAction primaryFingerPosition;
    private InputAction secondaryFingerPosition;
    private InputAction secondaryFingerTap;
    private Camera mainCamera;
    private float initialDistance;
    private Vector2 previousPrimaryTouchPosition;
    private Vector2 previousSecondaryTouchPosition;    
    private bool isPanning;

    private void Awake(){
        playerInput = GetComponent<PlayerInput>();
        primaryFingerPosition = playerInput.actions.FindAction("PrimaryFingerPosition");
        secondaryFingerPosition = playerInput.actions.FindAction("SecondaryFingerPosition");
        secondaryFingerTap = playerInput.actions.FindAction("SecondaryFingerContact");
        mainCamera = Camera.main;
    }

    private void OnEnable(){
        secondaryFingerTap.started += ZoomStart;
        secondaryFingerTap.canceled += ZoomEnd;
        secondaryFingerTap.started += OnSecondaryFingerContactStarted;
        secondaryFingerTap.canceled += OnSecondaryFingerContactCanceled;
    }

    private void OnDisable(){
        secondaryFingerTap.started -= ZoomStart;
        secondaryFingerTap.canceled -= ZoomEnd;

        secondaryFingerTap.started -= OnSecondaryFingerContactStarted;
        secondaryFingerTap.canceled -= OnSecondaryFingerContactCanceled;
    }

    private void OnSecondaryFingerContactStarted(InputAction.CallbackContext context)
    {
        isPanning = true;
        previousPrimaryTouchPosition = primaryFingerPosition.ReadValue<Vector2>();
        previousSecondaryTouchPosition = secondaryFingerPosition.ReadValue<Vector2>();
    }

    private void OnSecondaryFingerContactCanceled(InputAction.CallbackContext context)
    {
        isPanning = false;
    }

    private void ZoomStart(InputAction.CallbackContext context)
    {
        zoomCoroutine = StartCoroutine(ZoomDetection());
    }
    private void ZoomEnd(InputAction.CallbackContext context)
    {
        StopCoroutine(zoomCoroutine);
    }


    IEnumerator ZoomDetection(){
        while(true){
             float currentDistance = Vector2.Distance(primaryFingerPosition.ReadValue<Vector2>(), secondaryFingerPosition.ReadValue<Vector2>());

            float deltaDistance = initialDistance - currentDistance;

            // Calculate target orthographic size based on pinch gesture
            float targetOrthoSize = mainCamera.orthographicSize + deltaDistance * zoomSpeed;

            // Clamp the target orthographic size within the specified range
            targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthoSize, maxOrthoSize);

            // Smoothly lerp towards the target orthographic size
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);

            if (isPanning)
            {
                Vector2 currentPrimaryTouchPosition = primaryFingerPosition.ReadValue<Vector2>();
                Vector2 currentSecondaryTouchPosition = secondaryFingerPosition.ReadValue<Vector2>();

                Vector2 primaryTouchDelta = currentPrimaryTouchPosition - previousPrimaryTouchPosition;
                Vector2 secondaryTouchDelta = currentSecondaryTouchPosition - previousSecondaryTouchPosition;

                if (Vector2.Dot(primaryTouchDelta.normalized, secondaryTouchDelta.normalized) > .9f)
                {
                    Vector2 averageDelta = (primaryTouchDelta + secondaryTouchDelta) / 2;
                    PanCamera(averageDelta);
                }

                previousPrimaryTouchPosition = currentPrimaryTouchPosition;
                previousSecondaryTouchPosition = currentSecondaryTouchPosition;
            }
        }
    }

     private Vector2 GetAverageTouchPosition()
    {
        Vector2 primaryTouch = primaryFingerPosition.ReadValue<Vector2>();
        Vector2 secondaryTouch = secondaryFingerPosition.ReadValue<Vector2>();
        return (primaryTouch + secondaryTouch) / 2;
    }

    private void PanCamera(Vector2 delta)
    {
        Vector3 panMovement = new Vector3(-delta.x, -delta.y, 0) * Time.deltaTime;
        mainCamera.transform.Translate(panMovement, Space.World);
    }
}
