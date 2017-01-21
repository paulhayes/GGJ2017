using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerShip : MonoBehaviour {

    public Camera mainCamera;
    public Transform playerCameras;
    public Transform ship;
    public BlurOptimized blurFX;
    public ParticleSystem thrustParticles;
    public Rigidbody body;

    public bool mapMode;
    public float zoomInPos = 0;
    public float zoomOutPos = 2000;
    public float zoomSmooth;
    public float zoomMasSpeed;
    public float thrust;

    float zoomSpeed;

    public float cameraEase;

	void Start () {
		
	}
	
	void Update () {
        if (Input.GetButtonDown("Fire2")) {
            mapMode = !mapMode;
        }

        thrustParticles.emission.enabled = Input.GetAxis("Z Axis")>0;
        

        CameraPos();
	}

    void CameraPos() {
        Vector3 pos = playerCameras.position;
        pos.y = Mathf.SmoothDamp(pos.y, mapMode ? zoomOutPos : zoomInPos, ref zoomSpeed, zoomSmooth, zoomMasSpeed);

        blurFX.enabled = Mathf.InverseLerp(zoomInPos, zoomOutPos, pos.y) < 0.5f;
        mainCamera.enabled = Mathf.InverseLerp(zoomInPos, zoomOutPos, pos.y) < 0.5f;

        pos.x += cameraEase * (ship.position.x - pos.x);
        pos.z += cameraEase * (ship.position.z - pos.z);

        playerCameras.position = pos;

    }


}
