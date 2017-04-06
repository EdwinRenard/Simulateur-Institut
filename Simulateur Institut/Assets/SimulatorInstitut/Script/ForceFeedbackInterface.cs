using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ForceFeedbackInterface {

	[DllImport("user32")]
	private static extern int GetForegroundWindow();			//To access the windows affected by force feedback.
	[DllImport("DirectInputLibrary")]
	public static extern int InitDirectInput();					//Initialize the device.
	[DllImport("DirectInputLibrary")]
	public static extern int InitForceFeedback(int HWND);		//Initializes the force feedback of the device.
	[DllImport("DirectInputLibrary")]
	public static extern int SetDeviceForcesXY(int x, int y);	//Gives an action force on the X axis and / or the Y axis.
	[DllImport("DirectInputLibrary")]
	public static extern int StartEffect();						//Starts the initialized effect.
	[DllImport("DirectInputLibrary")]
	public static extern int StopEffect();						//Stops the current effect.
	[DllImport("DirectInputLibrary")]
	public static extern int DetectForceFeedbackDevice();		//Device detection compatible with force feedback.
	[DllImport("DirectInputLibrary")]
	public static extern int FreeForceFeedback();				//Releases the force feedback.
	[DllImport("DirectInputLibrary")]
	public static extern int FreeDirectInput();					//Releases the device.

	//Variables
	public int forceX { get; private set; }
	public int forceY { get; private set; }
	public bool forceFeedbackEnabled {get; private set;}

	/// <summary>
	/// Constructor of the class to handle the force feedback.
	/// </summary>
	public ForceFeedbackInterface()
	{
		InitialiseForceFeedback ();
	}
		
	/// <summary>
	/// Initializes the force feedback.
	/// </summary>
	private void InitialiseForceFeedback() 
	{ 
		if(forceFeedbackEnabled)
		{
			Debug.LogWarning("Force feedback attempted to initialise but was already running !");
			return;
		}

		//We retrieve the ID of the window that requires the force feedback.
		int hwnd = GetForegroundWindow();
		//Call to the function of the DLL to initialize the force feedback.
		if(InitForceFeedback(hwnd) >= 0){
			StartEffect();
			forceFeedbackEnabled = true;
		}
	}

	/// <summary>
	/// Force feedback reset function.
	/// </summary>
	public void ResetForceFeedback()
	{
		FreeForceFeedback();
		forceFeedbackEnabled = false;
		forceX = forceY = 0;
	}

	/// <summary>
	/// Full force feedback stop function. Stop all effects in progress and releases the force feedback.
	/// </summary>
	public void shutDownForceFeedback()
	{
		if(forceFeedbackEnabled)
		{
			StopEffect();
		}
		FreeDirectInput();
	}

	/// <summary>
	/// Function to give a certain force on the X and Y axes.
	/// </summary>
	public void SetDeviceForces(int forceX, int forceY)
	{
		this.forceX = forceX;
		this.forceY = forceY;

		int FFdetected = DetectForceFeedbackDevice();

		if (FFdetected >= 0 && !forceFeedbackEnabled) {
			InitialiseForceFeedback();
			forceFeedbackEnabled = true;
		}

		if (forceFeedbackEnabled)
		{
			if(FFdetected < 0) {
				//If the device is disconnected.
				StopEffect ();
				FreeForceFeedback ();
				forceFeedbackEnabled = false;

			} else {
				SetDeviceForcesXY(forceX, forceY);
			}
		}
	}
}
