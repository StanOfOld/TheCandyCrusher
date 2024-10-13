using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public int score = 0;
    TextMeshProUGUI textMesh;
    CandyMatrix matrix;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        matrix = FindObjectOfType<CandyMatrix>();

        matrix.MatchFound += AddScore;
    }

    private void AddScore(object sender, CandyMatrix.MatchArgs e)
    {
        score += (int) Math.Floor(100 * (e.score - 2));
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.text = "Score: " + score;
    }
}