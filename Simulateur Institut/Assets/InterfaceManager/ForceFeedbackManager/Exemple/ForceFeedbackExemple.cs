using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Exemple d'implémentation du retour de force.
//Ici on met en place un système pour que le volant retourne à sa position initiale tout seul.
//Pour ce faire on va découper l'espace de notre volant [-1,1] en trois zones(états).
// [-1, -deadzone[ le volant est à gauche.
// [-deadzone, deadzone] le volant est au centre.
// ]deadzone, 1] le volant est à droite.
public class ForceFeedbackExemple : MonoBehaviour {
	ForceFeedbackInterface myForceFeedback; //1) Notre classe de gestion du retour de force.
	float deadZone;
	bool b_etatChange = false;		//Un bouléen pour savoir si on attend un changement d'état.

	void Awake()
	{
		myForceFeedback = new ForceFeedbackInterface(); //2) Initialisation du retour de force.
	}
		
	void Start() {
		deadZone = TeamUtility.IO.InputManager.GetAxisConfiguration(0, "Horizontal").deadZone;
	}

	void Update(){//3) Application de l'effet. Ici l'effet doit être appliquée en continue, c'est pour cela qu'il se trouve dans l'Update.
		float axeVolant = TeamUtility.IO.InputManager.GetAxisRaw ("Horizontal");
		/*int force = myForceFeedback.forceX;
		if (!b_etatChange) { //Si on n'attend pas un changement d'état. Ce qui signifie que le volant est passer de la position neutre à la position gauche par exemple, donc on applique une force opposée.
			if (axeVolant < -(deadZone)) { //Si on est dans l'état où le volant est à gauche.
				myForceFeedback.SetDeviceForces(-3000, 0); //On lui dit d'aller à droite.
				b_etatChange = true;
			} else if (axeVolant > deadZone) { //Si on est dans l'état où le volant est à droite.
				myForceFeedback.SetDeviceForces(3000, 0); //On lui dit d'aller à gauche.
				b_etatChange = true;
			}
			if(axeVolant > -(deadZone) && axeVolant < deadZone && force != 0){ //Si on est dan l'état neutre.
				myForceFeedback.SetDeviceForces (0, 0); //On lui dit de ne pas bouger.
				b_etatChange = true;
			}
		} else {	//Si on est dans l'attente d'un changement d'état. Ce qui signifie qu'on attend que le volant bouge dans une zone.
			if(force >= 0 && axeVolant < (-deadZone)){//Le volant à bouger vers la gauche.
				b_etatChange = false;
			}
			if (force <= 0 && axeVolant > deadZone) {//Le volant à bouger vers la droite.
				b_etatChange = false;
			}
			if(axeVolant > -(deadZone) && axeVolant < deadZone){//Le volant à bouger au centre.
				b_etatChange = false;
			}
		}*/

		int force = Mathf.FloorToInt(10000 * axeVolant);
		myForceFeedback.SetDeviceForces (force, 0);
	}

	void OnApplicationQuit(){
		myForceFeedback.shutDownForceFeedback();//4) On libère le périphérique avec le retour de force.

	}
}
