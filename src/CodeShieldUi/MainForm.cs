using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeShieldCore.Engine;
using CodeShieldCore.Logging;
using CodeShieldCore.Model;

namespace CodeShieldUi;

public partial class MainForm : Form
{
    private ObfuscationConfig _config = new();
    private string _lastSuccessfulOutput = string.Empty;

    public MainForm()
    {
        InitializeComponent();
        SetupEventHandlers();
        PopulateDefaults();
        UpdateCliAndMsBuildPreview();
    }

    private void SetupEventHandlers()
    {
        // Drag and Drop
        txtInput.DragEnter += Control_DragEnter;
        txtInput.DragDrop += Control_DragDrop;
        this.DragEnter += Control_DragEnter;
        this.DragDrop += Control_DragDrop;

        // Browsing
        btnBrowseInput.Click += BtnBrowseInput_Click;
        btnBrowseOutput.Click += BtnBrowseOutput_Click;
        btnBrowseMap.Click += BtnBrowseMap_Click;
        txtInput.TextChanged += TxtInput_TextChanged;

        // Configuration
        btnLoadConfig.Click += BtnLoadConfig_Click;
        btnSaveConfig.Click += BtnSaveConfig_Click;

        // Sync CLI Preview on control changes
        chkEnableRenaming.CheckedChanged += (s, e) => { grpRenaming.Enabled = chkEnableRenaming.Checked; UpdateCliAndMsBuildPreview(); };
        chkPreservePublic.CheckedChanged += (s, e) => UpdateCliAndMsBuildPreview();
        chkRenameTypes.CheckedChanged += (s, e) => UpdateCliAndMsBuildPreview();
        chkRenameMethods.CheckedChanged += (s, e) => UpdateCliAndMsBuildPreview();
        chkRenameFields.CheckedChanged += (s, e) => UpdateCliAndMsBuildPreview();
        chkRenameProperties.CheckedChanged += (s, e) => UpdateCliAndMsBuildPreview();
        chkRenameParameters.CheckedChanged += (s, e) => UpdateCliAndMsBuildPreview();
        cmbScheme.SelectedIndexChanged += (s, e) => UpdateCliAndMsBuildPreview();

        chkEnableStringEncryption.CheckedChanged += (s, e) => {
            numMinStringLen.Enabled = chkEnableStringEncryption.Checked;
            numKeySeed.Enabled = chkEnableStringEncryption.Checked;
            UpdateCliAndMsBuildPreview();
        };
        numMinStringLen.ValueChanged += (s, e) => UpdateCliAndMsBuildPreview();
        numKeySeed.ValueChanged += (s, e) => UpdateCliAndMsBuildPreview();
        chkGenerateMap.CheckedChanged += (s, e) => {
            txtMap.Enabled = chkGenerateMap.Checked;
            btnBrowseMap.Enabled = chkGenerateMap.Checked;
            UpdateCliAndMsBuildPreview();
        };

        // Exclusions
        btnAddExclusion.Click += BtnAddExclusion_Click;
        btnRemoveExclusion.Click += BtnRemoveExclusion_Click;

        // Copy buttons
        btnCopyCli.Click += (s, e) => CopyToClipboard(txtCliCommand.Text, "CLI Command copied to clipboard!");
        btnCopyMsBuild.Click += (s, e) => CopyToClipboard(txtMsBuild.Text, "MSBuild target snippet copied to clipboard!");

        // Action buttons
        btnObfuscate.Click += async (s, e) => await RunObfuscationAsync();
        btnTestAssembly.Click += BtnTestAssembly_Click;
        btnOpenFolder.Click += BtnOpenFolder_Click;
        btnClearLog.Click += (s, e) => rtbLog.Clear();
    }

    private void PopulateDefaults()
    {
        cmbScheme.SelectedIndex = 0; // ShortLetters
        cmbExclusionType.SelectedIndex = 0; // Namespace
        numMinStringLen.Value = 1;
        numKeySeed.Value = 0;
    }

    private void Control_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void Control_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files != null && files.Length > 0)
            {
                string file = files[0];
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext is ".dll" or ".exe")
                {
                    txtInput.Text = file;
                }
                else if (ext is ".json" or ".codeshield")
                {
                    LoadConfigFile(file);
                }
            }
        }
    }

    private void BtnBrowseInput_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select .NET Assembly",
            Filter = ".NET Assemblies (*.dll;*.exe)|*.dll;*.exe|All files (*.*)|*.*"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            txtInput.Text = ofd.FileName;
        }
    }

    private void BtnBrowseOutput_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Select Destination for Obfuscated Assembly",
            Filter = ".NET Assembly (*.dll;*.exe)|*.dll;*.exe|All files (*.*)|*.*"
        };
        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            txtOutput.Text = sfd.FileName;
        }
    }

    private void BtnBrowseMap_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Select Destination for Symbol Map File",
            Filter = "JSON Map File (*.json)|*.json|All files (*.*)|*.*"
        };
        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            txtMap.Text = sfd.FileName;
        }
    }

    private void TxtInput_TextChanged(object? sender, EventArgs e)
    {
        string input = txtInput.Text.Trim();
        if (File.Exists(input))
        {
            string dir = Path.GetDirectoryName(input) ?? "";
            string name = Path.GetFileNameWithoutExtension(input);
            string ext = Path.GetExtension(input);

            txtOutput.Text = Path.Combine(dir, $"{name}.Obfuscated{ext}");
            txtMap.Text = Path.Combine(dir, $"{name}.Obfuscated{ext}.map.json");

            try
            {
                var fi = new FileInfo(input);
                if (AppHostHelper.IsAppHostExecutable(input, out string managedDll))
                {
                    var dllFi = new FileInfo(managedDll);
                    lblAssemblyDetails.Text = $"Target: .NET AppHost Launcher ({Path.GetFileName(input)}) -> Managed Assembly: {Path.GetFileName(managedDll)} ({dllFi.Length / 1024:N0} KB)";
                    lblAssemblyDetails.ForeColor = Color.FromArgb(37, 99, 235);
                }
                else
                {
                    lblAssemblyDetails.Text = $"File: {Path.GetFileName(input)} | Size: {fi.Length / 1024:N0} KB | Directory: {dir}";
                    lblAssemblyDetails.ForeColor = Color.FromArgb(16, 185, 129);
                }
            }
            catch
            {
                lblAssemblyDetails.Text = "Assembly selected.";
            }
        }
        else
        {
            lblAssemblyDetails.Text = "Select or drag & drop a .NET assembly to begin.";
            lblAssemblyDetails.ForeColor = Color.FromArgb(71, 85, 105);
        }

        UpdateCliAndMsBuildPreview();
    }

    private void BtnLoadConfig_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Load CodeShield Configuration",
            Filter = "CodeShield Config (*.json;*.codeshield)|*.json;*.codeshield|All files (*.*)|*.*"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            LoadConfigFile(ofd.FileName);
        }
    }

    private void LoadConfigFile(string path)
    {
        try
        {
            _config = ObfuscationConfig.LoadFromFile(path);
            ApplyConfigToUi(_config);
            AppendLog(LogLevel.Info, $"Loaded configuration: {path}");
            UpdateCliAndMsBuildPreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSaveConfig_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Save CodeShield Configuration",
            Filter = "CodeShield Config (*.json)|*.json|All files (*.*)|*.*",
            FileName = "codeshield.json"
        };
        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                var cfg = BuildCurrentConfig();
                cfg.SaveToFile(sfd.FileName);
                AppendLog(LogLevel.Success, $"Saved configuration: {sfd.FileName}");
                MessageBox.Show("Configuration saved successfully!", "CodeShield", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ApplyConfigToUi(ObfuscationConfig cfg)
    {
        txtInput.Text = cfg.InputAssemblyPath;
        txtOutput.Text = cfg.OutputAssemblyPath;
        txtMap.Text = cfg.MapFilePath;
        chkGenerateMap.Checked = cfg.GenerateMapFile;

        chkEnableRenaming.Checked = cfg.Renaming.Enabled;
        chkPreservePublic.Checked = cfg.Renaming.PreservePublicApi;
        chkRenameTypes.Checked = cfg.Renaming.RenameTypes;
        chkRenameMethods.Checked = cfg.Renaming.RenameMethods;
        chkRenameFields.Checked = cfg.Renaming.RenameFields;
        chkRenameProperties.Checked = cfg.Renaming.RenameProperties;
        chkRenameParameters.Checked = cfg.Renaming.RenameParameters;
        cmbScheme.SelectedIndex = (int)cfg.Renaming.Scheme;

        chkEnableStringEncryption.Checked = cfg.StringEncryption.Enabled;
        numMinStringLen.Value = Math.Max(1, cfg.StringEncryption.MinStringLength);
        numKeySeed.Value = Math.Max(0, cfg.StringEncryption.KeySeed);

        lstExclusions.Items.Clear();
        foreach (var ns in cfg.Exclusions.ExcludeNamespaces)
        {
            lstExclusions.Items.Add($"[Namespace] {ns}");
        }
        foreach (var t in cfg.Exclusions.ExcludeTypes)
        {
            lstExclusions.Items.Add($"[Type] {t}");
        }
        foreach (var m in cfg.Exclusions.ExcludeMethods)
        {
            lstExclusions.Items.Add($"[Method] {m}");
        }
    }

    private ObfuscationConfig BuildCurrentConfig()
    {
        var cfg = new ObfuscationConfig
        {
            InputAssemblyPath = txtInput.Text.Trim(),
            OutputAssemblyPath = txtOutput.Text.Trim(),
            GenerateMapFile = chkGenerateMap.Checked,
            MapFilePath = txtMap.Text.Trim(),
            Renaming = new RenamingOptions
            {
                Enabled = chkEnableRenaming.Checked,
                PreservePublicApi = chkPreservePublic.Checked,
                RenameTypes = chkRenameTypes.Checked,
                RenameMethods = chkRenameMethods.Checked,
                RenameFields = chkRenameFields.Checked,
                RenameProperties = chkRenameProperties.Checked,
                RenameParameters = chkRenameParameters.Checked,
                Scheme = (RenamingScheme)Math.Max(0, cmbScheme.SelectedIndex)
            },
            StringEncryption = new StringEncryptionOptions
            {
                Enabled = chkEnableStringEncryption.Checked,
                MinStringLength = (int)numMinStringLen.Value,
                KeySeed = (int)numKeySeed.Value
            }
        };

        foreach (var item in lstExclusions.Items)
        {
            string str = item.ToString() ?? "";
            if (str.StartsWith("[Namespace] "))
            {
                cfg.Exclusions.ExcludeNamespaces.Add(str["[Namespace] ".Length..]);
            }
            else if (str.StartsWith("[Type] "))
            {
                cfg.Exclusions.ExcludeTypes.Add(str["[Type] ".Length..]);
            }
            else if (str.StartsWith("[Method] "))
            {
                cfg.Exclusions.ExcludeMethods.Add(str["[Method] ".Length..]);
            }
        }

        return cfg;
    }

    private void BtnAddExclusion_Click(object? sender, EventArgs e)
    {
        string text = txtNewExclusion.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        string type = cmbExclusionType.SelectedItem?.ToString() ?? "Namespace";
        string tag = type switch
        {
            "Type Name" => "[Type]",
            "Method Name" => "[Method]",
            _ => "[Namespace]"
        };

        string entry = $"{tag} {text}";
        if (!lstExclusions.Items.Contains(entry))
        {
            lstExclusions.Items.Add(entry);
            txtNewExclusion.Clear();
            UpdateCliAndMsBuildPreview();
        }
    }

    private void BtnRemoveExclusion_Click(object? sender, EventArgs e)
    {
        if (lstExclusions.SelectedIndex >= 0)
        {
            lstExclusions.Items.RemoveAt(lstExclusions.SelectedIndex);
            UpdateCliAndMsBuildPreview();
        }
    }

    private void UpdateCliAndMsBuildPreview()
    {
        string input = string.IsNullOrWhiteSpace(txtInput.Text) ? "MyApp.dll" : txtInput.Text.Trim();
        string output = string.IsNullOrWhiteSpace(txtOutput.Text) ? "MyApp.Obfuscated.dll" : txtOutput.Text.Trim();

        var cliArgs = new List<string>();
        cliArgs.Add($"codeshield -i \"{input}\" -o \"{output}\"");

        if (chkEnableRenaming.Checked)
        {
            string scheme = cmbScheme.SelectedIndex switch
            {
                1 => "confusing",
                2 => "hash",
                3 => "unicode",
                _ => "short"
            };
            cliArgs.Add($"--scheme {scheme}");
            if (!chkPreservePublic.Checked)
            {
                cliArgs.Add("--no-preserve-public");
            }
        }
        else
        {
            cliArgs.Add("--no-rename");
        }

        if (chkEnableStringEncryption.Checked)
        {
            cliArgs.Add("--encrypt-strings");
        }
        else
        {
            cliArgs.Add("--no-encrypt-strings");
        }

        if (!chkGenerateMap.Checked)
        {
            cliArgs.Add("--no-map");
        }

        foreach (var item in lstExclusions.Items)
        {
            string s = item.ToString() ?? "";
            if (s.StartsWith("[Type] "))
            {
                cliArgs.Add($"--exclude-type \"{s["[Type] ".Length..]}\"");
            }
            else if (s.StartsWith("[Namespace] "))
            {
                cliArgs.Add($"--exclude-ns \"{s["[Namespace] ".Length..]}\"");
            }
        }

        txtCliCommand.Text = string.Join(" ", cliArgs);

        txtMsBuild.Text = $@"<!-- Add to your .csproj for automated build-time obfuscation -->
<Target Name=""ObfuscateWithCodeShield"" AfterTargets=""AfterBuild"" Condition=""'$(Configuration)' == 'Release'"">
  <Message Importance=""High"" Text=""[CodeShield] Obfuscating $(TargetPath)..."" />
  <Exec Command=""codeshield -i &quot;$(TargetPath)&quot; -o &quot;$(TargetPath)&quot; {(chkEnableRenaming.Checked && !chkPreservePublic.Checked ? "--no-preserve-public " : "")}{(chkEnableStringEncryption.Checked ? "--encrypt-strings " : "")}""
        StandardOutputImportance=""Low""
        StandardErrorImportance=""High"" />
</Target>";
    }

    private void CopyToClipboard(string text, string successMessage)
    {
        if (!string.IsNullOrEmpty(text))
        {
            Clipboard.SetText(text);
            AppendLog(LogLevel.Info, successMessage);
            MessageBox.Show(successMessage, "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async Task RunObfuscationAsync()
    {
        var config = BuildCurrentConfig();
        if (string.IsNullOrWhiteSpace(config.InputAssemblyPath))
        {
            MessageBox.Show("Please select an input assembly to obfuscate.", "Missing Target", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(config.InputAssemblyPath))
        {
            MessageBox.Show($"Input assembly file does not exist:\n{config.InputAssemblyPath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        btnObfuscate.Enabled = false;
        btnTestAssembly.Enabled = false;
        btnOpenFolder.Enabled = false;
        progressBar.Style = ProgressBarStyle.Marquee;
        lblStatus.Text = "Obfuscating assembly in progress...";
        lblStatus.ForeColor = Color.FromArgb(37, 99, 235);

        var logger = new DelegateLogger((level, msg) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AppendLog(level, msg));
            }
            else
            {
                AppendLog(level, msg);
            }
        });

        try
        {
            var engine = new ObfuscatorEngine();
            var result = await Task.Run(() => engine.Execute(config, logger));

            progressBar.Style = ProgressBarStyle.Blocks;
            if (result.Success)
            {
                progressBar.Value = 100;
                lblStatus.Text = $"Completed successfully in {result.Elapsed.TotalMilliseconds:F0}ms!";
                lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                _lastSuccessfulOutput = result.OutputPath;
                btnTestAssembly.Enabled = true;
                btnOpenFolder.Enabled = true;

                MessageBox.Show(
                    $"Assembly obfuscated successfully!\n\n" +
                    $"Output: {Path.GetFileName(result.OutputPath)}\n" +
                    $"Symbols Renamed: {result.TotalSymbolsRenamed}\n" +
                    $"Strings Encrypted: {result.StringsEncrypted}\n" +
                    $"Elapsed: {result.Elapsed.TotalMilliseconds:F0}ms",
                    "CodeShield Protection Succeeded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                progressBar.Value = 0;
                lblStatus.Text = "Obfuscation failed.";
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show($"Obfuscation failed: {result.ErrorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;
            lblStatus.Text = $"Error: {ex.Message}";
            lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
            AppendLog(LogLevel.Error, $"Exception: {ex.Message}");
        }
        finally
        {
            btnObfuscate.Enabled = true;
        }
    }

    private void BtnTestAssembly_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastSuccessfulOutput) || !File.Exists(_lastSuccessfulOutput))
        {
            MessageBox.Show("Obfuscated assembly not found.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            bool isExe = Path.GetExtension(_lastSuccessfulOutput).Equals(".exe", StringComparison.OrdinalIgnoreCase);
            string workingDir = Path.GetDirectoryName(_lastSuccessfulOutput) ?? "";

            if (isExe)
            {
                AppendLog(LogLevel.Info, $"[RUN] Launching test executable: \"{_lastSuccessfulOutput}\"...");
                var psi = new ProcessStartInfo(_lastSuccessfulOutput)
                {
                    WorkingDirectory = workingDir,
                    UseShellExecute = true
                };

                using var proc = Process.Start(psi);
                AppendLog(LogLevel.Success, $"Executable launched successfully (PID: {proc?.Id}).");
            }
            else
            {
                AppendLog(LogLevel.Info, $"[RUN] Launching test process: dotnet \"{_lastSuccessfulOutput}\"...");
                var psi = new ProcessStartInfo("dotnet", $"\"{_lastSuccessfulOutput}\"")
                {
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();

                    AppendLog(LogLevel.Info, "--- Test Process Output ---");
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        AppendLog(LogLevel.Success, stdout);
                    }
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        AppendLog(LogLevel.Error, stderr);
                    }
                    AppendLog(LogLevel.Info, $"--- Process Exited with Code: {proc.ExitCode} ---");

                    if (proc.ExitCode == 0)
                    {
                        MessageBox.Show($"Obfuscated assembly executed successfully with exit code 0!\n\nOutput:\n{stdout}", "Test Verification Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Process exited with code {proc.ExitCode}.\n\nStderr:\n{stderr}", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AppendLog(LogLevel.Error, $"Failed to launch test process: {ex.Message}");
            MessageBox.Show($"Failed to launch test: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        string target = !string.IsNullOrEmpty(_lastSuccessfulOutput) && File.Exists(_lastSuccessfulOutput)
            ? _lastSuccessfulOutput
            : txtOutput.Text.Trim();

        if (File.Exists(target))
        {
            Process.Start("explorer.exe", $"/select,\"{target}\"");
        }
        else
        {
            string dir = Path.GetDirectoryName(target) ?? "";
            if (Directory.Exists(dir))
            {
                Process.Start("explorer.exe", $"\"{dir}\"");
            }
        }
    }

    private void AppendLog(LogLevel level, string message)
    {
        Color color = level switch
        {
            LogLevel.Debug => Color.FromArgb(148, 163, 184),
            LogLevel.Info => Color.FromArgb(56, 189, 248),
            LogLevel.Success => Color.FromArgb(74, 222, 128),
            LogLevel.Warning => Color.FromArgb(250, 204, 21),
            LogLevel.Error => Color.FromArgb(248, 113, 113),
            _ => Color.FromArgb(226, 232, 240)
        };

        rtbLog.SelectionStart = rtbLog.TextLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionColor = color;
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        rtbLog.SelectionColor = rtbLog.ForeColor;
        rtbLog.ScrollToCaret();
    }
}
