using MobileDevice;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobileDevic
{
    public partial class Form1 : Form
    {
        iPhone iphone = new iPhone();
        public static string devicetype;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            serial.Text = "";
            baseband.Text = "";
            model.Text = "";
            bootloader.Text = "";
            jailbreakable.Text = "";
            unlockable.Text = "";
            jailbroken.Text = "";
            phone.Text = "";
            wifi.Text = "";
            sim.Text = "";
            imei.Text = "";
            imsi.Text = "";
            activation.Text = "";
            iphone.Connect += new MobileDevice.ConnectEventHandler(getInfo);
            iphone.Disconnect += new MobileDevice.ConnectEventHandler(remove);
            iphone.RecoveryModeEnter += new EventHandler(recovfound);
            if (iphone.IsConnected == true)
            {
                getInfo(null, null);
            }
        }

        private void recovfound(object sender, EventArgs args)
        {
            MessageBox.Show("A device in recovery mode has been found");
        }

        private void remove(object sender, ConnectEventArgs args)
        {
            this.Invoke((MethodInvoker)delegate
    {
        toolStripStatusLabel1.Text = "Waiting for iDevice...";
        serial.Text = "";
        baseband.Text = "";
        model.Text = "";
        bootloader.Text = "";
        jailbreakable.Text = "";
        unlockable.Text = "";
        jailbroken.Text = "";
        phone.Text = "";
        wifi.Text = "";
        sim.Text = "";
        imei.Text = "";
        imsi.Text = "";
        activation.Text = "";
    });
        }

        private void getInfo(object sender, EventArgs e)
        {
            devicetype = iphone.DeviceProductType;
            if (devicetype == "iPhone1,1")
            {
                devicetype = "iPhone 2G";
            }
            else if (devicetype == "iPhone1,2")
            {
                devicetype = "iPhone 3G";
            }
            else if (devicetype == "iPhone2,1")
            {
                devicetype = "iPhone 3G[S]";
            }
            else if (devicetype == "iPhone3,1")
            {
                devicetype = "iPhone 4 [GSM]";
            }
            else if (devicetype == "iPhone3,3")
            {
                devicetype = "iPhone 4 [CDMA]";
            }
            else if (devicetype == "iPhone4,1")
            {
                devicetype = "iPhone 4S";
            }
            else if (devicetype == "iPhone5,1")
            {
                devicetype = "iPhone 5";
            }
            else if (devicetype == "iPod1,1")
            {
                devicetype = "iPod Touch 1G";
            }
            else if (devicetype == "iPod2,1")
            {
                devicetype = "iPod Touch 2G";
            }
            else if (devicetype == "iPod3,1")
            {
                devicetype = "iPod Touch 3G";
            }
            else if (devicetype == "iPod4,1")
            {
                devicetype = "iPod Touch 4";
            }
            else if (devicetype == "iPad1,1")
            {
                devicetype = "iPad 1G";
            }
            else if (devicetype == "iPad2,1")
            {
                devicetype = "iPad 2 [WiFi]";
            }
            else if (devicetype == "iPad2,2")
            {
                devicetype = "iPad 2 [3G-GSM]";
            }
            else if (devicetype == "iPad2,3")
            {
                devicetype = "iPad 2 [3G-CDMA]";
            }
            else if (devicetype == "")
            {
                devicetype = "";
            }
            this.Invoke((MethodInvoker)delegate
                {
                    if ((((iphone.DeviceProductType == "iPhone1,1") | (iphone.DeviceProductType == "iPhone1,2")) | (iphone.DeviceProductType == "iPod1,1")) | (iphone.DeviceProductType == "iPod2,1"))
                    {
                        this.jailbreakable.Text = "YES";
                        this.jailbreakable.ForeColor = Color.Lime;
                    }
                    else if (((iphone.DeviceProductType == "iPad4,1") | (iphone.DeviceProductType == "iPad3,1")) | (iphone.DeviceProductType == "iPad5,1"))
                    {
                        this.jailbreakable.Text = "NO";
                        this.jailbreakable.ForeColor = Color.Red;
                    }
                    else
                    {
                        if ((this.iphone.DeviceVersion == "3.0") | (this.iphone.DeviceVersion == "3.0.1"))
                        {
                            this.jailbreakable.Text = "YES (with redsn0w)";
                        }
                        else if ((((((this.iphone.DeviceVersion == "3.1.2") | (this.iphone.DeviceVersion == "3.1.3")) | (this.iphone.DeviceVersion == "3.2")) | (this.iphone.DeviceVersion == "3.2.1")) | (this.iphone.DeviceVersion == "4.0")) | (this.iphone.DeviceVersion == "4.0.1"))
                        {
                            this.jailbreakable.Text = "YES (with Star/JBme)";
                        }
                        else if (this.iphone.DeviceVersion == "4.0.2")
                        {
                            this.jailbreakable.Text = "YES (with limera1n)";
                        }
                        else if (((this.iphone.DeviceVersion == "4.1") | (this.iphone.DeviceVersion == "4.2.1")) | (this.iphone.DeviceVersion == "4.2.6"))
                        {
                            this.jailbreakable.Text = "YES (with greenpois0n)";
                        }
                        else if ((this.iphone.DeviceVersion == "4.2.7") | (this.iphone.DeviceVersion == "4.2.8"))
                        {
                            this.jailbreakable.Text = "YES (with sn0wbreeze)";
                        }
                        else if (((this.iphone.DeviceVersion == "4.3.1") | (this.iphone.DeviceVersion == "4.3.2")) | (this.iphone.DeviceVersion == "4.3.3"))
                        {
                            this.jailbreakable.Text = "YES (with redsn0w)";
                        }
                        else if (((this.iphone.DeviceVersion == "5.0") | (this.iphone.DeviceVersion == "5.0.1")) | (this.iphone.DeviceVersion == "5.1.1"))
                        {
                            this.jailbreakable.Text = "YES (with redsn0w)";
                        }
                        else if (((this.iphone.DeviceVersion == "6.0") | (this.iphone.DeviceVersion == "6.0.1")))
                        {
                            this.jailbreakable.Text = "YES (with redsn0w)";
                        }
                        else
                        {
                            this.jailbreakable.Text = "NO";
                        }
                        if (this.jailbreakable.Text.Substring(0, 2) == "NO")
                        {
                            this.jailbreakable.ForeColor = Color.Red;
                        }
                        else
                        {
                            this.jailbreakable.ForeColor = Color.Lime;
                        }
                        if ((this.iphone.DeviceType == "iPod") | (this.iphone.DeviceType == "iPad"))
                        {
                            this.unlockable.Text = "N/A";
                            this.unlockable.ForeColor = Color.Cyan;
                        }
                        else if (iphone.DeviceProductType == "iPhone1,1")
                        {
                            this.unlockable.Text = "YES";
                            this.unlockable.ForeColor = Color.Lime;
                        }
                        else
                        {
                            if ((((((this.iphone.DeviceBasebandVersion == "01.59.00") | (this.iphone.DeviceBasebandVersion == "04.26.08")) | (this.iphone.DeviceBasebandVersion == "05.11.07")) | (this.iphone.DeviceBasebandVersion == "05.12.01")) | (this.iphone.DeviceBasebandVersion == "05.13.04")) | (this.iphone.DeviceBasebandVersion == "06.15.00"))
                            {
                                this.unlockable.Text = "YES";
                                this.unlockable.ForeColor = Color.Lime;
                            }
                            else
                            {
                                this.unlockable.Text = "NO";
                                this.unlockable.ForeColor = Color.Red;
                            }
                            if ((iphone.DeviceProductType == "iPhone1,2") | ((iphone.DeviceProductType == "iPhone2,1")))
                            {
                                this.unlockable.Text = "Upgrade to iPad BB";
                                this.unlockable.ForeColor = Color.Yellow;
                            }
                        }
                        if (iphone.IsJailbreak == true)
                        {
                            jailbroken.Text = "YES";
                            jailbroken.ForeColor = Color.Lime;
                        }
                        else
                        {
                            jailbroken.Text = "NO";
                            jailbroken.ForeColor = Color.Red;
                        }
                        activation.Text = iphone.ActivationState;
                        imsi.Text = iphone.IInternationalMobileSubscriberIdentity;
                        imei.Text = iphone.InternationalMobileEquipmentIdentity;
                        sim.Text = iphone.DeviceSIMStatus;
                        phone.Text = iphone.DevicePhoneNumber;
                        model.Text = iphone.DeviceModelNumber;
                        wifi.Text = iphone.DeviceWiFiAddress;
                        bootloader.Text = iphone.DeviceBasebandBootloaderVersion;
                        baseband.Text = iphone.DeviceBasebandVersion;
                        serial.Text = iphone.DeviceSerial;
                        if (iphone.DeviceName != "") { devicetype = devicetype + " (" + iphone.DeviceName + ")" ; }
                        toolStripStatusLabel1.Text = devicetype + " running iOS: " + iphone.DeviceVersion + " (" + iphone.DeviceBuildVersion + ")";
                    }
                });
        }
    }
}