using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;

public class CarBehaviorSimulator : MonoBehaviour {
	//Variables moteur.
	public bool start;						//Bouléen pour le démarrage de la voiture.
	public float maxEngineRPM;				//Régime maximun du moteur en Tour/min.
	public float minEngineRPM;				//Régime minimun du moteur en Tour/min.
	public float engineRPM;					//Régime du moteur en Tour/min.
	public AnimationCurve torqueCurve;		//Courbe de la puissance que délivre le moteur en fonction de son régime.

	public float brakeTorque;				//Puissance de freinage du véhicule.
	public float speed;						//Vitesse du véhicule.
	//Variables roues.
	public WheelCollider rearLeftWheel;		//Roue arrière gauche du véhicule.
	public WheelCollider rearRightWheel;	//Roue arrière droite du véhicule.
	public WheelCollider frontLeftWheel;	//Roue avant gauche du véhicule.
	public WheelCollider frontRightWheel;	//Roue avant droite du véhicule.
	//Variable volant.
	public float maxStreeringAngle;			//L'angle maximal de rotation des roues (Direction)
	//Variable boite de vitesse.
	public AnimationCurve ratioGear;		//Courbe du ratio en fonction de la vitesse.
	public float finalDriveRatio;			//Ratio final du véhicule.
	public int currentGear;					//Vitesse actuelle de la boite de vitesse.
											//0:Reculer | 1:Neutre | 2,3,4,5,...:Vitesse

	void Awake(){
		
		if (TeamUtility.IO.InputManager.GetJoystickNames ().Length > 0) {
			TeamUtility.IO.InputManager.SetInputConfiguration ("Logitech_Simulator", PlayerID.One);
		}
	}

	//Fonction appelé en premier. Elle sert à l'initialisation des variables.
	void Start () {
		start = true;
		engineRPM = 0.0f;
		maxEngineRPM = 5500.0f;
		minEngineRPM = 700.0f;
		maxStreeringAngle = 30.0f;
		brakeTorque = 2000.0f;

		finalDriveRatio = 3.23f;
		currentGear = 1; //0:R | 1:N | 2,3,4,5,..:V

		//Configuration des roues pour une meilleure adhérance.
		rearLeftWheel.ConfigureVehicleSubsteps(20,48,60);
		rearRightWheel.ConfigureVehicleSubsteps(20,48,60);
		frontLeftWheel.ConfigureVehicleSubsteps(20,48,60);
		frontRightWheel.ConfigureVehicleSubsteps(20,48,60);
	}

	//Fonction appelé à chaque frame.
	void Update(){
		speed = Mathf.Floor (this.GetComponent<Rigidbody> ().velocity.magnitude * 3.6f);

		if (TeamUtility.IO.InputManager.GetInputConfiguration(PlayerID.One).name == "Logitech_Simulator") {
			if (TeamUtility.IO.InputManager.GetButton ("Start")) {
				start = true;
			}
			if (TeamUtility.IO.InputManager.GetButton ("Stop")) {
				start = false;
			}
		}else {
			if (TeamUtility.IO.InputManager.GetButtonDown("Start")) {
				start = !start;
			}
		}

		ShiftGear();
		Interface ();
	}

	//Le FixedUpdate va être appelé de manière plus régulière que le Update. Il est donc plus approprié pour gérer la physique.
	void FixedUpdate() {
		if (start) {
			//Gerer la rotation du régime moteur en fonction des roues et de la vitesse choisi.
			engineRPM = minEngineRPM + Mathf.Abs (rearLeftWheel.rpm) + Mathf.Abs (rearRightWheel.rpm) / 2 * finalDriveRatio * ratioGear.Evaluate (currentGear);

			if (engineRPM < minEngineRPM) {
				start = false;
			}

			//Déplacement de la voiture
			float totalMotorTorque = torqueCurve.Evaluate (engineRPM) * ratioGear.Evaluate (currentGear) * Mathf.Clamp01(TeamUtility.IO.InputManager.GetAxisRaw("Accelerator"));
			rearLeftWheel.motorTorque = totalMotorTorque / 2.0f;
			rearRightWheel.motorTorque = totalMotorTorque / 2.0f;
			//Rotation physique des roues.
			rearLeftWheel.gameObject.transform.localRotation = Quaternion.Euler (Mathf.Abs (rearLeftWheel.rpm), 0.0f, 0.0f);
			rearRightWheel.gameObject.transform.localRotation = Quaternion.Euler (Mathf.Abs (rearRightWheel.rpm), 0.0f, 0.0f);

			//Gestion du frein.
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

	//Fonction de gestion de la boite de vitesse
	public void ShiftGear(){
		if (TeamUtility.IO.InputManager.GetInputConfiguration(PlayerID.One).name == "Logitech_Simulator"){
			if (Mathf.Clamp01(TeamUtility.IO.InputManager.GetAxisRaw ("Clutch")) > 0) {
				currentGear = 1;
				if (TeamUtility.IO.InputManager.GetButton ("Button8")) {
					currentGear = 2;
				}
				if (TeamUtility.IO.InputManager.GetButton ("Button9")) {
					currentGear = 3;
				}
				if (TeamUtility.IO.InputManager.GetButton ("Button10")) {
					currentGear = 4;
				}
				if (TeamUtility.IO.InputManager.GetButton ("Button11")) {
					currentGear = 5;
				}
				if (TeamUtility.IO.InputManager.GetButton ("Button12")) {
					currentGear = 6;
				}
				if (TeamUtility.IO.InputManager.GetButton ("Button13")) {
					currentGear = 6;
				}
				if (TeamUtility.IO.InputManager.GetButton ("Button14")) {
					currentGear = 0;
				}
			}
		} else {
			if (TeamUtility.IO.InputManager.GetButtonDown("GearUp")) {
				currentGear++;
			}
			if (TeamUtility.IO.InputManager.GetButtonDown("GearDown")) {
				currentGear--;
			}
		}
	}

	//Simplement pour afficher quelque variable lors de la conduite.
	public void Interface(){
		GameObject.Find ("Start").GetComponent<Text>().text = "Start : " + start;
		GameObject.Find ("Vitesse").GetComponent<Text>().text = "Vitesse : " + speed;
		GameObject.Find ("Gear").GetComponent<Text>().text = "Gear : " + currentGear;
		GameObject.Find ("RPM").GetComponent<Text>().text = "Moteur : " + engineRPM;
	}
}
