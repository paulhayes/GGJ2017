using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarGame : MonoBehaviour {

    public Signal[] signals;

    [SerializeField]
    GameObject armPivot, detector;

    [SerializeField]
    float detectorSpeed, detectorMinY, detectorMaxY;

    float xAxis;
    float yAxis;
    float zAxis;

    // Use this for initialization
    void Start() {
        //detector = armPivot.transform.FindChild("Detector").gameObject;
    }

    // Update is called once per frame
    void Update() {
        xAxis = Input.GetAxis("Horizontal");
        yAxis = Input.GetAxis("Vertical");
        zAxis = Input.GetAxis("Z Axis");

        if (Mathf.Abs(xAxis) >= 0.2f || Mathf.Abs(yAxis) >= 0.2f)
        {
            ShowArm();
            SetArmAngle();
            MoveDetector();

        } else
        {
            HideArm();
        }

        Debug.Log(zAxis);
        
    }

    void SetArmAngle()
    {
        float angle = (Mathf.Atan2(yAxis, xAxis) * Mathf.Rad2Deg) - 90f;
        armPivot.transform.localRotation = Quaternion.Euler(armPivot.transform.localRotation.x, armPivot.transform.localRotation.y, angle);
    }

    void ShowArm ()
    {
        armPivot.SetActive(true);
    }

    void HideArm ()
    {
        armPivot.SetActive(false);
    }

    void MoveDetector ()
    {
        Vector3 detectorPos = detector.transform.localPosition;
        /*if (zAxis > 0 && detectorPos.y <= detectorMaxY)
        {
            detector.transform.localPosition = new Vector3(detectorPos.x, detectorPos.y + zAxis * detectorSpeed, detectorPos.z);
        } else if (zAxis < 0 && detectorPos.y >= detectorMinY)
        {
            detector.transform.Translate(armPivot.transform.up * zAxis * detectorSpeed * Time.deltaTime);
        }*/
        if (zAxis >= 0)
            detector.transform.localPosition = new Vector3(detectorPos.x, detectorMinY + (zAxis * detectorMaxY * 2), detectorPos.z);
    }
}
