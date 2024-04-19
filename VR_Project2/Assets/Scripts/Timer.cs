using UnityEngine;
using System;
using System.IO;

public class Timer : MonoBehaviour
{
    private bool isGameRunning = false;
    private float startTime;
    private string filePath;

    void Start()
    {
        // Set the file path to the specific location where you want to save the file
        // Example: Save to a directory called "GameData" in the project root folder
        string directoryPath = Application.dataPath + "/GameData"; // Path to the directory
        string fileName = "game_times.txt"; // Name of the file
        filePath = Path.Combine(directoryPath, fileName); // Combine directory path and file name
    }

    public void StartGame()
    {
        isGameRunning = true;
        startTime = Time.time; // Record the start time when the game starts
    }

    public void EndGame()
    {
        if (isGameRunning)
        {
            isGameRunning = false;
            float gameTime = Time.time - startTime; // Calculate the time played in this session

            // Save the game time to the file
            SaveGameTime(gameTime);
        }
    }

    // Method to save game time to a file
    private void SaveGameTime(float gameTime)
    {
        try
        {
            // Ensure the directory exists before writing to the file
            string directoryPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directoryPath);

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(gameTime);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving game time: " + e.Message);
        }
    }
}

