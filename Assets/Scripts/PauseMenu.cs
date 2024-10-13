using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    Button pauseButton;
    Button resumeButton;
    Button quitButton;
    Transform pausePanel;

    Camera pauseCamera;

    Timer timer;

    CandyMatrix candyMatrix;

    // Start is called before the first frame update
    void Start()
    {
        pausePanel = transform.Find("Panel").transform;
        pauseButton = FindFirstObjectByType<PauseButton>().GetComponent<Button>();
        //Debug.Log(FindFirstObjectByType<PauseButton>());
        resumeButton = pausePanel.Find("ResumeBut").GetComponent<Button>();
        quitButton = pausePanel.Find("QuitBut").GetComponent<Button>();

        pauseCamera = FindFirstObjectByType<PauseCamera>().GetComponent<Camera>();

        timer = FindFirstObjectByType<Timer>();

        candyMatrix = FindFirstObjectByType<CandyMatrix>();

        resumeButton.onClick.AddListener(Resume);
    }

    private void Resume()
    {
        timer.timerIsRunning = true;
        pauseCamera.depth = 0;
        candyMatrix.canSelect = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
