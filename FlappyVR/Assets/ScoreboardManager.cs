using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

using UnityEngine.UI;


public class ScoreboardManager : MonoBehaviour
{
    private int[] topScores = new int[5];
    private string[] playerInitials = new string[5];
    private bool isScoreboardVisible = false;

    public TextMeshProUGUI scoreboardText; // UI Text element to display the scoreboard
    public TextMeshProUGUI inputText;      // UI Text element for user input
    private string currentInitials = "";   // To store player initials during input

    private int currentScore; // The score for the current game
    private InputDevice rightController; // Reference to the right controller

    void Start()
    {
        // Initialize top scores and initials from PlayerPrefs
        LoadScores();
    }

    void Update()
    {
        // Open the scoreboard when "X" button is pressed
        if (rightController.isValid && rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isXPressed) && isXPressed)
        {
            ToggleScoreboard();
        }

        // If the scoreboard is visible and a new score is added, prompt for initials input
        if (isScoreboardVisible && currentScore > topScores[4])
        {
            PromptForInitials();
        }
    }

    public void SetRightController(InputDevice controller)
    {
        rightController = controller;
    }

    void ToggleScoreboard()
    {
        // Toggle the visibility of the scoreboard
        isScoreboardVisible = !isScoreboardVisible;

        if (isScoreboardVisible)
        {
            DisplayScoreboard();
        }
        else
        {
            inputText.text = ""; // Hide initials input
        }
    }

    void DisplayScoreboard()
    {
        // Display the top 5 scores
        scoreboardText.text = "<b>Scoreboard</b>\n";
        for (int i = 0; i < 5; i++)
        {
            scoreboardText.text += $"{i + 1}. {playerInitials[i]} - {topScores[i]}\n";
        }
    }

    void PromptForInitials()
    {
        // If the player is entering initials
        inputText.text = "Enter your initials (2 letters): " + currentInitials;

        // Check for left/right controller button presses to update initials (simulating letter input)
        if (rightController.isValid && rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed) && isPressed)
        {
            currentInitials += "A"; // Simulating adding 'A'
        }

        // If the initials are two characters long, save them
        if (currentInitials.Length == 2)
        {
            SaveTopScore();
            currentInitials = ""; // Reset for future games
        }
    }

    void SaveTopScore()
    {
        // Find the index of the lowest score
        int lowestScoreIndex = -1;
        for (int i = 0; i < topScores.Length; i++)
        {
            if (currentScore > topScores[i])
            {
                lowestScoreIndex = i;
                break;
            }
        }

        // If the player has earned a top score
        if (lowestScoreIndex != -1)
        {
            // Shift down scores and initials
            for (int i = 4; i > lowestScoreIndex; i--)
            {
                topScores[i] = topScores[i - 1];
                playerInitials[i] = playerInitials[i - 1];
            }

            // Insert the new score and initials
            topScores[lowestScoreIndex] = currentScore;
            playerInitials[lowestScoreIndex] = currentInitials;

            // Save the new scores to PlayerPrefs
            SaveScores();
            DisplayScoreboard();
        }
    }

    void SaveScores()
    {
        for (int i = 0; i < topScores.Length; i++)
        {
            PlayerPrefs.SetInt("TopScore" + i, topScores[i]);
            PlayerPrefs.SetString("Initials" + i, playerInitials[i]);
        }
        PlayerPrefs.Save();
    }

    void LoadScores()
    {
        for (int i = 0; i < topScores.Length; i++)
        {
            topScores[i] = PlayerPrefs.GetInt("TopScore" + i, 0); // Default to 0 if no score saved
            playerInitials[i] = PlayerPrefs.GetString("Initials" + i, "AAA"); // Default to 'AAA' if no initials saved
        }
    }

    // Call this method when the game ends
    public void EndGame(int score)
    {
        currentScore = score;

        // Display the scoreboard and prompt for initials if needed
        ToggleScoreboard();
    }
}

