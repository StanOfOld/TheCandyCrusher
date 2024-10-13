using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CandyData", menuName = "ScriptableObjects/CandyData")]
public class CandyScriptableObject : ScriptableObject
{
    public string candyType;
    public Sprite sprite;
}
