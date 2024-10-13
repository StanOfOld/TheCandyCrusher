using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public static SoundSystem Instance;

    AudioSource audioSource;
    public AudioClip menuSoundtrack;
    public AudioClip[] levelSoundtracks;
    public AudioClip defaultLevelSoundtrack;

    SceneGameManager gameManager;

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
        audioSource = GetComponent<AudioSource>();

        gameManager = FindFirstObjectByType<SceneGameManager>();

        gameManager.LevelStarted += PlayLevelSound;
        gameManager.MenuEnter += PlayMenuSound;
    }

    private void PlayMenuSound(object sender, EventArgs e)
    {
        audioSource.clip = menuSoundtrack;
        audioSource.Play();
    }

    private void PlayLevelSound(object sender, int e)
    {
        Debug.Log(e);
        AudioClip clip;
        if(e > levelSoundtracks.Length)
        {
            Debug.Log("gh");
            clip = defaultLevelSoundtrack;
        }
        else
        {
            clip = levelSoundtracks[e - 1];
        }

        if(e == 0)
        {
            
        }

        if(audioSource.clip != clip) {
            audioSource.clip = clip;
            audioSource.Play(); 
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
