using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    [Header("VR Settings")]
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private float canvasScale = 0.003f;
    [Tooltip("Reconfigure VR settings when this is toggled")]
    [SerializeField] private bool reconfigureVR = false;

    private Canvas canvas;
    private bool lastReconfigureState = false;

    // Called when Start button is pressed
    public void StartGame()
    {
        // Clear selection before scene transition
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif

        // Clear the load flag to start a new game
        PlayerPrefs.SetInt("ShouldLoadGame", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneToLoad);
    }

    // Called when Load button is pressed
    public void LoadGame()
    {
        // Clear selection before scene transition
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif

        // Set a flag that tells the game scene to load saved data
        PlayerPrefs.SetInt("ShouldLoadGame", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneToLoad);
    }

    // Called when Quit button is pressed
    public void QuitGame()
    {
        Application.Quit();
    }

    void Start()
    {
        // Configure canvas for VR
        ConfigureCanvasForVR();

        // Ensure no objects are selected to prevent Inspector errors
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
    }

    void Update()
    {
        // Allow reconfiguring VR at runtime when toggle is changed
        if (reconfigureVR != lastReconfigureState)
        {
            lastReconfigureState = reconfigureVR;
            if (reconfigureVR)
            {
                ConfigureCanvasForVR();
            }
        }
    }

    private void ConfigureCanvasForVR()
    {
        // Get the Canvas component
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in parent hierarchy!");
            return;
        }

        // Set canvas to World Space for VR
        canvas.renderMode = RenderMode.WorldSpace;

        // Find the main camera (VR camera)
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera found!");
            return;
        }

        // Get the RectTransform of the canvas
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Center the pivot point
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Apply scale directly (much simpler approach)
        canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        // Position the canvas in front of the camera (centered)
        canvasRect.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;

        // Make it face the camera properly (opposite direction)
        canvasRect.rotation = Quaternion.LookRotation(mainCamera.transform.forward);

        // Ensure canvas has world camera set for proper raycasting
        canvas.worldCamera = mainCamera;

        // Configure VR UI interaction
        ConfigureVRUIInteraction();

        Debug.Log($"Canvas configured for VR - Scale: {canvasRect.localScale}, Distance: {distanceFromCamera}m, Position: {canvasRect.position}");
    }

    private void ConfigureVRUIInteraction()
    {
        // Remove standard GraphicRaycaster if present (doesn't work with VR)
        GraphicRaycaster standardRaycaster = canvas.GetComponent<GraphicRaycaster>();
        if (standardRaycaster != null)
        {
            Destroy(standardRaycaster);
        }

        // Add TrackedDeviceGraphicRaycaster for XR Interaction Toolkit
        TrackedDeviceGraphicRaycaster vrRaycaster = canvas.GetComponent<TrackedDeviceGraphicRaycaster>();
        if (vrRaycaster == null)
        {
            vrRaycaster = canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        Debug.Log("VR UI Raycaster configured");
    }

    private void OnEnable()
    {
        // Clear selection when this script enables
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
    }
}
