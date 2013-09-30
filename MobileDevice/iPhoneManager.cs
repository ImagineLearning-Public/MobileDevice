using System;

namespace MobileDevice
{
	public class iPhoneManager
	{
		private DeviceNotificationCallback _deviceNotificationCallback;
		public event EventHandler<iPhoneEventArgs> DeviceDiscovered;

		public unsafe void GetiPhonesAsync()
		{
			void* voidPtr;
			_deviceNotificationCallback = DeviceNotifyCallback;
			int num = MobileDevice.AMDeviceNotificationSubscribe(_deviceNotificationCallback, 0, 0, 0, out voidPtr);
			if (num != 0)
			{
				throw new Exception("AMDeviceNotificationSubscribe failed with error " + num);
			}
		}

		private unsafe void DeviceNotifyCallback(ref AMDeviceNotificationCallbackInfo callback_info)
		{
			var threadSafeEventHandler = DeviceDiscovered;
			if (threadSafeEventHandler != null)
			{
				var iPhone = CreateiPhone(callback_info.dev);
				threadSafeEventHandler(this, new iPhoneEventArgs { iPhone = iPhone });
			}
		}

		private unsafe iPhone CreateiPhone(void* handle)
		{
			var connectResult = MobileDevice.AMDeviceConnect(handle);
			if (connectResult == 1)
			{
				throw new Exception("Phone in recovery mode, support not yet implemented.");
			}
			if (MobileDevice.AMDeviceIsPaired(handle) == 0)
			{
				return null;
			}
			if (MobileDevice.AMDeviceValidatePairing(handle) != 0)
			{
				return null;
			}
			if (MobileDevice.AMDeviceStartSession(handle) == 1)
			{
				return null;
			}

			void* hService = null;
			void* hAFC = null;

			if (0 != MobileDevice.AMDeviceStartService(handle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc")), ref hService, null))
			{
				return null;
			}

			if (MobileDevice.AFCConnectionOpen(hService, 0, ref hAFC) != 0)
			{
				return null;
			}

			var iPhone = new iPhone(MobileDevice.AMDeviceCopyValue(handle, 0, "ActivationState"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "BasebandBootloaderVersion"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "BasebandVersion"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "BuildVersion"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "FirmwareVersion"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "UniqueDeviceID"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "IntegratedCircuitCardIdentity"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "iTunesHasConnected"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "ModelNumber"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "DeviceName"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "PhoneNumber"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "ProductType"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "SerialNumber"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "SIMStatus"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "DeviceClass"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "ProductVersion"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "WiFiAddress"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "InternationalMobileSubscriberIdentity"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "InternationalMobileEquipmentIdentity"),
									MobileDevice.AMDeviceCopyValue(handle, 0, "UniqueChipID"));
			iPhone.iPhoneHandle = handle;

			if (MobileDevice.AMDeviceStopSession(handle) != 0)
			{
				return null;
			}
			if (MobileDevice.AMDeviceDisconnect(handle) != 0)
			{
				return null;
			}

			if (MobileDevice.AFCConnectionOpen(hService, 0, ref hAFC) != 0)
			{
				return null;
			}

			if (MobileDevice.AFCConnectionClose(hAFC) != 0)
			{
				return null;
			}

			return iPhone;
		}
	}

	public class iPhoneEventArgs : EventArgs
	{
		public iPhone iPhone { get; set; }
	}
}