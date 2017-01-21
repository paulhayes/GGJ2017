using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarGame : MonoBehaviour {

    #region Declaring variables
    [Header("Signals")]
    public Signal[] signals;
    public AudioSource[] signalSources;

    [SerializeField]
    AudioSource noiseSource;

    [SerializeField]
    GameObject signal, innerRangeLight;
    [SerializeField]
    float signalWorldRange;

    [Header("Radar arm & detector")]
    [SerializeField]
    GameObject armPivot, detector;

    [SerializeField]
    float detectorSpeed, detectorMinY, detectorMaxY;

    [Header("Inner Range Timer")]
    [SerializeField]
    float innerRangeTimerLength;
    float innerRangeTimer;

    [SerializeField]
    RectTransform timerSlider;

    float armAngle;

    float xAxis;
    float yAxis;
    float zAxis;
    #endregion

    // Use this for initialization
    void Start() {
        PlaceSignals();

        signalSources = new AudioSource[signals.Length];
        for (int i = 0; i < signals.Length; i++)
        {
            GameObject sourceObj = new GameObject("Signal Source " + i.ToString());
            signalSources[i] = sourceObj.AddComponent<AudioSource>();
            if (signals[i].clip != null) {
                signalSources[i].clip = signals[i].clip;
                signalSources[i].loop = true;
                signalSources[i].Play();
            }
        }

        ResetInnerRangeTimer();
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log(innerRangeTimer);
        xAxis = Input.GetAxis("Horizontal");
        yAxis = Input.GetAxis("Vertical");
        zAxis = Input.GetAxisRaw("Z Axis");

        if (Mathf.Abs(xAxis) >= 0.1f || Mathf.Abs(yAxis) >= 0.1f)
        {
            ShowArm();
            SetArmAngle();
            SetDetectorPos();

        } else
        {
            HideArm();
        }

        if (signals.Length > 0 && signalSources.Length > 0)
            GetSignalStrengths();

        SetTimerSliderSize();
    }

    void GetSignalStrengths()
    {
        float totalStrength = 0;
        bool anySignalInnerRange = false;
        bool anySignalOuterRange = false;
        for (int i = 0; i < signals.Length; i++)
        {
            float strength = 0;

            Debug.Log(PlayerShip.position);

            if (Vector3.Distance(PlayerShip.position, signals[i].signalPosition) > signalWorldRange)
                continue;

            anySignalInnerRange |= (DetectorInInnerRange(signals[i]) && !signals[i].discovered);

            anySignalOuterRange |= (DetectorInOuterRange(signals[i], ref strength) && !signals[i].discovered);

            if (!signals[i].discovered)
            {
                if (strength > 0)
                {
                    signalSources[i].volume = strength;
                }
                else
                {
                    signalSources[i].volume = 0;
                }
                totalStrength += strength;
            } else
            {
                signalSources[i].Stop();
            }

        }

        if (anySignalInnerRange)
        {
            innerRangeLight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0, 1, 0);
            DecreaseInnerRangeTimer();
        } else if (anySignalOuterRange) {
            innerRangeLight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 0.65f, 0);
        } else
        {
            innerRangeLight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 0, 0);
            ResetInnerRangeTimer();
        }
            
        float staticVolume = 1 - totalStrength;
        noiseSource.volume = staticVolume;
        //Debug.Log(staticVolume);
    }

    void PlaceSignals ()
    {
        for (int i = 0; i < signals.Length; i++)
        {
            Vector3 signalPos = signals[i].frequency.position;
            GameObject newSignal = Instantiate(signal, signalPos, Quaternion.identity) as GameObject;
            newSignal.transform.GetChild(0).localScale = new Vector3(signals[i].innerRange*2, 0.1f, signals[i].innerRange*2);
            newSignal.transform.GetChild(1).localScale = new Vector3(signals[i].outerRange*2, 0.1f, signals[i].outerRange*2);
        }
    }

    bool DetectorInInnerRange (Signal signal)
    {
        Vector3 detectorPos = new Vector3(detector.transform.position.x, detector.transform.position.y, 0);
        Vector3 signalPos = new Vector3(signal.frequency.position.x, signal.frequency.position.y, 0);

        float distance = Vector3.Distance(detectorPos, signalPos);
        if (distance <= signal.innerRange)
        {
            if (innerRangeTimer <= 0)
            {
                signal.discovered = true;
            }
            return true;
        }
        return false;
    }

    bool DetectorInOuterRange(Signal signal, ref float strength)
    {
        Vector3 detectorPos = new Vector3(detector.transform.position.x, detector.transform.position.y, 0);
        Vector3 signalPos = new Vector3(signal.frequency.position.x, signal.frequency.position.y, 0);

        float distance = Vector3.Distance(detectorPos, signalPos);
        if (distance <= signal.outerRange)
        {
            strength = Mathf.InverseLerp(signal.outerRange, signal.innerRange, distance);
            return true;
        }

        return false;
    }

    void DecreaseInnerRangeTimer ()
    {
        if(innerRangeTimer >= 0)
            innerRangeTimer -= Time.deltaTime;
    }

    void ResetInnerRangeTimer ()
    {
        innerRangeTimer = innerRangeTimerLength;
    }

    void SetTimerSliderSize ()
    {
        timerSlider.localScale = new Vector2(1 - ((1 / innerRangeTimerLength) * innerRangeTimer), 1);
    }

    void SetArmAngle()
    {
        armAngle = (Mathf.Atan2(yAxis, xAxis) * Mathf.Rad2Deg) - 90f;
        armPivot.transform.localRotation = Quaternion.Euler(armPivot.transform.localRotation.x, armPivot.transform.localRotation.y, armAngle);
    }

    void ShowArm ()
    {
        armPivot.SetActive(true);
    }

    void HideArm ()
    {
        armPivot.SetActive(false);
    }

    void SetDetectorPos ()
    {
        Vector3 detectorPos = detector.transform.localPosition;
        if (zAxis >= 0)
            detector.transform.localPosition = new Vector3(detectorPos.x, detectorMinY + (zAxis * detectorMaxY * 2), detectorPos.z);
    }
}
