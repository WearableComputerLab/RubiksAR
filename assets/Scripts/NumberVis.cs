using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class NumberVis : MonoBehaviour
{

    public TextMeshProUGUI numberRed;
    public TextMeshProUGUI numberBlue;
    public TextMeshProUGUI numberWhite;
    

    public void UpdateNumbers()
    {
        numberRed.text = WebClient.Instance.ColorValues["red"].ToString();
        numberBlue.text = WebClient.Instance.ColorValues["blue"].ToString();
        numberWhite.text = WebClient.Instance.ColorValues["white"].ToString();
    }

    public void UpdateNumbersWrong(string[] wrongNumbers)
    {
        var rand = new Random();
        var selectedSet = wrongNumbers[rand.Next(wrongNumbers.Length)];
        var values = selectedSet.Split('/');

        numberRed.text = values[0];
        numberBlue.text = values[1];
        numberWhite.text = values[2];
    }
    
}
