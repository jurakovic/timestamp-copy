
# Timestamp Copy

Timestamp Copy integrates directly into the Windows File Explorer context menu, enabling you to **copy** and **paste** file and folder timestamps with ease.

This solution is especially useful when you need to preserve or replicate Date Created and Date Modified values across files or folders – ideal for organizing backups, restoring files, or syncing metadata.

![ContextMenu](img/contextmenu.png)  
<sup>(Context Menu)</sup>

### Download

[![GitHub Release](https://img.shields.io/github/v/release/jurakovic/timestamp-copy?include_prereleases)](https://github.com/jurakovic/timestamp-copy/releases/latest)

### Usage (Context Menu)

Right-click on a file or folder under the context menu and choose:

- `Copy` – to copy the selected file or folder's Date Created and Date Modified timestamps to a clipboard.

Right-click on another file or folder and choose:

- `Paste` – to apply previously copied timestamps.
- `Paste "Date Created"` – to apply only the Date Created.
- `Paste "Date Modified"` – to apply only the Date Modified.

Right-click on the same (or any other) file or folder and choose:

- `Undo` – to restore the previously overwritten timestamp(s).

### Requirements

- Windows 10/11 (x64)
- Administrator privileges (required for installation only)

There is nothing else to install. The executables are self-contained – no .NET runtime, and no PowerShell execution policy to change.

### Installation

1. Download `TimestampCopy-<version>-win-x64.zip` from the [latest release](https://github.com/jurakovic/timestamp-copy/releases/latest).
2. Unzip it somewhere permanent. The installer registers the path you unzipped it to, so pick a location you will not move afterwards. Keep the folder's contents together – `tscp.exe` on its own will not run.
3. Open an elevated terminal ('Run as Administrator') in that folder and install the context menu entries.

	```text
	tscp.exe -i
	```

	or install it in Background Mode (no terminal window at all)

	```text
	tscp.exe -b
	```

To remove the context menu entries and the stored clipboard data:

```text
tscp.exe -u
```

> Because the release is downloaded from the internet and not code-signed, Windows SmartScreen may warn the first time you run it. Choose *More info* → *Run anyway*.

To move the installation elsewhere, run `tscp.exe -u`, move the folder, then run `tscp.exe -i` again from the new location.

### Usage (CLI)

The tool is made to be run from the context menu, but it can also be run directly from the command line.

Parameters:
```text
-Help (-h)                       Print help.
-Version (-v)                    Print the current version.
-Install (-i)                    Install the context menu entries in Standalone Mode.
-InstallBackgroundMode (-b)      Install the context menu entries in Background Mode (runs without a terminal window).
-Uninstall (-u)                  Uninstall the context menu entries and remove related data.
-Copy (-c) <path>                Copy timestamps of the specified file or folder to the clipboard.
-Paste (-p) <path>               Paste the copied timestamps to the specified file or folder.
-PasteDateCreated (-pc) <path>   Paste only the copied Date Created timestamp to the specified file or folder.
-PasteDateModified (-pm) <path>  Paste only the copied Date Modified timestamp to the specified file or folder.
-Undo (-z)                       Restore the previous timestamps of the last modified file or folder.
-Quiet (-q)                      Suppress output messages. After run check the exit code.
-SkipConfirm (-y)                Skip confirmation prompts when applying changes.
*none*                           Show the install/uninstall menu.
```

Parameters are case-insensitive and also accept their long form (`-Copy` as well as `-c`).

Some examples:
```text
# Copy timestamps
tscp.exe -c "C:\Foo.txt"

# Paste timestamps
tscp.exe -p "D:\Bar.txt"

# Paste timestamps without output messages (confirm prompt still shown)
tscp.exe -p "D:\Bar.txt" -q

# Paste timestamps without output messages and confirm prompt
tscp.exe -p "D:\Bar.txt" -q -y

# Paste Date Created
tscp.exe -pc "D:\Bar.txt"

# Paste Date Modified
tscp.exe -pm "D:\Bar.txt"

# Undo
tscp.exe -z
```

Exit codes are `0` on success, `1` when an operation was refused (path missing, clipboard empty or corrupted, not running as Administrator), and `2` for a request the executable does not handle.

It can also be run without any argument, which will show the install/uninstall menu:

```text
Timestamp Copy (3.0.0)

[i] Install
[b] Install (Background Mode)
[u] Uninstall
[h] Help

[q] Quit

Choose option:
```

### Implementation Details

#### Operations

Five main operations are implemented:

**`Copy`**
- Copies the Date Created and Date Modified timestamps of a specified file or folder to the clipboard.
- It will output the specified file or folder's path and the copied timestamps.

**`Paste`**
- Applies the copied timestamps to a specified file or folder.
- It will output the specified file or folder's path and the current ("old") and copied ("new") timestamps.
- It will ask for confirmation before applying the changes.

**`Paste "Date Created"`**
- Applies only the copied Date Created timestamp to a specified file or folder.
- The rest of the logic is the same as for the `Paste` operation.

**`Paste "Date Modified"`**
- Applies only the copied Date Modified timestamp to a specified file or folder.
- The rest of the logic is the same as for the `Paste` operation.

**`Undo`**
- Restores the previous timestamps of the last modified file or folder.
- It is avaliable on all files and folders, but it will only restore the timestamps for the file or folder that was last used in the `Paste` (or `Undo`) operation.
- Each `Paste` operation, before overwriting timestamps with the previously copied ("new") ones, stores the specified file or folder's path and the current ("old") timestamps to an "undo-clipboard". (More details below.)
- The `Undo` itself then does the same as the `Paste` operation – it stores the restored file or folder's path and the current timestamps to a temporary location. If you again choose `Undo`, it will restore the timestamps back to the "new" values.
- That means if you choose `Undo` repeatedly, it will for the same file or folder rotate the timestamps between the "old" and "new" values.

#### Executables

The release folder contains two executables. They share a single copy of the runtime, which is why the folder has to stay together.

- **`tscp.exe`** – a console application. It handles installation and every operation run from a terminal or in Standalone Mode.
- **`tscpw.exe`** – the same operations with no console at all, used by Background Mode.

Two of them rather than one because whether a program gets a console window is fixed when it is built, not when it is run. A console program launched from Explorer has its window created before any of its own code runs, so a single executable could not avoid the flash. This is the same reason Windows ships both `python.exe` and `pythonw.exe`.

#### Modes

There are three modes, and each defines slightly different behavior. The mode is determined by the way the program is executed. There is a parameter for it, but it is not meant to be set by the user – the default is *Terminal*, and installation sets the mode for the context menu entries based on which install option you chose.

***`Terminal`***
- Running from an existing terminal uses that window to display output messages.
- It won't use *Pause* at the end of the operation, because the terminal will stay open anyway and you will see all messages.
- If `-q` option is used, it will suppress all output messages, but it will still show the confirmation prompt.
- If `-y` option is used, it will show output messages, but it will suppress the confirmation prompt and will automatically proceed with the operation as if the user has confirmed prompt.
- If both `-q` and `-y` options are used, it will suppress both output messages and the confirmation prompt and will automatically proceed with the operation as if the user has confirmed prompt.

***`Standalone`***
- This is the default mode for context menu integration.
- Each operation will run in a new terminal window.
- It will use *Pause* at the end of the operation to prevent automatically closing the window, so you can see the output messages.
- The window will close after pressing any key.
- No `-q` or `-y` options are used, so output messages and the confirmation prompt are shown.

***`Background`***
- This is an alternative option for context menu integration, and it runs `tscpw.exe`.
- It runs without a terminal window.
- There are no output messages or confirmation prompts, it will automatically proceed with the operation as if the user has confirmed prompt.
- If there were any errors, error message will be shown in a *MessageBox*.

#### Clipboard

As a "clipboard" two files in the `%LOCALAPPDATA%\TimestampCopy` folder are used.
- `clip` – stores the copied timestamps
- `clip-undo` – stores the data for the `Undo` operation

File contents are Base64 encoded to avoid manipulation to ensure that the data is stored in a consistent format.  
If the contents are not in the expected format, an error message is shown and the operation is aborted.

### Building from Source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```powershell
.\build.ps1
```

This publishes both executables into `publish\win-x64` as self-contained, trimmed, ReadyToRun binaries. Add `-Zip` to also produce the release archive.

### Screenshots

Copy  
![Copy](img/copy.png)

Paste  
![Copy](img/paste.png)

### Limitation

This tool is designed to work with **only one selected file or folder at a time**. While it does appear in the context menu when multiple items are selected, it will be executed **independently for each item**. This can lead to unexpected behavior. For accurate and predictable results, always use it with a single selection.

### Disclaimer

This tool is provided **as-is**, without any warranties or guarantees of fitness for a particular purpose. While it should work reliably in most cases, use it at your own risk.  

---

#### Old Versions

| Release | Source | Description |
| --- | --- | --- |
| [1.0.0](https://github.com/jurakovic/timestamp-copy/releases/tag/v1.0.0) | [1.0.0](https://github.com/jurakovic/timestamp-copy/tree/v1.0.0) | Initial [`tscp.sh`](https://github.com/jurakovic/timestamp-copy/blob/v1.0.0/tscp.sh) written in Bash. It was created solely for educational and experimental use. |
| [2.0.0-preview.1](https://github.com/jurakovic/timestamp-copy/releases/tag/v2.0.0-preview.1) | [2.0.0-preview.1](https://github.com/jurakovic/timestamp-copy/tree/v2.0.0-preview.1) | Direct port of the original Bash script into PowerShell, with only the minimal necessary changes made to ensure proper execution in a PowerShell environment. |
| [2.0.0](https://github.com/jurakovic/timestamp-copy/releases/tag/v2.0.0) | [2.0.0](https://github.com/jurakovic/timestamp-copy/tree/v2.0.0) | Complete rewrite of the original Bash script in native PowerShell syntax. |
| [2.1.0](https://github.com/jurakovic/timestamp-copy/releases/tag/v2.1.0) | [2.1.0](https://github.com/jurakovic/timestamp-copy/tree/v2.1.0) | Final PowerShell release. [`TimestampCopy.ps1`](https://github.com/jurakovic/timestamp-copy/blob/v2.1.0/TimestampCopy.ps1) added CLI parameters for all actions and the three script modes. Superseded by 3.0.0, which does the same work as a native executable and starts in about 60 ms instead of about 1300 ms. |

---

### [References](./REFERENCES.md)
