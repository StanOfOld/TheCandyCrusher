using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CandyEntity : MonoBehaviour
{
    public CandyScriptableObject candyScriptObj;
    public SpriteRenderer spriter;

    public bool inUse;

    bool initiated = false;

    [SerializeField] private Sprite unselectSprite;
    [SerializeField] private Sprite selectSprite;
    [SerializeField] private Sprite adjSprite;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init(CandyScriptableObject candyso)
    {
        spriter = gameObject.transform.Find("Sprite").GetComponent<SpriteRenderer>();
        gameObject.GetComponent<SpriteRenderer>().sprite = unselectSprite;

        candyScriptObj = candyso;
        spriter.sprite = candyso.sprite;

        initiated = true;
    }

    public CandyScriptableObject GetCandyType()
    {
        return candyScriptObj;
    }

    internal void Highlight(int v)
    {
        if(v == 0)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = selectSprite;
        }

        else if (v == 1)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = unselectSprite;
        }

        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = adjSprite;
        }
    }
}
