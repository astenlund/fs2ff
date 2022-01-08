# fs2ff (Flight Simulator to ForeFlight)

## What is it?
This fork of astenlund/fs2ff that emplements the GDL90 protocol as an option instead of using XPlane protocol. This is a work in progress as there is still a lot of things left to do.  I'm just doing this work to learn more about GDL90 and how the different EFBs work with it.

## How do I use it?

see astenlund/fs2ff

## Does it work with other EFB apps?
Yes any the use GDL90

### Garmin Pilot

Sort of.  This is a PITA the PC that is running FS2FF needs to have an IP of 10.29.39.1 and the device (iPad) needs to be in that subnet 10.29.39.x otherwise GP will ignore the traffic.  I have on board wifi on my PC.  It required enabling that has a hotspot setting a static ip on that NIC to 10.29.39.1 and then a static IP on the iPad of 10.29.39.2

### Other apps (not verified by me)

- FlyQ EFB (thanks, @erayymz)
- FltPlan GO (need to select XPlane as source of GPS data)

## Does it work with other flight simulators?

### X-Plane

No need for my app. X-Plane already has this broadcast capability built-in, see [this support page](https://foreflight.com/support/support-center/category/about-foreflight-mobile/204115525).

### Prepar3D

Should work straight out of the box for P3D 4.0 and up, so no need for my app. For older versions, use [FSUIPC](http://www.schiratti.com/dowson.html). See [this support page](https://foreflight.com/support/support-center/category/about-foreflight-mobile/204115345).

### Other flight sims

As of now, no other flight simulators have been tested.

## How do I build this?

1. Download and install [.NET Core SDK](https://dotnet.microsoft.com/download) and [Visual Studio Community](https://visualstudio.microsoft.com/downloads/).
1. Clone the repo or download and extract [a zip](https://github.com/astenlund/fs2ff/archive/master.zip).
1. Install MSFS SDK (see instructions below).
1. Navigate to the SDK on your hard drive and find the following two files:
   - "MSFS SDK\SimConnect SDK\lib\SimConnect.dll"
   - "MSFS SDK\SimConnect SDK\lib\managed\Microsoft.FlightSimulator.SimConnect.dll"
1. Create a folder called "lib" in the fs2ff folder (next to fs2ff.sln) and put the two dll:s therein.
1. Open fs2ff.sln with Visual Studio.
1. Build by pressing Ctrl-Shift-B.
1. Or from command-line: `dotnet build`.
1. To build a self-contained executable, run: `dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true`.

## Where do I get the MSFS SDK?

1. Hop into Flight Simulator.
1. Go to OPTIONS -> GENERAL -> DEVELOPERS and enable DEVELOPER MODE.
1. You will now have a new menu at the top. Click Help -> SDK Installer.
1. Let your browser download the installer and run it.
1. You might get a "Windows protected your PC" popup. If so, click More info -> Run anyway.
1. Go through the installation wizard and make sure that Core Components is selected.
1. When finished, you will likely find the SDK installed under "C:\MSFS SDK".

## What's with the "Windows protected your PC" popup?

This is Microsoft telling you that the app has not been cryptographically signed. Software that you download from big corporations does not present this behaviour, because these companies typically purchase certificates from trusted Certificate Authorities and sign their binaries before shipping to customers. This is cumbersome and expensive and not in the scope of this open source project. The binaries that I have provided are for convenience. If you do not trust me (and why should you?), you are more than welcome to build from source yourself (see instructions above).

## I have problems!

Don't ask
