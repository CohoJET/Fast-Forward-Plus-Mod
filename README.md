# Fast Forward Plus

Sets the simulation speed with a hotkey, instead of vanilla's 1x/3x toggle. By default `F1`-`F5` are 1x, 2x, 4x, 8x and 16x, and the current multiplier shows in the top-right corner while sped up.

Made with the [Approximately Up Modkit](https://github.com/CohoJET/Approximately-Up-Modkit) - place
under `Assets/Mods/` in a generated workspace.

## Configuration

`BepInEx/config/com.approximatelyup.mods.fastforwardplus.cfg`:

| Setting | Default | |
| --- | --- | --- |
| `Speeds.Bindings` | `F1=1, F2=2, F3=4, F4=8, F5=16` | Comma-separated `KEY=MULTIPLIER` pairs. |

Keys are Input System
[`Key`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.Key.html)
names.

## Quirks

- **About 18x is the ceiling that means anything.** A frame's catch-up is capped at
  `Core.MaximumDeltaTime` (0.3s), 18 fixed steps. Past that the simulation quietly runs *slower* than the number claims.
- **3x is what the developers tested.** Physics fidelity above that is your problem, not theirs.
- **The hotkeys do not know the game's own bindings.** Pick keys the game does not already use.

## Build

Build with the `Modkit > Mods > Build` menu command in Unity, or directly:

```
dotnet build Plugin/FastForwardPlus.csproj -p:"ModkitGameDir=<the folder holding GameAssembly.dll>"
```

## Install

Place in `BepInEx/plugins/FastForwardPlus/`.
