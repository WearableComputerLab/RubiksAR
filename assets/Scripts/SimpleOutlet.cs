using System;
using System.Collections;
using System.Collections.Generic;
using LSL;
using UnityEngine;

public class SimpleOutlet : MonoBehaviour
{
    public string streamName = "UnityMarkers";
    public string streamName2 = "UnityGaze";
    public string streamType = "Markers";
    public string streamType2 = "Markers";
    private StreamOutlet _outlet;
    private StreamOutlet _outlet2;
    private string[] _sample = {""};
    private string[] _sample2 = { "" };
    
    // Start is called before the first frame update
    void Start()
    {
        var hash = new Hash128();
        hash.Append(streamName);
        hash.Append(streamType);
        hash.Append(gameObject.GetInstanceID());
        StreamInfo streamInfo = new StreamInfo(streamName, streamType, 1, LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_string, hash.ToString());
        _outlet = new StreamOutlet(streamInfo);

        var hash2 = new Hash128();
        hash2.Append(streamName2);
        hash2.Append(streamType2);
        hash2.Append(gameObject.GetInstanceID());
        StreamInfo streamInfo2 = new StreamInfo(streamName2, streamType2, 1, LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_string, hash.ToString());
        _outlet2 = new StreamOutlet(streamInfo2);
    }

    public void SendMarker(string marker)
    {
        _sample[0] = marker;
        _outlet.push_sample(_sample);
    }

    public void SendGaze(string gaze)
    {
        _sample2[0] = gaze;
        _outlet2.push_sample(_sample2);
    }
}
