using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneGameManager : MonoBehaviour
{
    public static SceneGameManager Instance;
    CanvasGroup blackCanvas;
    public event System.EventHandler<int> LevelStarted;
    public event System.EventHandler MenuEnter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        blackCanvas = FindFirstObjectByType<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMenu()
    {
        StartCoroutine(LoadMenuScene());
    }

    private IEnumerator LoadMenuScene()
    {
        blackCanvas = FindFirstObjectByType<CanvasGroup>();
        yield return FadeToBlack();

        // Assuming "BattleScene" is the name of the battle scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        MenuEnter?.Invoke(this, EventArgs.Empty);
    }

    public void TransitionToGame(int level)
    {
        StartCoroutine(LoadGameScene(level));
    }

    private IEnumerator LoadGameScene(int level)
    {
        blackCanvas = FindFirstObjectByType<CanvasGroup>();
        yield return FadeToBlack();

        // Assuming "BattleScene" is the name of the battle scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        FindFirstObjectByType<CandyMatrix>().InitiateMatrix(level);
        LevelStarted?.Invoke(this, level);
    }

    private IEnumerator FadeToBlack()
    {
        yield return new WaitForSeconds(0.5f);

        float elapsedTime = 0f;
        float fadeDuration = 0.7f;

        while (elapsedTime < fadeDuration)
        {
            // Incrementally increase the alpha of the canvas group to create a fading effect
            blackCanvas.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the screen is fully black at the end
        blackCanvas.alpha = 1f;

        yield return null;
    }
}
