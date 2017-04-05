using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ForceFeedbackInterface {

	//Importation des différentes fonctions des DLL
	[DllImport("user32")]
	private static extern int GetForegroundWindow();			//Pour optenir la fenêtre windows affecté par le retour de force.
	[DllImport("DirectInputLibrary")]
	public static extern int InitDirectInput();					//Initialisation du périphérique.
	[DllImport("DirectInputLibrary")]
	public static extern int InitForceFeedback(int HWND);		//Initialise le retour de force du périphérique.
	[DllImport("DirectInputLibrary")]
	public static extern int SetDeviceForcesXY(int x, int y);	//Donne une force d'action sur l'axe X et/ou l'axe Y.
	[DllImport("DirectInputLibrary")]
	public static extern int StartEffect();						//Démarre l'effet initialisé.
	[DllImport("DirectInputLibrary")]
	public static extern int StopEffect();						//Arrête l'effet en cours.
	[DllImport("DirectInputLibrary")]
	public static extern int DetectForceFeedbackDevice();		//Detection de périphérique compatible avec le retour de force.
	[DllImport("DirectInputLibrary")]
	public static extern int FreeForceFeedback();				//Libère le retour de force.
	[DllImport("DirectInputLibrary")]
	public static extern int FreeDirectInput();					//Libère le périphérique.

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
		InitialiseForceFeedback ();
	}
		
	/// <summary>
	/// Initialise le retour de force.
	/// </summary>
	private void InitialiseForceFeedback() 
	{ 
		if(forceFeedbackEnabled)
		{
			Debug.LogWarning("Force feedback attempted to initialise but was already running !");
			return;
		}

		//On récupère l'ID de la fenêtre windows qui requis le retour de force.
		int hwnd = GetForegroundWindow();
		//Appel à la fonction de la DLL pour initialiser le retour de force.
		if(InitForceFeedback(hwnd) >= 0){
			StartEffect();
			forceFeedbackEnabled = true;
		}
	}

	/// <summary>
	/// Fonction de reset du retour de force.
	/// </summary>
	public void ResetForceFeedback()
	{
		FreeForceFeedback();
		forceFeedbackEnabled = false;
		forceX = forceY = 0;

	}

	/// <summary>
	/// Fonction d'arrêt complet du retour de force. Cela arrêt tous les effets en cours et libère le retour de force.
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
	/// Fonction pour donner une certaine force sur les axes X et Y.
	/// </summary>
	public void SetDeviceForces(int forceX, int forceY)
	{
		var temp = Time.realtimeSinceStartup;
		this.forceX = forceX;
		this.forceY = forceY;
		int FFdetected = DetectForceFeedbackDevice();

		if (FFdetected >= 0 && !forceFeedbackEnabled) {
			InitialiseForceFeedback ();
			forceFeedbackEnabled = true;
		}

		if (forceFeedbackEnabled)
		{
			if (FFdetected < 0)
			{
				//Si le device est déconnecté
				StopEffect();
				FreeForceFeedback();
				forceFeedbackEnabled = false;
			}
			SetDeviceForcesXY(forceX, forceY);
		
		}
		Debug.Log ("Temp : "+(Time.realtimeSinceStartup - temp).ToString());
	}
}
