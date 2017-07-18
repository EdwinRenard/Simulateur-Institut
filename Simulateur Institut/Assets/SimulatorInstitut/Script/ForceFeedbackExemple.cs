using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimulatorInstitut;

//Example of force feedback implementation.
//Here we will set up a system for the steering wheel to return to its initial position.
//For this purpose, the angle of the steering wheel is recovered and an opposing force proportional to the angle of the steering wheel is applied.
public class ForceFeedbackExemple : MonoBehaviour {
	ForceFeedbackInterface myForceFeedback; //1) Our force feedback management class.

	void Awake()
	{
		myForceFeedback = new ForceFeedbackInterface(); //2) Initialization of the force feedback.
	}

	void Update(){//3) Application of the effect. Here the effect must be applied continuously, that is why it is in the Update.
		float axeVolant = SimulatorInstitut.InputManager.GetAxisRaw ("Horizontal");

		int force = Mathf.FloorToInt(10000 * axeVolant);
		myForceFeedback.SetDeviceForces (force, 0);
		/*if (SimulatorInstitut.InputManager.GetButton("Button0")) 
		{
			Debug.Log ("Start Delay Forces in 5s.");
			myForceFeedback.SetDelayForces(5000,0,5.0f);
		}
		if (SimulatorInstitut.InputManager.GetButton("Button4")) 
		{
			Debug.Log ("Start Duration Forces in 1s.");
			myForceFeedback.SetDurationForces (5000, 0, 1.0f);
		}
		if (SimulatorInstitut.InputManager.GetButton("Button5")) 
		{
			Debug.Log ("Start Delay and Duration Forces in 5s and 2s.");
			myForceFeedback.SetCustomForces (5000, 0, 2.0f, 5.0f);
		}*/
	}

	void OnApplicationQuit(){
		myForceFeedback.shutDownForceFeedback();//4) The device is released with force feedback.

	}
}
