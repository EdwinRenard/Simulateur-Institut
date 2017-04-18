using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate()
        {
            // pass the input to the car!
            float h = TeamUtility.IO.InputManager.GetAxis("Horizontal");
            float v = TeamUtility.IO.InputManager.GetAxis("Accelerator");
		    v = (v+1)/2;
#if !MOBILE_INPUT
            float handbrake = TeamUtility.IO.InputManager.GetAxis("Brakes");
            m_Car.Move(h, -v, -v, handbrake);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }
    }
}
