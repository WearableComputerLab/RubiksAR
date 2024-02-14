using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class WebClient : MonoBehaviour
{
    public static WebClient Instance { get; private set; }

    private WebSocket ws;
    private float t;


    public bool serverConnect = false;
    public string serverAddress;

    private string _colorString;
    private Dictionary<string, int> _colorValues;

    public Dictionary<string, int> ColorValues
    {
        get => _colorValues;
        private set => _colorValues = value;
    }

    private void OnEnable()
    {
        Instance = this;
        ColorValues = new Dictionary<string, int>
        {
            {"red", 0},
            {"blue", 0},
            {"white", 0}
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!serverConnect) return;
        
        ws = new WebSocket("ws://" + serverAddress + ":5564");
    }

    // Update is called once per frame
    void Update()
    {
        if (!serverConnect) return;
        t += Time.deltaTime;
        
        if (t >= 1f)
        {
            if (ws.ReadyState != WebSocketState.Open)
            {
                Connect();
            }
            else
            {
                // ws.Send("GET");
                t = 0f;
            }
        }
        
    }

    private void Connect()
    {
        // ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

        // Debug.Log("Webserver Connection Attempt: " + ws.ReadyState);

        try
        {
            ws.Connect();
        }
        catch (Exception)
        {
            Debug.Log("Webserver Connection Attempt: Connection failed, trying again");
            return;
        }
        
        Debug.Log("Webserver Connection Attempt: " + ws.ReadyState);
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message Received from " + ((WebSocket)sender).Url + ", Data : " + e.Data);
            SortDataIntoValues(e.Data);
        };
    }

    private void SortDataIntoValues(string data)
    {
        if (ws == null) return; 
        if (!data.Contains("GET:")) return;

        var colorString = data.Replace("GET:", "");

        if (colorString == _colorString)
        {
            EventManager.Instance.EventQueue.Enqueue(() => EventManager.Instance.OnDataChange.Invoke());
            ws.Send("SUCCESS");
            return;
        }
        _colorString = colorString;

        var colors = colorString.Split(',');
        var values = new Dictionary<string, int>()
        {
            {"red", 0},
            {"blue", 0},
            {"white", 0}
        };
        foreach (var color in colors)
        {
            values[color] += 1;
        }
        
        ColorValues = values;
        EventManager.Instance.EventQueue.Enqueue(() => EventManager.Instance.OnDataChange.Invoke());
        ws.Send("SUCCESS");
    }

    public void SendGetRequest()
    {
        ws?.Send("GET");
    }

    public void SendStreamPush(string marker)
    {
        ws?.Send("MARKER:" + marker);
    }

    public void SendTrialCounter(int run, int trial, bool falseAnswer)
    {
        ws?.Send("RUN/TRIAL:" + run + "/" + trial + " False Answer: " + falseAnswer);
    }

    public void SendInfo(string message)
    {
        ws?.Send("INFO: " + message);
    }

    public void Send(string message)
    {
        ws?.Send(message);
    }
}
