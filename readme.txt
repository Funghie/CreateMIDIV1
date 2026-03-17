Windows MIDI Port Creator
=========================

by Phil Pendlebury (2026)

Overview
--------
Windows MIDI Port Creator creates loopback MIDI endpoints using the Windows MIDI Services SDK.

Requirements
------------
1. 64-bit Windows
2. .NET Framework 4.8.1 Runtime
3. Windows MIDI Services SDK installed (includes `midi.exe`)
   Install with:
   winget install Microsoft.WindowsMIDIServicesSDK
4. Windows MIDI Service running (service name: `midisrv`)
5. Administrator rights may be required to create endpoints

How to Use
----------
1. Launch `Windows MIDI Port Creator.exe`
2. Enter an endpoint name
3. Select MIDI version:
   - MIDI 1.0: creates two endpoints (`WM to <name>`, `WM from <name>`)
   - MIDI 2.0: creates one bidirectional endpoint (`<name>`)
4. Click `Create Port(s)`

Useful Links
------------
- Windows MIDI Services (Get latest runtime and tools):
  https://microsoft.github.io/MIDI/get-latest/
- Windows MIDI Services Discord:
  https://aka.ms/mididiscord
- Windows MIDI Services GitHub issues:
  https://aka.ms/midirepoissues

Notes
-----
- If creation fails, check the error details shown in the application.
- If `midi.exe` is missing, install the Windows MIDI Services SDK.

Acknowledgements
----------------
Thanks to Pete Brown (Microsoft).

Additional Information
----------------------
This application was written to work with the CN Remote Panel by Phil Pendlebury:
https://www.everythingcreative.biz/cnremote/
