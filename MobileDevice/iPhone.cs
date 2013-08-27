using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MobileDevice
{
	public class iPhone
	{
		private bool _connected;
		private string _currentDirectory;
		private DeviceNotificationCallback _dnc;
		private DeviceRestoreNotificationCallback _drn1;
		private DeviceRestoreNotificationCallback _drn2;
		private DeviceRestoreNotificationCallback _drn3;
		private DeviceRestoreNotificationCallback _drn4;
		internal unsafe void* hAFC;
		internal unsafe void* hService;
		internal unsafe void* iPhoneHandle;
		private static readonly char[] path_separators = new char[] { '/' };
		private bool _wasAfc2;

		public event ConnectEventHandler Connect;
		public event EventHandler DfuConnect;
		public event EventHandler DfuDisconnect;
		public event ConnectEventHandler Disconnect;
		public event EventHandler RecoveryModeEnter;
		public event EventHandler RecoveryModeLeave;

		public string HouseArrestBundleIdentifier { get; set; }

		private ConnectMode _connectMode;
		public enum ConnectMode
		{
			AFC,
			HouseArrest
		};

		public iPhone()
		{
			DoConstruction();
		}

		private unsafe bool ConnectToPhone(ConnectMode connectMode)
		{
			if (_connected)
			{
				MobileDevice.AFCConnectionClose(hAFC);
				_connected = false;
			}

			var connectResult = MobileDevice.AMDeviceConnect(iPhoneHandle);
			if (connectResult == 1)
			{
				throw new Exception("Phone in recovery mode, support not yet implemented.");
			}
			if (MobileDevice.AMDeviceIsPaired(iPhoneHandle) == 0)
			{
				return false;
			}
			if (MobileDevice.AMDeviceValidatePairing(iPhoneHandle) != 0)
			{
				return false;
			}
			if (MobileDevice.AMDeviceStartSession(iPhoneHandle) == 1)
			{
				return false;
			}

			switch (connectMode)
			{
				case ConnectMode.AFC:
					if (0 != MobileDevice.AMDeviceStartService(iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc2")), ref hService, null))
					{
						if (0 != MobileDevice.AMDeviceStartService(iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc")), ref hService, null))
						{
							return false;
						}
					}
					else
					{
						_wasAfc2 = true;
					}
					break;
				case ConnectMode.HouseArrest:
					if (string.IsNullOrEmpty(HouseArrestBundleIdentifier))
					{
						Console.WriteLine("Bundle identifier cannot be null when using house arrest service.");
						return false;
					}

					if (MobileDevice.AMDeviceStartHouseArrestService(iPhoneHandle,
						MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString(HouseArrestBundleIdentifier)), null,
						ref hService, 0) != 0)
					{
						Console.WriteLine("Unable to find bundle with id: {0}", HouseArrestBundleIdentifier);
						return false;
					}
					break;
			}
			_connectMode = connectMode;

			if (MobileDevice.AMDeviceStopSession(iPhoneHandle) != 0)
			{
				return false;
			}
			if (MobileDevice.AMDeviceDisconnect(iPhoneHandle) != 0)
			{
				return false;
			}

			if (MobileDevice.AFCConnectionOpen(hService, 0, ref hAFC) != 0)
			{
				return false;
			}

			_connected = true;
			return true;
		}

		public unsafe bool CreateDirectory(string path, ConnectMode connectMode = ConnectMode.AFC)
		{
			ResetService(connectMode);

			string str = FullPath(_currentDirectory, path);
			if (MobileDevice.AFCDirectoryCreate(hAFC, str) != 0)
			{
				return false;
			}
			return true;
		}

		public unsafe void DeleteDirectory(string path)
		{
			string str = FullPath(_currentDirectory, path);
			if (IsDirectory(str))
			{
				MobileDevice.AFCRemovePath(hAFC, str);
			}
		}

		public void DeleteDirectory(string path, bool recursive)
		{
			if (!recursive)
			{
				DeleteDirectory(path);
			}
			else
			{
				string str = FullPath(_currentDirectory, path);
				if (IsDirectory(str))
				{
					InternalDeleteDirectory(path);
				}
			}
		}

		public unsafe void DeleteFile(string path)
		{
			string str = FullPath(_currentDirectory, path);
			if (Exists(str))
			{
				MobileDevice.AFCRemovePath(hAFC, str);
			}
		}

		private void DfuConnectCallback(ref AMRecoveryDevice callback)
		{
			OnDfuConnect(new DeviceNotificationEventArgs(callback));
		}

		private void DfuDisconnectCallback(ref AMRecoveryDevice callback)
		{
			OnDfuDisconnect(new DeviceNotificationEventArgs(callback));
		}

		private unsafe void DoConstruction()
		{
			void* voidPtr;
			_dnc = NotifyCallback;
			_drn1 = DfuConnectCallback;
			_drn2 = RecoveryConnectCallback;
			_drn3 = DfuDisconnectCallback;
			_drn4 = RecoveryDisconnectCallback;
			int num = MobileDevice.AMDeviceNotificationSubscribe(_dnc, 0, 0, 0, out voidPtr);
			if (num != 0)
			{
				throw new Exception("AMDeviceNotificationSubscribe failed with error " + num);
			}
			num = MobileDevice.AMRestoreRegisterForDeviceNotifications(_drn1, _drn2, _drn3, _drn4, 0, null);
			if (num != 0)
			{
				throw new Exception("AMRestoreRegisterForDeviceNotifications failed with error " + num);
			}
			_currentDirectory = "/";
		}

		public unsafe void EnterDFU()
		{
			MobileDevice.AMRestorePerformDFURestore(iPhoneHandle);
		}

		public unsafe void EnterRecovery()
		{
			MobileDevice.AMDeviceEnterRecovery(iPhoneHandle);
		}

		public unsafe bool Exists(string path)
		{
			void* dict = null;
			int num = MobileDevice.AFCFileInfoOpen(hAFC, path, ref dict);
			if (num == 0)
			{
				MobileDevice.AFCKeyValueClose(dict);
			}
			return (num == 0);
		}

		public ulong FileSize(string path)
		{
			ulong num;
			bool flag;
			GetFileInfo(path, out num, out flag);
			return num;
		}

		public unsafe void FixRecovery()
		{
			MobileDevice.AMRecoveryModeDeviceSetAutoBoot(iPhoneHandle);
		}

		internal string FullPath(string path1, string path2)
		{
			string[] strArray;
			if (string.IsNullOrEmpty(path1))
			{
				path1 = "/";
			}
			if (string.IsNullOrEmpty(path2))
			{
				path2 = "/";
			}
			if (path2[0] == '/')
			{
				strArray = path2.Split(path_separators);
			}
			else if (path1[0] == '/')
			{
				strArray = (path1 + "/" + path2).Split(path_separators);
			}
			else
			{
				strArray = ("/" + path1 + "/" + path2).Split(path_separators);
			}
			var strArray2 = new string[strArray.Length];
			int count = 0;
			for (int i = 0; i < strArray.Length; i++)
			{
				if (strArray[i] == "..")
				{
					if (count > 0)
					{
						count--;
					}
				}
				else if (!(strArray[i] == ".") && !(strArray[i] == ""))
				{
					strArray2[count++] = strArray[i];
				}
			}
			return ("/" + string.Join("/", strArray2, 0, count));
		}

		private string Get_st_ifmt(string path)
		{
			return GetFileInfo(path)["st_ifmt"];
		}

		public string GetCurrentDirectory()
		{
			return _currentDirectory;
		}

		public unsafe string[] GetDirectories(string path)
		{
			if (!IsConnected)
			{
				throw new Exception("Not connected to phone");
			}
			void* dir = null;
			string s = FullPath(CurrentDirectory, path);
			if (MobileDevice.AFCDirectoryOpen(hAFC, Encoding.UTF8.GetBytes(s), ref dir) != 0)
			{
				throw new Exception("Path does not exist");
			}
			string buffer = null;
			var list = new ArrayList();
			MobileDevice.AFCDirectoryRead(hAFC, dir, ref buffer);
			while (buffer != null)
			{
				if (((buffer != ".") && (buffer != "..")) && IsDirectory(FullPath(s, buffer)))
				{
					list.Add(buffer);
				}
				MobileDevice.AFCDirectoryRead(hAFC, dir, ref buffer);
			}
			MobileDevice.AFCDirectoryClose(hAFC, dir);
			return (string[])list.ToArray(typeof(string));
		}

		public string GetDirectoryRoot(string path)
		{
			return "/";
		}

		public unsafe Dictionary<string, string> GetFileInfo(string path)
		{
			var dictionary = new Dictionary<string, string>();
			void* dict = null;
			if ((MobileDevice.AFCFileInfoOpen(this.hAFC, path, ref dict) == 0) && (dict != null))
			{
				void* voidPtr2;
				void* voidPtr3;
				while (((MobileDevice.AFCKeyValueRead(dict, out voidPtr2, out voidPtr3) == 0) && (voidPtr2 != null)) && (voidPtr3 != null))
				{
					string key = Marshal.PtrToStringAnsi(new IntPtr(voidPtr2));
					string str2 = Marshal.PtrToStringAnsi(new IntPtr(voidPtr3));
					dictionary.Add(key, str2);
				}
				MobileDevice.AFCKeyValueClose(dict);
			}
			return dictionary;
		}

		public void CopyFile(string inputFilePath, string destFilePath, ConnectMode connectMode = ConnectMode.AFC)
		{
			ResetService(connectMode);

			using (var destinationFile = iPhoneFile.OpenWrite(this, destFilePath))
			{
				var sourceFile = File.ReadAllBytes(inputFilePath);
				destinationFile.Write(sourceFile, 0, sourceFile.Length);
			}
		}

		private void ResetService(ConnectMode connectMode)
		{
			if (_connectMode != connectMode)
			{
				ConnectToPhone(connectMode);
			}
		}

		public unsafe void GetFileInfo(string path, out ulong size, out bool directory)
		{
			string str;
			var fileInfo = GetFileInfo(path);
			size = fileInfo.ContainsKey("st_size") ? ulong.Parse(fileInfo["st_size"]) : 0L;
			bool flag = false;
			directory = false;
			if (fileInfo.ContainsKey("st_ifmt") && ((str = fileInfo["st_ifmt"]) != null))
			{
				if (!(str == "S_IFDIR"))
				{
					if (str == "S_IFLNK")
					{
						flag = true;
					}
				}
				else
				{
					directory = true;
				}
			}
			if (flag)
			{
				void* dir = null;
				if (directory = MobileDevice.AFCDirectoryOpen(hAFC, Encoding.UTF8.GetBytes(path), ref dir) == 0)
				{
					MobileDevice.AFCDirectoryClose(hAFC, dir);
				}
			}
		}

		public unsafe string[] GetFiles(string path)
		{
			if (!_connected)
			{
				throw new Exception("Not connected to phone");
			}
			string s = FullPath(_currentDirectory, path);
			void* dir = null;
			if (MobileDevice.AFCDirectoryOpen(hAFC, Encoding.UTF8.GetBytes(s), ref dir) != 0)
			{
				throw new Exception("Path does not exist");
			}
			string buffer = null;
			var list = new ArrayList();
			MobileDevice.AFCDirectoryRead(hAFC, dir, ref buffer);
			while (buffer != null)
			{
				if (!IsDirectory(FullPath(s, buffer)))
				{
					list.Add(buffer);
				}
				MobileDevice.AFCDirectoryRead(hAFC, dir, ref buffer);
			}
			MobileDevice.AFCDirectoryClose(hAFC, dir);
			return (string[])list.ToArray(typeof(string));
		}

		private void InternalDeleteDirectory(string path)
		{
			string str = FullPath(_currentDirectory, path);
			string[] files = GetFiles(path);
			for (int i = 0; i < files.Length; i++)
			{
				DeleteFile(str + "/" + files[i]);
			}
			files = GetDirectories(path);
			for (int j = 0; j < files.Length; j++)
			{
				InternalDeleteDirectory(str + "/" + files[j]);
			}
			DeleteDirectory(path);
		}

		public bool IsDirectory(string path, ConnectMode connectMode = ConnectMode.AFC)
		{
			ResetService(connectMode);

			ulong num;
			bool flag;
			GetFileInfo(path, out num, out flag);
			return flag;
		}

		public bool IsFile(string path)
		{
			return (Get_st_ifmt(path) == "S_IFREG");
		}

		public bool IsLink(string path)
		{
			return (Get_st_ifmt(path) == "S_IFLNK");
		}

		private unsafe void NotifyCallback(ref AMDeviceNotificationCallbackInfo callback)
		{
			if (callback.msg == NotificationMessage.Connected)
			{
				iPhoneHandle = callback.dev;
				if (ConnectToPhone(ConnectMode.AFC))
				{
					OnConnect(new ConnectEventArgs(callback));
				}
			}
			else if (callback.msg == NotificationMessage.Disconnected)
			{
				_connected = false;
				OnDisconnect(new ConnectEventArgs(callback));
			}
		}

		protected void OnConnect(ConnectEventArgs args)
		{
			ConnectEventHandler connect = Connect;
			if (connect != null)
			{
				connect(this, args);
			}
		}

		protected void OnDfuConnect(DeviceNotificationEventArgs args)
		{
			EventHandler dfuConnect = DfuConnect;
			if (dfuConnect != null)
			{
				dfuConnect(this, args);
			}
		}

		protected void OnDfuDisconnect(DeviceNotificationEventArgs args)
		{
			EventHandler dfuDisconnect = DfuDisconnect;
			if (dfuDisconnect != null)
			{
				dfuDisconnect(this, args);
			}
		}

		protected void OnDisconnect(ConnectEventArgs args)
		{
			ConnectEventHandler disconnect = Disconnect;
			if (disconnect != null)
			{
				disconnect(this, args);
			}
		}

		protected void OnRecoveryModeEnter(DeviceNotificationEventArgs args)
		{
			EventHandler recoveryModeEnter = RecoveryModeEnter;
			if (recoveryModeEnter != null)
			{
				recoveryModeEnter(this, args);
			}
		}

		protected void OnRecoveryModeLeave(DeviceNotificationEventArgs args)
		{
			EventHandler recoveryModeLeave = RecoveryModeLeave;
			if (recoveryModeLeave != null)
			{
				recoveryModeLeave(this, args);
			}
		}

		public unsafe void RebootRecovery()
		{
			MobileDevice.AMRecoveryModeDeviceReboot(iPhoneHandle);
		}

		public unsafe void RebootRestore()
		{
			MobileDevice.AMRestoreModeDeviceReboot(iPhoneHandle);
		}

		public unsafe void ReConnect()
		{
			MobileDevice.AFCConnectionClose(hAFC);
			MobileDevice.AMDeviceStopSession(iPhoneHandle);
			MobileDevice.AMDeviceDisconnect(iPhoneHandle);
			ConnectToPhone(ConnectMode.AFC);
		}

		private void RecoveryConnectCallback(ref AMRecoveryDevice callback)
		{
			OnRecoveryModeEnter(new DeviceNotificationEventArgs(callback));
		}

		private void RecoveryDisconnectCallback(ref AMRecoveryDevice callback)
		{
			OnRecoveryModeLeave(new DeviceNotificationEventArgs(callback));
		}

		public unsafe bool Rename(string sourceName, string destName)
		{
			return (MobileDevice.AFCRenamePath(hAFC, FullPath(_currentDirectory, sourceName), FullPath(_currentDirectory, destName)) == 0);
		}

		public unsafe void SendCommandToDevice(string command)
		{
			MobileDevice.sendCommandToDevice(iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCFString(command)), 0);
		}

		public void SetCurrentDirectory(string path)
		{
			string str = FullPath(_currentDirectory, path);
			if (!IsDirectory(str))
			{
				throw new Exception("Invalid directory specified");
			}
			_currentDirectory = str;
		}

		public unsafe string ActivationState
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "ActivationState");
			}
		}

		public unsafe void* AFCHandle
		{
			get
			{
				return hAFC;
			}
		}

		public string CurrentDirectory
		{
			get
			{
				return _currentDirectory;
			}
			set
			{
				_currentDirectory = value;
			}
		}

		public unsafe void* Device
		{
			get
			{
				return iPhoneHandle;
			}
		}

		public unsafe string DeviceBasebandBootloaderVersion
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "BasebandBootloaderVersion");
			}
		}

		public unsafe string DeviceBasebandVersion
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "BasebandVersion");
			}
		}

		public unsafe string DeviceBuildVersion
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "BuildVersion");
			}
		}

		public unsafe string DeviceFirmwareVersion
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "FirmwareVersion");
			}
		}

		public unsafe string DeviceId
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "UniqueDeviceID");
			}
		}

		public unsafe string DeviceIntegratedCircuitCardIdentity
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "IntegratedCircuitCardIdentity");
			}
		}

		public unsafe string DeviceiTunesHasConnected
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "iTunesHasConnected");
			}
		}

		public unsafe string DeviceModelNumber
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "ModelNumber");
			}
		}

		public unsafe string DeviceName
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "DeviceName");
			}
		}

		public unsafe string DevicePhoneNumber
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "PhoneNumber");
			}
		}

		public unsafe string DeviceProductType
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "ProductType");
			}
		}

		public unsafe string DeviceSerial
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "SerialNumber");
			}
		}

		public unsafe string DeviceSIMStatus
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "SIMStatus");
			}
		}

		public unsafe string DeviceType
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "DeviceClass");
			}
		}

		public unsafe string DeviceVersion
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "ProductVersion");
			}
		}

		public unsafe string DeviceWiFiAddress
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "WiFiAddress");
			}
		}

		public unsafe string IInternationalMobileSubscriberIdentity
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "InternationalMobileSubscriberIdentity");
			}
		}

		public unsafe string InternationalMobileEquipmentIdentity
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "InternationalMobileEquipmentIdentity");
			}
		}

		public bool IsConnected
		{
			get
			{
				return _connected;
			}
		}

		public bool IsJailbreak
		{
			get
			{
				if (_wasAfc2)
				{
					return true;
				}
				if (!_connected)
				{
					return false;
				}
				return Exists("/Applications");
			}
		}

		public unsafe string ProductVersion
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "ih8sn0w");
			}
		}

		public unsafe string UniqueChipID
		{
			get
			{
				return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "UniqueChipID");
			}
		}
	}
}