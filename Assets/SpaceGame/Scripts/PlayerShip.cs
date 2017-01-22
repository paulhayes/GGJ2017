using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class PlayerShip : MonoBehaviour {

    public static Vector3 position = new Vector3(0, -40, 0);
    public static float rotation = 0;
    public static bool[] unlockedSignals;

    public Camera mainCamera;
    public Transform playerCameras;
    public Transform mapCentrePosition;
    public Transform ship;
    public BlurOptimized blurFX;
    public ParticleSystem thrustParticles;
    public ParticleSystem[] steeringThruster;
    public Rigidbody body;
    public AudioSource collisionAudioSource;
    public AudioSource thrustNoise;
    public AudioSource steeringNoise;
    public Camera uiMapCamera;


    [System.Serializable]
    public class CollisionSound {
        public float impactThreshold;
        public AudioClip[] clips;
        [HideInInspector]
        public int lastIndex;
    }

    public CollisionSound[] collisionsSounds;

    public float zoomInPos = 0;
    public float zoomOutPos = 2000;
    public float zoomSmooth;
    public float zoomMasSpeed;
    public float thrust;
    public float thrustRotation;

    Vector3 zoomSpeed;


    public float cameraEase;
    ParticleSystem.EmissionModule emissions;
    ParticleSystem.EmissionModule[] steeringEmitters;
    Vector3 easedCameraPosition;
    Vector3  rotThrust;
    float thrustInput;
    float steeringInput;

    bool mapMode;
    bool zooming;

    void Start () {
        uiMapCamera.enabled = mapMode;
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, rotation, 0);

        var mapCenterPos = mapCentrePosition.position;
        mapCenterPos.y = zoomOutPos;
        mapCentrePosition.position = mapCenterPos;

        var shipCamPosition = transform.position;
        shipCamPosition.y = zoomInPos;

        easedCameraPosition = mapMode ? mapCentrePosition.position : shipCamPosition;
        playerCameras.position = easedCameraPosition;

        emissions = thrustParticles.emission;
        steeringEmitters = new ParticleSystem.EmissionModule[steeringThruster.Length];

        for (int i = 0; i < steeringThruster.Length; i++) {
            steeringEmitters[i] = steeringThruster[i].emission;
        }
	}
	
	void Update () {
        if (Input.GetButtonDown("Fire4")) {
            mapMode = !mapMode;
            zooming = true;
            if(!mapMode) uiMapCamera.enabled = mapMode;

        }

        emissions.enabled = Input.GetAxis("Z Axis")>0;


        position = body.position;
        rotation = body.rotation.eulerAngles.y;


        thrustInput = Mathf.Clamp01(Input.GetAxis("Z Axis"));
        steeringInput = Input.GetAxis("Horizontal");

        steeringNoise.volume = Mathf.Clamp01(rotThrust.magnitude / thrustRotation) * 0.5f;
        thrustNoise.volume = thrustInput;
        if (steeringInput != 0)
        {
            rotThrust = thrustRotation * steeringInput * Vector3.up;
        }
        else
        {
            rotThrust = -thrustRotation * body.angularVelocity;
        }

        
        Vector3 pos = playerCameras.position;
        Vector3 dest = mapMode ? mapCentrePosition.position : easedCameraPosition;

        if (zooming)
        {
            pos = Vector3.SmoothDamp(pos, dest, ref zoomSpeed, zoomSmooth, zoomMasSpeed, Time.unscaledDeltaTime);

            if (Vector3.Distance(pos,dest)<0.31f)
            {
                zooming = false;
                uiMapCamera.enabled = mapMode;
            }
        }
        else if(!mapMode){
            pos = easedCameraPosition;
        }

        playerCameras.position = pos;
        float threshold = 0.1f;
        blurFX.enabled = Mathf.InverseLerp(zoomInPos, zoomOutPos, pos.y) < threshold;
        mainCamera.enabled = Mathf.InverseLerp(zoomInPos, zoomOutPos, pos.y) < threshold;

        float zoom = Mathf.InverseLerp(zoomOutPos, zoomInPos, pos.y);
        Time.timeScale = ( zooming || mapMode ) ? 0 : 1;

        if ( Input.GetButtonDown("Fire3" ) ) {
            Time.timeScale = 1f;
            
            SceneManager.LoadScene("RadioGame");

        }

    }

    protected void LateUpdate()
    {
        //transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private void FixedUpdate()
    {
        body.AddForce(thrust * body.transform.forward * thrustInput );
        /*
        if (thrustInput == 0 && thrustNoise.isPlaying) {
            thrustNoise.Stop();
        }
        if (thrustInput > 0 && !thrustNoise.isPlaying) {
            thrustNoise.Play();
        }
        */

        //body.AddTorque(thrustRotation * Input.GetAxis("Horizontal") * Vector3.up );
        
        body.angularVelocity += rotThrust;

        float thresh = 0.01f;
        bool thrustersOn = rotThrust.magnitude > thresh;
        bool thrusterRight = rotThrust.y > thresh;
        bool thrusterLeft = rotThrust.y < -thresh;

        steeringEmitters[0].enabled = steeringEmitters[2].enabled = thrustersOn && thrusterLeft;
        steeringEmitters[1].enabled = steeringEmitters[3].enabled = thrustersOn && thrusterRight;

        //body
        CameraPos();

    }

    void CameraPos() {
        if (!zooming) {
            Vector3 pos = easedCameraPosition;

            pos.x += Time.deltaTime * cameraEase * (ship.position.x - pos.x);
            pos.z += Time.deltaTime * cameraEase * (ship.position.z - pos.z);

            easedCameraPosition = pos;
        }


    }

    private void OnCollisionEnter(Collision collision)
    {
        float mag = collision.relativeVelocity.magnitude;
        float mass = collision.collider.attachedRigidbody.mass;
        Debug.Log(mag);
        for (int i = 0; i < collisionsSounds.Length; i++) {
            if (mass <= collisionsSounds[i].impactThreshold) {
                collisionsSounds[i].lastIndex = (collisionsSounds[i].lastIndex + Random.Range(1, collisionsSounds[i].clips.Length-1) ) % collisionsSounds[i].clips.Length;
                Debug.Log(collisionsSounds[i].clips[collisionsSounds[i].lastIndex].name);
                collisionAudioSource.PlayOneShot(collisionsSounds[i].clips[collisionsSounds[i].lastIndex], 1f);
                break;
            }

        }
    }

}
