using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ForceFeedbackInterface {

	//Importation des différentes fonctions des DLL
	[DllImport("user32")]
	private static extern int GetForegroundWindow();
	[DllImport("UnityForceFeedback")]
	public static extern int InitDirectInput();
	[DllImport("UnityForceFeedback")]
	public static extern int InitForceFeedback(int HWND);
	[DllImport("UnityForceFeedback")]
	public static extern int SetDeviceForcesXY(int x, int y);
	[DllImport("UnityForceFeedback")]
	public static extern int StartEffect();
	[DllImport("UnityForceFeedback")]
	public static extern int StopEffect();
	[DllImport("UnityForceFeedback")]
	public static extern int DetectForceFeedbackDevice();
	[DllImport("UnityForceFeedback")]
	public static extern int SetAutoCenter(bool autocentre);
	[DllImport("UnityForceFeedback")]
	public static extern int FreeForceFeedback();
	[DllImport("UnityForceFeedback")]
	public static extern int FreeDirectInput();

	//Variables
	public int forceX { get; private set; }
	public int forceY { get; private set; }
	public bool forceFeedbackEnabled {get; private set;}

	//Constructeur de la classe.
	/// <summary>
	/// Constructeur de la classe pour gérer le retour de force.
	/// </summary>
	public ForceFeedbackInterface()
	{
		InitDirectInput();
	}
		
	private void InitialiseForceFeedback() 
	{ 
		if(forceFeedbackEnabled)
		{
			Debug.LogWarning("Force feedback attempted to initialise but was already running !");
			return;
		}

		int hwnd = GetForegroundWindow();
		Debug.Log("Window HWdD = "+hwnd);

		if(InitForceFeedback(hwnd) >= 0)
		{
			StartEffect();
			forceFeedbackEnabled = true;
			SetAutoCenter(false);
		}
	}

	/// <summary>
	/// Fonction de reset du retour de force.
	/// </summary>
	public void ResetForceFeedback()
	{
		FreeForceFeedback();
		forceFeedbackEnabled = false;
	}

	/// <summary>
	/// Fonction d'arrêt complet du retour de force. Cela arrêt tous les effets en cours et libère le retour de force.
	/// </summary>
	public void shutDownForceFeedback()
	{
		if(forceFeedbackEnabled)
		{
			StopEffect();
			FreeDirectInput();
		}
	}

	/// <summary>
	/// fonction pour donner une certaine force sur les axes X et Y.
	/// </summary>
	public void SetDeviceForces(int forceX, int forceY)
	{
		if (DetectForceFeedbackDevice() >= 0 && !forceFeedbackEnabled)
		{
			InitialiseForceFeedback();
			forceFeedbackEnabled = true;
		}

		if (forceFeedbackEnabled)
		{
			int result = DetectForceFeedbackDevice();
			if (result < 0)
			{
				//Si le device est déconnecté
				Debug.LogWarning("ForceFeedback device disconnected");
				StopEffect();
				FreeForceFeedback();
				forceFeedbackEnabled = false;
			}
			SetDeviceForcesXY(forceX, forceY);
		}
	}
}
