using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Signal {

    public Transform frequency;

    public float outerRange, innerRange;
    float time;

    public AudioClip clip;

    public Vector3 signalPosition = Vector3.zero;
}
