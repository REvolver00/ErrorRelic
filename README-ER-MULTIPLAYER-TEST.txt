ERROR RELICS - LOCAL MULTIPLAYER TEST KIT
=============================================

What this is
------------
This kit is tailored to the uploaded ErrorRelics 0.93.1 clean source tree.

The project already contains:
- tools/prepare-smoke.ps1
- tests/MultiplayerSmokeDriver.cs
- an isolated test runtime/save design

This launcher reuses that design instead of touching the normal STS2 install/save.

The new 0.93.1 feature that still needs real co-op validation is:
Campfire Gift Relic
- choose one of your relics
- choose a teammate
- source relic is serialized before removal
- recipient receives a reconstructed relic through RelicCmd.Obtain
- recipient AfterObtained intentionally fires
- ERROR PROOF is excluded

INSTALL
-------
1. Copy these two files into the ErrorRelics project root:
   ER-Multiplayer-Test.bat
   ER-Multiplayer-Test.ps1

   They must sit next to:
   ErrorRelics.csproj

2. Double-click:
   ER-Multiplayer-Test.bat

3. The launcher tries to auto-detect:
   - Slay the Spire 2
   - BaseLib

   If it cannot, paste the requested path once.
   It stores only those paths in:
   .er-mp-test.json

MENU
----
1. AutoSmoke
   Runs the existing automated two-peer smoke test headlessly.
   It also runs the console assertions first.
   PASS requires the smoke PASS marker on BOTH host and client.

2. Gift2P
   Launches a visible Host and Client using the project's isolated runtime/save.
   The test-only smoke driver is disabled in this mode.
   Use this to exercise the actual new Gift Relic UI path.

3. CheckLogs
   Scans the newest session for exceptions, desync/disconnect markers,
   serialization failures, and "Gift relic failed".

4. Stop
   Stops only STS2 processes whose executable is inside:
   <game>\.errorrelic-multiplayer-tests

SAFETY
------
- Does NOT upload to Workshop.
- Does NOT modify version numbers.
- Does NOT use the normal STS2 save directory.
- Does NOT kill normal STS2 instances outside the isolated test runtime.
- Rebuilds the current ER source and repacks the current assets/localization.
- Keeps session logs under:
  multiplayer-test-results\<timestamp>-...

FIRST THING TO RUN
------------------
Run AutoSmoke once.

If it passes, run Gift2P and test:
1. Host -> Client normal relic.
2. Client -> Host normal relic.
3. ERROR relic transfer preserves H/E identity and description.
4. ERROR PROOF cannot be gifted.
5. Cancel relic picker.
6. Cancel target selection / leave campfire.
7. If anything goes wrong, stop and send the entire newest
   multiplayer-test-results session folder back for analysis.

NOTE
----
The existing automatic MultiplayerSmokeDriver does NOT cover the new Gift Relic UI.
Gift2P therefore intentionally uses the real UI/network path instead of pretending
that the old smoke test validates the new feature.


POWERSHELL COMPATIBILITY
------------------------
This v2 launcher automatically uses PowerShell 7 when available, otherwise it falls back to the Windows built-in PowerShell 5.1. You do not need to install PowerShell 7 just for this test kit.


V3 FIXES
--------
- Fully supports Windows PowerShell 5.1.
- No longer calls the project's PowerShell-7-only tools/prepare-smoke.ps1.
- Searches for BaseLib in:
  1) <game>\mods
  2) Steam Workshop content for app 2868840
  3) the user's Alchyr.Sts2.BaseLib NuGet cache
- If auto-detection still fails, paste the FOLDER that directly contains:
  BaseLib.dll
  BaseLib.json
  BaseLib.pck


V4 FIX
------
The PCK repack step now mirrors ErrorRelics.csproj:
- launches the REAL SlayTheSpire2.exe
- uses the REAL game folder as --path
- still writes ErrorRelics.pck only into the isolated multiplayer runtime

If the packer fails, the launcher now prints the tail of both:
- pack-current.out.log
- pack-current.err.log
directly in the console.


V5 FIX
------
Windows PowerShell 5.1 may report a blank ExitCode for the Godot packer
even when the pack succeeded. The launcher now validates pack success by:
1. ErrorRelics.pck exists
2. ErrorRelics.pck is non-empty
3. pack-current.out.log contains ERROR_ASSET_PACKED

This fixes the false failure shown after a successful PCK build.


V6 FIX
------
PowerShell 5.1 may finish WaitForExit before redirected stdout is fully flushed.
The PCK validator now waits up to 8 seconds for:
- ErrorRelics.pck to exist
- ErrorRelics.pck to be non-empty
- ERROR_ASSET_PACKED: to appear in pack-current.out.log

It also prints:
PCK validation: exists=... nonEmpty=... marker=...
so any future validation problem is immediately identifiable.
