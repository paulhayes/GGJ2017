using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisPool : MonoBehaviour {

    public PlayerShip ship;
    public BoxCollider spawnBox;
    public GameObject[] debrisTemplates;
    public int numDebris;
    public float outerSpawnRing;

    GameObject[] spawnedDebris;


	void Start () {
        spawnedDebris = new GameObject[numDebris];
        Bounds box = spawnBox.bounds;
        spawnBox.transform.position = ship.transform.position;

        for (int i = 0; i < numDebris; i++) {
            Vector3 pos = new Vector3(Random.Range(-box.extents.x, box.extents.x), Random.Range(-box.extents.y, box.extents.y), Random.Range(-box.extents.z, box.extents.z));
            spawnedDebris[i] = Instantiate(debrisTemplates[Random.Range(0, debrisTemplates.Length)], spawnBox.transform.position + pos, Quaternion.identity) ;
            spawnedDebris[i].GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere;
        }

	}
	
	void Update () {
        spawnBox.transform.position = ship.transform.position;
	}

    private void OnTriggerExit(Collider other)
    {
        //Debug.LogFormat("Exitied {0}", other);
        other.attachedRigidbody.velocity = Vector3.zero;

        Bounds box = spawnBox.bounds;
        float xSign = Mathf.Sign(ship.body.velocity.x);
        float zSign = Mathf.Sign(ship.body.velocity.z);
        bool spawnSide = Mathf.Abs(ship.body.velocity.normalized.x) > Random.value ;

        Vector3 pos = new Vector3(xSign * Random.Range(!spawnSide ? -box.extents.x : outerSpawnRing* box.extents.x, box.extents.x), Random.Range(-box.extents.y, box.extents.y), zSign*Random.Range(spawnSide?-box.extents.z: outerSpawnRing * box.extents.z, box.extents.z));
        /*
        if (!xBigger && Mathf.Abs(pos.x) < (outerSpawnRing*box.extents.x)) {
            pos.x = Mathf.Sign(pos.x) * outerSpawnRing *  box.extents.x;
        }
        if (xBigger && Mathf.Abs(pos.z) < (outerSpawnRing * box.extents.z))
        {
            pos.z = Mathf.Sign(pos.z) * outerSpawnRing * box.extents.z;
        }
        */
        other.transform.position = pos + spawnBox.transform.position;
    }
}
