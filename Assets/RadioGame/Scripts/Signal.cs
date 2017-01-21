using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Signal {

    public Transform position;

    public float outerRange, innerRange;
    float time;

    public AudioClip clip;
}
