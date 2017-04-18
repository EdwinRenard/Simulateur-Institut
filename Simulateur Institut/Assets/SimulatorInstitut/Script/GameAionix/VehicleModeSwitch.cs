using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using UnityStandardAssets.Vehicles.Aeroplane;

public class VehicleModeSwitch : MonoBehaviour
{

    private CarController CarController;
    private CarUserControl CarUser;
    private CarAudio CarAudio;

    private AeroplaneController AircraftController;
    private AeroplaneUserControl4Axis AircraftUser;
    private AeroplaneAudio AircraftAudio;
    private Rigidbody VehicleMass;

    [SerializeField]
    private GameObject WheelsHubs;

	// Use this for initialization
	void Start () {
        CarController = GetComponent<CarController>();
        CarUser = GetComponent<CarUserControl>();
        CarAudio = GetComponent<CarAudio>();
        VehicleMass = GetComponent<Rigidbody>();

        AircraftController = GetComponent<AeroplaneController>();
        AircraftUser = GetComponent<AeroplaneUserControl4Axis>();
        AircraftAudio = GetComponent<AeroplaneAudio>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (gameObject.GetComponent<PhotonView>().isMine == false && PhotonNetwork.connected == true)
        {
            return;
        }*/


        //Car  Mode
        if (Input.GetKeyDown(KeyCode.I))
        {
            WheelsHubs.SetActive(true);
            VehicleMass.mass = 1000;

            //AircraftController.enabled = false;
            AircraftUser.enabled = false;
            //AircraftAudio.enabled = false;

            //CarController.enabled = true;
            CarUser.enabled = true;
            //CarAudio.enabled = true;

            GetComponent<Rigidbody>().drag = 0.1f;
        }
        //Aircraft  Mode
        if (Input.GetKeyDown(KeyCode.O))
        {
            WheelsHubs.SetActive(false);
            VehicleMass.mass = 1;

            //CarController.enabled = false;
            CarUser.enabled = false;
            //CarAudio.enabled = false;

            //AircraftController.enabled = true;
            AircraftUser.enabled = true;
            //AircraftAudio.enabled = true;
            GetComponent<Rigidbody>().drag = 0.1f;
        }
    }
}
