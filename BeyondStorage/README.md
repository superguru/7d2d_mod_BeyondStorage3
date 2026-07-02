# Beyond Storage 3

The full documentation is directly maintained as part of the mod package source files.

➡️ [View full documentation](ModPackage/README.md)

---

## Development Setup

### Deploy Tool (required to run deployment builds)

The build system depends on [GAZ Mod Deploy](https://github.com/superguru/7d2d_gaz_mod_deploy)

Clone this and update `GAZModDeployDir` in the .csproj file.

Everything should be automatic after that, but if you have any issues, please refer to the GAZ Mod Deploy documentation.

### StructuredData

Clone the [StructuredData](https://github.com/superguru/StructuredData) repository and build separately.

It's recommended to use the Release build type.

### Reference paths

Update these path properties in the .csproj itself:
- `GameInstallDir`
- `GAZModDeployDir`
- `StructuredTextProjectDir`

## License

This mod is licensed under the Apache-2.0 License. See the LICENSE.txt for details.

*** PROJECT PAGE aka DEV NOTES `README.md` EOF ***