using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    Button pauseButton;
    Camera pauseCamera;
    Timer timer;
    CandyMatrix candyMatrix;

    // Start is called before the first frame update
    void Start()
    {
        pauseButton = GetComponent<Button>();

        pauseCamera = FindFirstObjectByType<PauseCamera>().GetComponent<Camera>();
        timer = FindFirstObjectByType<Timer>();
        candyMatrix = FindFirstObjectByType<CandyMatrix>();
        Debug.Log("trial");
        pauseButton.onClick.AddListener(() =>
        {
            Debug.Log("Pause");
            if (candyMatrix.canSelect)
            {
                timer.timerIsRunning = false;
                pauseCamera.depth = 2;
                candyMatrix.canSelect = false;
            }
        });

        if (pauseButton == null)
        {
            Debug.LogError("Pause Button component not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*private void Pause()
    {
        Debug.Log("Pause");
        if (candyMatrix.canSelect)
        {
            timer.timerIsRunning = false;
            pauseCamera.depth = 2;
            candyMatrix.canSelect = false;
        }
    }*/
}
