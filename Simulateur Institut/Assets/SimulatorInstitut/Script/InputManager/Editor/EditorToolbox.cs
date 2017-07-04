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
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using SimulatorInstitut;

namespace SimulatorInstitut
{
	public static class EditorToolbox
	{
		private static string _snapshotFile;
		private static string[] _axisNames;
		private static string[] _mouseAxisNames;
		private static string[] _joystickNames;


		public static string[] GenerateJoystickAxisNames()
		{
			if(_axisNames == null || _axisNames.Length != AxisConfiguration.MaxJoystickAxes)
			{
				_axisNames = new string[AxisConfiguration.MaxJoystickAxes];
				for(int i = 0; i < AxisConfiguration.MaxJoystickAxes; i++)
				{
					_axisNames[i] = string.Format("Axis_{0}", i);
				}
			}

			return _axisNames;
		}

		public static string[] GenerateMouseAxisNames()
		{
			if(_mouseAxisNames == null || _mouseAxisNames.Length != AxisConfiguration.MaxMouseAxes)
			{
				_mouseAxisNames = new string[AxisConfiguration.MaxMouseAxes];
				for(int i = 0; i < AxisConfiguration.MaxMouseAxes; i++)
				{
					_mouseAxisNames[i] = string.Format("Mouse_{0}", i);
				}
			}

			return _mouseAxisNames;
		}

		public static string[] GenerateJoystickNames()
		{
			if(_joystickNames == null || _joystickNames.Length != AxisConfiguration.MaxJoysticks)
			{
				_joystickNames = new string[AxisConfiguration.MaxJoysticks];
				for(int i = 0; i < AxisConfiguration.MaxJoysticks; i++)
				{
					_joystickNames[i] = string.Format("Joystick {0}", i + 1);
				}
			}

			return _joystickNames;
		}

		public static bool CanLoadSnapshot()
		{
			if(_snapshotFile == null)
			{
				_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}
			
			return File.Exists(_snapshotFile);
		}
		
		public static void CreateSnapshot(SimulatorInstitut.InputManager inputManager)
		{
			if(_snapshotFile == null)
			{
				_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}

			InputSaverXML inputSaver = new InputSaverXML(_snapshotFile);
			inputSaver.Save(inputManager.GetSaveParameters());
		}
		
		public static void LoadSnapshot(SimulatorInstitut.InputManager inputManager)
		{
			if(!CanLoadSnapshot())
				return;
			
			InputLoaderXML inputLoader = new InputLoaderXML(_snapshotFile);
            inputManager.Load(inputLoader.Load());
		}
		
		public static void KeyCodeField(ref string keyString, ref bool isEditing, string label, string controlName, string currentKey)
		{
			GUI.SetNextControlName(controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == controlName);
			if(!isEditing && hasFocus)
			{
				keyString = currentKey == string.Empty ? string.Empty : currentKey.ToString();
			}
			
			isEditing = hasFocus;
			if(isEditing)
			{
				keyString = EditorGUILayout.TextField(label, keyString);
			}
			else
			{
				EditorGUILayout.TextField(label, currentKey == string.Empty ? string.Empty : currentKey.ToString());
			}
		}

		public static bool HasJoystickMappingAddon()
		{
			return GetMappingImporterWindowType() != null;
		}

		public static void OpenImportJoystickMappingWindow(AdvancedInputEditor configurator)
		{
			Type type = GetMappingImporterWindowType();
			if(type == null)
				return;

			MethodInfo methodInfo = type.GetMethod("Open", BindingFlags.Static | BindingFlags.Public);
			if(methodInfo == null)
			{
				Debug.LogError("Unable to open joystick mapping import window");
			}

			methodInfo.Invoke(null, new object[] { configurator });
		}

		private static Type GetMappingImporterWindowType()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			return Array.Find<Type>(assembly.GetTypes(), (type) => { return type.Name == "MappingImportWindow"; });
		}

		public static bool HasInputAdapterAddon()
		{
			Assembly assembly = typeof(SimulatorInstitut.InputManager).Assembly;
			Type inputAdapterType = Array.Find<Type>(assembly.GetTypes(), (type) => { return type.Name == "InputAdapter"; });
			return inputAdapterType != null;
		}
		
		/// <summary>
		/// Used to get access to the hidden toolbar search field.
		/// Credits go to the user TowerOfBricks for finding the way to do it.
		/// </summary>
		public static string SearchField(string searchString, params GUILayoutOption[] layoutOptions)
		{
			Type type = typeof(EditorGUILayout);
			string methodName = "ToolbarSearchField";
			System.Object[] parameters = new System.Object[] { searchString, layoutOptions };
			string result = null;
			
			Type[] types = new Type[parameters.Length];
			for(int i = 0; i < types.Length; i++)
			{
				types[i] = parameters[i].GetType();
			}
			MethodInfo method = type.GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public),
												null, types, null);
			
			if(method.IsStatic)
			{
				result = (string)method.Invoke(null, parameters);
			}
			else
			{
				var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
									BindingFlags.Instance | BindingFlags.CreateInstance;
				System.Object obj = type.InvokeMember(null, bindingFlags, null, null, new System.Object[0]);
				
				result = (string)method.Invoke(obj, parameters);
			}
			
			return (result != null) ? result : "";
		}
	}
}
