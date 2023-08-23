using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class SequenceManager : MonoBehaviour
{
    public int runCount = 2;
    public int trialCount = 6; 
    public int falseTrialCount = 3;

    private bool _experimentStarted;
    private bool _experimentPaused;
    public bool autoStart;
    
    public CueVis cueVis;
    public NumberVis numVis;
    public ChartVis chartVis;
    public TextMeshProUGUI counterVis;
    public TextMeshProUGUI infoVis;
    public GameObject frameVis;
    public PressableButton startButton;

    private Vector3 _counterPos;
    private bool _newData;

    public SimpleOutletEvent outlet;

    public string[] wrongNumbers;

    void Start()
    {
        EventManager.Instance.OnDataChange.AddListener(DataReceived);
        if (autoStart) StartExperiment();
    }

    public void OnDisable()
    {
        EventManager.Instance.OnDataChange.RemoveListener(DataReceived);
    }

    public void StartExperiment()
    {
        if (!_experimentStarted)
        {
            _experimentStarted = true;
            StartCoroutine(RunMainSequence(2));
        }
        startButton.transform.parent.gameObject.SetActive(false);
        _experimentPaused = false;
        outlet.PushSample(1);
    }

    private void PauseExperiment()
    {
        _experimentPaused = true;
        startButton.transform.parent.gameObject.SetActive(true);
    }
    

    private IEnumerator RunMainSequence(int startAfter)
    {
        for (int run = 0; run < runCount; run++)
        {
            //Randomising trials that are wrong and correct for duration of run
            var rand = new Random();
            var trials = new List<bool>(trialCount);
            
            for (int i = 0; i < trialCount - falseTrialCount; i++)
            {
                trials.Add(true);
            }

            for (int i = 0; i < falseTrialCount; i++)
            {
                trials.Add(false);
            }

            var numTrials = trials.Count;
            while (numTrials > 1)
            {
                numTrials--;
                var randIndex = rand.Next(numTrials + 1);
                (trials[randIndex], trials[numTrials]) = (trials[numTrials], trials[randIndex]);
                
            }
            
            for (int trial = 0; trial < trialCount; trial++)
            {
                infoVis.text = run % 2 == 0 
                    ? "Condition: 1 (Numerical) \nTrial No: " + (trial + 1) 
                    : "Condition: 2 (Chart)\nTrial No: " + (trial + 1);
                infoVis.gameObject.SetActive(true);
                _counterPos = counterVis.transform.position;
                counterVis.transform.position = cueVis.transform.position;
                counterVis.gameObject.SetActive(true);

                yield return CountdownTimer(counterVis, startAfter, 0.5f);

                counterVis.gameObject.SetActive(false);
                cueVis.gameObject.SetActive(true);
                var cueCount = cueVis.GenerateCueSequence();

                yield return new WaitForSeconds(cueCount);
                cueVis.gameObject.SetActive(false);

                _newData = false;
                StartCoroutine(DataRequest());
                yield return new WaitUntil(() => _newData);
                // _newData = false;

                counterVis.transform.position = _counterPos;
                
                frameVis.SetActive(true);

                yield return new WaitForSeconds(1.5f);
                
                frameVis.SetActive(false);

                if(run % 2 == 0)
                {
                    numVis.gameObject.SetActive(true);
                    
                    if (trials[trial]) numVis.UpdateNumbers();
                    else numVis.UpdateNumbersWrong(wrongNumbers);
                    
                    yield return new WaitForSeconds(0.5f);
                    numVis.gameObject.SetActive(false); 
                }
                else
                {
                    chartVis.gameObject.SetActive(true);
                    
                    if (trials[trial]) chartVis.UpdateChart();
                    else chartVis.UpdateChartWrong(wrongNumbers);
                    yield return new WaitForSeconds(0.5f);
                    chartVis.gameObject.SetActive(false);
                }

                yield return new WaitForSeconds(2);
            }

            if (run == runCount - 1)
            {
                infoVis.text = "End of experiment!";
            }
            PauseExperiment(); 
            yield return new WaitUntil(() => _experimentPaused == false);
        }
    }


    private IEnumerator CountdownTimer(TextMeshProUGUI textObject, int seconds, float interval)
    {
        var counter = seconds;
        while (counter > 0)
        {
            textObject.text = counter.ToString();
            yield return new WaitForSeconds(interval);
            counter--;
        }  
    }

    private IEnumerator DataRequest()
    {
        while(!_newData)
        {
            WebClient.Instance.SendGetRequest();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void DataReceived()
    {
        _newData = true;
    }
}
