using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour {

    public Text text;
    public float delay = 5;

    void Start () {
        text.text = string.Format(text.text, PlayerShip.position.ToString());
	}
	
	void Update () {
        delay -= Time.deltaTime;

        if (Input.anyKeyDown && delay<=0) {
            Application.Quit();
        }
	}
}
