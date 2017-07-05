#region [Copyright (c) 2015 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2015 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//	and associated documentation files (the "Software"), to deal in the Software without restriction, 
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial 
//	portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
//	Modified by Edwin RENARD.
using UnityEngine;
using System;

namespace SimulatorInstitut
{
	[Serializable]
	public sealed class AxisConfiguration
	{
		#region [Constants]
		public const float Neutral = 0.0f;
		public const float Positive = 1.0f;
		public const float Negative = -1.0f;
		public const int MaxMouseAxes = 2;
		public const int MaxJoystickAxes = 6;
		public const int MaxJoysticks = 4;
		#endregion
		
		#region [Settings]
		/// <summary>
		/// Do not change the name of an axis at runtime because it will invalidate the lookup tables.
		/// </summary>
		public string name;
		public string description;
		public string positive;
		public string negative;
		public string altPositive;
		public string altNegative;
		public float deadZone;
		/// <summary
		/// The speed(in units/sec) at which a digital axis falls towards neutral.
		/// </summary>
		public float gravity = 1.0f;
		/// <summary>
		/// The speed(in units/sec) at which a digital axis moves towards the target value.
		/// </summary>
		public float sensitivity = 1.0f;
		/// <summary>
		/// If input switches direction, do we snap to neutral and continue from there?
		///	Only for digital axes.
		/// </summary>
		public bool snap;
		public bool invert;
		public InputType type = InputType.DigitalAxis;
		public int axis;
		public int joystick;
		#endregion

		private enum ButtonState
		{
			Pressed, JustPressed, Released, JustReleased
		}

		private string _rawAxisName;
		private float _value;
		private int _lastAxis;
		private int _lastJoystick;
		private InputType _lastType;
		private float _lastUpdateTime;
		private float _deltaTime;	
		
		public AxisConfiguration() :
			this("New Axis") { }
		
		public AxisConfiguration(string name)
		{
			this.name = name;
			description = string.Empty; 
			positive = string.Empty;
			altPositive = string.Empty;
			negative = string.Empty;
			altNegative = string.Empty;
			type = InputType.Button;
			gravity = 1.0f;
			sensitivity = 1.0f;
		}
		
		public void Initialize()
		{
			UpdateRawAxisName();
			_value = Neutral;
			_lastUpdateTime = Time.realtimeSinceStartup;
			Input.ResetInputAxes();
		}
		
		public void Update()
		{
			_deltaTime = InputManager.IgnoreTimescale ? (Time.realtimeSinceStartup - _lastUpdateTime) : Time.deltaTime;
			_lastUpdateTime = Time.realtimeSinceStartup;

			if(_lastType != type || _lastAxis != axis || _lastJoystick != joystick)
			{
				if(_lastType != type && (type == InputType.DigitalAxis || type == InputType.RemoteAxis))
					_value = Neutral;
				
				UpdateRawAxisName();
				_lastType = type;
				_lastAxis = axis;
				_lastJoystick = joystick;
			}
		}

		public float GetAxis(CustomJoystick joystickDevice)
		{
			float axis = Neutral;
			if(type == InputType.DigitalAxis || type == InputType.RemoteAxis)
			{
				axis = _value;
			}
			else if(type == InputType.MouseAxis)
			{
				if(_rawAxisName != null)
				{
					axis = (float) joystickDevice.getJoystickActionState(_rawAxisName) * sensitivity;
				}
			}
			else if(type == InputType.AnalogAxis)
			{
				if(_rawAxisName != null)
				{
					axis = (float) joystickDevice.getJoystickActionState(_rawAxisName);
					if(Mathf.Abs(axis) < deadZone)
					{
						axis = Neutral;
					}

					axis = Mathf.Clamp(axis * sensitivity, -1, 1);
				}
			}

			return invert ? -axis : axis;
		}

		///<summary>
		///	Returns raw input with no sensitivity or smoothing applyed.
		/// </summary>
		public float GetAxisRaw(CustomJoystick joystickDevice)
		{
			float axis = Neutral;
			if(type == InputType.DigitalAxis)
			{
				if(joystickDevice.getJoystickActionState(positive) == 1.0f || joystickDevice.getJoystickActionState(altPositive) == 1.0f)
					axis = Positive;
				else if(joystickDevice.getJoystickActionState(negative) == 1.0f || joystickDevice.getJoystickActionState(altNegative) == 1.0f)
					axis = Negative;
			}
			else if(type == InputType.MouseAxis || type == InputType.AnalogAxis)
			{
				if (_rawAxisName != null)
					axis = (float) joystickDevice.getJoystickActionState(_rawAxisName);
			}
			
			return invert ? -axis : axis;
		}
		
		public bool GetButton(CustomJoystick joystickDevice)
		{
			if (type == InputType.Button)
				return (joystickDevice.getJoystickActionState(positive) == 1.0) || (joystickDevice.getJoystickActionState(altPositive) == 1.0);

			return false;
		}

		public void SetMouseAxis(int axis)
		{
			if(type == InputType.MouseAxis)
			{
				this.axis = axis;
				_lastAxis = axis;
				UpdateRawAxisName();
			}
		}
		
		public void SetAnalogAxis(int joystick, int axis)
		{
			if(type == InputType.AnalogAxis)
			{
				this.joystick = joystick;
				this.axis = axis;
				_lastAxis = axis;
				_lastJoystick = joystick;
				UpdateRawAxisName();
			}
		}

		public void SetAnalogButton(int joystick, int axis)
		{
			if(type == InputType.AnalogButton)
			{
				this.joystick = joystick;
				this.axis = axis;
				_lastAxis = axis;
				_lastJoystick = joystick;
				UpdateRawAxisName();
			}
		}

		/// <summary>
		/// If the axis' input type is set to "RemoteAxis" the axis value will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteAxisValue(float value)
		{
			if(type == InputType.RemoteAxis)
			{
				_value = value;
			}
			else
			{
				Debug.LogWarning(string.Format("You are trying to manually change the value of axis \'{0}\' which is not of type \'RemoteAxis\'", name));
			}
		}

		public void Copy(AxisConfiguration source)
		{
			name = source.name;
			description = source.description;
			positive = source.positive;
			altPositive = source.altPositive;
			negative = source.negative;
			altNegative = source.altNegative;
			deadZone = source.deadZone;
			gravity = source.gravity;
			sensitivity = source.sensitivity;
			snap = source.snap;
			invert = source.invert;
			type = source.type;
			axis = source.axis;
			joystick = source.joystick;
		}

		public void Reset()
		{
			_value = Neutral;
		}

		private void UpdateRawAxisName()
		{
			if(type == InputType.MouseAxis)
			{
				if(axis < 0 || axis >= MaxMouseAxes) 
				{
					string message = string.Format("Desired mouse axis is out of range. Mouse axis will be clamped to {0}.", 
					                               Mathf.Clamp(axis, 0, MaxMouseAxes - 1));
					Debug.LogWarning(message);
				}

				if (axis == 0) {
				}
				_rawAxisName = string.Concat("Mouse_", Mathf.Clamp(axis, 0, MaxMouseAxes - 1));
			}
			else if(type == InputType.AnalogAxis || type == InputType.AnalogButton)
			{
				if(joystick < 0 || joystick >= MaxJoysticks)
				{
					string message = string.Format("Desired joystick is out of range. Joystick has been clamped to {0}.",
					                               Mathf.Clamp(joystick, 0, MaxJoysticks - 1));
					Debug.LogWarning(message);
				}
				if(axis >= MaxJoystickAxes)
				{
					string message = string.Format("Desired joystick axis is out of range. Joystick axis will be clamped to {0}.", 
					                               Mathf.Clamp(axis, 0, MaxJoystickAxes - 1));
					Debug.LogWarning(message);
				}

				_rawAxisName = string.Concat("Axis_", Mathf.Clamp(axis, 0, MaxJoystickAxes - 1));
			}
			else
			{
				_rawAxisName = string.Empty;
			}
		}
		
		public static InputType StringToInputType(string value)
		{
			if(string.IsNullOrEmpty(value)) {
				return InputType.Button;
			}
			try {
				return (InputType)Enum.Parse(typeof(InputType), value, true);
			}
			catch {
				return InputType.Button;
			}
		}
		
		public static AxisConfiguration Duplicate(AxisConfiguration source)
		{
			AxisConfiguration axisConfig = new AxisConfiguration();
			axisConfig.name = source.name;
			axisConfig.description = source.description;
			axisConfig.positive = source.positive;
			axisConfig.altPositive = source.altPositive;
			axisConfig.negative = source.negative;
			axisConfig.altNegative = source.altNegative;
			axisConfig.deadZone = source.deadZone;
			axisConfig.gravity = source.gravity;
			axisConfig.sensitivity = source.sensitivity;
			axisConfig.snap = source.snap;
			axisConfig.invert = source.invert;
			axisConfig.type = source.type;
			axisConfig.axis = source.axis;
			axisConfig.joystick = source.joystick;
			
			return axisConfig;
		}
	}
}