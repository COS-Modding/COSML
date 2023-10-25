# Examples
This directory contains many example mods to show the common uses of COSML.
These mods are intended to give beginners a look at how to start to develop mods using COSML, although proficient knowlege of C# is fairly required.  

## Setup
1. Copy `Directory.build.example.props` and rename it to `Directory.build.props`.
2. Edit the `ManagedDir` field with your game Managed folder (ex: `[...]\Steam\steamapps\common\Chants of Sennaar\Chants Of Sennaar_Data\Managed`).
3. Open the project with your IDE (Visual Studio 2022 or Rider recommended).
4. Build the project with eitheryour IDE or using the `dotnet` CLI.
5. Finally, copy the ouput mod dll from the build folder to `[...]\Managed\Mods\Mod Name\ModName.dll`.
