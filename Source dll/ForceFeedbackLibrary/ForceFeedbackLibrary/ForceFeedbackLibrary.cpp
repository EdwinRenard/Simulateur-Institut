//-----------------------------------------------------------------------------
// File: FFConst.cpp
//
// Desc: Demonstrates an application which sets a force feedback constant force 
//       determined by the user.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
//
// Maschine simulations modification for Unity
// Modified by Cameron Bonde ('Vectrex' on the forums)
// www.kartsim.com
// www.maschinesimulations.com
// 
// nb. Use at your own risk. So if your haptics enabled cat-suit throttles you, don't blame us ;)
//
//-----------------------------------------------------------------------------

#define EXPORT_API __declspec(dllexport) // Visual Studio needs annotating exported functions with this

#define STRICT
#define DIRECTINPUT_VERSION 0x0800

#include <dinput.h>
#include <math.h>
#include <iostream>


//-----------------------------------------------------------------------------
// Function prototypes 
//-----------------------------------------------------------------------------
BOOL CALLBACK EnumFFDevicesCallback(const DIDEVICEINSTANCE* pInst, VOID* pContext);
BOOL CALLBACK EnumAxesCallback(const DIDEVICEOBJECTINSTANCE* pdidoi, VOID* pContext);
HRESULT Acquire();

extern "C"
{
	EXPORT_API HRESULT InitDirectInput();
	EXPORT_API HRESULT InitForceFeedback(HWND hwnd);
	EXPORT_API HRESULT SetDeviceForcesXY(INT x, INT y);
	EXPORT_API HRESULT SetDelayForceXY(INT xForce, INT yForce, FLOAT delay);
	EXPORT_API HRESULT SetDurationForceXY(INT xForce, INT yForce, FLOAT delay);
	EXPORT_API HRESULT SetCustomForceXY(INT xForce, INT yForce, FLOAT duration, FLOAT delay);
	EXPORT_API HRESULT StartEffect();
	EXPORT_API HRESULT StopEffect();
	EXPORT_API HRESULT DetectForceFeedbackDevice();
	EXPORT_API VOID FreeForceFeedback();
	EXPORT_API VOID FreeDirectInput();
}

//-----------------------------------------------------------------------------
// Defines, constants, and global variables
//-----------------------------------------------------------------------------
#define SAFE_DELETE(p)  { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_RELEASE(p) { if(p) { (p)->Release(); (p)=NULL; } }

LPDIRECTINPUT8          g_DI = NULL;
LPDIRECTINPUTDEVICE8    g_Device = NULL;
LPDIRECTINPUTEFFECT     g_Effect = NULL;
BOOL                    g_Active = TRUE;
DWORD                   g_NumForceFeedbackAxis = 0;
LPDIRECTINPUTDEVICE8	g_LastDevice = NULL;

//-----------------------------------------------------------------------------
// Name: InitDirectInput()
// Desc: Register with the DirectInput subsystem and get a pointer to a IDirectInput interface we can use.
//-----------------------------------------------------------------------------
HRESULT InitDirectInput()
{
	if (NULL != g_DI)
	{
		return S_OK;
	}

	return DirectInput8Create(GetModuleHandle(NULL), DIRECTINPUT_VERSION, IID_IDirectInput8, (VOID**)&g_DI, NULL);
}

//-----------------------------------------------------------------------------
// Name: InitForceFeedback()
// Desc: Initialize the DirectInput variables.
//-----------------------------------------------------------------------------
HRESULT InitForceFeedback(HWND hwnd)
{
	HRESULT result;

	// Register with the DirectInput subsystem and get a pointer
	// to a IDirectInput interface we can use.
	if (FAILED(result = InitDirectInput()))
	{
		return result;
	}

	// Look for a force feedback device we can use
	if (FAILED(result = DetectForceFeedbackDevice()))
	{
		return result;
	}

	if (g_Device == NULL)
	{
		return -1;
	}

	// Set the data format to "simple joystick" - a predefined data format. A
	// data format specifies which controls on a device we are interested in,
	// and how they should be reported.
	//
	// This tells DirectInput that we will be passing a DIJOYSTATE structure to
	// IDirectInputDevice8::GetDeviceState(). Even though we won't actually do
	// it in this sample. But setting the data format is important so that the
	// DIJOFS_* values work properly.
	if (FAILED(result = g_Device->SetDataFormat(&c_dfDIJoystick)))
		return result;

	// Set the cooperative level to let DInput know how this device should
	// interact with the system and with other DInput applications.
	// Exclusive access is required in order to perform force feedback.
	if (FAILED(result = g_Device->SetCooperativeLevel(hwnd, DISCL_EXCLUSIVE | DISCL_BACKGROUND)))
	{
		return result;
	}

	// Reset number of axis
	g_NumForceFeedbackAxis = 0;

	// Enumerate and count the axes of the joystick 
	if (FAILED(result = g_Device->EnumObjects(EnumAxesCallback, (VOID*)&g_NumForceFeedbackAxis, DIDFT_AXIS)))
		return result;

	// This simple sample only supports one or two axis joysticks
	if (g_NumForceFeedbackAxis > 2)
		g_NumForceFeedbackAxis = 2;

	// This application needs only one effect: Applying raw forces.
	DWORD axes[2] = { DIJOFS_X, DIJOFS_Y };
	LONG direction[2] = { 0, 0 };
	DICONSTANTFORCE constantForce = { 0 };

	DIEFFECT effet;
	ZeroMemory(&effet, sizeof(effet));
	effet.dwSize = sizeof(DIEFFECT);
	effet.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
	effet.dwDuration = INFINITE;
	effet.dwSamplePeriod = 0;
	effet.dwGain = DI_FFNOMINALMAX;
	effet.dwTriggerButton = DIEB_NOTRIGGER;
	effet.dwTriggerRepeatInterval = 0;
	effet.cAxes = g_NumForceFeedbackAxis;
	effet.rgdwAxes = axes;
	effet.rglDirection = direction;
	effet.lpEnvelope = 0;
	effet.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	effet.lpvTypeSpecificParams = &constantForce;
	effet.dwStartDelay = 0;

	// Create the prepared effect
	if (FAILED(result = g_Device->CreateEffect(GUID_ConstantForce, &effet, &g_Effect, NULL)))
	{
		return result;
	}

	if (NULL == g_Effect)
		return E_FAIL;

	return Acquire();
}

// Initialise
HRESULT Acquire()
{
	if (g_Device == NULL)
	{
		return E_FAIL;
	}
	// Make sure the device is acquired, if we are gaining focus.
	return g_Device->Acquire();
}

HRESULT StartEffect()
{
	if (g_Device == NULL)
	{
		return E_FAIL;
	}
	if (g_Effect)
		return g_Effect->Start(1, 0); // Start the effect.
	else
		return E_FAIL;
}

HRESULT StopEffect()
{
	if (g_Device == NULL)
	{
		return E_FAIL;
	}
	if (g_Effect)
		return g_Effect->Stop();
	else
		return E_FAIL;
}

HRESULT DetectForceFeedbackDevice()
{
	if (g_DI == NULL) { return E_FAIL; }
	
	if (g_Device == NULL){
		HRESULT result = g_DI->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumFFDevicesCallback, NULL, DIEDFL_ATTACHEDONLY | DIEDFL_FORCEFEEDBACK);
		if (g_Device == NULL)
		{
			return E_FAIL;
		}
		else
		{
			return S_OK;
		}
	}
	else{
		return S_OK;
	}
}

//-----------------------------------------------------------------------------
// Name: EnumAxesCallback()
// Desc: Callback function for enumerating the axes on a joystick and counting
//       each force feedback enabled axis
//-----------------------------------------------------------------------------
BOOL CALLBACK EnumAxesCallback(const DIDEVICEOBJECTINSTANCE* pdidoi, VOID* pContext)
{
	DWORD* NumForceFeedbackAxis = (DWORD*)pContext;

	if ((pdidoi->dwFlags & DIDOI_FFACTUATOR) != 0)
		(*NumForceFeedbackAxis)++;

	return DIENUM_CONTINUE;
}




//-----------------------------------------------------------------------------
// Name: EnumFFDevicesCallback()
// Desc: Called once for each enumerated force feedback device. If we find
//       one, create a device interface on it so we can play with it.
//-----------------------------------------------------------------------------
BOOL CALLBACK EnumFFDevicesCallback(const DIDEVICEINSTANCE* pInst, VOID* pContext)
{
	LPDIRECTINPUTDEVICE8 device;
	HRESULT result;

	// Obtain an interface to the enumerated force feedback device.
	result = g_DI->CreateDevice(pInst->guidInstance, &device, NULL);
	
	// If it failed, then we can't use this device for some
	// bizarre reason.  (Maybe the user unplugged it while we
	// were in the middle of enumerating it.)  So continue enumerating
	if (FAILED(result))
	{
		g_Device = NULL;
		return DIENUM_CONTINUE;
	}

	// We successfully created an IDirectInputDevice8.  So stop looking 
	// for another one.
	//g_Device = device;

	if (g_LastDevice == NULL){
		g_LastDevice = g_Device = device;
	}
	else{
		if (g_LastDevice == device){
			g_Device = device;
		}
		else{
			return DIENUM_CONTINUE;
		}
	}

	return DIENUM_STOP;
}



//-----------------------------------------------------------------------------
// Name: SetDeviceForcesXY()
// Desc: Apply the X and Y forces to the effect we prepared.
//-----------------------------------------------------------------------------
HRESULT SetDeviceForcesXY(INT xForce, INT yForce)
{
	if (NULL == g_Effect) return E_FAIL;

	// Modifying an effect is basically the same as creating a new one, except
	// you need only specify the parameters you are modifying
	LONG direction[2] = { 0, 0 };

	DICONSTANTFORCE constantForce;

	if (g_NumForceFeedbackAxis == 1)
	{
		// If only one force feedback axis, then apply only one direction and 
		// keep the direction at zero
		constantForce.lMagnitude = xForce;
		direction[0] = 0;
	}
	else
	{
		// If two force feedback axis, then apply magnitude from both directions 
		direction[0] = xForce;
		direction[1] = yForce;
		constantForce.lMagnitude = (DWORD)sqrt((double)xForce * (double)xForce + (double)yForce * (double)yForce);
	}

	DIEFFECT effet;
	ZeroMemory(&effet, sizeof(effet));
	effet.dwSize = sizeof(DIEFFECT);
	effet.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
	effet.cAxes = g_NumForceFeedbackAxis;
	effet.rglDirection = direction;
	effet.lpEnvelope = 0;
	effet.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	effet.lpvTypeSpecificParams = &constantForce;
	effet.dwStartDelay = 0;

	// Now set the new parameters and start the effect immediately.
	return g_Effect->SetParameters(&effet, DIEP_DIRECTION | DIEP_TYPESPECIFICPARAMS | DIEP_START);
}

VOID FreeForceFeedback()
{
	// Unacquire the device one last time just in case 
	// the app tried to exit while the device is still acquired.
	if (g_Device)
		g_Device->Unacquire();

	// Release any DirectInput objects.
	SAFE_RELEASE(g_Effect);
	SAFE_RELEASE(g_Device);
	SAFE_RELEASE(g_LastDevice);
}

//-----------------------------------------------------------------------------
// Name: FreeDirectInput()
// Desc: Initialize the DirectInput variables.
//-----------------------------------------------------------------------------
VOID FreeDirectInput()
{
	FreeForceFeedback();
	SAFE_RELEASE(g_DI);
}

HRESULT SetDelayForceXY(INT xForce, INT yForce, FLOAT delay)
{
	if (NULL == g_Effect) return E_FAIL;

	LONG direction[2] = { 0, 0 };

	DICONSTANTFORCE constantForce;

	if (g_NumForceFeedbackAxis == 1)
	{
		constantForce.lMagnitude = xForce;
		direction[0] = 0;
	}
	else
	{
		direction[0] = xForce;
		direction[1] = yForce;
		constantForce.lMagnitude = (DWORD)sqrt((double)xForce * (double)xForce + (double)yForce * (double)yForce);
	}

	DIEFFECT effet;
	ZeroMemory(&effet, sizeof(effet));
	effet.dwSize = sizeof(DIEFFECT);
	effet.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
	effet.cAxes = g_NumForceFeedbackAxis;
	effet.dwDuration = INFINITE;
	effet.rglDirection = direction;
	effet.lpEnvelope = 0;
	effet.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	effet.lpvTypeSpecificParams = &constantForce;
	effet.dwStartDelay = delay * 1000000;		//In microsecond base

	return g_Effect->SetParameters(&effet, DIEP_DIRECTION | DIEP_TYPESPECIFICPARAMS | DIEP_DURATION | DIEP_STARTDELAY | DIEP_START);
}

HRESULT SetDurationForceXY(INT xForce, INT yForce, FLOAT duration)
{
	if (NULL == g_Effect) return E_FAIL;

	LONG direction[2] = { 0, 0 };

	DICONSTANTFORCE constantForce;

	if (g_NumForceFeedbackAxis == 1)
	{
		constantForce.lMagnitude = xForce;
		direction[0] = 0;
	}
	else
	{
		direction[0] = xForce;
		direction[1] = yForce;
		constantForce.lMagnitude = (DWORD)sqrt((double)xForce * (double)xForce + (double)yForce * (double)yForce);
	}

	DIEFFECT effet;
	ZeroMemory(&effet, sizeof(effet));
	effet.dwSize = sizeof(DIEFFECT);
	effet.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
	effet.cAxes = g_NumForceFeedbackAxis;
	effet.dwDuration = duration * 1000000;		//In microsecond base
	effet.rglDirection = direction;
	effet.lpEnvelope = 0;
	effet.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	effet.lpvTypeSpecificParams = &constantForce;
	effet.dwStartDelay = 0;

	return g_Effect->SetParameters(&effet, DIEP_DIRECTION | DIEP_TYPESPECIFICPARAMS | DIEP_DURATION | DIEP_STARTDELAY | DIEP_START);
}

HRESULT SetCustomForceXY(INT xForce, INT yForce, FLOAT duration, FLOAT delay)
{
	if (NULL == g_Effect) return E_FAIL;

	LONG direction[2] = { 0, 0 };

	DICONSTANTFORCE constantForce;

	if (g_NumForceFeedbackAxis == 1)
	{
		constantForce.lMagnitude = xForce;
		direction[0] = 0;
	}
	else
	{
		direction[0] = xForce;
		direction[1] = yForce;
		constantForce.lMagnitude = (DWORD)sqrt((double)xForce * (double)xForce + (double)yForce * (double)yForce);
	}

	DIEFFECT effet;
	ZeroMemory(&effet, sizeof(effet));
	effet.dwSize = sizeof(DIEFFECT);
	effet.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
	effet.cAxes = g_NumForceFeedbackAxis;
	effet.dwDuration = duration * 1000000;		//In microsecond base
	effet.rglDirection = direction;
	effet.lpEnvelope = 0;
	effet.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	effet.lpvTypeSpecificParams = &constantForce;
	effet.dwStartDelay =delay * 1000000;		//In microsecond base

	return g_Effect->SetParameters(&effet, DIEP_DIRECTION | DIEP_TYPESPECIFICPARAMS | DIEP_DURATION | DIEP_STARTDELAY | DIEP_START);
}
