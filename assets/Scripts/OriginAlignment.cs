using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginAlignment : MonoBehaviour
{
    [SerializeField]
    protected DefaultObserverEventHandler vuforiaTarget;

    private bool _vuforiaTargetFound;

    public bool VuforiaTargetFound
    {
        get => _vuforiaTargetFound;
        set => _vuforiaTargetFound = value;
    }
    
    void Start()
    {
        if(vuforiaTarget) transform.position = vuforiaTarget.transform.position;

    }

    void Update()
    {
        if(VuforiaTargetFound)
        {
            transform.position = vuforiaTarget.transform.position;
            transform.rotation = vuforiaTarget.transform.rotation;
        }
        
    }
    
    
}
