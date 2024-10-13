using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {

            int level = FindFirstObjectByType<CandyMatrix>().level;
            FindFirstObjectByType<SceneGameManager>().TransitionToGame(level);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
