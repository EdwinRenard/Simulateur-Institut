using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ForceFeedbackExemple : MonoBehaviour {
	ForceFeedbackInterface myForceFeedback;
	float deadZone;

	void Awake()
	{
		myForceFeedback = new ForceFeedbackInterface ();
	}

	void Start() {
		deadZone = 0.001f;//TeamUtility.IO.InputManager.GetAxisConfiguration(0, "Horizontal").deadZone;
		InvokeRepeating("ResetPositionVolant",1.0f,0.5f);
		//StartCoroutine("myCoroutine");
	}

	IEnumerator myCoroutine(){
		yield return new WaitForSeconds(0.1f);
		ResetPositionVolant ();
		yield return new WaitForSeconds (0.3f);
		StartCoroutine ("myCoroutine");
	}

	void ResetPositionVolant()
	{
		int forceX = 0;
		int forceY = 0;
		float axeVolant = TeamUtility.IO.InputManager.GetAxisRaw ("Horizontal");
		if (axeVolant > deadZone) {
			forceX = Mathf.Clamp(Mathf.FloorToInt(10000 * axeVolant),500,10000);
		}
		if (axeVolant < -(deadZone)) {
			forceX = -(Mathf.Clamp(Mathf.FloorToInt(10000 * Mathf.Abs(axeVolant)),500,10000));
		}
		Debug.Log ("Force appliquée");
		myForceFeedback.SetDeviceForces(forceX, forceY);
	}

	void OnApplicationQuit(){
		StopAllCoroutines ();
		myForceFeedback.shutDownForceFeedback();
	}
}
