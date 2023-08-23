using System.Collections;
using System.Collections.Generic;
using System.IO;
using LSL;
using UnityEngine;

public class SimpleOutletEvent : MonoBehaviour
{
    public string streamName = "LSLSimple";
    public string streamType = "Markers";
    private StreamOutlet _outlet;
    private string[] _sample = {""};
    
    
    void Start()
    {
        var hash = new Hash128();
        hash.Append(streamName);
        hash.Append(streamType);
        hash.Append(gameObject.GetInstanceID());
        var streamInfo = new StreamInfo(streamName, streamType, 1, LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_string, hash.ToString());
        _outlet = new StreamOutlet(streamInfo);
    }

    public void PushSample(int marker)
    {
        _sample[0] = marker.ToString();
        _outlet.push_sample(_sample);
    }
}
