using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameButtons : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera movingCamera;
    [SerializeField] private GameObject endPosition;

    [Header("Cameras")]
    public Camera menuCamera;

    [Header("Settings")]
    public float transitionDuration = 2.0f; // How long the move takes
    public GameObject menuUI; // The buttons/text to hide immediately

    private void Start()
    {
        playerController.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Retry
    public void RetryScene()
    {       
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartCoroutine(TransitionToGame());
    }

    // Quit
    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }

    // Start
    public void StartGame()
    {
        StartCoroutine(TransitionToGame());
    }

    // Settings
    public void OpenSettings()
    {
        playerController.settingsMenu.gameObject.SetActive(true);
    }

    // Close Settings
    public void Close()
    {
        playerController.settingsMenu.gameObject.SetActive(false);
    }

    private IEnumerator TransitionToGame()
    {
        // 1. Hide the Menu UI so we can see the transition
        if (menuUI != null) menuUI.SetActive(false);
        playerController.gameObject.SetActive(true);

        // 2. Store starting values
        float timeElapsed = 0f;
        Vector3 startPos = menuCamera.transform.position;
        Quaternion startRot = menuCamera.transform.rotation;

        // Get the target (Player Camera) position/rotation
        // Ensure player cam is in the right spot but disabled for now
        Vector3 endPos = playerController.playerCamera.transform.position;
        Quaternion endRot = playerController.playerCamera.transform.rotation;

        // 3. Smoothly move over time
        while (timeElapsed < transitionDuration)
        {
            // Calculate how far along we are (0 to 1)
            float t = timeElapsed / transitionDuration;

            // Optional: SmoothStep makes it start and stop slowly (Ease In/Out)
            t = t * t * (3f - 2f * t);

            // Move Position
            menuCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            // Rotate Rotation
            menuCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            timeElapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 4. Snap to final position to be precise
        menuCamera.transform.position = endPos;
        menuCamera.transform.rotation = endRot;

        // 5. Swap Cameras
        menuCamera.gameObject.SetActive(false);
        playerController.playerCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 6. Disable this Menu Manager object (optional)
        gameObject.SetActive(false);
    }
}
