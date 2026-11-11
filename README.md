# Secret of Mana Randomizer
It's a multi-mode randomizer for the SNES game Secret of Mana.

Features:
- Open world randomizer - the current most popular mode. Grants Flammie Drum early, and randomly distributes spells, mana seeds, characters, etc. to various locations.
- Vanilla randomizer - randomly swaps enemies, bosses, spells, etc. for a full vanilla run
- Ancient cave - older mode that procedurally generates maps.  Needs work!
- Boss rush - older mode where you fight all bosses in a random order.  Needs work!
- Chaos - older mode where you proceed through randomly-linked vanilla maps.  Needs work!

- For all modes, various optional quality of life improvements and bugfixes.

# Building
Check out and load the .sln in Visual Studio.  I've been using VS 2017.  Anything later than that should theoretically also work.

I'll be posting releases, so unless you're actively working on the project, you shouldn't need to build it.

## Building on Linux
The native Linux port lives under `src/` and targets .NET 8 with Avalonia.
It requires the .NET 8 SDK.

```
dotnet build src/SoMRandomizer.sln -c Release
dotnet run --project src/SoMRandomizer.App
```

Command-line ROM generation:

```
dotnet run --project src/SoMRandomizer.Cli -- srcRom=/path/to/SoM.smc \
    dstRom=/path/to/out.smc seed=0123456789ABCDEF "options=version=1.49 mode=open"
```

Build a self-contained Linux package (no .NET runtime needed on the target):

```
scripts/package-linux.sh
```

It writes `dist/SoMRandomizer-<version>-linux-x64/` plus a `.tar.gz` archive
containing the Avalonia app and the CLI. `PACKAGE_VERSION`, `RID` and `CONFIGURATION`
can be overridden via environment variables.

### Install from source

The installer builds the port and installs it. Run it as your normal user:
`sudo` is used only for copying files into a system directory, never for the
build. The default is a system-wide installation under `/usr/local`.

```
./install.sh                    # system-wide, /usr/local
./install.sh --local            # current user only, XDG directories, no sudo
./install.sh --prefix=/opt/som  # custom system-wide prefix
```

The self-contained GUI and CLI are installed under
`PREFIX/lib/somrandomizer/{app,cli}`; small launchers are placed in
`PREFIX/bin`. Desktop entry, icons, manual page and documentation are installed
under `PREFIX/share`.

Builds are kept in `${XDG_CACHE_HOME:-~/.cache}/somrandomizer/install-build`.
Use `--no-build` to reinstall that build without compiling; it is reused only
if it comes from this source tree, is a Release build for the requested RID and
matches the sources. `--build-dir=DIR` uses another build directory;
`--rid=RID` selects another .NET runtime identifier.

```
./install.sh --local --no-build     # reinstall the cached local build
./install.sh --local -U             # uninstall the local installation
sudo ./install.sh --uninstall       # uninstall the system-wide installation
./install.sh --dry-run              # show the effective options, change nothing
./install.sh --help
```

Every run prints its effective options first. The installer detects the
distribution and, if the .NET 8 SDK or the X11 runtime libraries are missing,
prints the exact package command for Arch, Fedora or Debian/Ubuntu. It never
installs system packages itself and never downloads or pipes remote scripts.

Avalonia 11.3 has no native Wayland backend: on a Wayland session the GUI runs
through XWayland. The compositor normally starts XWayland; if `DISPLAY` is not
available, install it (`xorg-xwayland` on Arch, `xorg-x11-server-Xwayland` on
Fedora, `xwayland` on Debian/Ubuntu) and start the app from the session.

Uninstallation is manifest-driven: only files recorded by this installer are
removed. The manifest is
`PREFIX/share/somrandomizer/install-manifest.txt` for system-wide installs and
`${XDG_STATE_HOME:-~/.local/state}/somrandomizer/install-manifest.txt` for
`--local`. Without a manifest nothing is removed, so a package-managed
installation is never touched. Before replacing or removing files, a running
GUI or CLI from this installation is closed with `SIGTERM` (same user, exact
executable path, up to 10 seconds; never `SIGKILL`). The installer does not
restart it.

The release tarball is a portable archive (`app/` and `cli/`); it does not
install desktop integration and has no manifest-based uninstaller.

GitHub Actions builds the original Windows app (`build-windows`) and the Linux
port plus package (`build-linux`) on every push and pull request.

# Using
Instructions will be here eventually for how to use the rando.  For now join the discord and ask if you have questions.

Releases will be posted here (no music pack)

and also here: https://secretofmanaancientcave.blogspot.com/ (with music pack)

# Contributing
Blerp

# Discord
https://discord.gg/YfmUHqU

We run a weekly open world seed, and this is currently the main source of general information, questions, etc. for the randomizer.
