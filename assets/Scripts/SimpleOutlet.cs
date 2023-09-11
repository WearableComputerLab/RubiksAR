using System;
using System.Collections;
using System.Collections.Generic;
using LSL;
using UnityEngine;

public class SimpleOutlet : MonoBehaviour
{
    public string streamName = "LSLUnity";
    public string streamType = "Markers";
    private StreamOutlet _outlet;
    private string[] _sample = {""};
    
    // Start is called before the first frame update
    void Start()
    {
        var hash = new Hash128();
        hash.Append(streamName);
        hash.Append(streamType);
        hash.Append(gameObject.GetInstanceID());
        StreamInfo streamInfo = new StreamInfo(streamName, streamType, 1, LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_string, hash.ToString());
        _outlet = new StreamOutlet(streamInfo);
    }

    public void SendMarker(string marker)
    {
        _sample[0] = marker;
        _outlet.push_sample(_sample);
    }
}
