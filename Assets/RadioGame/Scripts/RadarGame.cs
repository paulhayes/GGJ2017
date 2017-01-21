using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarGame : MonoBehaviour {

    public Signal[] signals;

    [SerializeField]
    GameObject armPivot;

    float xAxis;
    float yAxis;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(xAxis) >= 0.1f || Mathf.Abs(yAxis) >= 0.1f)
        {
            SetArmAngle();
        }
        
    }

    void SetArmAngle()
    {
        float angle = (Mathf.Atan2(yAxis, xAxis) * Mathf.Rad2Deg) - 90f;
        armPivot.transform.localRotation = Quaternion.Euler(armPivot.transform.localRotation.x, armPivot.transform.localRotation.y, angle);
    }
}
