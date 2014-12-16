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

		internal unsafe void* hAFC;
		internal unsafe void* hService;
		internal unsafe void* iPhoneHandle;

		public unsafe void* AFCHandle
		{
			get
			{
				return hAFC;
			}
		}

		public unsafe void* Device
		{
			get
			{
				return iPhoneHandle;
			}
		}

		public string ActivationState { get; private set; }
		public string DeviceBasebandBootloaderVersion { get; private set; }
		public string DeviceBasebandVersion { get; private set; }
		public string DeviceBuildVersion { get; private set; }
		public string DeviceFirmwareVersion { get; private set; }
		public string DeviceId { get; private set; }
		public string DeviceIntegratedCircuitCardIdentity { get; private set; }
		public string DeviceiTunesHasConnected { get; private set; }
		public string DeviceModelNumber { get; private set; }
		public string DeviceName { get; private set; }
		public string DevicePhoneNumber { get; private set; }
		public string DeviceProductType { get; private set; }
		public string DeviceSerial { get; private set; }
		public string DeviceSIMStatus { get; private set; }
		public string DeviceType { get; private set; }
		public string DeviceVersion { get; private set; }
		public string DeviceWiFiAddress { get; private set; }
		public string IInternationalMobileSubscriberIdentity { get; private set; }
		public string InternationalMobileEquipmentIdentity { get; private set; }
		public string UniqueChipID { get; private set; }

		internal iPhone()
		{
		}

		internal iPhone(string activationState, string deviceBasebandBootloaderVersion, string deviceBasebandVersion, string deviceBuildVersion, string deviceFirmwareVersion,
					  string deviceId, string deviceIntegratedCircuitCardIdentity, string deviceiTunesHasConnected, string deviceModelNumber, string deviceName,
					  string devicePhoneNumber, string deviceProductType, string deviceSerial, string deviceSIMStatus, string deviceType, string deviceVersion,
					  string deviceWiFiAddress, string internationalMobileSubscriberIdentity, string internationalMobileEquipmentIdentity, string uniqueChipID)
		{

			ActivationState = activationState;
			DeviceBasebandBootloaderVersion = deviceBasebandBootloaderVersion;
			DeviceBasebandVersion = deviceBasebandVersion;
			DeviceBuildVersion = deviceBuildVersion;
			DeviceFirmwareVersion = deviceFirmwareVersion;
			DeviceId = deviceId;
			DeviceIntegratedCircuitCardIdentity = deviceIntegratedCircuitCardIdentity;
			DeviceiTunesHasConnected = deviceiTunesHasConnected;
			DeviceModelNumber = deviceModelNumber;
			DeviceName = deviceName;
			DevicePhoneNumber = devicePhoneNumber;
			DeviceProductType = deviceProductType;
			DeviceSerial = deviceSerial;
			DeviceSIMStatus = deviceSIMStatus;
			DeviceType = deviceType;
			DeviceVersion = deviceVersion;
			DeviceWiFiAddress = deviceWiFiAddress;
			IInternationalMobileSubscriberIdentity = internationalMobileSubscriberIdentity;
			InternationalMobileEquipmentIdentity = internationalMobileEquipmentIdentity;
			UniqueChipID = uniqueChipID;
		}

		public unsafe bool ConnectViaHouseArrest(string bundleIdentifier)
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

			if (string.IsNullOrEmpty(bundleIdentifier))
			{
				Console.WriteLine("Bundle identifier cannot be null when using house arrest service.");
				return false;
			}

			// Connect via house arrest, only Documents directory is accessible on device
			if (MobileDevice.AMDeviceStartHouseArrestService(iPhoneHandle,
				MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString(bundleIdentifier)), null,
				ref hService, 0) != 0)
			{
				Console.WriteLine("Unable to find bundle with id: {0}", bundleIdentifier);
				return false;
			}

			// Need to stop session and disconnect for house arrest to work, comment out for AFC
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

		public unsafe bool ConnectViaAFC()
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

			// Connect via AFC, only Media directory is accessible on device
			if (0 != MobileDevice.AMDeviceStartService(iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc2")), ref hService, null))
			{
				if (0 != MobileDevice.AMDeviceStartService(iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc")), ref hService, null))
				{
					return false;
				}
			}

			_connected = true;
			return true;
		}

		public unsafe bool CreateDirectory(string path)
		{
			if (MobileDevice.AFCDirectoryCreate(hAFC, path) != 0)
			{
				return false;
			}
			return true;
		}

		public unsafe void DeleteDirectory(string path)
		{
			if (IsDirectory(path))
			{
				MobileDevice.AFCRemovePath(hAFC, path);
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
				if (IsDirectory(path))
				{
					InternalDeleteDirectory(path);
				}
			}
		}

		public unsafe void DeleteFile(string path)
		{
			if (Exists(path))
			{
				MobileDevice.AFCRemovePath(hAFC, path);
			}
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

		private string Get_st_ifmt(string path)
		{
			return GetFileInfo(path)["st_ifmt"];
		}

		public unsafe string[] GetDirectories(string path)
		{
			if (!_connected)
			{
				throw new Exception("Not connected to phone");
			}
			void* dir = null;
			if (MobileDevice.AFCDirectoryOpen(hAFC, Encoding.UTF8.GetBytes(path), ref dir) != 0)
			{
				throw new Exception("Path does not exist");
			}
			string buffer = null;
			var list = new ArrayList();
			MobileDevice.AFCDirectoryRead(hAFC, dir, ref buffer);
			while (buffer != null)
			{
				if (((buffer != ".") && (buffer != "..")) && IsDirectory(path))
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

		public void CopyFile(string inputFilePath, string destFilePath)
		{
			using (var destinationFile = iPhoneFile.OpenWrite(this, destFilePath))
			{
				var sourceFile = File.ReadAllBytes(inputFilePath);
				destinationFile.Write(sourceFile, 0, sourceFile.Length);
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
			void* dir = null;
			if (MobileDevice.AFCDirectoryOpen(hAFC, Encoding.UTF8.GetBytes(path), ref dir) != 0)
			{
				throw new Exception("Path does not exist");
			}
			string buffer = null;
			var list = new ArrayList();
			MobileDevice.AFCDirectoryRead(hAFC, dir, ref buffer);
			while (buffer != null)
			{
                if (!IsDirectory(string.Format("{0}/{1}", path, buffer)))
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
			string[] files = GetFiles(path);
			for (int i = 0; i < files.Length; i++)
			{
				DeleteFile(path + "/" + files[i]);
			}
			files = GetDirectories(path);
			for (int j = 0; j < files.Length; j++)
			{
				InternalDeleteDirectory(path + "/" + files[j]);
			}
			DeleteDirectory(path);
		}

		public bool IsDirectory(string path)
		{
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

		public unsafe void ReConnect(string bundleIdentifier)
		{
			MobileDevice.AFCConnectionClose(hAFC);
			MobileDevice.AMDeviceStopSession(iPhoneHandle);
			MobileDevice.AMDeviceDisconnect(iPhoneHandle);
			ConnectViaHouseArrest(bundleIdentifier);
		}

		public unsafe bool Rename(string sourceName, string destName)
		{
			return (MobileDevice.AFCRenamePath(hAFC, sourceName, destName) == 0);
		}

		public unsafe void SendCommandToDevice(string command)
		{
			MobileDevice.sendCommandToDevice(iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCFString(command)), 0);
		}


	}
}