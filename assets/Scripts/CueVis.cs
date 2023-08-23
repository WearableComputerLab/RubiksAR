using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class CueVis : MonoBehaviour
{
    public Color32 firstCue;
    public Color32 secondCue;
    public Color32 thirdCue;
    public List<RectTransform> cueObjects;
    // Start is called before the first frame update
    void Start()
    {
        // GenerateCueSequence();
    }
    
    public int GenerateCueSequence()
    {
        foreach (var cueObject in cueObjects)
        {
            cueObject.gameObject.SetActive(false);
        }
        var rand = new Random();
        var numCues = rand.Next(1, 3);

        var rand2 = new Random();
        List<int> cueList = new List<int>();
        
        for (int i = 0; i < numCues; i++)
        {
            var num = rand2.Next(cueObjects.Count);
            while (cueList.Contains(num))
            {
                num = rand2.Next(cueObjects.Count);
            }
            cueList.Add(num);
        }
        
        for (int i = 0; i < cueList.Count; i++)
        {
            var cueObject = cueObjects[cueList[i]];
            cueObject.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            var color = GetColorFromIndex(i);
            cueObject.GetComponentInChildren<TextMeshProUGUI>().color = color;
            cueObject.GetComponent<Image>().color = color;
            cueObject.gameObject.SetActive(true);
        }

        return cueList.Count;
    }
    

    private Color GetColorFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return firstCue;
            case 1:
                return secondCue;
            case 2:
                return thirdCue;
            default:
                return Color.white;
        }
    }
}
