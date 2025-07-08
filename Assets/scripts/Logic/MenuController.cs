using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    
    [Header("Scene Settings")]
    [SerializeField] private string mainSceneName = "MainScene";
    [SerializeField] private bool preloadScene = true;
    [SerializeField] private float preloadDelay = 2f; // Delay before starting preload
    
    // Preloading variables
    private AsyncOperation sceneLoadOperation;
    private bool isScenePreloaded = false;
    
    void Start()
    {
        // Find the START button if not assigned
        if (startButton == null)
        {
            startButton = GameObject.Find("StartButton")?.GetComponent<Button>();
            
            // Alternative: Find by searching for a button with "Start" in its name
            if (startButton == null)
            {
                Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
                foreach (Button button in buttons)
                {
                    if (button.name.ToLower().Contains("start"))
                    {
                        startButton = button;
                        break;
                    }
                }
            }
        }
        
        // Subscribe to button click event
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
            Debug.Log("START button found and connected!");
        }
        else
        {
            Debug.LogWarning("START button not found! Please assign it in the inspector or make sure it exists in the scene.");
        }
        
        // Start preloading the scene if enabled
        if (preloadScene)
        {
            StartCoroutine(PreloadMainSceneDelayed());
        }
    }
    
    void Update()
    {
        // Optional: Allow starting with keyboard for testing
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnStartButtonPressed();
        }
    }
    
    /// <summary>
    /// Called when the START button is pressed
    /// </summary>
    public void OnStartButtonPressed()
    {
        Debug.Log("START button pressed! Loading MainScene...");
        LoadMainScene();
    }
    
    /// <summary>
    /// Load the main scene
    /// </summary>
    private void LoadMainScene()
    {
        // If scene is preloaded, activate it instantly
        if (isScenePreloaded && sceneLoadOperation != null)
        {
            Debug.Log("Activating preloaded scene - instant loading!");
            sceneLoadOperation.allowSceneActivation = true;
        }
        else
        {
            // Fallback to normal loading if preload failed or wasn't enabled
            Debug.Log("Loading scene normally (not preloaded)");
            
            // Check if the scene exists in build settings
            if (Application.CanStreamedLevelBeLoaded(mainSceneName))
            {
                SceneManager.LoadScene(mainSceneName);
            }
            else
            {
                Debug.LogError($"Scene '{mainSceneName}' not found in Build Settings! Please add it to File > Build Settings > Scenes in Build");
                
                // Try with index 1 as fallback (assuming index 0 is menu, index 1 is main)
                if (SceneManager.sceneCountInBuildSettings > 1)
                {
                    Debug.Log("Trying to load scene at index 1...");
                    SceneManager.LoadScene(1);
                }
            }
        }
    }
    
    /// <summary>
    /// Preload the main scene with a delay
    /// </summary>
    private IEnumerator PreloadMainSceneDelayed()
    {
        // Wait for the specified delay before starting preload
        yield return new WaitForSeconds(preloadDelay);
        
        // Check if the scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(mainSceneName))
        {
            Debug.Log($"Starting to preload scene '{mainSceneName}'...");
            yield return StartCoroutine(PreloadMainScene());
        }
        else
        {
            Debug.LogWarning($"Cannot preload scene '{mainSceneName}' - not found in Build Settings!");
        }
    }
    
    /// <summary>
    /// Preload the main scene in the background
    /// </summary>
    private IEnumerator PreloadMainScene()
    {
        // Start loading the scene asynchronously but don't activate it yet
        sceneLoadOperation = SceneManager.LoadSceneAsync(mainSceneName);
        sceneLoadOperation.allowSceneActivation = false; // Don't activate until we're ready
        
        // Wait until the scene is 90% loaded (Unity stops at 90% when allowSceneActivation is false)
        while (sceneLoadOperation.progress < 0.9f)
        {
            Debug.Log($"Preloading progress: {sceneLoadOperation.progress * 100f:F1}%");
            yield return null;
        }
        
        isScenePreloaded = true;
        Debug.Log($"Scene '{mainSceneName}' preloaded successfully! Ready for instant activation.");
    }
    
    /// <summary>
    /// Get the current preload progress (0 to 1)
    /// </summary>
    public float GetPreloadProgress()
    {
        if (sceneLoadOperation != null)
        {
            return sceneLoadOperation.progress;
        }
        return 0f;
    }
    
    /// <summary>
    /// Check if the scene is preloaded and ready
    /// </summary>
    public bool IsScenePreloaded()
    {
        return isScenePreloaded;
    }

    void OnDestroy()
    {
        // Stop preloading if still in progress
        if (sceneLoadOperation != null && !isScenePreloaded)
        {
            sceneLoadOperation = null;
        }
        
        // Unsubscribe from button events to prevent memory leaks
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonPressed);
        }
    }
}
