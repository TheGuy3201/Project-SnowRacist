using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    [Header("VR Canvas Settings")]
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float canvasScale = 0.0006f;

    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera vrCamera;

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
        vrCamera = Camera.main;
        if (vrCamera == null)
        {
            Debug.LogWarning("No main camera found!");
            return;
        }

        // Cache the RectTransform
        canvasRect = canvas.GetComponent<RectTransform>();

        // Set canvas scale
        canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        // Ensure canvas has world camera set for proper raycasting
        canvas.worldCamera = vrCamera;

        // Configure VR UI interaction
        ConfigureVRUIInteraction();
    }

    void Update()
    {
        FollowCamera();
    }

    private void FollowCamera()
    {
        if (vrCamera == null || canvasRect == null)
        {
            return;
        }

        // Calculate target position in front of the camera
        Vector3 targetPosition = vrCamera.transform.position + vrCamera.transform.forward * distanceFromCamera;

        // Smoothly move canvas to target position
        canvasRect.position = Vector3.Lerp(canvasRect.position, targetPosition, followSpeed * Time.deltaTime);

        // Make canvas face the camera
        Quaternion targetRotation = Quaternion.LookRotation(vrCamera.transform.forward);
        canvasRect.rotation = Quaternion.Slerp(canvasRect.rotation, targetRotation, followSpeed * Time.deltaTime);
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

        // Configure EventSystem for VR input
        ConfigureEventSystem();

        Debug.Log("VR UI Raycaster configured");
    }

    private void ConfigureEventSystem()
    {
        // Find or create EventSystem
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
        }

        // Remove StandaloneInputModule if present (doesn't work with VR)
        StandaloneInputModule standaloneInput = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneInput != null)
        {
            Destroy(standaloneInput);
        }

        // Add XRUIInputModule for VR controller input
        XRUIInputModule xrInputModule = eventSystem.GetComponent<XRUIInputModule>();
        if (xrInputModule == null)
        {
            xrInputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }

        Debug.Log("EventSystem configured for VR input");
    }

    private void OnEnable()
    {
        // Clear selection when this script enables
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
    }
}
