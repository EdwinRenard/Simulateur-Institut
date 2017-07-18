// DBoxInterfaceLibrary.cpp	 : Defines the exported functions for the DLL application.
//

#define EXPORT_API __declspec(dllexport)
#define STRICT

#include <cstdio>
#include <conio.h>
#include <Windows.h>
#include "LiveMotionSdk\dboxLiveMotion.h"
#include <math.h>

#pragma comment(lib, DBOX_LIVEMOTION_LIB)

extern "C"
{
	EXPORT_API HRESULT InitDBoxPlatform();
	EXPORT_API VOID StopDBoxPlatform();
	EXPORT_API HRESULT Move(float heave, float pitch, float roll);
	EXPORT_API HRESULT Vibration(float rate, float strength);
}

const char* const APP_KEY = "AmvalorDevSim";
const dbox::U32 APP_BUILD = 1000;
const double PI = 3.141592654f;

struct SimConfig{
	dbox::F32 MasterGain;
	dbox::F32 MasterSpectrum;

	DBOX_STRUCTINFO_BEGIN()
		DBOX_STRUCTINFO_FIELD(SimConfig, MasterGain, dbox::FT_FLOAT32, dbox::FM_MASTER_GAIN_DB)
		DBOX_STRUCTINFO_FIELD(SimConfig, MasterSpectrum, dbox::FT_FLOAT32, dbox::FM_MASTER_SPECTRUM_DB)
	DBOX_STRUCTINFO_END()
};

struct MotionConfig{
	dbox::F32 EngineRpmIdle;
	dbox::F32 EngineRpmMax;
	dbox::F32 EngineTorqueMax;

	DBOX_STRUCTINFO_BEGIN()
		DBOX_STRUCTINFO_FIELD(MotionConfig, EngineRpmIdle, dbox::FT_FLOAT32, dbox::FM_ENGINE_RPM_IDLE)
		DBOX_STRUCTINFO_FIELD(MotionConfig, EngineRpmMax, dbox::FT_FLOAT32, dbox::FM_ENGINE_RPM_MAX)
		DBOX_STRUCTINFO_FIELD(MotionConfig, EngineTorqueMax, dbox::FT_FLOAT32, dbox::FM_ENGINE_TORQUE_MAX)
	DBOX_STRUCTINFO_END()
};

struct MotionData{
	dbox::F32 Roll;		// -1.0 to 1.0
	dbox::F32 Pitch;	// -1.0 to 1.0
	dbox::F32 Heave;	// -1.0 to 1.0
	dbox::F32 EngineRpm;	//Vibration rate
	dbox::F32 EngineTorque;	// Vibration Strength

	DBOX_STRUCTINFO_BEGIN()
		DBOX_STRUCTINFO_FIELD(MotionData, Roll, dbox::FT_FLOAT32, dbox::FM_RAW_ROLL)
		DBOX_STRUCTINFO_FIELD(MotionData, Pitch, dbox::FT_FLOAT32, dbox::FM_RAW_PITCH)
		DBOX_STRUCTINFO_FIELD(MotionData, Heave, dbox::FT_FLOAT32, dbox::FM_RAW_HEAVE)
		DBOX_STRUCTINFO_FIELD(MotionData, EngineRpm, dbox::FT_FLOAT32, dbox::FM_ENGINE_RPM)
		DBOX_STRUCTINFO_FIELD(MotionData, EngineTorque, dbox::FT_FLOAT32, dbox::FM_ENGINE_TORQUE)
	DBOX_STRUCTINFO_END()
};

MotionData oMotionData;

/// These are your unique event ids that you'll use when calling PostEvent.
enum AppEvents {
	SIM_CONFIG = 1000,
	MOTION_CONFIG = 2000,
	MOTION_DATA = 3000,
};


/// This function is used to prevent possible error when the application ends before the normal execution.
/// This includes Closing the console window, CTRL-C, Windows Shutdown, Log Off...
bool WINAPI OnAbnormalTerminate(DWORD /*dwCtrlType*/) {
	Move(0, 0, 0);
	dbox::LiveMotion::Stop();
	dbox::LiveMotion::Close();
	dbox::LiveMotion::Terminate();
	return true;
}

EXPORT_API HRESULT InitDBoxPlatform(){
	SetConsoleCtrlHandler((PHANDLER_ROUTINE)OnAbnormalTerminate, true);

	dbox::ELiveMotionResult hr = dbox::LiveMotion::Initialize(APP_KEY, APP_BUILD);
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}
	hr = dbox::LiveMotion::RegisterEvent(SIM_CONFIG, dbox::EM_CONFIG_UPDATE, SimConfig::GetStructInfo());
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}
	hr = dbox::LiveMotion::RegisterEvent(MOTION_CONFIG, dbox::EM_CONFIG_UPDATE, MotionConfig::GetStructInfo());
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}
	hr = dbox::LiveMotion::RegisterEvent(MOTION_DATA, dbox::EM_FRAME_UPDATE, MotionData::GetStructInfo());
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	hr = dbox::LiveMotion::Open();
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	hr = dbox::LiveMotion::ResetState();
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	SimConfig oSimConfig;
	oSimConfig.MasterGain = 0; // 0dB
	oSimConfig.MasterSpectrum = 0; // 0dB
	hr = dbox::LiveMotion::PostEvent(SIM_CONFIG, oSimConfig);
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	MotionConfig oMotionConfig;
	oMotionConfig.EngineRpmIdle = 750.4f;
	oMotionConfig.EngineRpmMax = 3420.2f;
	oMotionConfig.EngineTorqueMax = 447.42f;
	hr = dbox::LiveMotion::PostEvent(MOTION_CONFIG, oMotionConfig);
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	oMotionData.Roll = 0.0f;
	oMotionData.Pitch = 0.0f;
	oMotionData.Heave = 0.0f;
	oMotionData.EngineRpm = 0.0f;
	oMotionData.EngineTorque = 0.0f;
	hr = dbox::LiveMotion::PostEvent(MOTION_DATA, oMotionData);
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	hr = dbox::LiveMotion::Start();
	if (hr != dbox::LMR_SUCCESS){
		return hr;
	}

	Sleep(10000);

	return S_OK;
}

EXPORT_API VOID StopDBoxPlatform(){
	Vibration(0.0, 0.0);
	Move(0.0, 0.0, 0.0);

	Sleep(10000);

	if (dbox::LiveMotion::IsStarted()){
		dbox::LiveMotion::Stop();
	}
	if (dbox::LiveMotion::IsOpened()){
		dbox::LiveMotion::Close();
	}
	if (dbox::LiveMotion::IsInitialized()){
		dbox::LiveMotion::Terminate();
	}
}

EXPORT_API HRESULT Move(float heave, float pitch, float roll){
	if (!dbox::LiveMotion::IsInitialized){
		HRESULT hr = InitDBoxPlatform();
		if (hr == E_FAIL){
			return E_FAIL;
		}
	}

	oMotionData.Heave = heave;
	oMotionData.Pitch = pitch;
	oMotionData.Roll = roll;

	dbox::LiveMotion::PostEvent(MOTION_DATA, oMotionData);

	return S_OK;
}

EXPORT_API HRESULT Vibration(float rate, float strength){
	oMotionData.EngineRpm = rate;
	oMotionData.EngineTorque = strength;

	dbox::LiveMotion::PostEvent(MOTION_DATA, oMotionData);
	return S_OK;
}