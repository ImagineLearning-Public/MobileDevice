ImagineLearning-MobileDevice
============================

.NET library to facilitate communication with iDevices. Fork from mobiledevice.codeplex.com, making use of house arrest service (as opposed to AFC like original) to transfer files to an app's documents directory (AFC only exposes Media directory).

Usage
============================
See FSHouseArrest demo which uses iTunes house arrest service to transfer files to an app's sandbox directory.

ex (copy directory from local to app sandbox on device):

FSHouseArrest.exe -s "C:\temp\Caches" -d "../Library/Caches/" -b "com.ImagineLearning.SomeApp"
