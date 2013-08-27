namespace MobileDevice
{
    using System;

    public class ConnectEventArgs : EventArgs
    {
        private unsafe void* device;
        private NotificationMessage message;

        internal unsafe ConnectEventArgs(AMDeviceNotificationCallbackInfo cbi)
        {
            this.message = cbi.msg;
            this.device = cbi.dev;
        }

        public unsafe void* Device
        {
            get
            {
                return this.device;
            }
        }

        public NotificationMessage Message
        {
            get
            {
                return this.message;
            }
        }
    }
}

