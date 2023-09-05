using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

public class SequenceManager : MonoBehaviour
{
    public int runCount = 2;
    public int trialCount = 6; 
    public int falseTrialCount = 3;
    

    private bool _experimentStarted;
    private bool _experimentPaused = true;
    private int _startingTask;
    private float _time;
    
    public bool autoStart;
    public CueVis cueVis;
    public NumberVis numVis;
    public ChartVis chartVis;
    public TextMeshProUGUI counterVis;
    public TextMeshProUGUI infoVis;
    public GameObject frameVis;
    public GameObject frameVisCross;
    public GameObject arrowVis;
    
    public PressableButton startButton;

    public FuzzyGazeInteractor gazeInteractor;

    private Vector3 _counterPos;
    private bool _newData;

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


    public void SelectStartingTask(bool value)
    {
        _startingTask = value ? 0 : 1;
        infoVis.text = "Press start when ready";
    }
    
    public void StartExperiment()
    {
        if (!_experimentStarted)
        {
            _experimentStarted = true;
            _experimentPaused = false;
            StartCoroutine(RunMainSequence());
        }
        startButton.transform.parent.gameObject.SetActive(false);
        _experimentPaused = false;
        infoVis.gameObject.SetActive(false);
        // WebClient.Instance.SendStreamPush("01");
    }

    private void PauseExperiment()
    {
        _experimentPaused = true;
        startButton.transform.parent.gameObject.SetActive(true);
    }
    
    private void Update()
    {
        // if (_experimentPaused) return;
        // _time += Time.deltaTime;
        // if (!(_time > 0.02f)) return;
        // var eyePos = gazeInteractor.PreciseHitResult.raycastHit.transform.position;
        // WebClient.Instance.SendStreamPush("GAZE:" + eyePos);
        // _time = 0f;
    }

    private IEnumerator RunMainSequence()
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
            
            WebClient.Instance.SendStreamPush(run % 2 == _startingTask ? "10" : "11");
            
            for (int trial = 0; trial < trialCount; trial++)
            {
                
                _counterPos = counterVis.transform.position;
                counterVis.transform.position = cueVis.transform.position;
                counterVis.gameObject.SetActive(true);

                yield return CountdownTimer(counterVis, 2, 0.5f);

                counterVis.gameObject.SetActive(false);
                cueVis.gameObject.SetActive(true);
                //push cues to eeg
                //outlet.PushSample("02");
                WebClient.Instance.SendStreamPush("02");
                var cueCount = cueVis.GenerateCueSequence();

                yield return new WaitForSeconds(cueCount);
                cueVis.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.8f);
                arrowVis.SetActive(true);

                _newData = false;
                StartCoroutine(DataRequest());
                yield return new WaitUntil(() => _newData);
                //data received
                //outlet.PushSample("03");
                WebClient.Instance.SendStreamPush("03");

                counterVis.transform.position = _counterPos;
                
                arrowVis.SetActive(false);
                frameVis.SetActive(true);

                yield return new WaitForSeconds(1);
                
                // frameVis.SetActive(false);

                var r = new Random();
                var randWait = (float) r.NextDouble() * (0.7f - 0.4f) + 0.4f;
                yield return new WaitForSeconds(randWait);
                
                frameVisCross.SetActive(false);
                
                if(run % 2 == _startingTask)
                {
                    numVis.gameObject.SetActive(true);

                    if (trials[trial])
                    {
                        //outlet.PushSample("20");
                        WebClient.Instance.SendStreamPush("20");
                        numVis.UpdateNumbers();
                    }
                    else
                    {
                        //outlet.PushSample("30");
                        WebClient.Instance.SendStreamPush("30");
                        numVis.UpdateNumbersWrong(wrongNumbers);
                    }
                    
                    yield return new WaitForSeconds(0.75f);
                    numVis.gameObject.SetActive(false);
                }
                else
                {
                    chartVis.gameObject.SetActive(true);

                    if (trials[trial])
                    {
                        //outlet.PushSample("21");
                        WebClient.Instance.SendStreamPush("21");
                        chartVis.UpdateChart();
                    }
                    else
                    {
                        //outlet.PushSample("31");
                        WebClient.Instance.SendStreamPush("31");
                        chartVis.UpdateChartWrong(wrongNumbers);
                    }
                    yield return new WaitForSeconds(0.75f);
                    chartVis.gameObject.SetActive(false);
                }
                frameVisCross.SetActive(true);
                frameVis.gameObject.SetActive(false);
                //outlet.PushSample("04");
                WebClient.Instance.SendStreamPush("04");
                yield return new WaitForSeconds(0.75f);
                
            }
            //outlet.PushSample(run % 2 == 0 ? "40" : "41");
            WebClient.Instance.SendStreamPush(run % 2 == _startingTask ? "40" : "41");
            
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
