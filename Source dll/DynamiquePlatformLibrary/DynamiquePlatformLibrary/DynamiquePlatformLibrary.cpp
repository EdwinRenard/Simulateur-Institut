// DynamiquePlatformLibrary.cpp : Defines the exported functions for the DLL application.
//
#define EXPORT_API __declspec(dllexport)
#define STRICT
#define DIRECTINPUT_VERSION 0x0800
#define MAX_NUM_AXIS 4
#define MAX_NUM_BUTTONS 28
#define DAQmxErrChk(functionCall) if( DAQmxFailed(error=(functionCall)) ) goto Error; else

#include <dinput.h>
#include <math.h>
#include <iostream>
#include <vector>
#include <string>
#include <NIDAQmx.h>

using namespace std;

extern "C"
{
	EXPORT_API HRESULT InitDynamiqueJoystick(HWND hwnd);
	EXPORT_API VOID StopDynamiqueJoystick();
	EXPORT_API VOID UpdateState();
	EXPORT_API double getActionState(const char* action);
}

struct JoystickState{
	LONG RawAxis[MAX_NUM_AXIS];
	BYTE Button[MAX_NUM_BUTTONS];
};

enum Action{
	Axis_0 = 0,
	Axis_1,
	Axis_2,
	Axis_3,
	Button0,
	Button1,
	Button2,
	Button3,
	Button4,
	Button5,
	Button6,
	Button7,
	LeftIndicator,
	RightIndicator,
	Sidelight,
	CrossingLight,
	RoadLight,
	Call,
	FrontFogLamp,
	RearFogLamp,
	Mist,
	Int,
	Low,
	High,
	RearWiper,
	RearWasher1,
	RearWasher2,
	FrontWasher,
	StartButton,
	Klaxon,
	RedButton,
	Ceinture,
	END_ACTIONS
};

JoystickState joystickState;
LPDIRECTINPUT8 g_DI = NULL;
LPDIRECTINPUTDEVICE8 g_DeviceGear = NULL;
LPDIRECTINPUTDEVICE8 g_DevicePedal = NULL;
std::vector<DIDEVICEOBJECTINSTANCE> JoystickObjetsList;
DIDEVICEOBJECTINSTANCE* actionJoyObj[MAX_NUM_AXIS + MAX_NUM_BUTTONS];
DIDEVCAPS didc;

int32 error = 0;
TaskHandle taskHandle = 0;
uInt8 dataLeftCommand[8];
uInt8 dataRightCommand[8];
uInt8 dataPanelCommand[8];
char errBuff[2048];
int32 i;
int32 read, bytesPerSamp;


BOOL CALLBACK EnumDeviceObjectsProc(LPCDIDEVICEOBJECTINSTANCE lpddoi, LPVOID pvRef)
{
	vector<DIDEVICEOBJECTINSTANCE>& JoyObjList = *(vector<DIDEVICEOBJECTINSTANCE>*)pvRef;
	JoyObjList.push_back(*lpddoi);
	return DIENUM_CONTINUE;
}

BOOL CALLBACK EnumJoysticksCallback(const DIDEVICEINSTANCE* pInst, VOID* lpvContext)
{
	vector<DIDEVICEINSTANCE>& JoyDevList = *(vector<DIDEVICEINSTANCE>*)lpvContext;
	JoyDevList.push_back(*pInst);
	return DIENUM_CONTINUE;
}

BOOL CALLBACK EnumWnd(HWND hwnd, LPARAM lParam){
	*(HWND*)lParam = hwnd;
	return FALSE;
}

bool EnumJoyDevices(IDirectInput8* pDI, vector<DIDEVICEINSTANCE>& JoyDevList){
	JoyDevList.clear();
	HRESULT hr = pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumJoysticksCallback, (void*)&JoyDevList, DIEDFL_ATTACHEDONLY);
	if (FAILED(hr))
		return false;
	return true;
}

bool EnumJoyAxis(IDirectInputDevice8* device, vector<DIDEVICEOBJECTINSTANCE>& JoystickObjetsList){
	JoystickObjetsList.clear();

	HRESULT hr = device->EnumObjects(EnumDeviceObjectsProc, (void*)&JoystickObjetsList, DIDFT_ALL);
	if (FAILED(hr)){
		cout << "Fail Enumerate Device Objects (" << hr << ")" << endl;
		return false;
	}

	return true;
}

HRESULT InitDevice(HWND hwnd);

HRESULT InitDynamiqueJoystick(HWND hwnd){
	HINSTANCE hInst = (HINSTANCE)GetModuleHandle(NULL);

	memset(&joystickState, 0, sizeof(joystickState));

	HRESULT hr = DirectInput8Create(hInst, DIRECTINPUT_VERSION, IID_IDirectInput8, (VOID**)&g_DI, NULL);
	if (FAILED(hr)){
		cout << "Error Create Instance (" << hr << ")" << endl;
		return E_FAIL;
	}

	return InitDevice(hwnd);
}

VOID StopDynamiqueJoystick(){
	if (g_DeviceGear != NULL){
		g_DeviceGear->Release();
		g_DeviceGear = NULL;
	}

	if (g_DevicePedal != NULL){
		g_DevicePedal->Release();
		g_DevicePedal = NULL;
	}

	if (g_DI != NULL){
		g_DI->Release();
		g_DI = NULL;
	}

	DAQmxStopTask(taskHandle);
	DAQmxClearTask(taskHandle);
}

DIOBJECTDATAFORMAT InitObjDataFormat(const GUID *pguid, DWORD dwOfs, DWORD dwType, DWORD dwFlags)
{
	DIOBJECTDATAFORMAT tmp;
	tmp.pguid = pguid;
	tmp.dwOfs = dwOfs;
	tmp.dwType = dwType;
	tmp.dwFlags = dwFlags;
	return tmp;
}

HRESULT InitDevice(HWND hwnd){
	vector<DIDEVICEINSTANCE> JoystickDeviceList;
	HRESULT hr;
	if (!EnumJoyDevices(g_DI, JoystickDeviceList)){
		cout << "Error Enumeration Joystick Devices." << endl;
		return E_FAIL;
	}
	cout << "Direct Input Interface created !" << endl;
	for (int i = 0; i < JoystickDeviceList.size(); i++){
		cout << "Direct Input device Name : " << JoystickDeviceList[i].tszProductName << endl;
		if (string(JoystickDeviceList[i].tszProductName).find("Pedals") != string::npos){
			LPDIRECTINPUTDEVICE8 deviceTemp;
			hr = g_DI->CreateDevice(JoystickDeviceList[i].guidInstance, &deviceTemp, NULL);
			if (FAILED(hr)){
				cout << "Fail Create Device" << endl;
				return E_FAIL;
			}

			hr = deviceTemp->QueryInterface(IID_IDirectInputDevice8, (LPVOID*)&g_DevicePedal);
			if (FAILED(hr)){
				cout << "Fail Create Pedals Device" << endl;
				return E_FAIL;
			}

			cout << "Pedals Device Created !" << endl;

			JoystickObjetsList.clear();
			if (!EnumJoyAxis(g_DevicePedal, JoystickObjetsList))
				return E_FAIL;

			int axisIndex = 0;
			vector<DIOBJECTDATAFORMAT> MyFormat;
			for (size_t i = 0; i < JoystickObjetsList.size(); i++)
			{
				int instance = DIDFT_GETINSTANCE(JoystickObjetsList[i].dwType);
				if ((JoystickObjetsList[i].dwType&DIDFT_AXIS) != 0){
					if (axisIndex >= MAX_NUM_AXIS)
						break;

					if (JoystickObjetsList[i].guidType == GUID_RxAxis){
						cout << "Index X : " << axisIndex << endl;
						MyFormat.push_back(InitObjDataFormat(&JoystickObjetsList[i].guidType, FIELD_OFFSET(JoystickState, RawAxis[Axis_0]), JoystickObjetsList[i].dwType, 0));
						axisIndex++;
					}
					else if (JoystickObjetsList[i].guidType == GUID_RyAxis){
						cout << "Index Y : " << axisIndex << endl;
						MyFormat.push_back(InitObjDataFormat(&JoystickObjetsList[i].guidType, FIELD_OFFSET(JoystickState, RawAxis[Axis_1]), JoystickObjetsList[i].dwType, 0));
						axisIndex++;
					}
					else if (JoystickObjetsList[i].guidType == GUID_RzAxis){
						cout << "Index Z : " << axisIndex << endl;
						MyFormat.push_back(InitObjDataFormat(&JoystickObjetsList[i].guidType, FIELD_OFFSET(JoystickState, RawAxis[Axis_2]), JoystickObjetsList[i].dwType, 0));
						axisIndex++;
					}
				}
			}

			DIDATAFORMAT MyJoyFormat;
			memset(&MyJoyFormat, 0, sizeof(MyJoyFormat));
			MyJoyFormat.dwSize = sizeof(DIDATAFORMAT);
			MyJoyFormat.dwObjSize = sizeof(DIOBJECTDATAFORMAT);
			MyJoyFormat.dwFlags = DIDF_ABSAXIS;
			MyJoyFormat.dwDataSize = sizeof(JoystickState);
			MyJoyFormat.dwNumObjs = (DWORD)MyFormat.size();
			MyJoyFormat.rgodf = &MyFormat[0];
			hr = g_DevicePedal->SetDataFormat(&MyJoyFormat);

			if (FAILED(hr)){
				cout << "Fail Set Data Format Gear Device (" << hr << ")" << endl;
				return E_FAIL;
			}

			hr = g_DevicePedal->SetCooperativeLevel(hwnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
			if (FAILED(hr)){
				cout << "Fail Set Cooperative Device (" << hr << ")" << endl;
				return E_FAIL;
			}



			g_DevicePedal->Acquire();
		}
		else if (string(JoystickDeviceList[i].tszProductName).find("Gear") != string::npos){
			hr = g_DI->CreateDevice(JoystickDeviceList[i].guidInstance, &g_DeviceGear, NULL);

			if (FAILED(hr)){
				cout << "Fail Create Gear Device (" << hr << ")" << endl;
				return E_FAIL;
			}

			cout << "Gear Device Created !" << endl;

			JoystickObjetsList.clear();
			if (!EnumJoyAxis(g_DeviceGear, JoystickObjetsList))
				return E_FAIL;

			vector<DIOBJECTDATAFORMAT> MyFormat;
			for (size_t i = 0; i < JoystickObjetsList.size(); i++)
			{
				int instance = DIDFT_GETINSTANCE(JoystickObjetsList[i].dwType);
				if ((JoystickObjetsList[i].dwType&DIDFT_BUTTON) != 0 && instance < MAX_NUM_BUTTONS)
				{
					MyFormat.push_back(InitObjDataFormat(&GUID_Button, FIELD_OFFSET(JoystickState, Button[instance]), JoystickObjetsList[i].dwType, 0));
				}
			}

			DIDATAFORMAT MyJoyFormat;
			memset(&MyJoyFormat, 0, sizeof(MyJoyFormat));
			MyJoyFormat.dwSize = sizeof(DIDATAFORMAT);
			MyJoyFormat.dwObjSize = sizeof(DIOBJECTDATAFORMAT);
			MyJoyFormat.dwFlags = DIDF_ABSAXIS;
			MyJoyFormat.dwDataSize = sizeof(JoystickState);
			MyJoyFormat.dwNumObjs = (DWORD)MyFormat.size();
			MyJoyFormat.rgodf = &MyFormat[0];
			hr = g_DeviceGear->SetDataFormat(&MyJoyFormat);

			if (FAILED(hr)){
				cout << "Fail Set Data Format Gear Device (" << hr << ")" << endl;
				return E_FAIL;
			}

			hr = g_DeviceGear->SetCooperativeLevel(hwnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
			if (FAILED(hr)){
				cout << "Fail Set Cooperative Device (" << hr << ")" << endl;
				return E_FAIL;
			}

			g_DeviceGear->Acquire();
		}
	}
	return S_OK;
}

double GetRawAxisPos(int i){
	if ((i >= 0) && (i < MAX_NUM_AXIS)){
		return (1.0 - (joystickState.RawAxis[i] / 65535.0));
	}
	return 0;
}

bool GetButtonPos(int i){
	if ((i >= MAX_NUM_AXIS) && (i < (MAX_NUM_AXIS + MAX_NUM_BUTTONS))){
		return joystickState.Button[i - MAX_NUM_AXIS] != 0;
	}
	return false;
}

double getActionState(const char* action){

	UpdateState();

	if (strcmp(action,"Button0") == 0){
		return GetButtonPos(Button0);
	}
	else if (strcmp(action, "Button1") == 0){
		return GetButtonPos(Button1);
	}
	else if (strcmp(action, "Button2") == 0){
		return GetButtonPos(Button2);
	}
	else if (strcmp(action, "Button3") == 0){
		return GetButtonPos(Button3);
	}
	else if (strcmp(action, "Button4") == 0){
		return GetButtonPos(Button4);
	}
	else if (strcmp(action, "Button5") == 0){
		return GetButtonPos(Button5);
	}
	else if (strcmp(action, "Button6") == 0){
		return GetButtonPos(Button6);
	}
	else if (strcmp(action, "Button7") == 0){
		return GetButtonPos(Button7);
	}
	else if (strcmp(action, "Axis_0") == 0){
		return GetRawAxisPos(Axis_0);
	}
	else if (strcmp(action, "Axis_1") == 0){
		return GetRawAxisPos(Axis_1);
	}
	else if (strcmp(action, "Axis_2") == 0){
		return GetRawAxisPos(Axis_2);
	}
	else if (strcmp(action, "Axis_3") == 0){
		return GetRawAxisPos(Axis_3);
	}
	else if (strcmp(action, "LeftIndicator") == 0){
		return dataLeftCommand[1] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "RightIndicator") == 0){
		return dataLeftCommand[0] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Sidelight") == 0){
		return dataLeftCommand[2] == 0 && dataLeftCommand[4] == 1 && dataLeftCommand[3] == 1 ? 1 : 0;
	}
	else if (strcmp(action, "CrossingLight") == 0){
		return dataLeftCommand[4] == 0 && dataLeftCommand[2] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "RoadLight") == 0){
		return dataLeftCommand[3] == 0 && dataLeftCommand[2] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Call") == 0){
		return dataLeftCommand[3] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "FrontFogLamp") == 0){
		return dataLeftCommand[5] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "RearFogLamp") == 0){
		return dataLeftCommand[6] == 0 && dataLeftCommand[5] == 0 && dataLeftCommand[2] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Mist") == 0){
		return dataRightCommand[3] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Int") == 0){
		return dataRightCommand[1] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Low") == 0){
		return dataRightCommand[3] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "High") == 0){
		return dataRightCommand[2] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "RearWiper") == 0){
		return dataRightCommand[6] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "RearWasher1") == 0){
		return dataRightCommand[4] == 0 && dataRightCommand[6] != 0 ? 1 : 0;
	}
	else if (strcmp(action, "RearWasher2") == 0){
		return dataRightCommand[6] == 0 && dataRightCommand[4] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "FrontWasher") == 0){
		return dataRightCommand[5] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "StartButton") == 0){
		return dataPanelCommand[0] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Klaxon") == 0){
		return dataPanelCommand[2] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "RedButton") == 0){
		return dataPanelCommand[1] == 0 ? 1 : 0;
	}
	else if (strcmp(action, "Ceinture") == 0){
		return dataPanelCommand[3] == 1 ? 1 : 0;
	}
	else{
		return 0.0;
	}
}

void UpdateState(){
	if (g_DeviceGear != NULL){
		JoystickState tmp;
		HRESULT hr = g_DeviceGear->GetDeviceState(sizeof(JoystickState), &tmp);

		if (FAILED(hr)){
			cout << "Fail Update State : " << hr << endl;
			hr = g_DeviceGear->Acquire();
			if (FAILED(hr)){
				cout << "Fail acquire (" << hr << ")" << endl;
			}
			else{
				cout << "Success acquire" << endl;
			}
		}
		else{
			cout << "Success" << endl;
			for (int i = MAX_NUM_AXIS; i < MAX_NUM_AXIS+8; i++){
				joystickState.Button[i - MAX_NUM_AXIS] = tmp.Button[i - MAX_NUM_AXIS];
			}
		}
	}

	if (g_DevicePedal != NULL){
		JoystickState tmp;
		HRESULT hr = g_DevicePedal->GetDeviceState(sizeof(JoystickState), &tmp);

		if (FAILED(hr)){
			cout << "Fail Update State : " << hr << endl;
			hr = g_DevicePedal->Acquire();
			if (FAILED(hr)){
				cout << "Fail acquire (" << hr << ")" << endl;
			}
			else{
				cout << "Success acquire" << endl;
			}
		}
		else{
			cout << "Success" << endl;
			for (int i = Axis_0; i <= Axis_2; i++){
				joystickState.RawAxis[i - Axis_0] = tmp.RawAxis[i - Axis_0];
			}
		}
	}

	uInt8 dataTemp[24];
	DAQmxErrChk(DAQmxCreateTask("", &taskHandle));
	DAQmxErrChk(DAQmxCreateDIChan(taskHandle, "Dev1/port0/line0:7,Dev1/port1/line0:7,Dev1/port2/line0:7", "", DAQmx_Val_ChanForAllLines));
	DAQmxErrChk(DAQmxStartTask(taskHandle));
	DAQmxErrChk(DAQmxReadDigitalLines(taskHandle, 1, 10.0, DAQmx_Val_GroupByChannel, dataTemp, 24, &read, &bytesPerSamp, NULL));

	for (int i = 0; i < 8; i++){
		dataLeftCommand[i] = dataTemp[i];
		dataRightCommand[i] = dataTemp[8 + i];
		dataPanelCommand[i] = dataTemp[16 + i];
	}

Error:
	if (DAQmxFailed(error)){
		DAQmxGetExtendedErrorInfo(errBuff, 2048);
	}
	if (taskHandle != 0){
		DAQmxStopTask(taskHandle);
		DAQmxClearTask(taskHandle);
	}
	if (DAQmxFailed(error)){
		cout << "DAQmx Error : " << errBuff << endl;
	}
}