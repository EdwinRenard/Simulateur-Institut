using System;
using SimulatorInstitut;
using UnityEngine;

/// <summary>
/// Class for the keyboard.
/// </summary>
public class KeyboardJoystick : CustomJoystick
{
	public KeyboardJoystick()
	{
		InitCustomJoystick ();
	}

	public override void InitCustomJoystick() { }

	public override void StopCustomJoystick() { }

	public override double getJoystickActionState(string action)
	{
		KeyCode actionKey = StringToKey (action);

		if (actionKey == KeyCode.None)
		{
			if (String.Equals(action, "Mouse_1")) {
				return Input.GetAxisRaw("Mouse X");
			} else if (String.Equals(action, "Mouse_2")) {
				return Input.GetAxisRaw("Mouse Y");
			} else {
				return 0.0;
			}
		}
		else 
		{
			return Input.GetKey(action) ? 1.0 : 0.0;
		}
	}

	public override string getName()
	{
		return "Keyboard & Mouse Device";
	}

	private KeyCode StringToKey(string value)
	{
		if (string.IsNullOrEmpty (value)) 
		{
			return KeyCode.None;
		}
			
		try{
			return (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
		}
		catch{
			return KeyCode.None;
		}
	}

}
