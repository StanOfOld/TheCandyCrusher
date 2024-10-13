using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CandyCounter : MonoBehaviour
{
    [SerializeField] CandyScriptableObject cso;
    TextMeshProUGUI textMesh;
    CandyMatrix matrix;
    public int remaining = 3;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        matrix = FindObjectOfType<CandyMatrix>();
        matrix.MatchFound += ReduceCounter;
    }

    private void ReduceCounter(object sender, CandyMatrix.MatchArgs e)
    {
        if (cso == e.scriptableObject) { remaining -= (int)Math.Floor(e.score - 2); }
        if(remaining < 0) remaining = 0;
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.text = "Remaining: " + remaining;
    }
}
