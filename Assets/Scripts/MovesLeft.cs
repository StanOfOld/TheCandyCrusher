using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovesLeft : MonoBehaviour
{
    TextMeshProUGUI textMesh;
    CandyMatrix matrix;
    public int movesl = 15;
    bool canMove = true;

    public event System.EventHandler MovesZero;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        matrix = FindObjectOfType<CandyMatrix>();
        matrix.SwappingOccured += reduceMoves;

    }

    private void reduceMoves(object sender, EventArgs e)
    {
        movesl--;
        if (movesl < 0)
        {
            movesl = 0;
        }

        else if (movesl == 0)
        {
            MovesZero?.Invoke(this, System.EventArgs.Empty);
        }
        textMesh.text = "Moves Left: " + movesl;
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove)
        {
            if(movesl <= 0)
            {
                canMove = false;
            }
        }

        textMesh.text = "Moves Left: " + movesl;
    }
}
