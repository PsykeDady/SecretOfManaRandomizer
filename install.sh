#!/usr/bin/env bash
# Secret of Mana Randomizer installer: build this source tree and install the
# Avalonia GUI and the command-line generator system-wide or for one user.
# Run "./install.sh --help" for the full usage.

set -Eeuo pipefail

SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)
readonly SCRIPT_DIR
readonly MANIFEST_HEADER="# SoMRandomizer install manifest"
readonly BUILD_MARKER=.somrandomizer-install-build

usage() {
    cat <<'EOF'
Secret of Mana Randomizer installer - native Linux port (Avalonia + .NET 8)

Usage:
  ./install.sh [--local] [--no-build] [--build-dir=DIR] [--prefix=DIR] [--rid=RID]
  ./install.sh [--local] -U | --uninstall
  ./install.sh -h | --help

Installation (default):
  Builds the Linux port from this source tree (Release, self-contained) and
  installs the graphical randomizer and its command-line generator. Run it as
  your normal user: sudo is used only for copying files into a system
  directory that you cannot write to, never for building. sudo is checked
  after the build: if it refuses (wrong or expired password), on a terminal
  you are asked whether to retry ("Retry installing the build? [y/N]"); N, or
  no terminal, stops with nothing changed and the build kept.

  Every installation builds from scratch in
  ${XDG_CACHE_HOME:-~/.cache}/somrandomizer/install-build (emptied first,
  kept afterwards). To install again without compiling:
  --no-build              reuse that build as it is. It must exist, come from
                          this source tree, be a Release build for the requested
                          RID and be up to date with the sources; otherwise
                          nothing is installed and you are told to run without
                          --no-build.

  System-wide (default)  prefix /usr/local, change with --prefix=DIR:
                           DIR/bin/somrandomizer (GUI launcher)
                           DIR/bin/somrandomizer-cli (command-line generator)
                           DIR/lib/somrandomizer/app/ (self-contained GUI)
                           DIR/lib/somrandomizer/cli/ (self-contained CLI)
                           DIR/share/applications/somrandomizer.desktop
                           DIR/share/icons/hicolor/*/apps/somrandomizer.png
                           DIR/share/man/man1/somrandomizer.1
                           DIR/share/doc/somrandomizer/ (README, LICENSE)
                           DIR/share/somrandomizer/install-manifest.txt
  --local                for the current user only, no root and no sudo:
                           ${XDG_BIN_HOME:-~/.local/bin}/somrandomizer
                           ${XDG_BIN_HOME:-~/.local/bin}/somrandomizer-cli
                           ~/.local/lib/somrandomizer/{app,cli}
                           ${XDG_DATA_HOME:-~/.local/share}/applications,
                             icons, man, doc
                           ${XDG_STATE_HOME:-~/.local/state}/somrandomizer/install-manifest.txt
                         Your shell files are never edited; if the bin directory
                         is not in PATH you get a warning (the menu entry uses
                         the full path anyway).

  Installing again over an existing installation updates it in place; files
  that are no longer part of the randomizer are removed.

  The builds are self-contained: the target machine does not need the .NET
  runtime. Building requires the .NET 8 SDK; the GUI also needs the usual X11
  client libraries (X11, Xext, Xrandr, Xi, Xcursor, Xrender, SM, ICE,
  fontconfig, freetype and OpenGL). Avalonia 11.3 has no native Wayland
  backend: on a Wayland session the GUI runs through XWayland, which the
  compositor normally starts. Install/enable XWayland if DISPLAY is not
  available. The installer detects Arch, Fedora and Debian/Ubuntu (and
  derivatives through /etc/os-release ID_LIKE) and prints the exact package
  command when something is missing; it never installs packages itself.

Running the randomizer:
  Before files are replaced (install, update) or removed (uninstall), a running
  randomizer of this installation is closed: only processes of your user whose
  executable is exactly this installation's lib/somrandomizer/app/SoMRandomizer
  (GUI) or lib/somrandomizer/cli/SoMRandomizer.Cli (CLI) get SIGTERM. Nothing
  is matched by name and nothing else is ever signalled (not another copy
  started from a different location). The installer waits up to 10 seconds;
  if the application has not exited by then, it stops with an error and
  changes nothing, and is never killed forcibly. Everything that can fail
  first (build, staging, checks, sudo) happens before closing it.
  The randomizer is not restarted afterwards: start it again from the menu.

Uninstallation:
  -U, --uninstall         remove the system-wide installation (prefix /usr/local
                          or --prefix=DIR)
  --local -U              remove the --local installation of the current user
  Only the files listed in the install manifest are removed. Without a
  manifest nothing is removed (no guessing), so a package-managed installation
  is never touched.

Other options:
  --prefix=DIR            system-wide prefix (default /usr/local); not with --local
  --dry-run               show what would be done, change nothing
                          (every run first prints its effective options:
                          action, installation type, rebuild, ...,
                          each with its origin, default or option)
  --build-dir=DIR         use this build directory instead of the default
                          cache; it is rebuilt, or with --no-build only checked
  --rid=RID               .NET runtime identifier (default linux-x64, or $RID)
  -h, --help              show this help

Exit status: 0 success, 1 failure, 2 invalid command line.

Examples:
  ./install.sh                     system-wide, build and install
  ./install.sh --local             only for me, no root
  ./install.sh --local --no-build  install the previous --local build
  ./install.sh --prefix=/opt/som   system-wide under /opt/som
  ./install.sh --local -U          remove my --local installation
  sudo ./install.sh --uninstall    remove the system-wide installation
EOF
}

say() { printf '%s\n' "$*"; }
warn() { printf 'install.sh: warning: %s\n' "$*" >&2; }
die() {
    printf 'install.sh: error: %s\n' "$*" >&2
    exit 1
}
usage_error() {
    printf 'install.sh: %s\nRun "./install.sh --help" for usage.\n' "$*" >&2
    exit 2
}

# --- command line ---------------------------------------------------------

action=install
scope=system
prefix=/usr/local
prefix_given=0
build_dir=""
no_build=0
dry_run=0
rid=${RID:-linux-x64}
rid_given=0

for arg in "$@"; do
    case $arg in
        -h | --help) usage; exit 0 ;;
        --local) scope=local ;;
        -U | --uninstall) action=uninstall ;;
        --prefix=*) prefix=${arg#--prefix=}; prefix_given=1 ;;
        --build-dir=*) build_dir=${arg#--build-dir=}; [[ -n $build_dir ]] || usage_error "--build-dir requires a directory";;
        --no-build) no_build=1 ;;
        --dry-run) dry_run=1 ;;
        --rid=*) rid=${arg#--rid=}; rid_given=1 ;;
        *) usage_error "unknown option \"$arg\"" ;;
    esac
done

if [[ $action == uninstall && -n $build_dir ]]; then
    usage_error "--build-dir only applies to installation"
fi
if [[ $action == uninstall && $no_build == 1 ]]; then
    usage_error "--no-build only applies to installation"
fi
if [[ $action == uninstall && $rid_given == 1 ]]; then
    usage_error "--rid only applies to installation"
fi
if [[ $action == install || $rid_given == 1 ]]; then
    [[ $rid =~ ^[A-Za-z0-9_.-]+$ ]] ||
        usage_error "the .NET runtime identifier must match [A-Za-z0-9_.-]+ (got \"$rid\")"
fi
if [[ $scope == local && $prefix_given == 1 ]]; then
    usage_error "--prefix is for system-wide installation; --local uses the XDG directories"
fi
if [[ -n $build_dir ]]; then
    [[ $build_dir == /* ]] || usage_error "--build-dir must be an absolute path"
    [[ $build_dir != / ]] || usage_error "--build-dir=/ is not allowed"
    [[ $build_dir != *$'\n'* ]] || usage_error "--build-dir must not contain a newline"
fi

# --- distribution ---------------------------------------------------------

os_release=${SOMRANDOMIZER_OS_RELEASE:-/etc/os-release}
os_id=""
os_like=""
os_name="unknown Linux"
os_version=""

read_os_release() {
    [[ -r $os_release ]] || return 0
    local key value
    while IFS='=' read -r key value; do
        value=${value%\"}; value=${value#\"}; value=${value%\'}; value=${value#\'}
        case $key in
            ID) os_id=$value ;;
            ID_LIKE) os_like=$value ;;
            PRETTY_NAME) os_name=$value ;;
            VERSION_ID) os_version=$value ;;
        esac
    done < "$os_release"
}

# arch | fedora | debian | unknown, from ID first, then ID_LIKE.
distro_family() {
    local token
    # shellcheck disable=SC2086 # ID_LIKE is a space-separated list
    for token in $os_id $os_like; do
        case $token in
            arch) echo arch; return ;;
            fedora | rhel | centos) echo fedora; return ;;
            debian | ubuntu) echo debian; return ;;
        esac
    done
    echo unknown
}

read_os_release
family=$(distro_family)

# Avalonia 11.3 has X11 and Win32 backends only. On Wayland the GUI needs
# XWayland; detect it so the dependency report can say so.
session_type() {
    if [[ ${XDG_SESSION_TYPE:-} == wayland || -n ${WAYLAND_DISPLAY:-} ]]; then
        printf '%s\n' wayland
    elif [[ ${XDG_SESSION_TYPE:-} == x11 || -n ${DISPLAY:-} ]]; then
        printf '%s\n' x11
    else
        printf '%s\n' unknown
    fi
}
session=$(session_type)
readonly session

build_deps_command() {
    case $family in
        arch) echo "sudo pacman -S --needed dotnet-sdk-8.0" ;;
        fedora) echo "sudo dnf install dotnet-sdk-8.0" ;;
        debian) echo "sudo apt install dotnet-sdk-8.0" ;;
        *) echo "install the .NET 8 SDK with your package manager (https://dotnet.microsoft.com/download/dotnet/8.0)" ;;
    esac
}

# --- paths ----------------------------------------------------------------

# XDG base directory variables must be absolute; others are ignored (spec).
xdg_dir() {
    local value=$1 fallback=$2
    if [[ $value == /* ]]; then printf '%s\n' "$value"; else printf '%s\n' "$fallback"; fi
}

clean_path() { # strip trailing slashes, keep "/"
    local p=$1
    while [[ $p == */ && $p != / ]]; do p=${p%/}; done
    printf '%s\n' "$p"
}

[[ -z $build_dir ]] || build_dir=$(clean_path "$build_dir")

if [[ $scope == local && $EUID -eq 0 && -n ${SUDO_USER:-} && ${SUDO_USER} != root ]]; then
    usage_error "--local installs for the current user; run it without sudo"
fi
if [[ $scope == local ]]; then
    [[ ${HOME:-} == /* ]] || die "HOME is not set to an absolute path; cannot install with --local"
    bindir=$(clean_path "$(xdg_dir "${XDG_BIN_HOME:-}" "$HOME/.local/bin")")
    libbase=$(clean_path "$HOME/.local/lib")
    libdir=$libbase/somrandomizer
    datadir=$(clean_path "$(xdg_dir "${XDG_DATA_HOME:-}" "$HOME/.local/share")")
    manifest=$(clean_path "$(xdg_dir "${XDG_STATE_HOME:-}" "$HOME/.local/state")")/somrandomizer/install-manifest.txt
    prefix=$(clean_path "$HOME/.local")
else
    prefix=$(clean_path "$prefix")
    [[ $prefix == /* ]] || usage_error "--prefix must be an absolute path"
    [[ $prefix != / ]] || usage_error "--prefix=/ is not allowed; use /usr/local (default) or /usr"
    [[ $prefix != *$'\n'* ]] || usage_error "--prefix must not contain a newline"
    bindir=$prefix/bin
    libbase=$prefix/lib
    libdir=$libbase/somrandomizer
    datadir=$prefix/share
    manifest=$datadir/somrandomizer/install-manifest.txt
fi
readonly bindir libbase libdir datadir manifest prefix
installed_bin=$bindir/somrandomizer
installed_cli=$bindir/somrandomizer-cli
installed_app=$libdir/app/SoMRandomizer
installed_cli_bin=$libdir/cli/SoMRandomizer.Cli

if [[ $EUID -eq 0 && -n ${SUDO_USER:-} && ${SUDO_USER} != root ]]; then
    real_uid=$(id -u -- "$SUDO_USER")
else
    real_uid=$EUID
fi

# A destination is allowed only under bindir, libdir or datadir (or the
# manifest), as an absolute path without "." or ".." components.
safe_target() {
    local path=$1
    [[ $path == /* && $path != *$'\n'* ]] || return 1
    case "/$path/" in */../* | */./*) return 1 ;; esac
    [[ $path == "$manifest" || $path == "$bindir"/?* || $path == "$libdir"/?* || $path == "$datadir"/?* ]]
}

# Nearest existing ancestor of a path is writable by us.
writable() {
    local p=$1
    while [[ ! -e $p ]]; do p=$(dirname -- "$p"); done
    [[ -w $p ]]
}

SUDO=()
need_privileges() {
    [[ $scope == system ]] || return 1
    local dir
    for dir in "$bindir" "$libdir" "$datadir" "$(dirname -- "$manifest")"; do
        writable "$dir" || return 0
    done
    return 1
}

choose_privileges() {
    if need_privileges; then
        if [[ $EUID -eq 0 ]]; then
            die "cannot write to $prefix even as root"
        fi
        command -v sudo > /dev/null || die "writing to $prefix needs root privileges and sudo is not available; run as root or use --local"
        SUDO=(sudo)
        say "Root privileges are needed to write to $prefix; sudo will be used for copying files only."
    fi
}

# Validate sudo before anything is changed. A refused password (wrong, or
# expired) is not fatal at once on a terminal: the user can fix it and retry
# without rebuilding. Without a terminal, or on "N", nothing is changed.
authenticate_sudo() { # authenticate_sudo HINT
    ((${#SUDO[@]})) || return 0
    local answer
    while ! sudo -v; do
        warn "sudo authentication failed (wrong or expired password?)"
        if [[ ! -t 0 ]]; then
            die "nothing was changed; $1"
        fi
        say "If your password has expired, change it in another terminal (passwd), then retry."
        answer=""
        read -r -p "$( [[ $action == install ]] && echo "Retry installing the build?" || echo "Retry the uninstallation?") [y/N] " answer || answer=""
        [[ $answer == [yY] || $answer == [yY][eE][sS] ]] || die "nothing was changed; $1"
    done
}

# Quote a path for the Exec key when needed (XDG desktop entry spec).
desktop_exec_value() {
    local path=$1
    if [[ $path =~ [[:space:]\"\`\$\\] ]]; then
        path=${path//\\/\\\\}; path=${path//\"/\\\"}; path=${path//\`/\\\`}; path=${path//\$/\\\$}
        printf '"%s"\n' "$path"
    else
        printf '%s\n' "$path"
    fi
}

# --- build directory and source state ------------------------------------

if [[ $EUID -eq 0 ]]; then
    cache_home=$(getent passwd root | cut -d: -f6)/.cache
else
    cache_home=$(xdg_dir "${XDG_CACHE_HOME:-}" "${HOME:-/nonexistent}/.cache")
fi
default_build=$(clean_path "$cache_home")/somrandomizer/install-build
readonly default_build

source_version() {
    if [[ -n ${PACKAGE_VERSION:-} ]]; then
        printf '%s\n' "$PACKAGE_VERSION"
    elif command -v git > /dev/null && git -C "$SCRIPT_DIR" rev-parse --is-inside-work-tree > /dev/null 2>&1; then
        git -C "$SCRIPT_DIR" describe --tags --always --dirty 2> /dev/null || printf '%s\n' 0.0.0
    else
        printf '%s\n' 0.0.0
    fi
}

# Hash of every input that affects the published binaries. Assets and docs
# are copied from the source tree at install time, so they are not part of it.
source_hash() {
    (
        cd "$SCRIPT_DIR"
        {
            find src -type f ! -path '*/bin/*' ! -path '*/obj/*' -print0
            if [[ -f global.json ]]; then
                printf '%s\0' global.json
            fi
        } | sort -z | xargs -0 sha256sum | sha256sum | awk '{print $1}'
    )
}

marker_value() { # marker_value BUILD KEY
    sed -n "s/^$2=//p" "$1/$BUILD_MARKER" | head -n 1
}

write_build_marker() { # write_build_marker BUILD HASH
    {
        printf 'source=%s\n' "$SCRIPT_DIR"
        printf 'hash=%s\n' "$2"
        printf 'version=%s\n' "$(source_version)"
        printf 'rid=%s\n' "$rid"
    } > "$1/$BUILD_MARKER"
}

# The build belongs to this source tree and the requested RID. --no-build
# additionally requires the sources to be unchanged.
check_build_origin() { # check_build_origin BUILD
    local build=$1 marker_source marker_rid
    [[ -f $build/$BUILD_MARKER ]] ||
        die "there is no build to reuse in $build; run the installer without --no-build"
    marker_source=$(marker_value "$build" source)
    marker_rid=$(marker_value "$build" rid)
    [[ $(cd -- "$marker_source" 2> /dev/null && pwd -P) == "$SCRIPT_DIR" ]] ||
        die "$build was built for another source tree ($marker_source); run the installer without --no-build"
    [[ $marker_rid == "$rid" ]] ||
        die "$build was built for $marker_rid, not $rid; run the installer without --no-build"
}

check_build_fresh() { # check_build_fresh BUILD HASH
    local build=$1 hash=$2
    [[ $(marker_value "$build" hash) == "$hash" ]] ||
        die "the build in $build is not up to date with the sources; run the installer without --no-build"
    [[ -x $build/app/SoMRandomizer && -x $build/cli/SoMRandomizer.Cli ]] ||
        die "the build in $build is incomplete; run the installer without --no-build"
}

# --- running instances ----------------------------------------------------

stop_timeout=${SOMRANDOMIZER_STOP_TIMEOUT:-10} # seconds; overridable for tests
[[ $stop_timeout =~ ^[1-9][0-9]?$ ]] || usage_error "SOMRANDOMIZER_STOP_TIMEOUT must be 1-99 seconds"
stopped_instances=0

# Start time (field 22 of /proc/PID/stat) tells a process apart from a later
# one that reuses its PID. A zombie has already exited: no start time.
process_start() {
    local stat
    { IFS= read -r stat < "/proc/$1/stat"; } 2> /dev/null || return 1
    stat=${stat##*) }
    local -a fields
    read -r -a fields <<< "$stat"
    [[ ${fields[0]} != Z ]] || return 1
    printf '%s\n' "${fields[19]}"
}

process_uid() {
    local key value _
    while read -r key value _; do
        [[ $key == Uid: ]] && { printf '%s\n' "$value"; return 0; }
    done < "/proc/$1/status" 2> /dev/null
    return 1
}

# "PID UID START" for every process running this installation's GUI or CLI.
# The command name only preselects candidates cheaply; the decision is the
# exact executable path (" (deleted)" once a newer file replaced it).
running_instances() {
    local dir pid comm exe
    for dir in /proc/[0-9]*; do
        pid=${dir#/proc/}
        { IFS= read -r comm < "$dir/comm"; } 2> /dev/null || continue
        [[ $comm == SoMRandomizer || $comm == SoMRandomizer.C* ]] || continue
        exe=$(readlink -- "$dir/exe" 2> /dev/null) || continue
        exe=${exe% (deleted)}
        [[ $exe == "$installed_app" || $exe == "$installed_cli_bin" ]] || continue
        printf '%s %s %s\n' "$pid" "$(process_uid "$pid")" "$(process_start "$pid")"
    done
}

describe_instances() {
    local pid uid start found=0 will="will be"
    ((dry_run)) && will="would be"
    while read -r pid uid start; do
        found=1
        if [[ $uid == "$real_uid" ]]; then
            option "running" "PID $pid, $will closed (SIGTERM, up to ${stop_timeout} s) before files change"
        else
            option "running" "PID $pid of user $uid, left running (not yours)"
        fi
    done < <(running_instances)
    ((found)) || option "running" "no instance of this installation"
}

# Close this user's running randomizer of this installation, or fail without
# having changed anything. Never SIGKILL, never other users, never by name.
stop_running_instances() {
    local -a pids=() starts=() others=()
    local pid uid start
    while read -r pid uid start; do
        if [[ $uid == "$real_uid" ]]; then
            pids+=("$pid")
            starts+=("$start")
        else
            others+=("$pid")
        fi
    done < <(running_instances)
    if ((${#others[@]})); then
        warn "the randomizer is also running for other users (PID ${others[*]}); it is not stopped and keeps the version it started until it is restarted"
    fi
    ((${#pids[@]})) || return 0

    say "Closing the running Secret of Mana Randomizer (PID ${pids[*]}) ..."
    local i
    for i in "${!pids[@]}"; do
        kill -TERM "${pids[$i]}" 2> /dev/null || true
    done
    local waited=0 alive
    while :; do
        alive=()
        for i in "${!pids[@]}"; do
            [[ $(process_start "${pids[$i]}" 2> /dev/null) == "${starts[$i]}" ]] && alive+=("${pids[$i]}")
        done
        ((${#alive[@]})) || break
        if ((waited >= stop_timeout * 10)); then
            die "the randomizer (PID ${alive[*]}) did not exit within ${stop_timeout} s after SIGTERM; nothing was changed. Quit it and run the installer again."
        fi
        sleep 0.1
        waited=$((waited + 1))
    done
    stopped_instances=1
    say "Secret of Mana Randomizer closed."
}

# --- dependency checks ----------------------------------------------------

version_at_least() { # version_at_least 8.0.100 8.0.428
    [[ $(printf '%s\n%s\n' "$2" "$1" | sort -V | head -n1) == "$2" ]]
}

dotnet_cmd=${DOTNET:-dotnet}
if ! command -v "$dotnet_cmd" > /dev/null 2>&1; then
    if [[ -n ${DOTNET_ROOT:-} && -x ${DOTNET_ROOT}/dotnet ]]; then
        dotnet_cmd=${DOTNET_ROOT}/dotnet
    fi
fi

check_build_dependencies() {
    ((no_build)) && return 0
    local version
    if ! command -v "$dotnet_cmd" > /dev/null 2>&1; then
        say "Missing build requirements: the .NET 8 SDK (dotnet was not found in PATH${DOTNET_ROOT:+, nor in DOTNET_ROOT=$DOTNET_ROOT})"
        say "Detected: $os_name (family: $family). Install it with:"
        say "  $(build_deps_command)"
        die "missing build requirements"
    fi
    version=$("$dotnet_cmd" --version 2> /dev/null) ||
        die "dotnet is installed but cannot select a .NET SDK; check global.json and install the .NET 8 SDK ($(build_deps_command))"
    if ! version_at_least "$version" 8.0.100; then
        say "Found .NET SDK $version, but this port requires .NET 8 or newer."
        say "Detected: $os_name (family: $family). Install it with:"
        say "  $(build_deps_command)"
        die ".NET SDK is too old"
    fi
}

# Map missing shared libraries to distribution packages. These libraries are
# loaded by Avalonia/X11 at runtime, so they do not appear in ldd output.
declare -A RUNTIME_PKG_ARCH=(
    [libX11.so.6]=libx11 [libXext.so.6]=libxext [libXrandr.so.2]=libxrandr
    [libXi.so.6]=libxi [libXcursor.so.1]=libxcursor [libXrender.so.1]=libxrender
    [libSM.so.6]=libsm [libICE.so.6]=libice [libfontconfig.so.1]=fontconfig
    [libfreetype.so.6]=freetype2 [libGL.so.1]=mesa
)
declare -A RUNTIME_PKG_FEDORA=(
    [libX11.so.6]=libX11 [libXext.so.6]=libXext [libXrandr.so.2]=libXrandr
    [libXi.so.6]=libXi [libXcursor.so.1]=libXcursor [libXrender.so.1]=libXrender
    [libSM.so.6]=libSM [libICE.so.6]=libICE [libfontconfig.so.1]=fontconfig
    [libfreetype.so.6]=freetype [libGL.so.1]=mesa-libGL
)
declare -A RUNTIME_PKG_DEBIAN=(
    [libX11.so.6]=libx11-6 [libXext.so.6]=libxext6 [libXrandr.so.2]=libxrandr2
    [libXi.so.6]=libxi6 [libXcursor.so.1]=libxcursor1 [libXrender.so.1]=libxrender1
    [libSM.so.6]=libsm6 [libICE.so.6]=libice6 [libfontconfig.so.1]=libfontconfig1
    [libfreetype.so.6]=libfreetype6 [libGL.so.1]=libgl1
)

runtime_deps_command() { # runtime_deps_command SONAME...
    local -a packages=()
    local -A seen=()
    local soname pkg
    for soname in "$@"; do
        case $family in
            arch) pkg=${RUNTIME_PKG_ARCH[$soname]:-} ;;
            fedora) pkg=${RUNTIME_PKG_FEDORA[$soname]:-} ;;
            debian) pkg=${RUNTIME_PKG_DEBIAN[$soname]:-} ;;
            *) pkg="" ;;
        esac
        if [[ -n $pkg && -z ${seen[$pkg]:-} ]]; then
            seen[$pkg]=1
            packages+=("$pkg")
        fi
    done
    if ((${#packages[@]} == 0)); then
        printf '%s\n' "install the X11 client and font libraries with your package manager"
        return
    fi
    case $family in
        arch) printf 'sudo pacman -S --needed %s\n' "${packages[*]}" ;;
        fedora) printf 'sudo dnf install %s\n' "${packages[*]}" ;;
        debian) printf 'sudo apt install %s\n' "${packages[*]}" ;;
    esac
}

xwayland_deps_command() {
    case $family in
        arch) echo "sudo pacman -S --needed xorg-xwayland" ;;
        fedora) echo "sudo dnf install xorg-x11-server-Xwayland" ;;
        debian) echo "sudo apt install xwayland" ;;
        *) echo "install XWayland with your package manager" ;;
    esac
}

runtime_missing=()
check_runtime_dependencies() {
    runtime_missing=()
    command -v ldconfig > /dev/null || return 0
    local cache=$work/ldconfig.txt soname
    ldconfig -p > "$cache" 2> /dev/null || return 0
    for soname in libX11.so.6 libXext.so.6 libXrandr.so.2 libXi.so.6 \
                  libXcursor.so.1 libXrender.so.1 libSM.so.6 libICE.so.6 \
                  libfontconfig.so.1 libfreetype.so.6 libGL.so.1; do
        if ! grep -qF -- "$soname " "$cache"; then
            runtime_missing+=("$soname")
        fi
    done
    if ((${#runtime_missing[@]})); then
        say "Missing GUI runtime libraries: ${runtime_missing[*]}"
        say "Detected: $os_name (family: $family). Install them with:"
        say "  $(runtime_deps_command "${runtime_missing[@]}")"
        say "The randomizer can be installed now, but the GUI will not start until these libraries are available."
    fi
    check_wayland_runtime
}

# The GUI is Avalonia 11.3 (X11 only). A Wayland session is supported through
# XWayland, normally started by the compositor; report when that is not usable.
check_wayland_runtime() {
    [[ $session == wayland ]] || return 0
    if [[ -z ${DISPLAY:-} ]]; then
        say "Wayland session detected, but DISPLAY is unset."
        say "Avalonia 11.3 has no native Wayland backend: enable/install XWayland so the GUI can use X11."
        say "Install it with:"
        say "  $(xwayland_deps_command)"
        say "Then start the randomizer from the Wayland session, where the compositor exports DISPLAY."
        return 0
    fi
    if command -v xdpyinfo > /dev/null && ! xdpyinfo -display "$DISPLAY" > /dev/null 2>&1; then
        say "Wayland session detected, but the XWayland display $DISPLAY is not reachable."
        say "Enable/install XWayland, or start the randomizer from the Wayland session where DISPLAY works."
        say "Install it with:"
        say "  $(xwayland_deps_command)"
    fi
}

# --- work directory -------------------------------------------------------

work=""
tmp_base=$(clean_path "${TMPDIR:-/tmp}")
cleanup() {
    local status=$?
    if [[ -n $work && -d $work && $work == "$tmp_base"/somrandomizer-install.* ]]; then
        rm -rf -- "$work"
    fi
    if ((status != 0 && stopped_instances)); then
        printf 'install.sh: the randomizer was closed before the failure; start it again with: %s\n' "$installed_bin" >&2
    fi
}
trap cleanup EXIT

# --- manifest -------------------------------------------------------------

# Validate in the main shell (a "die" inside a process substitution would only
# end the subshell), then read the recorded paths.
check_manifest() {
    local first=""
    IFS= read -r first < "$manifest" || true
    [[ $first == "$MANIFEST_HEADER" ]] || die "$manifest is not a SoMRandomizer install manifest; nothing changed"
}

read_manifest() { # prints the recorded paths of a checked manifest
    grep -v '^#' "$manifest" | grep -v '^$' || true
}

remove_file() {
    local path=$1
    if ! safe_target "$path"; then
        warn "refusing to remove $path: outside $bindir, $libdir and $datadir"
        return 0
    fi
    if [[ -d $path && ! -L $path ]]; then
        warn "refusing to remove directory $path"
        return 0
    fi
    if [[ -e $path || -L $path ]]; then
        "${SUDO[@]}" rm -f -- "$path"
    fi
}

remove_empty_dir() {
    local dir=$1
    [[ -d $dir ]] || return 0
    "${SUDO[@]}" rmdir -- "$dir" 2> /dev/null || true
}

# Empty directories that belong to this installation alone, deepest first.
# Shared parents (share/applications, share/icons/hicolor, ...) are kept.
remove_owned_dirs() {
    local f dir root
    local -a dirs=()
    for f in "$@"; do
        local -a roots=("$libdir" "$datadir/somrandomizer" "$datadir/doc/somrandomizer")
        for root in "${roots[@]}"; do
            [[ $f == "$root"/* ]] || continue
            dir=$(dirname -- "$f")
            while [[ $dir == "$root" || $dir == "$root"/* ]]; do
                dirs+=("$dir")
                dir=$(dirname -- "$dir")
            done
        done
    done
    ((${#dirs[@]})) || return 0
    while IFS= read -r dir; do
        remove_empty_dir "$dir"
    done < <(printf '%s\n' "${dirs[@]}" | sort -u | awk '{ print length, $0 }' | sort -rn | cut -d' ' -f2-)
}

# The desktop entry declares no MIME types, so no desktop database needs
# updating; only an existing hicolor icon cache is refreshed.
refresh_desktop_caches() {
    if command -v gtk-update-icon-cache > /dev/null && [[ -f $datadir/icons/hicolor/index.theme ]]; then
        "${SUDO[@]}" gtk-update-icon-cache -q -t -f "$datadir/icons/hicolor" 2> /dev/null || true
    fi
}

# --- plan -----------------------------------------------------------------

option() { printf '  %-17s = %s\n' "$1" "$2"; }

session_description() {
    case $session in
        wayland) printf '%s\n' "wayland (GUI via XWayland; Avalonia 11.3 has no native Wayland backend)" ;;
        x11) printf '%s\n' "x11" ;;
        *) printf '%s\n' "unknown (GUI needs X11 or XWayland)" ;;
    esac
}

# Printed first by every run (also --dry-run): the effective settings and
# where each one comes from.
print_options() {
    say "Secret of Mana Randomizer installer"
    option "action" "$action$( [[ $action == uninstall ]] && echo " (-U)" || echo " (default)")"
    if [[ $scope == system ]]; then
        option "installation type" "system-wide, all users (prefix $prefix$( ((prefix_given)) && echo ", --prefix" || echo ", default"))"
    else
        option "installation type" "local, current user only (--local)"
    fi
    if [[ $action == install ]]; then
        if ((no_build)); then
            option "rebuild" "false: reuse ${build_dir:-$default_build} (--no-build; must be up to date)"
            if [[ -f ${build_dir:-$default_build}/$BUILD_MARKER ]]; then
                local built
                built=$(marker_value "${build_dir:-$default_build}" version)
                option "version" "build ${built:-unknown}, sources $(source_version)"
            fi
        elif [[ -n $build_dir ]]; then
            option "rebuild" "true: build in $build_dir (--build-dir)"
        else
            option "rebuild" "true: from scratch in $default_build (default)"
        fi
        option "runtime" "$rid"
    fi
    option "dry run" "$( ((dry_run)) && echo true || echo false)"
    option "binary" "$installed_bin"
    option "library" "$libdir"
    option "data" "$datadir"
    option "manifest" "$manifest"
    if need_privileges; then option "privileges" "sudo for file copies"; else option "privileges" "none"; fi
    option "stop timeout" "${stop_timeout} s"
    option "distribution" "$os_name (family: $family)"
    option "session" "$(session_description)"
    describe_instances
}

# --- install --------------------------------------------------------------

do_install() {
    check_build_dependencies
    choose_privileges
    work=$(mktemp -d "$tmp_base/somrandomizer-install.XXXXXX")

    local version hash build install_version reuse=0
    version=$(source_version)
    hash=$(source_hash)
    if ((no_build)); then
        build=${build_dir:-$default_build}
        check_build_origin "$build"
        check_build_fresh "$build" "$hash"
        install_version=$(marker_value "$build" version)
        say "Reusing the build in $build (--no-build: nothing is compiled)"
        reuse=1
    fi
    if ((reuse)); then
        :
    elif [[ -n $build_dir ]]; then
        build=$build_dir
        if [[ -e $build && ! -f $build/$BUILD_MARKER && -n $(find "$build" -mindepth 1 -maxdepth 1 -print -quit 2> /dev/null) ]]; then
            die "$build exists but was not created by the SoMRandomizer installer; remove it yourself"
        fi
        if [[ -f $build/$BUILD_MARKER ]]; then
            local existing_source
            existing_source=$(marker_value "$build" source)
            [[ $(cd -- "$existing_source" 2> /dev/null && pwd -P) == "$SCRIPT_DIR" ]] ||
                die "$build belongs to another source tree ($existing_source); choose another --build-dir"
        fi
        mkdir -p -- "$build" || die "cannot create $build"
        : > "$build/$BUILD_MARKER"
        say "Building Secret of Mana Randomizer in $build (--build-dir) ..."
        rm -rf -- "$build/app" "$build/cli"
        build_package "$build"
        write_build_marker "$build" "$hash"
        install_version=$version
    else
        build=$default_build
        # From scratch every time, but only in a directory this installer
        # created (marker file) or an empty/missing one.
        if [[ -e $build && ! -f $build/$BUILD_MARKER && -n $(find "$build" -mindepth 1 -maxdepth 1 -print -quit 2> /dev/null) ]]; then
            die "$build exists but was not created by the SoMRandomizer installer; remove it yourself"
        fi
        rm -rf -- "$build"
        mkdir -p -- "$build" || die "cannot create $build"
        : > "$build/$BUILD_MARKER"
        say "Building Secret of Mana Randomizer (Release, $rid) ..."
        build_package "$build"
        write_build_marker "$build" "$hash"
        install_version=$version
    fi

    stage_package "$build"
    check_runtime_dependencies
    install_version=${install_version:-unknown}

    # Destination of every staged file.
    local -a sources=() targets=()
    local rel target
    while IFS= read -r -d '' rel; do
        rel=${rel#./}
        case $rel in
            bin/*) target=$bindir/${rel#bin/} ;;
            lib/somrandomizer/*) target=$libdir/${rel#lib/somrandomizer/} ;;
            share/*) target=$datadir/${rel#share/} ;;
            *) die "unexpected staged file $rel" ;;
        esac
        safe_target "$target" || die "refusing to install outside the destination: $target"
        sources+=("$work/stage/$rel")
        targets+=("$target")
    done < <(cd "$work/stage" && find . \( -type f -o -type l \) -print0 | sort -z)

    # Files of a previous installation that are no longer installed.
    local -a stale=()
    if [[ -f $manifest ]]; then
        check_manifest
        local old
        declare -A wanted=()
        for target in "${targets[@]}"; do wanted["$target"]=1; done
        while IFS= read -r old; do
            [[ -n ${wanted[$old]:-} ]] || stale+=("$old")
        done < <(read_manifest)
        say "Updating the existing installation ($manifest)"
    fi

    # Last step that can still fail on its own: the sudo password, asked for
    # before the running application is closed.
    authenticate_sudo "the build in $build is kept: run the installer again with --no-build to install it without compiling"
    stop_running_instances

    say "Installing ${#targets[@]} files ..."
    local i
    for i in "${!targets[@]}"; do
        if [[ -L ${sources[$i]} ]]; then
            "${SUDO[@]}" mkdir -p -- "$(dirname -- "${targets[$i]}")"
            "${SUDO[@]}" cp -P -f -- "${sources[$i]}" "${targets[$i]}"
        else
            "${SUDO[@]}" install -D -m "$(stat -c %a -- "${sources[$i]}")" -- "${sources[$i]}" "${targets[$i]}"
        fi
    done
    for target in "${stale[@]}"; do remove_file "$target"; done
    remove_owned_dirs "${stale[@]}"

    local list=$work/manifest
    {
        printf '%s\n' "$MANIFEST_HEADER"
        printf '# version: %s\n' "$install_version"
        printf '# scope: %s\n' "$scope"
        printf '%s\n' "${targets[@]}"
    } > "$list"
    "${SUDO[@]}" install -D -m 0644 -- "$list" "$manifest"
    refresh_desktop_caches

    say "Secret of Mana Randomizer is installed: $installed_bin"
    if [[ ":$PATH:" != *":$bindir:"* ]]; then
        warn "$bindir is not in your PATH. The menu entry works anyway; to run \"somrandomizer-cli\" from a terminal, add it to PATH yourself."
    fi
    if ((stopped_instances)); then
        say "The randomizer was closed for the update and is not restarted automatically."
    fi
    say "Start it from the application menu (Secret of Mana Randomizer) or with: $installed_bin"
}

build_package() { # build_package BUILD
    local build=$1
    export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
    if ! "$dotnet_cmd" publish "$SCRIPT_DIR/src/SoMRandomizer.App/SoMRandomizer.App.csproj" \
            -c Release -r "$rid" --self-contained true --nologo \
            -p:DebugType=None -p:DebugSymbols=false -p:GenerateDocumentationFile=false \
            -o "$build/app" > "$work/build-app.log" 2>&1; then
        tail -n 30 "$work/build-app.log" >&2
        die "building the graphical application failed (log above)"
    fi
    if ! "$dotnet_cmd" publish "$SCRIPT_DIR/src/SoMRandomizer.Cli/SoMRandomizer.Cli.csproj" \
            -c Release -r "$rid" --self-contained true --nologo \
            -p:DebugType=None -p:DebugSymbols=false -p:GenerateDocumentationFile=false \
            -o "$build/cli" > "$work/build-cli.log" 2>&1; then
        tail -n 30 "$work/build-cli.log" >&2
        die "building the command-line generator failed (log above)"
    fi
    [[ -x $build/app/SoMRandomizer ]] || die "the build has no app/SoMRandomizer executable"
    [[ -x $build/cli/SoMRandomizer.Cli ]] || die "the build has no cli/SoMRandomizer.Cli executable"
}

stage_package() { # stage_package BUILD
    local build=$1 stage=$work/stage
    mkdir -p -- "$stage/bin" "$stage/lib/somrandomizer/app" "$stage/lib/somrandomizer/cli"
    cp -a -- "$build/app/." "$stage/lib/somrandomizer/app/"
    cp -a -- "$build/cli/." "$stage/lib/somrandomizer/cli/"

    write_wrapper "$stage/bin/somrandomizer" "$installed_app"
    write_wrapper "$stage/bin/somrandomizer-cli" "$installed_cli_bin"

    install -D -m 0644 -- "$SCRIPT_DIR/assets/somrandomizer.desktop" "$stage/share/applications/somrandomizer.desktop"
    local exec_value escaped
    exec_value=$(desktop_exec_value "$installed_bin")
    escaped=${exec_value//\\/\\\\}
    escaped=${escaped//|/\\|}
    escaped=${escaped//&/\\&}
    sed -i "s|^Exec=somrandomizer\$|Exec=$escaped|" "$stage/share/applications/somrandomizer.desktop"
    if [[ $exec_value == "$installed_bin" ]]; then
        sed -i "s|^TryExec=somrandomizer\$|TryExec=$escaped|" "$stage/share/applications/somrandomizer.desktop"
    else
        sed -i '/^TryExec=/d' "$stage/share/applications/somrandomizer.desktop"
    fi

    mkdir -p -- "$stage/share/icons"
    cp -a -- "$SCRIPT_DIR/assets/icons/hicolor" "$stage/share/icons/"
    install -D -m 0644 -- "$SCRIPT_DIR/assets/man/somrandomizer.1" "$stage/share/man/man1/somrandomizer.1"
    # The man page names the actual installation prefix, not a placeholder.
    local doc_prefix=$prefix escaped_prefix
    escaped_prefix=${doc_prefix//\\/\\\\}
    escaped_prefix=${escaped_prefix//|/\\|}
    escaped_prefix=${escaped_prefix//&/\\&}
    sed -i "s|@PREFIX@|$escaped_prefix|g" "$stage/share/man/man1/somrandomizer.1"

    install -D -m 0644 -- "$SCRIPT_DIR/README.md" "$stage/share/doc/somrandomizer/README.md"
    install -D -m 0644 -- "$SCRIPT_DIR/LICENSE" "$stage/share/doc/somrandomizer/LICENSE"

    local top
    for top in "$stage"/*; do
        case ${top##*/} in
            bin | lib | share) ;;
            *) die "unexpected staged directory ${top##*/}; only bin/, lib/ and share/ can be installed" ;;
        esac
    done
    [[ -x $stage/bin/somrandomizer ]] || die "the staged tree has no bin/somrandomizer"
    [[ -x $stage/bin/somrandomizer-cli ]] || die "the staged tree has no bin/somrandomizer-cli"
    [[ -x $stage/lib/somrandomizer/app/SoMRandomizer ]] || die "the staged tree has no GUI executable"
}

write_wrapper() { # write_wrapper PATH TARGET
    local path=$1 target=$2
    mkdir -p -- "$(dirname -- "$path")"
    {
        printf '#!/bin/sh\nexec '
        printf '%q' "$target"
        printf ' "$@"\n'
    } > "$path"
    chmod 0755 -- "$path"
}

# --- uninstall ------------------------------------------------------------

do_uninstall() {
    local -a files=()
    local line
    if [[ ! -f $manifest ]]; then
        if [[ -e $installed_bin ]]; then
            die "$installed_bin exists but there is no install manifest at $manifest (installed by a package or by hand?); nothing removed"
        fi
        die "no SoMRandomizer $scope installation found (no $manifest)"
    fi
    check_manifest
    while IFS= read -r line; do files+=("$line"); done < <(read_manifest)
    choose_privileges
    authenticate_sudo "run the uninstallation again when sudo works"
    stop_running_instances

    say "Removing ${#files[@]} files ..."
    local f
    for f in "${files[@]}"; do remove_file "$f"; done
    "${SUDO[@]}" rm -f -- "$manifest"
    remove_owned_dirs "${files[@]}"
    remove_empty_dir "$(dirname -- "$manifest")"
    refresh_desktop_caches

    say "Secret of Mana Randomizer ($scope) is uninstalled."
    say "No user settings or generated ROMs were installed, so nothing else was removed."
}

# --- main -----------------------------------------------------------------

print_options
if ((dry_run)); then
    if [[ $action == uninstall && -f $manifest ]]; then
        check_manifest
        say "Would remove:"
        read_manifest | sed 's/^/  /'
    fi
    exit 0
fi

case $action in
    install) do_install ;;
    uninstall) do_uninstall ;;
esac
