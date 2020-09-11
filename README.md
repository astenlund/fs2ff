# fs2ff (Flight Simulator to ForeFlight)

## What is it?

This is a utility app that connects Microsoft Flight Simulator 2020 with the Electronic Flight Bag (EFB) app ForeFlight, sharing virtual GPS data from the former to the latter. This allows the use of ForeFlight as a navigational aid when simming. Much more cost-effective than flying an actual airplane!

## How do I use it?

Simple! Just follow these easy steps:
1. Start Microsoft Flight Simulator. As soon as you get to the main menu, the game is ready to accept connections.
1. While waiting for MSFS to start (it takes a while!), download fs2ff.exe from the [latest release](https://github.com/astenlund/fs2ff/releases/latest).
1. Double-click the file.
1. If you get a scary red popup that tells you that "Windows protected your PC", click More info -> Run anyway.
1. In the fs2ff app, click the big Connect button. Unless anything goes wrong, you will now be connected to MSFS.
1. To verify the connection, open ForeFlight on your iPad or iPhone and navigate to More -> Devices.
1. Do not forget to activate Auto Center (upper right corner) in the map view in ForeFlight.
1. Now go fly!

__N.B.__ Since I have not bothered to create an installer for this app, you will just have to save it somewhere on your harddrive where you can find it again. Why not your desktop? ;)

## Ok, but what does it actually do?

The app uses Microsoft's SimConnect SDK to continuously collect information about the player's own aircraft in the game, and then broadcast these data so they can be picked up by ForeFlight. This requires that the ForeFlight device is connected to the same Wi-Fi network as the Flight Sim computer. Also, UDP port 49002 must not be blocked by any firewalls.

The app might also work with SkyDemon and other EFB apps, but this remains untested as of yet. Also untested are earlier versions of Flight Simulator as well as Prepar3D.

If you are using X-Plane, there is already built-in functionality that does exactly this, see [this support page](https://foreflight.com/support/support-center/category/about-foreflight-mobile/204115525).

## What doesn't it do (yet)?

Other traffic is not being picked up just yet. This means that if you are in a flight with friends, you will not be able to see them in ForeFlight. This might be added in a future update.

## How do I build this?

1. Download [.NET Core SDK](https://dotnet.microsoft.com/download) and [Visual Studio Community](https://visualstudio.microsoft.com/downloads/).
1. Clone the repo, or download the source code for the [latest release](https://github.com/astenlund/fs2ff/releases/latest).
1. Extract the zip file.
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
1. You might get a scary red popup. If so, click More info -> Run anyway.
1. Go through the installation wizard and make sure that Core Components is selected.
1. When finished, you will likely find the SDK installed under "C:\MSFS SDK".

## Why do I get a scary red popup when running the app for the first time?

This is Microsoft telling you that the app has not been cryptographically signed. Software that you download from big corporations does not present this behaviour, because these companies typically purchase certificates from trusted Certificate Authorities and sign their binaries before shipping to customers. This is cumbersome and expensive and not in the scope of this open source project. The binaries that I have provided are for convenience. If you do not trust me (and why should you?), you are more than welcome to build from source yourself (see instructions above).

## I have problems!

If you are experiencing any technical issues with the app (or if you see potential for other improvements), please open up an [issue](https://github.com/astenlund/fs2ff/issues), and I will be happy to take a look.
