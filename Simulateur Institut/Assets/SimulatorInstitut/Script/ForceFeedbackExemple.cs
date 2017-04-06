using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
		float axeVolant = TeamUtility.IO.InputManager.GetAxisRaw ("Horizontal");

		int force = Mathf.FloorToInt(10000 * axeVolant);
		myForceFeedback.SetDeviceForces (force, 0);
	}

	void OnApplicationQuit(){
		myForceFeedback.shutDownForceFeedback();//4) The device is released with force feedback.

	}
}
