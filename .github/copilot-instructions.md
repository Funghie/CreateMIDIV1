# Copilot Instructions

## Project Guidelines
- For this app, MIDI 2.0 creates one bidirectional endpoint named exactly as entered (e.g., 'Phil'). UI preview for MIDI 2.0 may still show '(A)' and '(B)' labels while creating only one endpoint.
- When applying improvements to this project, preserve current working functionality and avoid behavior-breaking changes.
- In CreateMIDI, loopMIDI imports are always MIDI 1.0 and must be stored exactly as imported in the recreate list as `<imported name>|1`, without adding `WM to` or `WM from` prefixes.
- In CreateMIDI, the current work is only to store created ports for later restoration; the tracking file should preserve existing recreate entries and not overwrite or duplicate an existing `name|type` line when the same port is created again.
- Use specific custom icons for the top-right buttons instead of generated placeholder glyphs, and integrate icon assets through project resources.

## Text Formatting Preferences
- Use simple plain-text formatting with clear line spacing for text files like `readme.txt`.
- Distribute `Created MIDI Ports.txt` with a specific explanatory comment header block, including a `# BEGIN HERE` marker when the file is missing, and not as an empty file. 
- Include helpful explanatory comment lines in `Created MIDI Ports.txt`.