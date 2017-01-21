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
    public ParticleSystem[] steeringThruster;
    public Rigidbody body;
    public AudioSource collisionAudioSource;
    public AudioSource thrustNoise;


    [System.Serializable]
    public class CollisionSound {
        public float impactThreshold;
        public AudioClip[] clips;
        [HideInInspector]
        public int lastIndex;
    }

    public CollisionSound[] collisionsSounds;

    public bool mapMode;
    public float zoomInPos = 0;
    public float zoomOutPos = 2000;
    public float zoomSmooth;
    public float zoomMasSpeed;
    public float thrust;
    public float thrustRotation;

    float zoomSpeed;

    public static Vector3 position = Vector3.zero;

    public float cameraEase;
    ParticleSystem.EmissionModule emissions;
    ParticleSystem.EmissionModule[] steeringEmitters;



	void Start () {
        emissions = thrustParticles.emission;
        steeringEmitters = new ParticleSystem.EmissionModule[steeringThruster.Length];

        for (int i = 0; i < steeringThruster.Length; i++) {
            steeringEmitters[i] = steeringThruster[i].emission;
        }
	}
	
	void Update () {
        if (Input.GetButtonDown("Fire4")) {
            mapMode = !mapMode;
        }

        emissions.enabled = Input.GetAxis("Z Axis")>0;


        position = body.position;

    }

    protected void LateUpdate()
    {
        //transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private void FixedUpdate()
    {
        float thrustInput = Mathf.Clamp01(Input.GetAxis("Z Axis"));
        body.AddForce(thrust * body.transform.forward * thrustInput );
        /*
        if (thrustInput == 0 && thrustNoise.isPlaying) {
            thrustNoise.Stop();
        }
        if (thrustInput > 0 && !thrustNoise.isPlaying) {
            thrustNoise.Play();
        }
        */
        thrustNoise.volume = thrustInput;

        //body.AddTorque(thrustRotation * Input.GetAxis("Horizontal") * Vector3.up );
        Vector3 rotThrust = Vector3.zero;
        if (Input.GetAxis("Horizontal") != 0)
        {
            rotThrust =  thrustRotation * Input.GetAxis("Horizontal") * Vector3.up;
            
        }
        else {
            rotThrust = -thrustRotation * body.angularVelocity;
        }

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
        Vector3 pos = playerCameras.position;
        pos.y = Mathf.SmoothDamp(pos.y, mapMode ? zoomOutPos : zoomInPos, ref zoomSpeed, zoomSmooth, zoomMasSpeed);

        float threshold = 0.1f;
        blurFX.enabled = Mathf.InverseLerp(zoomInPos, zoomOutPos, pos.y) < threshold;
        mainCamera.enabled = Mathf.InverseLerp(zoomInPos, zoomOutPos, pos.y) < threshold;

        //float zoom = Mathf.InverseLerp(zoomOutPos, zoomInPos, pos.y);
        //Time.timeScale = zoom;

        pos.x += Time.unscaledDeltaTime * cameraEase * (ship.position.x - pos.x);
        pos.z += Time.unscaledDeltaTime * cameraEase * (ship.position.z - pos.z);

        playerCameras.position = pos;

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
                collisionAudioSource.PlayOneShot(collisionsSounds[i].clips[collisionsSounds[i].lastIndex], Mathf.Clamp01(mag/2f));
                break;
            }

        }
    }

}
