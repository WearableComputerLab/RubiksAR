using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

public class SequenceManager : MonoBehaviour
{

    public enum Task { Numbers = 0, Graph = 1}

    [Header("Experiment Settings")]
    public int runCount = 2;
    public int trialCount = 6; 
    public int falseTrialCount = 3;
    public Task startingTask = Task.Numbers;
    

    private bool _experimentStarted;
    private bool _experimentPaused = true;
    private float _time;
    private bool _canSendGaze;
    private bool _alignmentActive = true;

    [Header("Tools")]
    public bool autoStart;
    public GameObject origin;
    public SimpleOutlet outlet;
    public CueVis cueVis;
    public NumberVis numVis;
    public ChartVis chartVis;
    public TextMeshProUGUI counterVis;
    public TextMeshProUGUI infoVis;
    public GameObject frameVis;
    public GameObject frameVisCross;
    public GameObject arrowVis;

    
    public PressableButton startButton;
    public PressableButton trainingButton;

    public MRTKRayInteractor[] rayInteractors;
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


    public void SelectStartingTask(Task value)
    {
        startingTask = value;
        //infoVis.text = "Press start when ready";
    }
    
    public void StartExperiment()
    {
        if (!_experimentStarted)
        {
            _experimentStarted = true;
            StartCoroutine(RunMainSequence());
        }
        if (!_experimentPaused) return;
        //startButton.transform.parent.gameObject.SetActive(false);
        
        _experimentPaused = false;
        infoVis.gameObject.SetActive(false);
    }

    public void StartTraining()
    {
        StartCoroutine(RunTrainingSequence());
        //startButton.transform.parent.gameObject.SetActive(false);
        _experimentPaused = false;
        infoVis.gameObject.SetActive(false);
    }

    private void PauseExperiment()
    {
        _experimentPaused = true;
        //startButton.transform.parent.gameObject.SetActive(true);
        infoVis.text = "Task complete, start when ready";
        infoVis.gameObject.SetActive(true);
    }

    private void ToggleAlignment(bool toggle)
    {
        Debug.Log("Alignment active: " + toggle);
        _alignmentActive = toggle;
        origin.GetComponent<ObjectManipulator>().enabled = toggle;
        origin.GetComponent<BoundsControl>().enabled = toggle;
        origin.GetComponent<BoxCollider>().enabled = toggle;
        cueVis.gameObject.SetActive(toggle);
        foreach(var interactor in rayInteractors)
        {
            interactor.gameObject.SetActive(toggle);
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            ToggleAlignment(!_alignmentActive);
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            StartTraining();
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            StartExperiment();
        }
        
        if (_experimentPaused) return;
        if (!_canSendGaze) return;
        
        _time += Time.deltaTime;
        if (!(_time > 0.02f)) return;
        var eyePos = gazeInteractor.rayEndPoint;
        // Debug.LogError(eyePos);
        //WebClient.Instance.SendStreamPush("GAZE:" + eyePos);
        outlet.SendGaze(eyePos.ToString());
        _time = 0f;
    }

    private List<bool> RandomiseTrials(int trialAmount, int falseTrialAmount)
    {
        var rand = new Random();
        var trials = new List<bool>(trialCount);
            
        for (int i = 0; i < trialAmount - falseTrialAmount; i++)
        {
            trials.Add(true);
        }

        for (int i = 0; i < falseTrialAmount; i++)
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

        return trials;
    }

    private IEnumerator SequenceInitSetup()
    {
        _counterPos = counterVis.transform.position;
        counterVis.transform.position = cueVis.transform.position;
        counterVis.gameObject.SetActive(true);

        yield return CountdownTimer(counterVis, 2, 0.5f);

        counterVis.gameObject.SetActive(false);
        cueVis.gameObject.SetActive(true);
    }

    private IEnumerator SequenceShowCues()
    {
        var cueCount = cueVis.GenerateCueSequence();

        yield return new WaitForSeconds(cueCount);
        cueVis.gameObject.SetActive(false);
    }

    private IEnumerator SequenceDataRequest()
    {
        yield return new WaitForSeconds(0.8f);
        
        arrowVis.SetActive(true);

        _newData = false;
        WebClient.Instance.Send("RESET");
        StartCoroutine(DataRequest());
        
        yield return new WaitUntil(() => _newData);
    }

    private IEnumerator SequenceShowFrame()
    {
        counterVis.transform.position = _counterPos;
        arrowVis.SetActive(false);
        frameVis.SetActive(true);
        yield return new WaitForSeconds(1);
        var r = new Random();
        var randWait = (float) r.NextDouble() * (0.7f - 0.4f) + 0.4f;
        yield return new WaitForSeconds(randWait);
                
        frameVisCross.SetActive(false);
    }

    private IEnumerator SequenceShowNumberVis(bool correct, bool practice)
    {
        numVis.gameObject.SetActive(true);

        if (correct)
        {
            if (!practice) outlet.SendMarker("20");
            // if(!practice) WebClient.Instance.SendStreamPush("20");
            numVis.UpdateNumbers();
        }
        else
        {
            if (!practice) outlet.SendMarker("30");
            // if(!practice) WebClient.Instance.SendStreamPush("30");
            numVis.UpdateNumbersWrong(wrongNumbers);
        }
        if (!practice) _canSendGaze = true;         
        yield return new WaitForSeconds(0.75f);
        numVis.gameObject.SetActive(false);
    }

    private IEnumerator SequenceShowChartVis(bool correct, bool practice)
    {
        chartVis.gameObject.SetActive(true);

        if (correct)
        {
            if (!practice) outlet.SendMarker("21");
            // if (!practice) WebClient.Instance.SendStreamPush("21");
            chartVis.UpdateChart();
        }
        else
        {
            if (!practice) outlet.SendMarker("31");
            // if (!practice) WebClient.Instance.SendStreamPush("31");
            chartVis.UpdateChartWrong(wrongNumbers);
        }
        if(!practice) _canSendGaze = true;
        yield return new WaitForSeconds(0.75f);
        chartVis.gameObject.SetActive(false);
    }

    private IEnumerator RunTrainingSequence()
    {
        var practiceTrials = RandomiseTrials(20, 4);
        
        for (int i = 0; i < 20; i++)
        {
            infoVis.text = i < 10 ? "Number Training Round" : "Graph Training Round";
            WebClient.Instance.SendInfo("Training trial " + i+1);
            infoVis.gameObject.SetActive(true);
            yield return SequenceInitSetup();
            yield return SequenceShowCues();
            yield return SequenceDataRequest();
            yield return SequenceShowFrame();
            if(i < 10)
            {
                yield return SequenceShowNumberVis(practiceTrials[i], true);
            }
            else
            {
                yield return SequenceShowChartVis(practiceTrials[i], true);
            }
            frameVisCross.SetActive(true);
            frameVis.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.75f);
        }
        
        PauseExperiment(); 
        yield return new WaitUntil(() => _experimentPaused == false);
    }
    
    private IEnumerator RunMainSequence()
    {
        for (int run = 0; run < runCount; run++)
        {
            var trials = RandomiseTrials(trialCount, falseTrialCount);
            
            outlet.SendMarker(run % 2 == (int) startingTask ? "10" : "11");
            // WebClient.Instance.SendStreamPush(run % 2 == _startingTask ? "10" : "11");
            
            for (int trial = 0; trial < trialCount; trial++)
            {
                WebClient.Instance.SendTrialCounter(run + 1, trial + 1, trials[trial]);
                outlet.SendMarker(run % 2 == (int) startingTask ? "50" : "51");
                // WebClient.Instance.SendStreamPush(run % 2 == _startingTask ? "50" : "51");
                yield return SequenceInitSetup();

                // WebClient.Instance.SendStreamPush("02");
                outlet.SendMarker("02");
                yield return SequenceShowCues();
                
                // WebClient.Instance.SendStreamPush("05");
                outlet.SendMarker("05");
                yield return SequenceDataRequest();

                // WebClient.Instance.SendStreamPush("03");
                outlet.SendMarker("03");
                yield return SequenceShowFrame();
                
                
                if(run % 2 == (int) startingTask)
                {
                    yield return SequenceShowNumberVis(trials[trial], false);
                }
                else
                {
                    yield return SequenceShowChartVis(trials[trial], false);
                }
                
                frameVisCross.SetActive(true);
                frameVis.gameObject.SetActive(false);
                _canSendGaze = false;
                outlet.SendMarker("04");
                // WebClient.Instance.SendStreamPush("04");
                yield return new WaitForSeconds(0.75f);
                
            }
            // WebClient.Instance.SendStreamPush(run % 2 == _startingTask ? "40" : "41");
            outlet.SendMarker(run % 2 == (int) startingTask ? "40" : "41");
            
            if (run == runCount - 1)
            {
                infoVis.text = "End of experiment!";
                infoVis.gameObject.SetActive(true);
            }
            WebClient.Instance.SendInfo("Run " + run+1 + " has ended!");
            
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
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void DataReceived()
    {
        _newData = true;
    }
}
