using System.Drawing;
using System.Windows.Forms;

namespace CodeShieldUi;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel panelHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private Button btnLoadConfig;
    private Button btnSaveConfig;

    private TabControl tabControlMain;
    private TabPage tabFiles;
    private TabPage tabProtections;
    private TabPage tabExclusions;
    private TabPage tabCiCd;

    // Files Tab
    private GroupBox grpFiles;
    private Label lblInput;
    private TextBox txtInput;
    private Button btnBrowseInput;
    private Label lblOutput;
    private TextBox txtOutput;
    private Button btnBrowseOutput;
    private CheckBox chkGenerateMap;
    private Label lblMap;
    private TextBox txtMap;
    private Button btnBrowseMap;
    private Label lblAssemblyDetails;

    // Protections Tab
    private GroupBox grpRenaming;
    private CheckBox chkEnableRenaming;
    private CheckBox chkPreservePublic;
    private CheckBox chkRenameTypes;
    private CheckBox chkRenameMethods;
    private CheckBox chkRenameFields;
    private CheckBox chkRenameProperties;
    private CheckBox chkRenameParameters;
    private Label lblScheme;
    private ComboBox cmbScheme;

    private GroupBox grpStrings;
    private CheckBox chkEnableStringEncryption;
    private Label lblMinStringLen;
    private NumericUpDown numMinStringLen;
    private Label lblKeySeed;
    private NumericUpDown numKeySeed;
    private Label lblStringEncNote;

    // Exclusions Tab
    private GroupBox grpExclusions;
    private ListBox lstExclusions;
    private ComboBox cmbExclusionType;
    private TextBox txtNewExclusion;
    private Button btnAddExclusion;
    private Button btnRemoveExclusion;
    private Label lblExclusionHelp;

    // CI/CD Tab
    private GroupBox grpCliCommand;
    private TextBox txtCliCommand;
    private Button btnCopyCli;
    private GroupBox grpMsBuild;
    private TextBox txtMsBuild;
    private Button btnCopyMsBuild;

    // Bottom Action Bar & Logs
    private Panel panelBottom;
    private Button btnObfuscate;
    private Button btnTestAssembly;
    private Button btnOpenFolder;
    private ProgressBar progressBar;
    private Label lblStatus;
    private RichTextBox rtbLog;
    private Button btnClearLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        panelHeader = new Panel();
        lblTitle = new Label();
        lblSubtitle = new Label();
        btnLoadConfig = new Button();
        btnSaveConfig = new Button();

        tabControlMain = new TabControl();
        tabFiles = new TabPage();
        tabProtections = new TabPage();
        tabExclusions = new TabPage();
        tabCiCd = new TabPage();

        // Files Tab
        grpFiles = new GroupBox();
        lblInput = new Label();
        txtInput = new TextBox();
        btnBrowseInput = new Button();
        lblOutput = new Label();
        txtOutput = new TextBox();
        btnBrowseOutput = new Button();
        chkGenerateMap = new CheckBox();
        lblMap = new Label();
        txtMap = new TextBox();
        btnBrowseMap = new Button();
        lblAssemblyDetails = new Label();

        // Protections Tab
        grpRenaming = new GroupBox();
        chkEnableRenaming = new CheckBox();
        chkPreservePublic = new CheckBox();
        chkRenameTypes = new CheckBox();
        chkRenameMethods = new CheckBox();
        chkRenameFields = new CheckBox();
        chkRenameProperties = new CheckBox();
        chkRenameParameters = new CheckBox();
        lblScheme = new Label();
        cmbScheme = new ComboBox();

        grpStrings = new GroupBox();
        chkEnableStringEncryption = new CheckBox();
        lblMinStringLen = new Label();
        numMinStringLen = new NumericUpDown();
        lblKeySeed = new Label();
        numKeySeed = new NumericUpDown();
        lblStringEncNote = new Label();

        // Exclusions Tab
        grpExclusions = new GroupBox();
        lstExclusions = new ListBox();
        cmbExclusionType = new ComboBox();
        txtNewExclusion = new TextBox();
        btnAddExclusion = new Button();
        btnRemoveExclusion = new Button();
        lblExclusionHelp = new Label();

        // CI/CD Tab
        grpCliCommand = new GroupBox();
        txtCliCommand = new TextBox();
        btnCopyCli = new Button();
        grpMsBuild = new GroupBox();
        txtMsBuild = new TextBox();
        btnCopyMsBuild = new Button();

        // Bottom Bar & Log
        panelBottom = new Panel();
        btnObfuscate = new Button();
        btnTestAssembly = new Button();
        btnOpenFolder = new Button();
        progressBar = new ProgressBar();
        lblStatus = new Label();
        rtbLog = new RichTextBox();
        btnClearLog = new Button();

        panelHeader.SuspendLayout();
        tabControlMain.SuspendLayout();
        tabFiles.SuspendLayout();
        tabProtections.SuspendLayout();
        tabExclusions.SuspendLayout();
        tabCiCd.SuspendLayout();
        grpFiles.SuspendLayout();
        grpRenaming.SuspendLayout();
        grpStrings.SuspendLayout();
        grpExclusions.SuspendLayout();
        grpCliCommand.SuspendLayout();
        grpMsBuild.SuspendLayout();
        panelBottom.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numMinStringLen).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numKeySeed).BeginInit();
        SuspendLayout();

        // 
        // panelHeader
        // 
        panelHeader.BackColor = Color.FromArgb(30, 41, 59);
        panelHeader.Controls.Add(btnSaveConfig);
        panelHeader.Controls.Add(btnLoadConfig);
        panelHeader.Controls.Add(lblSubtitle);
        panelHeader.Controls.Add(lblTitle);
        panelHeader.Dock = DockStyle.Top;
        panelHeader.Location = new Point(0, 0);
        panelHeader.Name = "panelHeader";
        panelHeader.Size = new Size(984, 75);
        panelHeader.TabIndex = 0;

        // lblTitle
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(20, 12);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(168, 30);
        lblTitle.Text = "🛡️ CodeShield";

        // lblSubtitle
        lblSubtitle.AutoSize = true;
        lblSubtitle.Font = new Font("Segoe UI", 9.5F);
        lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblSubtitle.Location = new Point(24, 44);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(350, 17);
        lblSubtitle.Text = ".NET Assembly Obfuscation & Build Protection Engine";

        // btnLoadConfig
        btnLoadConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLoadConfig.BackColor = Color.FromArgb(51, 65, 85);
        btnLoadConfig.FlatAppearance.BorderSize = 0;
        btnLoadConfig.FlatStyle = FlatStyle.Flat;
        btnLoadConfig.ForeColor = Color.White;
        btnLoadConfig.Location = new Point(710, 20);
        btnLoadConfig.Name = "btnLoadConfig";
        btnLoadConfig.Size = new Size(125, 34);
        btnLoadConfig.TabIndex = 1;
        btnLoadConfig.Text = "📁 Load Config...";
        btnLoadConfig.UseVisualStyleBackColor = false;

        // btnSaveConfig
        btnSaveConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSaveConfig.BackColor = Color.FromArgb(51, 65, 85);
        btnSaveConfig.FlatAppearance.BorderSize = 0;
        btnSaveConfig.FlatStyle = FlatStyle.Flat;
        btnSaveConfig.ForeColor = Color.White;
        btnSaveConfig.Location = new Point(845, 20);
        btnSaveConfig.Name = "btnSaveConfig";
        btnSaveConfig.Size = new Size(125, 34);
        btnSaveConfig.TabIndex = 2;
        btnSaveConfig.Text = "💾 Save Config...";
        btnSaveConfig.UseVisualStyleBackColor = false;

        // 
        // tabControlMain
        // 
        tabControlMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tabControlMain.Controls.Add(tabFiles);
        tabControlMain.Controls.Add(tabProtections);
        tabControlMain.Controls.Add(tabExclusions);
        tabControlMain.Controls.Add(tabCiCd);
        tabControlMain.Font = new Font("Segoe UI", 9.5F);
        tabControlMain.Location = new Point(12, 85);
        tabControlMain.Name = "tabControlMain";
        tabControlMain.SelectedIndex = 0;
        tabControlMain.Size = new Size(960, 280);
        tabControlMain.TabIndex = 1;

        // 
        // tabFiles
        // 
        tabFiles.Controls.Add(grpFiles);
        tabFiles.Location = new Point(4, 25);
        tabFiles.Name = "tabFiles";
        tabFiles.Padding = new Padding(10);
        tabFiles.Size = new Size(952, 251);
        tabFiles.Text = "  📁 Assembly Files  ";
        tabFiles.UseVisualStyleBackColor = true;

        // grpFiles
        grpFiles.Controls.Add(lblAssemblyDetails);
        grpFiles.Controls.Add(btnBrowseMap);
        grpFiles.Controls.Add(txtMap);
        grpFiles.Controls.Add(lblMap);
        grpFiles.Controls.Add(chkGenerateMap);
        grpFiles.Controls.Add(btnBrowseOutput);
        grpFiles.Controls.Add(txtOutput);
        grpFiles.Controls.Add(lblOutput);
        grpFiles.Controls.Add(btnBrowseInput);
        grpFiles.Controls.Add(txtInput);
        grpFiles.Controls.Add(lblInput);
        grpFiles.Dock = DockStyle.Fill;
        grpFiles.Location = new Point(10, 10);
        grpFiles.Name = "grpFiles";
        grpFiles.Size = new Size(932, 231);
        grpFiles.TabIndex = 0;
        grpFiles.TabStop = false;
        grpFiles.Text = "Target Assembly & Output Paths (Supports Drag and Drop)";

        // lblInput
        lblInput.AutoSize = true;
        lblInput.Location = new Point(16, 32);
        lblInput.Name = "lblInput";
        lblInput.Size = new Size(130, 17);
        lblInput.Text = "Input Assembly (.dll/.exe):";

        // txtInput
        txtInput.AllowDrop = true;
        txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtInput.Location = new Point(155, 29);
        txtInput.Name = "txtInput";
        txtInput.Size = new Size(660, 24);
        txtInput.TabIndex = 0;

        // btnBrowseInput
        btnBrowseInput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseInput.Location = new Point(825, 27);
        btnBrowseInput.Name = "btnBrowseInput";
        btnBrowseInput.Size = new Size(90, 28);
        btnBrowseInput.TabIndex = 1;
        btnBrowseInput.Text = "Browse...";

        // lblOutput
        lblOutput.AutoSize = true;
        lblOutput.Location = new Point(16, 72);
        lblOutput.Name = "lblOutput";
        lblOutput.Size = new Size(136, 17);
        lblOutput.Text = "Output Obfuscated Path:";

        // txtOutput
        txtOutput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtOutput.Location = new Point(155, 69);
        txtOutput.Name = "txtOutput";
        txtOutput.Size = new Size(660, 24);
        txtOutput.TabIndex = 2;

        // btnBrowseOutput
        btnBrowseOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseOutput.Location = new Point(825, 67);
        btnBrowseOutput.Name = "btnBrowseOutput";
        btnBrowseOutput.Size = new Size(90, 28);
        btnBrowseOutput.TabIndex = 3;
        btnBrowseOutput.Text = "Browse...";

        // chkGenerateMap
        chkGenerateMap.AutoSize = true;
        chkGenerateMap.Checked = true;
        chkGenerateMap.CheckState = CheckState.Checked;
        chkGenerateMap.Location = new Point(155, 110);
        chkGenerateMap.Name = "chkGenerateMap";
        chkGenerateMap.Size = new Size(244, 21);
        chkGenerateMap.TabIndex = 4;
        chkGenerateMap.Text = "Generate Symbol Mapping File (.json)";

        // lblMap
        lblMap.AutoSize = true;
        lblMap.Location = new Point(16, 142);
        lblMap.Name = "lblMap";
        lblMap.Size = new Size(90, 17);
        lblMap.Text = "Map File Path:";

        // txtMap
        txtMap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtMap.Location = new Point(155, 139);
        txtMap.Name = "txtMap";
        txtMap.Size = new Size(660, 24);
        txtMap.TabIndex = 5;

        // btnBrowseMap
        btnBrowseMap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseMap.Location = new Point(825, 137);
        btnBrowseMap.Name = "btnBrowseMap";
        btnBrowseMap.Size = new Size(90, 28);
        btnBrowseMap.TabIndex = 6;
        btnBrowseMap.Text = "Browse...";

        // lblAssemblyDetails
        lblAssemblyDetails.AutoSize = true;
        lblAssemblyDetails.ForeColor = Color.FromArgb(71, 85, 105);
        lblAssemblyDetails.Location = new Point(155, 185);
        lblAssemblyDetails.Name = "lblAssemblyDetails";
        lblAssemblyDetails.Size = new Size(310, 17);
        lblAssemblyDetails.Text = "Select or drag & drop a .NET assembly to begin.";

        // 
        // tabProtections
        // 
        tabProtections.Controls.Add(grpStrings);
        tabProtections.Controls.Add(grpRenaming);
        tabProtections.Location = new Point(4, 25);
        tabProtections.Name = "tabProtections";
        tabProtections.Padding = new Padding(10);
        tabProtections.Size = new Size(952, 251);
        tabProtections.Text = "  🔒 Protections  ";
        tabProtections.UseVisualStyleBackColor = true;

        // grpRenaming
        grpRenaming.Controls.Add(cmbScheme);
        grpRenaming.Controls.Add(lblScheme);
        grpRenaming.Controls.Add(chkRenameParameters);
        grpRenaming.Controls.Add(chkRenameProperties);
        grpRenaming.Controls.Add(chkRenameFields);
        grpRenaming.Controls.Add(chkRenameMethods);
        grpRenaming.Controls.Add(chkRenameTypes);
        grpRenaming.Controls.Add(chkPreservePublic);
        grpRenaming.Controls.Add(chkEnableRenaming);
        grpRenaming.Location = new Point(10, 10);
        grpRenaming.Name = "grpRenaming";
        grpRenaming.Size = new Size(465, 231);
        grpRenaming.TabIndex = 0;
        grpRenaming.TabStop = false;
        grpRenaming.Text = "Identifier Renaming";

        // chkEnableRenaming
        chkEnableRenaming.AutoSize = true;
        chkEnableRenaming.Checked = true;
        chkEnableRenaming.CheckState = CheckState.Checked;
        chkEnableRenaming.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        chkEnableRenaming.Location = new Point(16, 26);
        chkEnableRenaming.Name = "chkEnableRenaming";
        chkEnableRenaming.Size = new Size(187, 21);
        chkEnableRenaming.TabIndex = 0;
        chkEnableRenaming.Text = "Enable Symbol Renaming";

        // chkPreservePublic
        chkPreservePublic.AutoSize = true;
        chkPreservePublic.Checked = true;
        chkPreservePublic.CheckState = CheckState.Checked;
        chkPreservePublic.ForeColor = Color.FromArgb(30, 64, 175);
        chkPreservePublic.Location = new Point(16, 54);
        chkPreservePublic.Name = "chkPreservePublic";
        chkPreservePublic.Size = new Size(244, 21);
        chkPreservePublic.TabIndex = 1;
        chkPreservePublic.Text = "Preserve Public API (for class libraries)";

        // sub checkboxes
        chkRenameTypes.AutoSize = true;
        chkRenameTypes.Checked = true;
        chkRenameTypes.CheckState = CheckState.Checked;
        chkRenameTypes.Location = new Point(36, 82);
        chkRenameTypes.Name = "chkRenameTypes";
        chkRenameTypes.Size = new Size(111, 21);
        chkRenameTypes.TabIndex = 2;
        chkRenameTypes.Text = "Rename Types";

        chkRenameMethods.AutoSize = true;
        chkRenameMethods.Checked = true;
        chkRenameMethods.CheckState = CheckState.Checked;
        chkRenameMethods.Location = new Point(175, 82);
        chkRenameMethods.Name = "chkRenameMethods";
        chkRenameMethods.Size = new Size(128, 21);
        chkRenameMethods.TabIndex = 3;
        chkRenameMethods.Text = "Rename Methods";

        chkRenameFields.AutoSize = true;
        chkRenameFields.Checked = true;
        chkRenameFields.CheckState = CheckState.Checked;
        chkRenameFields.Location = new Point(36, 110);
        chkRenameFields.Name = "chkRenameFields";
        chkRenameFields.Size = new Size(108, 21);
        chkRenameFields.TabIndex = 4;
        chkRenameFields.Text = "Rename Fields";

        chkRenameProperties.AutoSize = true;
        chkRenameProperties.Checked = true;
        chkRenameProperties.CheckState = CheckState.Checked;
        chkRenameProperties.Location = new Point(175, 110);
        chkRenameProperties.Name = "chkRenameProperties";
        chkRenameProperties.Size = new Size(137, 21);
        chkRenameProperties.TabIndex = 5;
        chkRenameProperties.Text = "Rename Properties";

        chkRenameParameters.AutoSize = true;
        chkRenameParameters.Checked = true;
        chkRenameParameters.CheckState = CheckState.Checked;
        chkRenameParameters.Location = new Point(36, 138);
        chkRenameParameters.Name = "chkRenameParameters";
        chkRenameParameters.Size = new Size(142, 21);
        chkRenameParameters.TabIndex = 6;
        chkRenameParameters.Text = "Rename Parameters";

        // lblScheme
        lblScheme.AutoSize = true;
        lblScheme.Location = new Point(16, 180);
        lblScheme.Name = "lblScheme";
        lblScheme.Size = new Size(110, 17);
        lblScheme.Text = "Renaming Scheme:";

        // cmbScheme
        cmbScheme.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbScheme.FormattingEnabled = true;
        cmbScheme.Items.AddRange(new object[] {
            "ShortLetters (a, b, c...)",
            "Confusing (I, l, 1, O, 0, _)",
            "RandomHash (_0x4f1a...)",
            "UnreadableUnicode (\\u200B...)"
        });
        cmbScheme.Location = new Point(135, 176);
        cmbScheme.Name = "cmbScheme";
        cmbScheme.Size = new Size(240, 24);
        cmbScheme.TabIndex = 7;

        // grpStrings
        grpStrings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpStrings.Controls.Add(lblStringEncNote);
        grpStrings.Controls.Add(numKeySeed);
        grpStrings.Controls.Add(lblKeySeed);
        grpStrings.Controls.Add(numMinStringLen);
        grpStrings.Controls.Add(lblMinStringLen);
        grpStrings.Controls.Add(chkEnableStringEncryption);
        grpStrings.Location = new Point(485, 10);
        grpStrings.Name = "grpStrings";
        grpStrings.Size = new Size(457, 231);
        grpStrings.TabIndex = 1;
        grpStrings.TabStop = false;
        grpStrings.Text = "String Literal Encryption";

        // chkEnableStringEncryption
        chkEnableStringEncryption.AutoSize = true;
        chkEnableStringEncryption.Checked = true;
        chkEnableStringEncryption.CheckState = CheckState.Checked;
        chkEnableStringEncryption.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        chkEnableStringEncryption.Location = new Point(16, 26);
        chkEnableStringEncryption.Name = "chkEnableStringEncryption";
        chkEnableStringEncryption.Size = new Size(226, 21);
        chkEnableStringEncryption.TabIndex = 0;
        chkEnableStringEncryption.Text = "Enable String Literal Encryption";

        // lblMinStringLen
        lblMinStringLen.AutoSize = true;
        lblMinStringLen.Location = new Point(16, 68);
        lblMinStringLen.Name = "lblMinStringLen";
        lblMinStringLen.Size = new Size(157, 17);
        lblMinStringLen.Text = "Minimum String Length:";

        // numMinStringLen
        numMinStringLen.Location = new Point(185, 65);
        numMinStringLen.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        numMinStringLen.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numMinStringLen.Name = "numMinStringLen";
        numMinStringLen.Size = new Size(80, 24);
        numMinStringLen.TabIndex = 1;
        numMinStringLen.Value = new decimal(new int[] { 1, 0, 0, 0 });

        // lblKeySeed
        lblKeySeed.AutoSize = true;
        lblKeySeed.Location = new Point(16, 110);
        lblKeySeed.Name = "lblKeySeed";
        lblKeySeed.Size = new Size(155, 17);
        lblKeySeed.Text = "Key Seed (0 for random):";

        // numKeySeed
        numKeySeed.Location = new Point(185, 107);
        numKeySeed.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        numKeySeed.Name = "numKeySeed";
        numKeySeed.Size = new Size(120, 24);
        numKeySeed.TabIndex = 2;

        // lblStringEncNote
        lblStringEncNote.ForeColor = Color.FromArgb(71, 85, 105);
        lblStringEncNote.Location = new Point(16, 150);
        lblStringEncNote.Name = "lblStringEncNote";
        lblStringEncNote.Size = new Size(420, 65);
        lblStringEncNote.Text = "Replaces plaintext string literals with encrypted payloads and injects an isolated, zero-dependency static runtime decryptor method directly into the target assembly.";

        // 
        // tabExclusions
        // 
        tabExclusions.Controls.Add(grpExclusions);
        tabExclusions.Location = new Point(4, 25);
        tabExclusions.Name = "tabExclusions";
        tabExclusions.Padding = new Padding(10);
        tabExclusions.Size = new Size(952, 251);
        tabExclusions.Text = "  🚫 Exclusions & Rules  ";
        tabExclusions.UseVisualStyleBackColor = true;

        // grpExclusions
        grpExclusions.Controls.Add(lblExclusionHelp);
        grpExclusions.Controls.Add(btnRemoveExclusion);
        grpExclusions.Controls.Add(btnAddExclusion);
        grpExclusions.Controls.Add(txtNewExclusion);
        grpExclusions.Controls.Add(cmbExclusionType);
        grpExclusions.Controls.Add(lstExclusions);
        grpExclusions.Dock = DockStyle.Fill;
        grpExclusions.Location = new Point(10, 10);
        grpExclusions.Name = "grpExclusions";
        grpExclusions.Size = new Size(932, 231);
        grpExclusions.TabIndex = 0;
        grpExclusions.TabStop = false;
        grpExclusions.Text = "Exclusion Rules (Preserve specific types or namespaces from obfuscation)";

        // lstExclusions
        lstExclusions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstExclusions.FormattingEnabled = true;
        lstExclusions.ItemHeight = 16;
        lstExclusions.Location = new Point(16, 25);
        lstExclusions.Name = "lstExclusions";
        lstExclusions.Size = new Size(580, 196);
        lstExclusions.TabIndex = 0;

        // cmbExclusionType
        cmbExclusionType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbExclusionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbExclusionType.FormattingEnabled = true;
        cmbExclusionType.Items.AddRange(new object[] { "Namespace", "Type Name", "Method Name" });
        cmbExclusionType.Location = new Point(610, 25);
        cmbExclusionType.Name = "cmbExclusionType";
        cmbExclusionType.Size = new Size(130, 24);
        cmbExclusionType.TabIndex = 1;

        // txtNewExclusion
        txtNewExclusion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtNewExclusion.Location = new Point(610, 58);
        txtNewExclusion.Name = "txtNewExclusion";
        txtNewExclusion.PlaceholderText = "e.g. MyNamespace.* or MyClass";
        txtNewExclusion.Size = new Size(305, 24);
        txtNewExclusion.TabIndex = 2;

        // btnAddExclusion
        btnAddExclusion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAddExclusion.Location = new Point(610, 92);
        btnAddExclusion.Name = "btnAddExclusion";
        btnAddExclusion.Size = new Size(145, 30);
        btnAddExclusion.TabIndex = 3;
        btnAddExclusion.Text = "➕ Add Exclusion";

        // btnRemoveExclusion
        btnRemoveExclusion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRemoveExclusion.Location = new Point(770, 92);
        btnRemoveExclusion.Name = "btnRemoveExclusion";
        btnRemoveExclusion.Size = new Size(145, 30);
        btnRemoveExclusion.TabIndex = 4;
        btnRemoveExclusion.Text = "❌ Remove Selected";

        // lblExclusionHelp
        lblExclusionHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblExclusionHelp.ForeColor = Color.FromArgb(71, 85, 105);
        lblExclusionHelp.Location = new Point(610, 135);
        lblExclusionHelp.Name = "lblExclusionHelp";
        lblExclusionHelp.Size = new Size(305, 80);
        lblExclusionHelp.Text = "Support wildcard '*' matching (e.g., 'MyApp.Plugins.*' or '*Controller'). Useful for reflection-heavy classes, dependency injection targets, and serialization DTOs.";

        // 
        // tabCiCd
        // 
        tabCiCd.Controls.Add(grpMsBuild);
        tabCiCd.Controls.Add(grpCliCommand);
        tabCiCd.Location = new Point(4, 25);
        tabCiCd.Name = "tabCiCd";
        tabCiCd.Padding = new Padding(10);
        tabCiCd.Size = new Size(952, 251);
        tabCiCd.Text = "  ⚡ Build Script & CI/CD  ";
        tabCiCd.UseVisualStyleBackColor = true;

        // grpCliCommand
        grpCliCommand.Controls.Add(btnCopyCli);
        grpCliCommand.Controls.Add(txtCliCommand);
        grpCliCommand.Location = new Point(10, 10);
        grpCliCommand.Name = "grpCliCommand";
        grpCliCommand.Size = new Size(465, 231);
        grpCliCommand.TabIndex = 0;
        grpCliCommand.TabStop = false;
        grpCliCommand.Text = "Command Line (CLI) for Build Scripts";

        // txtCliCommand
        txtCliCommand.Font = new Font("Consolas", 8.5F);
        txtCliCommand.Location = new Point(15, 25);
        txtCliCommand.Multiline = true;
        txtCliCommand.Name = "txtCliCommand";
        txtCliCommand.ReadOnly = true;
        txtCliCommand.ScrollBars = ScrollBars.Vertical;
        txtCliCommand.Size = new Size(435, 150);
        txtCliCommand.TabIndex = 0;

        // btnCopyCli
        btnCopyCli.Location = new Point(15, 185);
        btnCopyCli.Name = "btnCopyCli";
        btnCopyCli.Size = new Size(180, 32);
        btnCopyCli.TabIndex = 1;
        btnCopyCli.Text = "📋 Copy CLI Command";

        // grpMsBuild
        grpMsBuild.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpMsBuild.Controls.Add(btnCopyMsBuild);
        grpMsBuild.Controls.Add(txtMsBuild);
        grpMsBuild.Location = new Point(485, 10);
        grpMsBuild.Name = "grpMsBuild";
        grpMsBuild.Size = new Size(457, 231);
        grpMsBuild.TabIndex = 1;
        grpMsBuild.TabStop = false;
        grpMsBuild.Text = "MSBuild Post-Build Target (.csproj)";

        // txtMsBuild
        txtMsBuild.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtMsBuild.Font = new Font("Consolas", 8F);
        txtMsBuild.Location = new Point(15, 25);
        txtMsBuild.Multiline = true;
        txtMsBuild.Name = "txtMsBuild";
        txtMsBuild.ReadOnly = true;
        txtMsBuild.ScrollBars = ScrollBars.Vertical;
        txtMsBuild.Size = new Size(427, 150);
        txtMsBuild.TabIndex = 0;

        // btnCopyMsBuild
        btnCopyMsBuild.Location = new Point(15, 185);
        btnCopyMsBuild.Name = "btnCopyMsBuild";
        btnCopyMsBuild.Size = new Size(180, 32);
        btnCopyMsBuild.TabIndex = 1;
        btnCopyMsBuild.Text = "📋 Copy MSBuild Target";

        // 
        // panelBottom
        // 
        panelBottom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panelBottom.BackColor = Color.FromArgb(241, 245, 249);
        panelBottom.Controls.Add(lblStatus);
        panelBottom.Controls.Add(progressBar);
        panelBottom.Controls.Add(btnOpenFolder);
        panelBottom.Controls.Add(btnTestAssembly);
        panelBottom.Controls.Add(btnObfuscate);
        panelBottom.Location = new Point(12, 372);
        panelBottom.Name = "panelBottom";
        panelBottom.Size = new Size(960, 60);
        panelBottom.TabIndex = 2;

        // btnObfuscate
        btnObfuscate.BackColor = Color.FromArgb(37, 99, 235);
        btnObfuscate.FlatAppearance.BorderSize = 0;
        btnObfuscate.FlatStyle = FlatStyle.Flat;
        btnObfuscate.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnObfuscate.ForeColor = Color.White;
        btnObfuscate.Location = new Point(15, 10);
        btnObfuscate.Name = "btnObfuscate";
        btnObfuscate.Size = new Size(190, 40);
        btnObfuscate.TabIndex = 0;
        btnObfuscate.Text = "🛡️ Obfuscate Now";
        btnObfuscate.UseVisualStyleBackColor = false;

        // btnTestAssembly
        btnTestAssembly.BackColor = Color.FromArgb(16, 185, 129);
        btnTestAssembly.Enabled = false;
        btnTestAssembly.FlatAppearance.BorderSize = 0;
        btnTestAssembly.FlatStyle = FlatStyle.Flat;
        btnTestAssembly.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnTestAssembly.ForeColor = Color.White;
        btnTestAssembly.Location = new Point(215, 10);
        btnTestAssembly.Name = "btnTestAssembly";
        btnTestAssembly.Size = new Size(175, 40);
        btnTestAssembly.TabIndex = 1;
        btnTestAssembly.Text = "▶ Test Obfuscated App";
        btnTestAssembly.UseVisualStyleBackColor = false;

        // btnOpenFolder
        btnOpenFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpenFolder.Location = new Point(810, 10);
        btnOpenFolder.Name = "btnOpenFolder";
        btnOpenFolder.Size = new Size(135, 40);
        btnOpenFolder.TabIndex = 2;
        btnOpenFolder.Text = "📂 Open Folder";

        // progressBar
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(400, 12);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(395, 16);
        progressBar.TabIndex = 3;

        // lblStatus
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
        lblStatus.Location = new Point(400, 32);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(395, 18);
        lblStatus.Text = "Ready";

        // 
        // rtbLog
        // 
        rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        rtbLog.BackColor = Color.FromArgb(15, 23, 42);
        rtbLog.BorderStyle = BorderStyle.None;
        rtbLog.Font = new Font("Consolas", 9.5F);
        rtbLog.ForeColor = Color.FromArgb(226, 232, 240);
        rtbLog.Location = new Point(12, 440);
        rtbLog.Name = "rtbLog";
        rtbLog.ReadOnly = true;
        rtbLog.Size = new Size(960, 240);
        rtbLog.TabIndex = 3;
        rtbLog.Text = "";

        // btnClearLog
        btnClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClearLog.Location = new Point(860, 686);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Size = new Size(110, 26);
        btnClearLog.TabIndex = 4;
        btnClearLog.Text = "Clear Log";

        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(248, 250, 252);
        ClientSize = new Size(984, 718);
        Controls.Add(btnClearLog);
        Controls.Add(rtbLog);
        Controls.Add(panelBottom);
        Controls.Add(tabControlMain);
        Controls.Add(panelHeader);
        MinimumSize = new Size(900, 650);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CodeShield - .NET Obfuscation Tool";

        panelHeader.ResumeLayout(false);
        panelHeader.PerformLayout();
        tabControlMain.ResumeLayout(false);
        tabFiles.ResumeLayout(false);
        tabProtections.ResumeLayout(false);
        tabExclusions.ResumeLayout(false);
        tabCiCd.ResumeLayout(false);
        grpFiles.ResumeLayout(false);
        grpFiles.PerformLayout();
        grpRenaming.ResumeLayout(false);
        grpRenaming.PerformLayout();
        grpStrings.ResumeLayout(false);
        grpStrings.PerformLayout();
        grpExclusions.ResumeLayout(false);
        grpExclusions.PerformLayout();
        grpCliCommand.ResumeLayout(false);
        grpCliCommand.PerformLayout();
        grpMsBuild.ResumeLayout(false);
        grpMsBuild.PerformLayout();
        panelBottom.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numMinStringLen).EndInit();
        ((System.ComponentModel.ISupportInitialize)numKeySeed).EndInit();
        ResumeLayout(false);
    }
}
