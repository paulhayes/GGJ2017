using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Signal {

    public Transform frequency;
    public Vector3 signalPosition = Vector3.zero;

    public float outerRange, innerRange;
    //float time;

    public bool discovered;
    public AudioClip clip;
    [Range(0, 1)]
    public float maxClipVolume = 1f;
}
