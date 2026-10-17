# 🛡️ CodeShield - .NET Obfuscation & Build Protection Tool

**CodeShield** is a modern, modular .NET code obfuscation tool featuring both an interactive **Windows Forms GUI** and a lightweight, headless **Command-Line Interface (CLI)** designed for build scripts, MSBuild post-build tasks, and CI/CD pipelines (GitHub Actions, Azure DevOps, GitLab CI, etc.).

---

## 🚀 Key Features

- **Symbol Renaming Protection:**
  - Renames Types (classes, structs, interfaces, enums), Methods, Fields, Properties, and Parameters.
  - Supports multiple naming schemes:
    - `ShortLetters`: Compact sequential names (`a`, `b`, ..., `aa`) minimizing binary size.
    - `Confusing`: Visually deceptive combinations of `I`, `l`, `1`, `O`, `0`, and `_`.
    - `RandomHash`: Randomized hexadecimal tokens (`_0x4a1f...`).
    - `UnreadableUnicode`: Invisible zero-width unicode characters breaking basic decompilers.
  - Smart preservation of entry points (`Main`), constructors (`.ctor`, `.cctor`), special runtime methods, and overrides.
  - Configurable **Public API preservation** (essential for class libraries).

- **String Literal Encryption:**
  - Detects all literal strings in IL method bodies (`ldstr`).
  - Scrambles them using dynamic symmetric XOR encryption keys.
  - Injects a self-contained, zero-dependency static decrypter helper method into the assembly module.
  - Plaintext strings are completely eliminated from IL decompilers and hex inspectors.

- **Exclusion System:**
  - Exclude specific namespaces, types, or methods with wildcard patterns (e.g., `MyApp.PublicContracts.*`, `*Controller`).
  - Regex pattern matching support.

- **Symbol Mapping Export:**
  - Automatically emits a `.map.json` file mapping original symbol signatures to obfuscated names for stack trace de-obfuscation and diagnostics.

- **Dual Interfaces:**
  - **Windows Forms UI (`CodeShieldUi`)**: Modern desktop app with drag-and-drop assembly loading, real-time logging console, configuration save/load, and instant "Copy CLI Command" / "Copy MSBuild Snippet" generators.
  - **CLI Tool (`CodeShieldCli`)**: Headless console runner with standard exit codes (`0` for success, non-zero on failure), configurable flags, and JSON configuration support.

---

## 📂 Project Architecture

```
c:\TT\CodeShield\
├── CodeShield.slnx                    # Modern .NET solution file
├── CodeShield.targets                 # MSBuild targets file for automated build integration
├── build-and-obfuscate.ps1            # Automated build script example
├── src/
│   ├── CodeShieldCore/               # Reusable obfuscation engine & IL transforms (Mono.Cecil)
│   │   ├── Model/                     # Configuration, options, mapping, and result models
│   │   ├── Naming/                    # Naming generators (ShortLetters, Confusing, Hash, Unicode)
│   │   ├── Protections/               # RenamingProtection & StringEncryptionProtection
│   │   ├── Logging/                   # Console, Delegate, and Event loggers
│   │   └── Engine/                    # ObfuscatorEngine assembly loader & pipeline coordinator
│   ├── CodeShieldCli/                # Command-line interface for build scripts & CI/CD
│   └── CodeShieldUi/                 # Windows Forms desktop application
├── samples/
│   └── CodeShieldSampleApp/          # Verification app with confidential strings and logic
└── tests/
    └── CodeShieldTests/              # Automated unit and integration test suite
```

---

## 🖥️ Running the Windows Forms UI

To launch the desktop GUI:

```powershell
dotnet run --project src/CodeShieldUi/CodeShieldUi.csproj
```

### Features in the GUI:
1. **Drag & Drop:** Drag any compiled `.dll` or `.exe` directly into the window.
2. **Preset & Configs:** Save and load project presets (`.codeshield` or `.json`).
3. **Interactive Protections:** Toggle renaming, configure schemes, adjust string encryption settings, and add exclusions.
4. **Copy CLI / MSBuild:** Generate the exact command line or MSBuild snippet to copy directly into your repository.
5. **Real-time Log & Test:** View real-time color-coded obfuscation progress and click **"Test Obfuscated App"** to run and verify the binary immediately!

---

## ⚡ Using the CLI in Build Scripts

### 1. Basic CLI Usage

```powershell
# Basic obfuscation (renaming + string encryption)
codeshield -i path/to/MyApp.dll -o path/to/MyApp.Obfuscated.dll

# Standalone executable (obfuscates all symbols, including public ones)
codeshield -i MyApp.dll -o MyApp.Protected.dll --no-preserve-public --scheme confusing --encrypt-strings

# Using a saved configuration file
codeshield -c codeshield.json

# Using a config file with CLI overrides
codeshield -c codeshield.json -i bin/Release/net10.0/MyApp.dll -o dist/MyApp.dll
```

### 2. CLI Options Reference

| Flag | Description | Default |
|---|---|---|
| `-i, --input <file>` | Path to input assembly (`.dll` / `.exe`) | *Required* |
| `-o, --output <file>` | Path for obfuscated assembly | `[input].Obfuscated.dll` |
| `-c, --config <file>` | Load settings from `.codeshield` or `.json` file | None |
| `-m, --map <file>` | Path for symbol mapping file | `[output].map.json` |
| `--no-map` | Disable symbol mapping generation | Mapping enabled |
| `--rename` / `--no-rename` | Enable / disable identifier renaming | Enabled |
| `--scheme <name>` | `short` \| `confusing` \| `hash` \| `unicode` | `short` |
| `--preserve-public` | Preserve public classes & members (libraries) | Enabled |
| `--no-preserve-public` | Obfuscate public members too (apps/exes) | Disabled |
| `--encrypt-strings` | Enable string literal encryption | Enabled |
| `--no-encrypt-strings` | Disable string literal encryption | Disabled |
| `--exclude-type <name>` | Exclude type or wildcard pattern (repeatable) | None |
| `--exclude-ns <namespace>` | Exclude namespace or wildcard (repeatable) | None |
| `--init [file]` | Generate a starter configuration template | `codeshield.json` |
| `-v, --verbose` | Verbose diagnostic logging | Normal |
| `-q, --quiet` | Minimal console output for clean CI logs | Normal |

### 3. Exit Codes for CI/CD

- `0`: Success (build script proceeds).
- `1`: Invalid arguments or input file not found.
- `2`: Obfuscation engine error.

---

## 🛠️ Automated MSBuild Integration

You can automate CodeShield directly inside your `.csproj` so that it runs every time you build in `Release` configuration:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <!-- Run CodeShield automatically after compilation in Release builds -->
  <Target Name="ObfuscateWithCodeShield" AfterTargets="AfterBuild" Condition="'$(Configuration)' == 'Release'">
    <Message Importance="High" Text="[CodeShield] Obfuscating $(TargetPath)..." />
    <Exec Command="codeshield -i &quot;$(TargetPath)&quot; -o &quot;$(TargetPath)&quot; --encrypt-strings --no-preserve-public"
          StandardOutputImportance="Low"
          StandardErrorImportance="High" />
    <Message Importance="High" Text="[CodeShield] Obfuscation complete!" />
  </Target>
</Project>
```

Alternatively, simply import [`CodeShield.targets`](CodeShield.targets):

```xml
<Import Project="path\to\CodeShield.targets" Condition="Exists('path\to\CodeShield.targets')" />
```

---

## 📋 JSON Configuration File (`codeshield.json`)

Generate a starter template:
```powershell
codeshield --init
```

Example `codeshield.json`:
```json
{
  "inputAssemblyPath": "bin/Release/net10.0/MyProject.dll",
  "outputAssemblyPath": "bin/Release/net10.0/MyProject.Obfuscated.dll",
  "generateMapFile": true,
  "mapFilePath": "bin/Release/net10.0/MyProject.map.json",
  "renaming": {
    "enabled": true,
    "scheme": "Confusing",
    "preservePublicApi": false,
    "renameTypes": true,
    "renameMethods": true,
    "renameFields": true,
    "renameProperties": true,
    "renameParameters": true
  },
  "stringEncryption": {
    "enabled": true,
    "keySeed": 0,
    "minStringLength": 1
  },
  "exclusions": {
    "excludeNamespaces": [
      "MyProject.Contracts.*"
    ],
    "excludeTypes": [
      "KeepThisClass",
      "*Controller"
    ],
    "excludeMethods": [
      "SpecialHookMethod"
    ]
  }
}
```

---

## 🧪 Testing and Verification

Run the automated test suite:
```powershell
dotnet test
```

Run the end-to-end sample build & obfuscation script:
```powershell
pwsh -File ./build-and-obfuscate.ps1
```
"# CodeShield" 
