# MobileDevice

[![Build status](https://ci.appveyor.com/api/projects/status?id=xber12ij1cnq90nm)](https://ci.appveyor.com/project/imaginelearning-public-mobiledevice)

.NET library to facilitate communication with iDevices. Fork from mobiledevice.codeplex.com, making use of house arrest service (as opposed to AFC like original) to transfer files to an app's documents directory (AFC only exposes app's media directory).

## Usage

### SingleSync

See SingleSync demo which uses iTunes house arrest service to transfer files to an app's (Sandbox)/Library/Caches directory.

Example: Copy directory from local to app sandbox on device

	SingleSync.exe -s "C:\temp\Caches" -d "/Library/Caches/" -b "com.ImagineLearning.SomeAppName"

### MultiSync

Same concept as SingleSync but it's a GUI tool that syncs a directory to every iPad that is visible in iTunes.

## License

The original library was licensed under GPLv2, by extension so is this fork.