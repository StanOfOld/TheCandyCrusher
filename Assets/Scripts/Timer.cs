using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] public float timeRemaining = 90f;  // Set how long the timer will last
    public bool timerIsRunning = false;  // Flag to control timer state
    TextMeshProUGUI textMesh;

    public event System.EventHandler TimerZero;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        StartTimer();
    }

    public void StartTimer()
    {
        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;  // Reduce the timeRemaining by the time since the last frame
                DisplayTime(timeRemaining);  // Display the time in a UI element (optional)
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;  // Stop the timer when time runs out
                // You can trigger other actions here when the timer ends
                TimerZero?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }

    // Method to display time in the UI in minutes and seconds format
    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;  // Adjusting to avoid 00:00 displaying for a full second

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  // Calculate minutes
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);  // Calculate seconds

        textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);  // Update the UI Text
    }
}
