# Beyond Storage 3

The full documentation is directly maintained as part of the mod package source files.

➡️ [View full documentation](ModPackage/README.md)

---

## Development Setup

### Markdown to BBCode converter
Used to convert the README.md file in ModPackage\Docs to a commonly used mod publishing site format.

The generated file is ModPackage\Docs\README.bbcode.txt and can be copied and pasted into any website that uses this format for page submissions or forum posts.

You can find it at [superguru/md_to_bbcode](https://github.com/superguru/md_to_bbcode)

This has been forked from [michaelsstuff/md_to_bbcode](https://github.com/michaelsstuff/md_to_bbcode) and this tool and also my fork complies with the [GPL-3.0 license](https://www.gnu.org/licenses/gpl-3.0.en.html)

Change `<MDtoBBCodeDir>` in the .csproj file to the location you cloned the converter repo to.

### Deploy Tool

Required to run deployment builds, for both Debug and Release builds.

The build system depends on [GAZ Mod Deploy](https://github.com/superguru/7d2d_gaz_mod_deploy)

Change `<GAZModDeployDir>` in the .csproj file to the location you cloned the deploy tool repo to.

Everything should be automatic after that, but if you have any issues, please refer to the GAZ Mod Deploy documentation.

### StructuredData

Clone the [StructuredData](https://github.com/superguru/StructuredData) repository and build separately.

It's recommended to use the Release build type for assembly references and for deployments.

### Reference paths

Update these path properties in the .csproj itself:
- `GameInstallDir`
- `GAZModDeployDir`
- `StructuredTextProjectDir`

## License

This mod is licensed under the Apache-2.0 License. See the LICENSE.txt for details.

*** PROJECT PAGE aka DEV NOTES `README.md` EOF ***