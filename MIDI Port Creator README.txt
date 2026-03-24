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
5. Administrator rights may be required to create endpoints and configure startup recreation

How to Use
----------
1. Launch `Windows MIDI Port Creator.exe`
2. Enter an endpoint name
3. Select MIDI version:
   - MIDI 1.0: creates two endpoints (`WM to <name>`, `WM from <name>`)
   - MIDI 2.0: creates one bidirectional endpoint (`<name>`)
   - When creating MIDI 2.0 endpoints, they may show up as (`<name>`) (A) and (`<name>`) (B) in some applications. This is expected behaviour.
4. Click `Create Port(s)`

Recreate Ports on Startup
-------------------------
Use the checkbox:
`Recreate ports on system startup (uses Task Scheduler)`

to enable automatic restoration of previously created ports after sign-in/startup.

When enabled:
- The app creates/updates a scheduled task for startup recreation.
- Created ports are read from `Created MIDI Ports.txt` in the application folder.
- The app waits for the Windows MIDI service (`midisrv`) before attempting recreation.

When disabled:
- The scheduled startup recreation task is removed/disabled.

Created MIDI Ports File (`Created MIDI Ports.txt`)
---------------------------------------------------
The file stores one port per line in this format:

`<Port Name>|<Type>`

Where:
- `1` = MIDI 1.0
- `2` = MIDI 2.0

Examples:
- `My Loop Port|1`
- `My MIDI 2 Port|2`

You can edit this file manually to remove ports you no longer want recreated.

The parser ignores:
- Blank lines
- Comment lines beginning with `#`
- Comment lines beginning with `//`

Importing loopMIDI Ports
------------------------
Use the `Import loopMIDI` button to migrate loopMIDI ports to Windows MIDI 1.0.

Each detected loopMIDI port is created with the exact same name.

After import, the app shows a migration summary, including the port names created.

Temporary Endpoint Behaviour
----------------------------
Ports created by this tool are temporary endpoints in Windows MIDI Services.

Without startup recreation enabled, they do not persist after reboot.
They also do not persist after restarting the Windows MIDI service (`midisrv`).

If startup recreation is enabled, the app restores saved ports on startup using
entries in `Created MIDI Ports.txt`.

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
