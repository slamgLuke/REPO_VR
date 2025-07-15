using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed to reload the scene
using UnityEngine.UI; // Needed for the fade screen

public class PlayerStatus : MonoBehaviour
{
    [Header("References")]
    public OVRPlayerController playerController;
    public Transform cameraRig; // Assign your OVRCameraRig transform
    public Image fadeScreen; // Assign the black UI Image here

    private bool isCaught = false;

    void Start()
    {
        // Make sure the fade screen is transparent at the start
        if (fadeScreen != null)
        {
            fadeScreen.color = new Color(0, 0, 0, 0);
        }
    }

    // This is the public function the ENEMY will call
    public void InitiateDeathSequence(Transform enemyLookAtTarget)
    {
        if (isCaught) return; // Prevent this from running more than once
        isCaught = true;

        StartCoroutine(DeathCoroutine(enemyLookAtTarget));
    }

    private IEnumerator DeathCoroutine(Transform enemyLookAtTarget)
    {
        // 1. Freeze the player's movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 2. Force the player to look at the enemy over a short time
        float lookDuration = 0.5f; // How long it takes to turn the head
        float timer = 0f;
        Quaternion startRotation = cameraRig.rotation;

        while (timer < lookDuration)
        {
            Vector3 lookDirection = enemyLookAtTarget.position - cameraRig.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Smoothly interpolate the camera rig's rotation
            cameraRig.rotation = Quaternion.Slerp(startRotation, targetRotation, timer / lookDuration);

            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 3. Wait for a moment to build suspense
        yield return new WaitForSeconds(1.0f);

        // 4. Fade the screen to black
        float fadeDuration = 1.5f;
        timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeScreen.color = Color.black; // Ensure it's fully black

        // 5. What happens next? (e.g., Reload the scene)
        // For example, to reload the current scene:
        GameState.SetStatus(GameState.Status.NO_WIN);
        SceneManager.LoadScene("MenuScene");
    }
}