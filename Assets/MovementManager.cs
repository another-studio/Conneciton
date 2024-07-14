using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using enhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class MovementManager : MonoBehaviour
{
    public float zoomSpeed;
    public float minOrthoSize = 1f; // Minimum orthographic size of the camera
    public float maxOrthoSize = 10f; // Maximum orthographic size of the camera
    public float panningSpeed = 1f; // Panning speed multiplier

    private PlayerInput playerInput;
    private Coroutine zoomCoroutine;
    private InputAction primaryFingerPosition;
    private InputAction secondaryFingerPosition;
    private InputAction secondaryFingerTap;
    private Camera mainCamera;
    private Vector2 previousPrimaryTouchPosition;
    private Vector2 previousSecondaryTouchPosition;
    private bool isPanning;
    private float initialDistance;

    public Toggle isPanningToggle;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        primaryFingerPosition = playerInput.actions.FindAction("PrimaryFingerPosition");
        secondaryFingerPosition = playerInput.actions.FindAction("SecondaryFingerPosition");
        secondaryFingerTap = playerInput.actions.FindAction("SecondaryFingerContact");
        mainCamera = Camera.main;
        enhancedTouch.EnhancedTouchSupport.Enable();
    }

    private void OnEnable()
    {
        secondaryFingerTap.started += OnSecondaryFingerContactStarted;
        secondaryFingerTap.canceled += OnSecondaryFingerContactCanceled;
    }

    private void OnDisable()
    {
        secondaryFingerTap.started -= OnSecondaryFingerContactStarted;
        secondaryFingerTap.canceled -= OnSecondaryFingerContactCanceled;
    }

    private void OnSecondaryFingerContactStarted(InputAction.CallbackContext context)
    {
        if (enhancedTouch.Touch.activeTouches.Count == 2)
        {
            Debug.Log("Secondary finger contact started");
            isPanning = true;
            previousPrimaryTouchPosition = primaryFingerPosition.ReadValue<Vector2>();
            previousSecondaryTouchPosition = secondaryFingerPosition.ReadValue<Vector2>();
            initialDistance = Vector2.Distance(previousPrimaryTouchPosition, previousSecondaryTouchPosition);
            if (zoomCoroutine == null)
            {
                zoomCoroutine = StartCoroutine(ZoomDetection());
            }
        }
    }

    private void OnSecondaryFingerContactCanceled(InputAction.CallbackContext context)
    {
        if (enhancedTouch.Touch.activeTouches.Count < 2)
        {
            Debug.Log("Secondary finger contact canceled");
            isPanning = false;
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }
        }
    }

    IEnumerator ZoomDetection()
    {
        while (true)
        {
            if (enhancedTouch.Touch.activeTouches.Count == 2)
            {
                Vector2 currentPrimaryTouchPosition = primaryFingerPosition.ReadValue<Vector2>();
                Vector2 currentSecondaryTouchPosition = secondaryFingerPosition.ReadValue<Vector2>();
                float currentDistance = Vector2.Distance(currentPrimaryTouchPosition, currentSecondaryTouchPosition);

                float deltaDistance = initialDistance - currentDistance;

                // Calculate target orthographic size based on pinch gesture
                float targetOrthoSize = mainCamera.orthographicSize + deltaDistance * zoomSpeed;

                // Clamp the target orthographic size within the specified range
                targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthoSize, maxOrthoSize);

                // Smoothly lerp towards the target orthographic size
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
                Debug.Log("Orthographic size: " + mainCamera.orthographicSize);
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (isPanning && enhancedTouch.Touch.activeTouches.Count == 2)
        {
            Vector2 currentPrimaryTouchPosition = primaryFingerPosition.ReadValue<Vector2>();
            Vector2 currentSecondaryTouchPosition = secondaryFingerPosition.ReadValue<Vector2>();

            Vector2 primaryTouchDelta = currentPrimaryTouchPosition - previousPrimaryTouchPosition;
            Vector2 secondaryTouchDelta = currentSecondaryTouchPosition - previousSecondaryTouchPosition;
            Debug.Log("Vector 2 dot of panning is " + Vector2.Dot(primaryTouchDelta.normalized, secondaryTouchDelta.normalized));

            if (Vector2.Dot(primaryTouchDelta.normalized, secondaryTouchDelta.normalized) > 0.9f)
            {
                Vector2 averageDelta = (primaryTouchDelta + secondaryTouchDelta) / 2;
                PanCamera(averageDelta);
            }

            previousPrimaryTouchPosition = currentPrimaryTouchPosition;
            previousSecondaryTouchPosition = currentSecondaryTouchPosition;
        }
    }

    private void PanCamera(Vector2 delta)
    {
        Vector3 panMovement = new Vector3(-delta.x, -delta.y, 0) * panningSpeed * Time.deltaTime;
        mainCamera.transform.Translate(panMovement, Space.World);
    }
}
