ImagineLearning-MobileDevice
============================

Allows communication with iDevices. Fork from mobiledevice.codeplex.com, making use of house arrest service (as opposed to AFC) to transfer files to an app's documents directory.

Usage
============================
See FSHouseArrest demo which uses iTune's house arrest service to transfer files to an app's sandbox directory.

ex (copy directory from local to app sandbox on device):

FSHouseArrest.exe -s "C:\temp\Caches" -d "../Library/Caches/" -b "com.ImagineLearning.SomeApp"
