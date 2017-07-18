using UnityEngine;
using UnityEngine.UI;
using SimulatorInstitut;

public class CarBehaviorSimulator : MonoBehaviour {
	//Variables engine.
	public bool start;						//Boolean for the start of the car.
	public float maxEngineRPM;				//Maximum engine speed in Rotation Per Min.
	public float minEngineRPM;				//Minimum engine speed in Rotation Per Min.
	public float engineRPM;					//Engine speed in Rotation Per Min.
	public AnimationCurve torqueCurve;		//The curve of the power delivered by the engine according to its speed.

	public float brakeTorque;				//Braking power of the vehicle.
	public float speed;
	//Variables wheels.
	public WheelCollider rearLeftWheel;		//Left rear wheel of the vehicle.
	public WheelCollider rearRightWheel;	//Right rear wheel of the vehicle.
	public WheelCollider frontLeftWheel;	//Left front wheel of the vehicle.
	public WheelCollider frontRightWheel;	//Right front wheel of the vehicle.
	//Variables flywheel.
	public float maxStreeringAngle;			//The maximum angle of rotation of the wheels (Steering)
	//Variables gearbox.
	public AnimationCurve ratioGear;		//Ratio curve as a function of speed.
	public float finalDriveRatio;			//Final vehicle ratio.
	public int currentGear;					//Current speed of the gearbox.
		
	void Start () {			
		start = true;
		engineRPM = 0.0f;
		maxEngineRPM = 5500.0f;
		minEngineRPM = 700.0f;
		maxStreeringAngle = 30.0f;
		brakeTorque = 2000.0f;

		finalDriveRatio = 3.23f;
		currentGear = 1; //0:R | 1:N | 2,3,4,5,6:V

		//Wheel configuration for better grip.
		rearLeftWheel.ConfigureVehicleSubsteps(20,48,60);
		rearRightWheel.ConfigureVehicleSubsteps(20,48,60);
		frontLeftWheel.ConfigureVehicleSubsteps(20,48,60);
		frontRightWheel.ConfigureVehicleSubsteps(20,48,60);

		Debug.Log("InputConfiguration : "+ SimulatorInstitut.InputManager.GetInputConfiguration(PlayerID.One).name);
	}
		
	void Update(){
		speed = Mathf.Floor (this.GetComponent<Rigidbody> ().velocity.magnitude * 3.6f);

		if (SimulatorInstitut.InputManager.GetInputConfiguration(PlayerID.One).name == "Logitech_Simulator") {
			if (SimulatorInstitut.InputManager.GetButton ("Start")) {
				start = true;
			}
			if (SimulatorInstitut.InputManager.GetButton ("Stop")) {
				start = false;
			}
		}else {
			if (SimulatorInstitut.InputManager.GetButton("Start")) {
				start = !start;
			}
		}

		ShiftGear();
	}

	void FixedUpdate() {
		if (start) {
			//Management of the rotation of the engine speed according to the wheels and the chosen speed.						//Else negative value in reverse.
			engineRPM = minEngineRPM + Mathf.Abs (rearLeftWheel.rpm) + Mathf.Abs (rearRightWheel.rpm) / 2 * finalDriveRatio * Mathf.Abs(ratioGear.Evaluate (currentGear));

			if (engineRPM < minEngineRPM) {
				start = false;
			}

			//Moving the car.
			float totalMotorTorque = torqueCurve.Evaluate (engineRPM) * ratioGear.Evaluate (currentGear) * Mathf.Clamp01(SimulatorInstitut.InputManager.GetAxisRaw("Accelerator"));
			rearLeftWheel.motorTorque = totalMotorTorque / 2.0f;
			rearRightWheel.motorTorque = totalMotorTorque / 2.0f;

			//Brake management.
			if (Mathf.Clamp01(SimulatorInstitut.InputManager.GetAxisRaw("Brakes")) > 0) {
				rearLeftWheel.brakeTorque = brakeTorque * SimulatorInstitut.InputManager.GetAxisRaw("Brakes");
				rearRightWheel.brakeTorque = brakeTorque;
			} else {
				rearLeftWheel.brakeTorque = 0;
				rearRightWheel.brakeTorque = 0;
			}
		}else {
			engineRPM = 0;
		}

		//Orientation of front wheels.
		float angleRotation = maxStreeringAngle * SimulatorInstitut.InputManager.GetAxisRaw("Horizontal");
		frontLeftWheel.steerAngle = angleRotation;
		frontRightWheel.steerAngle = angleRotation;
		//Physical orientation of the wheels.
		frontLeftWheel.gameObject.transform.localRotation = Quaternion.Euler (frontLeftWheel.rpm, angleRotation, 0.0f);
		frontRightWheel.gameObject.transform.localRotation = Quaternion.Euler (frontRightWheel.rpm, angleRotation, 0.0f);
		rearLeftWheel.gameObject.transform.localRotation = Quaternion.Euler (Mathf.Abs (rearLeftWheel.rpm), 0.0f, 0.0f);
		rearRightWheel.gameObject.transform.localRotation = Quaternion.Euler (Mathf.Abs (rearRightWheel.rpm), 0.0f, 0.0f);
	}

	//Function for gears management.
	public void ShiftGear(){
		if (SimulatorInstitut.InputManager.GetInputConfiguration(PlayerID.One).name == "Logitech_Simulator"){
			if (Mathf.Clamp01(SimulatorInstitut.InputManager.GetAxisRaw ("Clutch")) > 0) {
				currentGear = 1;
				if (SimulatorInstitut.InputManager.GetButton ("Button8")) {
					currentGear = 2;
				}
				if (SimulatorInstitut.InputManager.GetButton ("Button9")) {
					currentGear = 3;
				}
				if (SimulatorInstitut.InputManager.GetButton ("Button10")) {
					currentGear = 4;
				}
				if (SimulatorInstitut.InputManager.GetButton ("Button11")) {
					currentGear = 5;
				}
				if (SimulatorInstitut.InputManager.GetButton ("Button12")) {
					currentGear = 6;
				}
				if (SimulatorInstitut.InputManager.GetButton ("Button13")) {
					currentGear = 6;
				}
				if (SimulatorInstitut.InputManager.GetButton ("Button14")) {
					currentGear = 0;
				}
			}
		} else {
			if (SimulatorInstitut.InputManager.GetButton("GearUp")) {
				currentGear++;
			}
			if (SimulatorInstitut.InputManager.GetButton("GearDown")) {
				currentGear--;
			}
		}
	}
}
