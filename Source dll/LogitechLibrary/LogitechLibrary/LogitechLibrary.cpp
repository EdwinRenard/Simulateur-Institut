// InputDynamiqueLibrary.cpp : Defines the exported functions for the DLL application.
//
#define EXPORT_API __declspec(dllexport)
#define STRICT
#define DIRECTINPUT_VERSION 0x0800
#define MAX_NUM_AXIS 4
#define MAX_NUM_BUTTONS 19

#include <dinput.h>
#include <math.h>
#include <iostream>
#include <vector>
#include <string>

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
	Button8,
	Button9,
	Button10,
	Button11,
	Button12,
	Button13,
	Button14,
	Button15,
	Button16,
	Button17,
	Button18,
	END_ACTIONS
};

JoystickState joystickState;
LPDIRECTINPUT8 g_DI = NULL;
LPDIRECTINPUTDEVICE8 g_Device = NULL;
std::vector<DIDEVICEOBJECTINSTANCE> JoystickObjetsList;
DIDEVICEOBJECTINSTANCE* actionJoyObj[MAX_NUM_AXIS + MAX_NUM_BUTTONS];
DIDEVCAPS didc;


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
	if (g_Device != NULL){
		g_Device->Release();
		g_Device = NULL;
	}

	if (g_DI != NULL){
		g_DI->Release();
		g_DI = NULL;
	}
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
		if (string(JoystickDeviceList[i].tszProductName).find("Logitech") != string::npos){
			LPDIRECTINPUTDEVICE8 deviceTemp;
			hr = g_DI->CreateDevice(JoystickDeviceList[i].guidInstance, &deviceTemp, NULL);
			if (FAILED(hr)){
				cout << "Fail Create Device" << endl;
				return E_FAIL;
			}

			hr = deviceTemp->QueryInterface(IID_IDirectInputDevice8, (LPVOID*)&g_Device);
			if (FAILED(hr)){
				cout << "Fail Create Device" << endl;
				return E_FAIL;
			}

			cout << "Device Created !" << endl;

			JoystickObjetsList.clear();
			if (!EnumJoyAxis(g_Device, JoystickObjetsList))
				return E_FAIL;

			int axisIndex = 0;
			vector<DIOBJECTDATAFORMAT> MyFormat;
			for (size_t i = 0; i < JoystickObjetsList.size(); i++)
			{
				int instance = DIDFT_GETINSTANCE(JoystickObjetsList[i].dwType);
				if ((JoystickObjetsList[i].dwType&DIDFT_AXIS) != 0){
					if (axisIndex >= MAX_NUM_AXIS)
						break;

					MyFormat.push_back(InitObjDataFormat(&JoystickObjetsList[i].guidType, FIELD_OFFSET(JoystickState, RawAxis[axisIndex]), JoystickObjetsList[i].dwType, 0));
					if (JoystickObjetsList[i].guidType == GUID_RxAxis){
						actionJoyObj[Axis_0] = &JoystickObjetsList[i];
						axisIndex++;
					}
					else if (JoystickObjetsList[i].guidType == GUID_RyAxis){
						actionJoyObj[Axis_1] = &JoystickObjetsList[i];
						axisIndex++;
					}
					else if (JoystickObjetsList[i].guidType == GUID_RzAxis){
						actionJoyObj[Axis_2] = &JoystickObjetsList[i];
						axisIndex++;
					}
					else if (JoystickObjetsList[i].guidType == GUID_XAxis){
						actionJoyObj[Axis_2] = &JoystickObjetsList[i];
						axisIndex++;
					}
				}
				if ((JoystickObjetsList[i].dwType&DIDFT_BUTTON) != 0 && instance < MAX_NUM_BUTTONS)
				{
					MyFormat.push_back(InitObjDataFormat(&GUID_Button, FIELD_OFFSET(JoystickState, Button[instance]), JoystickObjetsList[i].dwType, 0));
					actionJoyObj[MAX_NUM_AXIS + instance] = &JoystickObjetsList[i];
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
			hr = g_Device->SetDataFormat(&MyJoyFormat);

			if (FAILED(hr)){
				cout << "Fail Set Data Format Device (" << hr << ")" << endl;
				return E_FAIL;
			}

			hr = g_Device->SetCooperativeLevel(hwnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
			if (FAILED(hr)){
				cout << "Fail Set Cooperative Device (" << hr << ")" << endl;
				return E_FAIL;
			}

			g_Device->Acquire();
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

	if (strcmp(action, "Button0") == 0){
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
	else if (strcmp(action, "Button8") == 0){
		return GetButtonPos(Button8);
	}
	else if (strcmp(action, "Button9") == 0){
		return GetButtonPos(Button9);
	}
	else if (strcmp(action, "Button10") == 0){
		return GetButtonPos(Button10);
	}
	else if (strcmp(action, "Button11") == 0){
		return GetButtonPos(Button11);
	}
	else if (strcmp(action, "Button12") == 0){
		return GetButtonPos(Button12);
	}
	else if (strcmp(action, "Button13") == 0){
		return GetButtonPos(Button13);
	}
	else if (strcmp(action, "Button14") == 0){
		return GetButtonPos(Button14);
	}
	else if (strcmp(action, "Button15") == 0){
		return GetButtonPos(Button15);
	}
	else if (strcmp(action, "Button16") == 0){
		return GetButtonPos(Button16);
	}
	else if (strcmp(action, "Button17") == 0){
		return GetButtonPos(Button17);
	}
	else if (strcmp(action, "Button18") == 0){
		return GetButtonPos(Button18);
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
	else{
		return 0.0;
	}
}

void UpdateState(){
	if (g_Device != NULL){
		JoystickState tmp;
		HRESULT hr = g_Device->GetDeviceState(sizeof(JoystickState), &tmp);

		if (FAILED(hr)){
			cout << "Fail Update State : " << hr << endl;
			hr = g_Device->Acquire();
			if (FAILED(hr)){
				cout << "Fail acquire (" << hr << ")" << endl;
			}
			else{
				cout << "Success acquire" << endl;
			}
		}
		else{
			cout << "Success" << endl;
			for (int i = MAX_NUM_AXIS; i < END_ACTIONS; i++){
				joystickState.Button[i - MAX_NUM_AXIS] = tmp.Button[i - MAX_NUM_AXIS];
			}
			for (int i = Axis_0; i <= Axis_3; i++){
				joystickState.RawAxis[i - Axis_0] = tmp.RawAxis[i - Axis_0];
			}
		}
	}
}