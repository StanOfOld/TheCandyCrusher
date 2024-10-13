using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {

            FindFirstObjectByType<SceneGameManager>().ReturnToMenu();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
