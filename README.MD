# CraftingOverhaul

A Valheim mod built on [Jötunn](https://github.com/Valheim-Modding/Jotunn) that makes crafting stations less tedious:

- **Search box** above the recipe list, matching recipe, category and ingredient names.
- **Collapsible categories**, grouped by item type or by biome (toggle in game).
- **Craft and build from nearby chests**, so you don't have to haul materials around first.

See [`CraftingOverhaul/README.md`](CraftingOverhaul/README.md) for the full feature list and settings. That file is also the Thunderstore package README.

## Project layout

| Path | Contents |
|---|---|
| `CraftingOverhaul/CraftingOverhaul.cs` | Plugin entry point; applies each Harmony patch class separately so one broken patch doesn't take down the rest |
| `CraftingOverhaul/ModConfig.cs` | BepInEx config entries |
| `CraftingOverhaul/RecipeList/` | Search bar, category headers and grouping (item type / biome) |
| `CraftingOverhaul/Biomes/` | Data-driven biome detection: `Collectors/` read game data into sources, `BiomeSolver.cs` holds the rules |
| `CraftingOverhaul/Containers/` | Finding usable nearby containers and counting/taking materials from them |
| `CraftingOverhaul/Dump/` | `co_dump` and `co_biome` console commands |
| `Corruption/` | Second plugin: the corruption endgame mod (`docs/corruption-design.md`). `Content/` holds items, pieces and patches, `Rules/` the game-independent rules, `Data/` tunable data tables (e.g. `affixes.csv`) |
| `CraftingOverhaul.Tests/` | Unit tests for game-independent logic of both plugins (`dotnet test`) |
| `FEATURES.md` | Every feature, its status, config and Harmony patch targets |
| `docs/PLAYER_SETUP.md` | Install guide for the players and the server admin |
| `docs/` | Start with `CURRENT_STATE.md` (status and next tasks); also `ARCHITECTURE.md`, `DECISIONS.md`, `GAME_DESIGN.md` (links the Corruption design, its review, `AFFIX_RULES.md` and the storage/quest brief), research notes and the ideas backlog |
| `JotunnModUnity/` | Unity project for making asset bundles (unused so far) |

## Development environment setup

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com) with the .NET desktop workload (or any IDE with the .NET SDK).
2. Install [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) into your Valheim folder, and [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/) into `BepInEx/plugins`.
3. Clone this repository and open `CraftingOverhaul.sln`. NuGet packages are restored automatically on build.
4. The Valheim install is detected through Steam. If yours isn't found, create `Environment.props` in the repository root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <VALHEIM_INSTALL>X:\PathToYourSteamLibrary\steamapps\common\Valheim</VALHEIM_INSTALL>
    <!-- Where Debug builds are copied to -->
    <MOD_DEPLOYPATH>$(VALHEIM_INSTALL)\BepInEx\plugins</MOD_DEPLOYPATH>
  </PropertyGroup>
</Project>
```

5. Build. `DoPrebuild.props` has `ExecutePrebuild` set to `true`, so on the first build (and after every game update) Jötunn generates publicized copies of the game assemblies in `valheim_Data/Managed/publicized_assemblies`. The mod compiles against those, which is how it can use the game's private fields and methods directly.

### Build output

`publish.ps1` runs after every build on Windows:

* **Debug**: copies `CraftingOverhaul.dll` (plus `.pdb`/`.mdb` for debugging) to `<Valheim>\BepInEx\plugins\CraftingOverhaul`, so you can start the game and test immediately.
* **Release**: zips the dll, `Package/manifest.json`, `Package/icon.png` and `CraftingOverhaul/README.md` into `bin\Release\CraftingOverhaul.dll.zip`, ready for Thunderstore.

## After a game update

Rebuild. The prebuild step regenerates the publicized assemblies. If a patched method changed, the mod logs `Failed to apply patch ...` in `BepInEx/LogOutput.log` and disables only that feature.

## Developing Assets with Unity

New Assets can be created with Unity and imported into Valheim using the mod. A Unity project is included in this repository under `<repo>\JotunnModUnity`.

### Unity Editor Setup

1. [Download](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe) UnityHub directly from Unity or install it with the Visual Studio Installer via `Individual Components` -> `Visual Studio Tools for Unity`.
2. You will need an Unity account to register your PC and get a free licence. Create the account, login with it in Unity Hub and get your licence via `Settings` -> `Licence Management`.
3. Install Unity Editor version 2020.3.33f.
4. Copy all `assembly_*.dll` from `<ValheimDir>\valheim_Data\Managed` into `<repo>\JotunnModUnity\Assets\Assemblies`. **Do this directly in the filesystem - don't open Unity first or import the dlls directly in Unity**.
5. **Warning:** These assembly files are copyrighted material and you can theoretically get into trouble when you distribute them in your github repository. To avoid that there is a .gitignore file in the Unity project folder. Keep that when you clone or copy this repository.
6. Open Unity Hub and add the JotunnModUnity project.
7. Open the project.
7. Install the `AssetBundle Browser` package in the Unity Editor via `Window`-> `Package Manager` for easy bundle creation.

## Debugging

You can enable remote debugging of your mod code at runtime via dnSpy or Visual Studio. Before being able to attach a remote debugger you will have to prepare your game install and turn it into a "Development Build" once:

1. Locate your Unity Editor installation from the previous step and navigate to `<UnityInstall>\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono`
2. Copy `UnityPlayer.dll` and `WinPixEventRuntime.dll` from that folder into your game installation folder. Overwrite existing files.
3. Open the `<Valheim>\valheim_Data\boot.config` with a text editor (Notepad++ for example) and add a new line `player-connection-debug=1` to it.
4. When starting up Valheim you should see a `Development Build` text at the lower-right corner of the screen.

### Debugging with Visual Studio

Your own code can be debugged in source with Visual Studio itself.

1. Install Visual Studio Tools for Unity (can be done in Visual Studio installer via `Individual Components` -> `Visual Studio Tools for Unity`)
3. Build the project with target `Debug`. The publish.ps1 PowerShell script from this repo...
   * copies the generated mod .dll and .pdb to \<ValheimDir>\BepInEx\plugins after a successful build
   * automatically generates a CraftingOverhaul.dll.mdb file, which is needed for Unity/mono debugging. It should be in \<ValheimDir>\BepInEx\plugins, too.
4. Start Valheim (either directly from Steam or hit F5 in Visual Studio when Steam is running)
5. Go to `Debug` -> `Attach Unity debugger`
6. You should see your local game instance listed as a target to attach the debugger to. If it is not there, try hitting `Refresh` as the debugger only appears after the game has loaded into the main menu.

### Unity project after a game update

1. Copy all `assembly_*.dll` from `<ValheimDir>\valheim_Data\Managed` into `<repo>\JotunnModUnity\Assets\Assemblies`. <br />
  **Do this directly in the filesystem - don't import the dlls in Unity**.
2. Go to Unity Editor and press `Ctrl+R`. This reloads all files from the filesystem and "re-imports" the copied dlls into the project.
