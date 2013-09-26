# MobileDevice

[![Build status](https://ci.appveyor.com/api/projects/status?id=xber12ij1cnq90nm)](https://ci.appveyor.com/project/imaginelearning-public-mobiledevice)

.NET library to facilitate communication with iDevices. Fork from mobiledevice.codeplex.com, making use of house arrest service (as opposed to AFC like original) to transfer files to an app's documents directory (AFC only exposes Media directory).

## Usage

See FSHouseArrest demo which uses iTunes house arrest service to transfer files to an app's sandbox directory.

Example: Copy directory from local to app sandbox on device

	FSHouseArrest.exe -s "C:\temp\Caches" -d "../Library/Caches/" -b "com.ImagineLearning.SomeApp"

## License

The original library was licensed under GPLv2, by extension so is this fork.
