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
   - When creating MIDI 2.0 endpoints, they may show up as (`<name>`) (A) and (`<name>`) (B) in some applications. This is expected behaviour.
4. Click `Create Port(s)`

<img width="448" height="420" alt="CreateMIDI1" src="https://github.com/user-attachments/assets/1339c5a0-38e2-4d39-97ea-0041d54f4164" />
<img width="448" height="420" alt="CreateMIDI2" src="https://github.com/user-attachments/assets/ea2c5ad6-1d5a-4868-84a9-f0e94cb2c7d6" />


Importing loopMIDI Ports
------------------------
Use the `Import loopMIDI` button to migrate loopMIDI ports to Windows MIDI 1.0.

Each detected loopMIDI port is created with the exact same name.

After import, the app shows a migration summary, including the port names created.

<img width="448" height="420" alt="CreateloopMIDI" src="https://github.com/user-attachments/assets/48a8b80d-24d8-4251-afdf-4837dfd6a397" />


Temporary Endpoint Behaviour
----------------------------
At this stage, all ports created by this tool are temporary.

They do not persist after a reboot.
They do not persist after restarting the Windows MIDI service (`midisrv`).

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
