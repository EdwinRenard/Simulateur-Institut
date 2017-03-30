using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ForceFeedbackExemple : MonoBehaviour {
	ForceFeedbackInterface myForceFeedback;
	float deadZone;
	bool b_working = false;

	void Awake()
	{
		myForceFeedback = new ForceFeedbackInterface ();
	}

	void Start() {
		deadZone = TeamUtility.IO.InputManager.GetAxisConfiguration(0, "Horizontal").deadZone;
	}

	void Update(){
		float axeVolant = TeamUtility.IO.InputManager.GetAxisRaw ("Horizontal");
		int force = myForceFeedback.forceX;
		if (!b_working) {
			if (axeVolant < -(deadZone)) {
				myForceFeedback.SetDeviceForces(-3500, 0);
				Debug.Log("Neg");
				b_working = true;
			} else if (axeVolant > deadZone) {
				myForceFeedback.SetDeviceForces(3500, 0);
				Debug.Log("Pos");
				b_working = true;
			}
			if(axeVolant > -(deadZone) && axeVolant < deadZone && force != 0){
				if (axeVolant < -0.01) {
					myForceFeedback.SetDeviceForces(-1000, 0);
				}
				if (axeVolant > 0.01) {
					myForceFeedback.SetDeviceForces (1000, 0);
				} else {
					myForceFeedback.SetDeviceForces (0, 0);
				}
				b_working = true;
			}
		} else {
			if(force >= 0 && axeVolant < (-deadZone)){
				b_working = false;
			}
			if (force <= 0 && axeVolant > deadZone) {
				b_working = false;
			}
			if(axeVolant > -(deadZone) && axeVolant < deadZone){
				b_working = false;
			}
		}
	}

	void OnApplicationQuit(){
		StopAllCoroutines ();
		myForceFeedback.shutDownForceFeedback();
	}
}
