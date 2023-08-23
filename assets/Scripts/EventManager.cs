using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public ConcurrentQueue<Action> EventQueue { get; private set; }

    public UnityEvent OnDataChange;

    private void OnEnable()
    {
        Instance = this;
        EventQueue = new ConcurrentQueue<Action>();
    }
    

    // Update is called once per frame
    void Update()
    {
        if (!EventQueue.TryPeek(out var result)) return;
        while (EventQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }
}
