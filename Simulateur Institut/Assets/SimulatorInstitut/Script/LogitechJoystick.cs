using UnityEngine;
using System;
using System.Runtime.InteropServices;
using SimulatorInstitut;

/// <summary>
/// Class for the logitech.
/// </summary>
public class LogitechJoystick : CustomJoystick
{
	[DllImport("user32")]
	private static extern int GetForegroundWindow ();

	[DllImport("LogitechSimulatorLibrary")]
	public static extern int InitDynamiqueJoystick(int hwnd);

	[DllImport("LogitechSimulatorLibrary", CallingConvention=CallingConvention.Cdecl)]
	public static extern double getActionState(string action);

	[DllImport("LogitechSimulatorLibrary")]
	public static extern void StopDynamiqueJoystick();

	public LogitechJoystick()
	{
		InitCustomJoystick ();
	}

	public override void InitCustomJoystick()
	{
		int hwnd = GetForegroundWindow();
		if (InitDynamiqueJoystick (hwnd) != 0) 
		{
			Debug.Log("Error Initialisation Joystick.");
			return;
		}
	}

	public override void StopCustomJoystick()
	{
		StopDynamiqueJoystick();
	}

	public override double getJoystickActionState(string action)
	{
		return getActionState(action);
	}

	public override string getName()
	{
		return "Logitech Device.";
	}
}
