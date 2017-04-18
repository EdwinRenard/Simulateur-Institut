using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aiBonusBox : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	void Update () {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "aiVehicleCollision")
        {
            gameObject.SetActive(false);
        }
    }
}
