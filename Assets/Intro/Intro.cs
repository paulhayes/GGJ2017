using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

    public GameObject[] screens;
    int index = 0;

	void Start () {
        screens[index].SetActive(true);
    }

    void Update () {
        if (Input.anyKeyDown || Input.GetButtonDown("Fire1")) {
            index++;

            if (index == screens.Length)
            {
                OnIntroComplete();
            }
            else {
                if(index>0)
                    screens[index].SetActive(false);
                screens[index].SetActive(true);
            }
        }
	}

    public void OnIntroComplete() {
        SceneManager.LoadScene("SpaceGame");
    }
}
