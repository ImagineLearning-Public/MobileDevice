# MobileDevice

[![Build status](https://ci.appveyor.com/api/projects/status?id=xber12ij1cnq90nm)](https://ci.appveyor.com/project/imaginelearning-public-mobiledevice)

.NET library to facilitate communication with iDevices. Fork from mobiledevice.codeplex.com, making use of house arrest service (as opposed to AFC like original) to transfer files to an app's documents directory (AFC only exposes app's media directory).

## Usage

### SingleSync

Command line tool which uses iTunes house arrest service to sync a local directory to an iOS app's sandbox directory.

Example: Copy directory from local to (AppSandbox)/Library/Caches on device

	SingleSync.exe -s "C:\temp\Caches" -d "/Library/Caches/" -b "com.ImagineLearning.SomeAppName"

### MultiSync

Same concept as SingleSync but it's a GUI tool that will sync the directory to every iDevice visible in iTunes.

## License

The original library was licensed under GPLv2, by extension so is this fork.