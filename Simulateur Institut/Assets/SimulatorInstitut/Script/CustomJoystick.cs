using UnityEngine;
using System.Collections;

namespace SimulatorInstitut
{
	public abstract class CustomJoystick
	{
		/// <summary>
		/// Inits the custom joystick.
		/// </summary>
		public abstract void InitCustomJoystick();

		/// <summary>
		/// Stops the custom joystick.
		/// </summary>
		public abstract void StopCustomJoystick();

		/// <summary>
		/// Gets the state of the joystick action.
		/// </summary>
		/// <returns>The joystick action state.</returns>
		public abstract double getJoystickActionState(string action);

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <returns>The name.</returns>
		public abstract string getName();
	}
}
