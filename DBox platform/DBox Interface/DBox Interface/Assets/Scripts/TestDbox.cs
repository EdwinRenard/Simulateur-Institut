using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class TestDbox : MonoBehaviour {
	
	bool b_vibration = false;
	float rpm = 0.0f;
	float torque = 0.0f;
	float heave = 0.0f;
	float pitch = 0.0f;
	float roll = 0.0f;
	DboxInterface dbox;

	void Start(){
		dbox = new DboxInterface ();
	}

	void Update(){
		rpm = UnityEngine.GameObject.Find ("Slider_RPM").GetComponent<Slider> ().value;
		torque = UnityEngine.GameObject.Find ("Slider_Torque").GetComponent<Slider> ().value;

		if (Input.GetKeyDown(KeyCode.H))
			b_vibration = !b_vibration;

		if (b_vibration) {
			UnityEngine.GameObject.Find ("Button_Vibration").GetComponent<Button> ().enabled = true;
			dbox.VibrationPlatform (rpm, torque);
		} else {
			UnityEngine.GameObject.Find ("Button_Vibration").GetComponent<Button> ().enabled = false;
			dbox.VibrationPlatform (0.0f, 0.0f);
		}

		if (Input.GetKey(KeyCode.Z)) {
			pitch = 1.0f;
			UnityEngine.GameObject.Find ("Button_Z").GetComponent<Button> ().enabled = true;
		} else {
			UnityEngine.GameObject.Find ("Button_Z").GetComponent<Button> ().enabled = false;
		}
		if (Input.GetKey(KeyCode.S)) {
			pitch = -1.0f;
			UnityEngine.GameObject.Find ("Button_S").GetComponent<Button> ().enabled = true;
		} else {
			UnityEngine.GameObject.Find ("Button_S").GetComponent<Button> ().enabled = false;
		}
		if (Input.GetKey(KeyCode.Q)) {
			roll = 1.0f;
			UnityEngine.GameObject.Find ("Button_Q").GetComponent<Button> ().enabled = true;
		} else {
			UnityEngine.GameObject.Find ("Button_Q").GetComponent<Button> ().enabled = false;
		}
		if (Input.GetKey(KeyCode.D)) {
			roll = -1.0f;
			UnityEngine.GameObject.Find ("Button_D").GetComponent<Button> ().enabled = true;
		} else {
			UnityEngine.GameObject.Find ("Button_D").GetComponent<Button> ().enabled = false;
		}

		if (Input.GetKey(KeyCode.Space)) {
			heave = 1.0f;
			UnityEngine.GameObject.Find ("Button_Space").GetComponent<Button> ().enabled = true;
		} else {
			UnityEngine.GameObject.Find ("Button_Space").GetComponent<Button> ().enabled = false;
		}

		if (Input.GetKey(KeyCode.LeftControl)) {
			heave = -1.0f;
			UnityEngine.GameObject.Find ("Button_LeftCtrl").GetComponent<Button> ().enabled = true;

		} else {
			UnityEngine.GameObject.Find ("Button_LeftCtrl").GetComponent<Button> ().enabled = false;
		}

		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit ();
		}

		UnityEngine.GameObject.Find ("Text_Heave").GetComponent<Text> ().text = "Heave : " + heave.ToString();
		UnityEngine.GameObject.Find ("Text_Pitch").GetComponent<Text> ().text = "Pitch : " + pitch.ToString();
		UnityEngine.GameObject.Find ("Text_Roll").GetComponent<Text> ().text = "Roll : " + roll.ToString();
		UnityEngine.GameObject.Find ("Text_RPM").GetComponent<Text> ().text = "RPM : " + rpm.ToString();
		UnityEngine.GameObject.Find ("Text_Torque").GetComponent<Text> ().text = "Torque : " + torque.ToString();

		UnityEngine.GameObject.Find ("Slider_Heave").GetComponent<Slider> ().value = heave;
		UnityEngine.GameObject.Find ("Slider_Pitch").GetComponent<Slider> ().value = pitch;
		UnityEngine.GameObject.Find ("Slider_Roll").GetComponent<Slider> ().value = roll;


		dbox.MovePlatform(heave, pitch, roll);
		dbox.Update ();

		heave = 0.0f;
		pitch = 0.0f;
		roll = 0.0f;
	}

	void OnApplicationQuit(){
		dbox.StopDbox();
		System.Threading.Thread.Sleep(5000);
	}

}
