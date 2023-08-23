using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ChartVis : MonoBehaviour
{
    public GameObject barRed;
    public GameObject barBlue;
    public GameObject barWhite;

    public void Start()
    {
        EventManager.Instance.OnDataChange.AddListener(UpdateChart);
    }
    
    public void OnDisable()
    {
        EventManager.Instance.OnDataChange.RemoveListener(UpdateChart);
    }

    public void UpdateChart()
    {
        SetChartVis(WebClient.Instance.ColorValues["red"], WebClient.Instance.ColorValues["blue"], WebClient.Instance.ColorValues["white"]);
    }

    public void UpdateChartWrong(string[] wrongNumbers)
    {
        var rand = new Random();

        var selectedSet = wrongNumbers[rand.Next(wrongNumbers.Length)];
        var values = selectedSet.Split('/');
        
        SetChartVis(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
    }

    private void SetChartVis(int red, int blue, int white)
    {
        var tempRed = red + 1;
        foreach (var item in barRed.GetComponentsInChildren(typeof(RectTransform), true))
        {
            if (tempRed > 0)
            {
                item.gameObject.SetActive(true);
                tempRed--;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
            
        }
        
        var tempBlue = blue + 1;
        foreach (var item in barBlue.GetComponentsInChildren(typeof(RectTransform), true))
        {
            if (tempBlue > 0)
            {
                item.gameObject.SetActive(true);
                tempBlue--;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        var tempWhite = white + 1;
        foreach (var item in barWhite.GetComponentsInChildren(typeof(RectTransform), true))
        {
            if (tempWhite > 0)
            {
                item.gameObject.SetActive(true);
                tempWhite--;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        barRed.SetActive(true);
        barBlue.SetActive(true);
        barWhite.SetActive(true);
    }
}
