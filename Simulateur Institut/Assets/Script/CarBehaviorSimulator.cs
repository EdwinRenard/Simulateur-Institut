using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;

public class CarBehaviorSimulator : MonoBehaviour {
	//Variable moteur.
	public bool start;
	public float maxEngineRPM;
	public float minEngineRPM;
	public float engineRPM;
	public AnimationCurve torqueCurve;

	public float brakeTorque;
	public float speed;
	//Variables roues.
	public WheelCollider rearLeftWheel;
	public WheelCollider rearRightWheel;
	public WheelCollider frontLeftWheel;
	public WheelCollider frontRightWheel;
	//Variable volant.
	public float maxStreeringAngle;
	//Variable boite de vitesse.
	public AnimationCurve ratioGear;
	public float finalDriveRatio;
	public int currentGear;

	void Start () {
		start = true;
		engineRPM = 0.0f;
		maxEngineRPM = 5500.0f;
		minEngineRPM = 700.0f;
		maxStreeringAngle = 30.0f;
		brakeTorque = 2000.0f;

		finalDriveRatio = 3.23f;
		currentGear = 1; //0:R | 1:N | 2,3,4,5,..:V

		rearLeftWheel.ConfigureVehicleSubsteps(20,48,60);
		rearRightWheel.ConfigureVehicleSubsteps(20,48,60);
		frontLeftWheel.ConfigureVehicleSubsteps(20,48,60);
		frontRightWheel.ConfigureVehicleSubsteps(20,48,60);
	}

	void Update(){
		speed = Mathf.Floor (this.GetComponent<Rigidbody> ().velocity.magnitude * 3.6f);

		if(TeamUtility.IO.InputManager.GetButton("Start_Engine")){
			start = true;
		}
		if(TeamUtility.IO.InputManager.GetButton("Stop_Engine")) {
			start = false;
		}

		ShiftGear();
		Interface ();
	}

	void FixedUpdate() {
		if (start) {
			//Gerer la rotation du régime moteur en fonction des roues et de la vitesse choisi.
			engineRPM = minEngineRPM + Mathf.Abs (rearLeftWheel.rpm) + Mathf.Abs (rearRightWheel.rpm) / 2 * finalDriveRatio * ratioGear.Evaluate (currentGear);

			/*if (engineRPM < minEngineRPM) {
				start = false;
			}*/

			//Déplacement de la voiture
			float totalMotorTorque = torqueCurve.Evaluate (engineRPM) * ratioGear.Evaluate (currentGear) * Mathf.Clamp01(TeamUtility.IO.InputManager.GetAxisRaw("Accelerator"));
			rearLeftWheel.motorTorque = totalMotorTorque / 2.0f;
			rearRightWheel.motorTorque = totalMotorTorque / 2.0f;
			//Rotation physique des roues.
			rearLeftWheel.gameObject.transform.localRotation = Quaternion.Euler (Mathf.Abs (rearLeftWheel.rpm), 0.0f, 0.0f);
			rearRightWheel.gameObject.transform.localRotation = Quaternion.Euler (Mathf.Abs (rearRightWheel.rpm), 0.0f, 0.0f);

			if (Mathf.Clamp01(TeamUtility.IO.InputManager.GetAxisRaw("Brakes")) > 0) {
				rearLeftWheel.brakeTorque = brakeTorque * TeamUtility.IO.InputManager.GetAxisRaw("Brakes");
				rearRightWheel.brakeTorque = brakeTorque;
			} else {
				rearLeftWheel.brakeTorque = 0;
				rearRightWheel.brakeTorque = 0;
			}
		}else {
			engineRPM = 0;
		}

		//Orientation des roues avants.
		float angleRotation = maxStreeringAngle * TeamUtility.IO.InputManager.GetAxisRaw("Horizontal");
		frontLeftWheel.steerAngle = angleRotation;
		frontRightWheel.steerAngle = angleRotation;
		//Rotation physique des roues.
		frontLeftWheel.gameObject.transform.localRotation = Quaternion.Euler (frontLeftWheel.rpm, angleRotation, 0.0f);
		frontRightWheel.gameObject.transform.localRotation = Quaternion.Euler (frontRightWheel.rpm, angleRotation, 0.0f);
	}

	public void ShiftGear(){
		if (Mathf.Clamp01(TeamUtility.IO.InputManager.GetAxisRaw("Clutch")) > 0) {
			currentGear = 1;
			if (TeamUtility.IO.InputManager.GetButton("1er Vitesse")) {
				currentGear = 2;
			}
			if (TeamUtility.IO.InputManager.GetButton("2e Vitesse")) {
				currentGear = 3;
			}
			if (TeamUtility.IO.InputManager.GetButton("3e Vitesse")) {
				currentGear = 4;
			}
			if (TeamUtility.IO.InputManager.GetButton("4e Vitesse")) {
				currentGear = 5;
			}
			if (TeamUtility.IO.InputManager.GetButton("5e Vitesse")) {
				currentGear = 6;
			}
			if (TeamUtility.IO.InputManager.GetButton("6e Vitesse")) {
				currentGear = 6;
			}
			if (TeamUtility.IO.InputManager.GetButton("Marche_Arrière")) {
				currentGear = 0;
			}
		}
	}

	public void Interface(){
		GameObject.Find ("Start").GetComponent<Text>().text = "Start : " + start;
		GameObject.Find ("Vitesse").GetComponent<Text>().text = "Vitesse : " + speed;
		GameObject.Find ("Gear").GetComponent<Text>().text = "Gear : " + currentGear;
		GameObject.Find ("RPM").GetComponent<Text>().text = "Moteur : " + engineRPM;
	}
}
