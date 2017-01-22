using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RadarGame : MonoBehaviour {

    #region Declaring variables
    [Header("Signals")]
    public Signal[] signals;
    public AudioSource[] signalSources;

    [SerializeField]
    AudioSource noiseSource;

    [SerializeField]
    Sprite signalSprite;
    [SerializeField]
    GameObject innerRangeLight, signalDiscoveredText;
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

        if (PlayerShip.unlockedSignals == null) {
            PlayerShip.unlockedSignals = new bool[signals.Length];
        }

        HideSignalDiscoveredText();
        //PlaceSignals();

        signalSources = new AudioSource[signals.Length];
        for (int i = 0; i < signals.Length; i++)
        {
            GameObject sourceObj = new GameObject("Signal Source " + i.ToString());
            signalSources[i] = sourceObj.AddComponent<AudioSource>();
            signals[i].discovered = PlayerShip.unlockedSignals[i];

            if (signals[i].discovered)
            {
                InitializeSignalSprite(signals[i]);
            }

            if (signals[i].clip != null) {
                signalSources[i].clip = signals[i].clip;
                signalSources[i].loop = true;
                signalSources[i].volume = signals[i].maxClipVolume;
                signalSources[i].Play();
            }
        }

        ResetInnerRangeTimer();
        ShowArm();
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log(innerRangeTimer);
        xAxis = Input.GetAxis("Horizontal");
        yAxis = Input.GetAxis("Vertical");
        zAxis = Input.GetAxisRaw("Z Axis");

        if (Mathf.Abs(xAxis) >= 0.1f || Mathf.Abs(yAxis) >= 0.1f)
        {
            
            SetArmAngle();
            SetDetectorPos();
            //ShowArm();

        } else
        {
            //HideArm();
            ResetArmAngle();
            SetDetectorPos();
        }

        if (signals.Length > 0 && signalSources.Length > 0)
            GetSignalStrengths();

        SetTimerSliderSize();

        if (Input.GetButtonDown("Fire3")) {
            SceneManager.LoadScene("SpaceGame");
        }
    }

    void GetSignalStrengths()
    {
        float totalStrength = 0;
        bool anySignalInnerRange = false;
        bool anySignalOuterRange = false;
        Signal strongestSignal = null;
        float strongestSignalStrength = 0;

        for (int i = 0; i < signals.Length; i++)
        {
            float strength = 0;

            if (Vector3.Distance(PlayerShip.position, signals[i].signalPosition) > signalWorldRange)
            {
                HideSignalSprite(signals[i]);
                continue;
            }

            ShowSignalSprite(signals[i]);

            anySignalInnerRange |= (DetectorInInnerRange(signals[i]));

            anySignalOuterRange |= (DetectorInOuterRange(signals[i], ref strength));
            if (strength >= strongestSignalStrength)
            {
                strongestSignal = signals[i];
                strongestSignalStrength = strength;
            }

            if (strength > 0)
            {
                signalSources[i].volume = signals[i].maxClipVolume * strength;
            }
            else
            {
                signalSources[i].volume = 0;
            }
            totalStrength += strength;

        }

        if (anySignalInnerRange) {
            if (!strongestSignal.discovered){
                DecreaseInnerRangeTimer();
            }
            
            innerRangeLight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0, 1, 0);
        } else if (anySignalOuterRange) {
            innerRangeLight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 0.65f, 0);
            ResetInnerRangeTimer();
        } else
        {
            innerRangeLight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 0, 0);
            ResetInnerRangeTimer();
        }
            
        float staticVolume = 1 - totalStrength;
        noiseSource.volume = staticVolume;
        //Debug.Log(staticVolume);
    }

    void InitializeSignalSprite (Signal signal)
    {
        signal.frequency.localScale = new Vector3(signal.innerRange * 2, signal.innerRange * 2, 1);
        Vector3 signalPos = signal.frequency.position;

        SpriteRenderer signalSpriteRender = signal.frequency.gameObject.AddComponent<SpriteRenderer>();
        signalSpriteRender.sprite = signalSprite;
        signalSpriteRender.sortingLayerName = "Radar_Icons";
    }

    void ShowSignalSprite (Signal signal)
    {
        SpriteRenderer signalSpriteRender = signal.frequency.gameObject.GetComponent<SpriteRenderer>();
        if (signalSpriteRender != null)
        {
            signalSpriteRender.enabled = true;
        }
    }

    void HideSignalSprite (Signal signal)
    {
        SpriteRenderer signalSpriteRender = signal.frequency.gameObject.GetComponent<SpriteRenderer>();
        if (signalSpriteRender != null)
        {
            signalSpriteRender.enabled = false;
        }
    }

    /*void PlaceSignals ()
    {
        for (int i = 0; i < signals.Length; i++)
        {
            Vector3 signalPos = signals[i].frequency.position;
            //GameObject newSignal = Instantiate(signalSprite, signalPos, Quaternion.identity) as GameObject;
            newSignal.transform.GetChild(0).localScale = new Vector3(signals[i].innerRange*2, 0.1f, signals[i].innerRange*2);
            newSignal.transform.GetChild(1).localScale = new Vector3(signals[i].outerRange*2, 0.1f, signals[i].outerRange*2);
        }
    }*/

    bool DetectorInInnerRange (Signal signal)
    {
        Vector3 detectorPos = new Vector3(detector.transform.position.x, detector.transform.position.y, 0);
        Vector3 signalPos = new Vector3(signal.frequency.position.x, signal.frequency.position.y, 0);

        float distance = Vector3.Distance(detectorPos, signalPos);
        if (distance <= signal.innerRange)
        {
            if (innerRangeTimer <= 0)
            {
                int index = System.Array.IndexOf<Signal>(signals, signal);
                PlayerShip.unlockedSignals[index] = signal.discovered = true;
                InitializeSignalSprite(signal);
                OnTimerFinish();
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

    void ShowSignalDiscoveredText ()
    {
        signalDiscoveredText.SetActive(true);
        StartCoroutine(WaitForSignalTimer());
    }

    IEnumerator WaitForSignalTimer ()
    {
        yield return new WaitForSeconds(3);
        HideSignalDiscoveredText();
    }

    void HideSignalDiscoveredText ()
    {
        signalDiscoveredText.SetActive(false);
    }

    void ResetInnerRangeTimer ()
    {
        innerRangeTimer = innerRangeTimerLength;
    }

    void OnTimerFinish ()
    {
        ResetInnerRangeTimer();
        ShowSignalDiscoveredText();
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

    void ResetArmAngle()
    {
        armPivot.transform.localRotation = Quaternion.identity;
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
