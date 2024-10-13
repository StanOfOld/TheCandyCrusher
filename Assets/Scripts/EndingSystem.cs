using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EndingSystem : MonoBehaviour
{

    Timer timer;

    CandyMatrix candyMatrix;

    MovesLeft movesLeft;

    bool end = false;

    public float fadeDuration = 2f; // Duration for the fade effect

    CanvasGroup canvasGroup;

    ScoreCanvas scoreCanvas;

    ScoringSystem scoringSystem;

    [SerializeField] CandyCounter[] candyCounters;

    [SerializeField] Sprite starSprite;
    [SerializeField] Sprite starSpriteN;

    [SerializeField] int scoreTwoStar;
    [SerializeField] int scoreThreeStar;

    ScoreCamera scoreCamera;

    // Start is called before the first frame update
    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        candyMatrix = FindFirstObjectByType<CandyMatrix>();
        movesLeft = FindFirstObjectByType<MovesLeft>();

        canvasGroup = FindFirstObjectByType<CanvasGroup>();

        scoreCanvas = FindFirstObjectByType<ScoreCanvas>();

        scoringSystem = FindFirstObjectByType<ScoringSystem>();

        scoreCamera = FindFirstObjectByType<ScoreCamera>();

        timer.TimerZero += HandleEndTimer;
        movesLeft.MovesZero += HandleEndMoves;

        
    }

    private void FixedUpdate()
    {
        if(end)
        {
            candyMatrix.canSelect = false;
        }
    }

    private void HandleEndMoves(object sender, EventArgs e)
    {
        timer.timerIsRunning = false;
        candyMatrix.canSelect = false;

        if(!end)StartCoroutine(FadeScreenToBlack()); // Trigger fade effect
        end = true;
    }

    private void HandleEndTimer(object sender, EventArgs e)
    {
        candyMatrix.canSelect = false;

        if (!end) StartCoroutine(FadeScreenToBlack()); // Trigger fade effect
        end = true;
    }

    private IEnumerator FadeScreenToBlack()
    {
        yield return new WaitUntil(() => candyMatrix.isActive == false);
        yield return new WaitForSeconds(1);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Incrementally increase the alpha of the canvas group to create a fading effect
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the screen is fully black at the end
        canvasGroup.alpha = 1f;

        // You can trigger further events here, such as loading a new scene or displaying the end screen
        yield return OnEndReached();
    }

    private IEnumerator OnEndReached()
    {
        // Add code here for what happens when the game ends, such as showing a game-over screen
        Debug.Log("End of game reached.");

        int finalScore = scoringSystem.score;
        scoreCanvas.transform.Find("ScoreFinal").GetComponent<TextMeshProUGUI>().text = finalScore.ToString();

        //Find if Level Completed or not
        bool success = true;
        foreach(CandyCounter cc in candyCounters)
        {
            if(cc.remaining > 0)
            {
                success = false;
                break;
            }
        }

        scoreCanvas.transform.Find("SFDisplay").GetComponent<TextMeshProUGUI>().text = success ? "Success!" : "Fail";

        Transform stars = scoreCanvas.transform.Find("Stars");

        if (success)
        {
            stars.Find("AtiAi1").GetComponent<UnityEngine.UI.Image>().sprite = starSprite;
            Destroy(scoreCanvas.transform.Find("RetryButton").gameObject);
            scoreCanvas.transform.Find("ContinueButton").transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Continue";
        }
        else
        {
            scoreCanvas.transform.Find("ContinueButton").transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Main Menu";
        }
        if(success && finalScore > scoreTwoStar)
        {
            stars.Find("AtiAi2").GetComponent<UnityEngine.UI.Image>().sprite = starSprite;
        }
        if (success && finalScore > scoreThreeStar)
        {
            stars.Find("AtiAi3").GetComponent<UnityEngine.UI.Image>().sprite = starSprite;
        }

        scoreCamera.GetComponent<Camera>().depth = 2;

        yield return FadeScreenBack();
    }

    private IEnumerator FadeScreenBack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Incrementally decrease the alpha of the canvas group to create a fading effect
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // You can trigger further events here, such as loading a new scene or displaying the end screen
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
