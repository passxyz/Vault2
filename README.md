# .NET MAUI Cross-Platform Application Development

## How to build

- Windows, Android and iOS can be built using Visual Studio 2022 on Windows

- iOS and macOS can be built on a Mac using command line

### Build and run on Mac

To build and run iOS and macOS app on Mac, we need to use command line.

#### Build an iOS app with .NET CLI

`dotnet build -t:Run -f net8.0-ios`

#### Launch the iOS app on a specific simulator

`dotnet build -t:Run -f net8.0-ios -p:_DeviceName=:v2:udid=E25BBE37-69BA-4720-B6FD-D54C97791E79`

#### Build a Mac Catalyst app with .NET CLI

`dotnet build -t:Run -f net8.0-maccatalyst`

Reference:

- [mauibiometrics](https://github.com/cedricgabrang/mauibiometrics)