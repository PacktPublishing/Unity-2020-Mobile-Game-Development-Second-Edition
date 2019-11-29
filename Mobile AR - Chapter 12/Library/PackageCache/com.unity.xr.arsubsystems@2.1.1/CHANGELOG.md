# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.1.1] - 2019-06-25
### Improvements
- Fix `CameraFocusMode` handling in `XRCameraSubsystem`.  This fixes an issue when running on a provider that does not set the default focus mode to `CameraFocusMode.Fixed`.

## [2.1.0] - 2019-06-25
- 2019.3 verified release

### New
- Add support for `NotTrackingReason`.

## [2.1.0-preview.3] - 2019-06-17
### Improvements
- Added functionality to the `XRSessionSubsystem` to enable synchronizing the Unity frame with the AR session update. See `XRSessionSubsystem.matchFrameRate` and `XRSessionSubsystem.frameRate`.

## [2.1.0-preview.2] - 2019-05-16
### Fixes
- Fix documentation links.

## [2.1.0-preview.1] - 2019-05-06
### New
- Add an image tracking subsystem.
- Add an environment probe subsystem.
- Add a face tracking subystem.
- Add an object tracking subsystem for detecting previously scanned 3D objects.

## [2.0.1] - 2019-03-12
- 2019.2 verified release

## [1.0.0-preview.1] - 2019-01-14
- This is the first release of *Unity Package com.unity.xr.subsystems.
