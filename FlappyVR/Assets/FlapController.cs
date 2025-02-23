using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class FlapController : MonoBehaviour
{
    public Rigidbody rb;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText; // UI for the timer
    private int score = 0;
    private bool hasStarted = false; // Track if the game has started
    private float remainingTime = 60f; // 1-minute timer
    private bool timerRunning = false;

    public Transform headTransform;

    public float flapStrength = 5f;
    public float forwardBoost = 2f;
    public float flapThreshold = 0.05f;

    private InputDevice leftController;
    private InputDevice rightController;
    private Vector3 previousLeftPos;
    private Vector3 previousRightPos;

    public AudioSource flapSound;
    public AudioSource pipeHitSound;
    public AudioSource scoreSound;
    public AudioSource backgroundNoise;

    private bool isGameOver = false;

    void Start()
    {   
        AudioSource[] audioSources = GetComponents<AudioSource>();
        flapSound = audioSources[0];
        pipeHitSound = audioSources[1];
        scoreSound = audioSources[2];
        backgroundNoise = audioSources[3];

        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;  // Use Unity’s built-in gravity

        // Reset Rigidbody position
        rb.position = new Vector3(-3.5f, 16f, 11f);

        // Freeze rotation to keep player upright
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Display welcome message
        if (scoreText != null)
        {
            scoreText.text = "<b> Welcome to Flappy VR!</b>\nFlap your arms to fly! Collect coins to gain points!\nBeware the faulty platforms.\nPress 'A' to start.";
        }

        if (timerText != null)
        {
            timerText.text = ""; // Hide the timer before the game starts
        }

        // Auto-assign headTransform if not set
        if (headTransform == null)
        {
            headTransform = Camera.main.transform;
        }

        // Rotate camera 270 degrees in Y-axis
        headTransform.rotation = Quaternion.Euler(0, 270, 0);

        // Get VR controllers
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);

        if (devices.Count >= 2)
        {
            leftController = devices[0];
            rightController = devices[1];
        }

        Debug.Log("debugger on");
    }

    void Update()
    {
        if (!hasStarted)
        {
            // Check if the "A" button is pressed to start the game
            if (rightController.isValid && rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed) && isPressed)
            {
                hasStarted = true;
                timerRunning = true;
                isGameOver = false;
                UpdateScoreUI();
                UpdateTimerUI();
            }
        }
        else if (timerRunning)
        {
            // Countdown timer in whole seconds
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;

                if (remainingTime <= 0)
                {
                    remainingTime = 0;
                    timerRunning = false;
                    EndGame("Time's up! \nPress 'B' to restart.");
                }

                UpdateTimerUI();
            }
        }

        // Restart game when pressing "B" after game over
        if (rightController.isValid && rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isBPressed) && isBPressed)
        {
            RestartGame();
        }
        
        // Check if the score is below zero
            if (score <= 0 && hasStarted == true)
            {
                EndGame("<b>Game Over :(</b>\nPress 'B' to Restart");
            }
    }

    void FixedUpdate()
    {
        if (!hasStarted) return; // Stop movement until game starts

        // Detect controller movement
        Vector3 leftPos, rightPos;

        if (leftController.TryGetFeatureValue(CommonUsages.devicePosition, out leftPos) &&
            rightController.TryGetFeatureValue(CommonUsages.devicePosition, out rightPos))
        {
            Vector3 leftVelocity = leftPos - previousLeftPos;
            Vector3 rightVelocity = rightPos - previousRightPos;

            previousLeftPos = leftPos;
            previousRightPos = rightPos;

            float flapSpeed = (leftVelocity.magnitude + rightVelocity.magnitude) / 2;

            Debug.Log("Flap Speed: " + flapSpeed);

            if (flapSpeed > flapThreshold)
            {
                Debug.Log("Flap detected! Applying force.");
                
                float flapMultiplier = Mathf.Clamp(flapSpeed * 2, 1, 3);

                Vector3 force = (Vector3.up * flapStrength * flapMultiplier) +
                                (headTransform.forward * forwardBoost * flapMultiplier);

                rb.AddForce(force, ForceMode.Impulse);
                // Play flap sound
                if (flapSound != null)
                {
                    flapSound.Play();
                }
            }
        }
    }

    void EndGame(string message)
    {
        if (scoreText != null)
        {
            scoreText.text = message;
        }
        timerRunning = false;
        hasStarted = false;
        isGameOver = true;
    }

    void RestartGame()
    {
        // Reset relevant values before reloading
        score = 0;
        remainingTime = 60f;
        UpdateScoreUI();
        UpdateTimerUI();
        rb.position = new Vector3(-3.5f, 16f, 11f); // Reset position
        rb.velocity = Vector3.zero; // Reset velocity

        timerRunning = true;
        isGameOver = false;
        hasStarted = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScoreZone"))
        {
            score++;
            UpdateScoreUI();
            scoreSound.Play();
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pipe"))
        {
            // Vibrate both controllers
            TriggerHapticFeedback(leftController, 0.7f, 0.2f); // 70% strength for 0.2 seconds
            TriggerHapticFeedback(rightController, 0.7f, 0.2f);

            // Decrease score
            score--;

            // Update score display
            UpdateScoreUI();

            // Remove the collided pipe from the scene
            Destroy(collision.gameObject);

            pipeHitSound.Play();

            // Check if the score is below zero
            if (score <= 0 && hasStarted == true)
            {
                EndGame("<b>Game Over :(</b>\nPress 'B' to Restart");
            }
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"Time: {minutes}:{seconds:D2}"; // Ensures double-digit seconds (e.g., 1:09, 1:08)
        }
    }

    void TriggerHapticFeedback(InputDevice device, float amplitude, float duration)
    {
        if (device.isValid)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
}







