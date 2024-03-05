**C**hants **O**f **S**ennaar **M**od **L**oader (COSML)
=========================

A Chants Of Sennaar mod loader, using [MonoMod](https://github.com/MonoMod/MonoMod).

If you're a mod developer, there are [examples](https://github.com/COS-Modding/COSML/tree/main/Examples) in the repo.

Feel free to join the [modding discord server](https://discord.com/invite/cPsYmn7pdj).

How to install
=======
1. Download COSML in the [releases](https://github.com/COS-Modding/COSML/releases/latest).
2. Unzip the content in your game folder: `Chants of Sennaar\Chants Of Sennaar_Data\Managed\`.
3. Create manually a folder named `Mods` within the `Managed` folder and put your mods there.
4. Launch the game and enjoy!

To uninstall COSML, do `Verify integrity of game files...` on the game's property on Steam.

Internationalization (i18n)
=======
You can replace in-game translations or add custom languages. Simply duplicate `template.txt` in `Managed\I18n\` and name it after its language code. Then fill it with your translations.

Any i18n file named in the following list will replace their corresponding in-game language: `en, fr, es, es-419, pt, de, it, zh, zh-tw, ja, ko, ru, cs, pl`.

Useful links
=======
All sprites of the game: https://drive.google.com/drive/folders/1ISNfrq57yj26VVAu6vrk9cHR6tlRqGDr?usp=sharing

All fonts of the game: https://drive.google.com/drive/folders/1sn8N94b-fIlBANODLGepdt_WyhPYvYO6?usp=sharing

Build
=======

**If you want to use the API, for making mods or using them, please use a release or the installer.**

1. Clone the repository.
2. Copy the Managed folder from your Hollow Knight installation into the solution folder and name it `Vanilla`.
3. Build the solution using an IDE or `dotnet build`.
4. The result will be in `OutputFinal`.

Special thanks
=======
- [Rundisc_](https://www.rundisc.io/chants-of-sennaar/) for making this gorgeous game <3
- [Hollow Knight Modding API](https://github.com/hk-modding/api) used as a template to make this mod loader.

License
=======
Distributed under the MIT [license](https://github.com/COS-Modding/COSML/blob/main/LICENSE).
