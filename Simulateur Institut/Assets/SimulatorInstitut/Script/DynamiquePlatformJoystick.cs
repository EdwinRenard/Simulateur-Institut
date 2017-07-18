using UnityEngine;
using System;
using System.Runtime.InteropServices;
using SimulatorInstitut;

/// <summary>
/// Class for the dynamique platform.
/// </summary>
public class DynamiquePlatformJoystick : CustomJoystick
{
	[DllImport("user32")]
	private static extern int GetForegroundWindow ();

	[DllImport("DynamiquePlatformLibrary")]
	public static extern int InitDynamiqueJoystick(int hwnd);

	[DllImport("DynamiquePlatformLibrary")]
	public static extern void UpdateState();

	[DllImport("DynamiquePlatformLibrary", CallingConvention=CallingConvention.Cdecl)]
	public static extern double getActionState(string action);

	[DllImport("DynamiquePlatformLibrary")]
	public static extern void StopDynamiqueJoystick();

	public DynamiquePlatformJoystick()
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
		return "Dynamique Platform Device.";
	}
}
