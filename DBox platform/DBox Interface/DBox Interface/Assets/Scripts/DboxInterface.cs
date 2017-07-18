using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class DboxInterface : MonoBehaviour {
	[DllImport("DBoxInterfaceLibrary")]
	public static extern int InitDBoxPlatform ();

	[DllImport("DBoxInterfaceLibrary")]
	public static extern void StopDBoxPlatform();

	[DllImport("DBoxInterfaceLibrary")]
	public static extern int Move(float heave, float pitch, float roll);

	[DllImport("DBoxInterfaceLibrary")]
	public static extern int Vibration(float rate, float strength);

	float pas;
	float rpm;		//The motor's rate.
	float torque;	//The motor's power.
	float heave;	//Current move Up/Down
	float pitch;	//Current move Front/Back
	float roll;		//Current move Left/Right
	float t_pitch;	//The final move Up/Down
	float t_roll;	//The final move Front/Back
	float t_heave;	//The final move Left/Right

	public DboxInterface(){
		rpm = 0.0f;
		torque = 0.0f;
		heave = 0.0f;
		pitch = 0.0f;
		roll = 0.0f;
		t_pitch = 0.0f;
		t_roll = 0.0f;
		t_heave = 0.0f;
		pas = 0.02f;

		//Initialize the Dbox platform.
		int res = InitDBoxPlatform ();
		Debug.Log ("Init : " + res);	//If res = 0, it's good.
	}
		
	~DboxInterface(){
		StopDbox();
	}

	public void Update(){
				//For a smooth move.
		heave = (float)Math.Round (Mathf.Lerp (heave, t_heave, pas), 3);
		roll = (float)Math.Round (Mathf.Lerp (roll, t_roll, pas), 3);
		pitch = (float)Math.Round (Mathf.Lerp (pitch, t_pitch, pas), 3);

		//Send move's data.
		Move(heave, pitch, roll);
		//Send vibration's data.
		Vibration(rpm, torque);

	}
		
	public void MovePlatform(float heave, float pitch, float roll){
		t_heave = heave;
		t_pitch = pitch;
		t_roll = roll;
	}

	public void VibrationPlatform(float rate, float strength){
		rpm = rate;
		torque = strength;
	}

	public void StopDbox(){
		//Reset the platform's position.
		Move(0.0f, 0.0f, 0.0f);
		//Stop the vibration.
		Vibration (0.0f, 0.0f);
		System.Threading.Thread.Sleep(5000);
		StopDBoxPlatform();
	}
}
