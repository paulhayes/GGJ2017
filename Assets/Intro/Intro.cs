using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

	void Start () {
        OnIntroComplete();

    }
	
	void Update () {
		
	}

    public void OnIntroComplete() {
        SceneManager.LoadScene("SpaceGame");
    }
}
