using FNF.BouyomiChanApp;
using FNF.Utility;
using FNF.XmlSerializerSetting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Plugin_Extension
{
    // プリセット保存用クラス.
    public class PresetItem
    {
        public string Name { get; set; }
        public int VoiceType { get; set; }
        public int Speed { get; set; }
        public int Tone { get; set; }
        public int Volume { get; set; }
        public PresetItem() { }
    }

    public class Plugin_VideoExtension : IPlugin
    {
        private Settings_preset _Settings;
        private SettingFormData_preset _SettingFormData;
        private string _SettingFile = Base.CallAsmPath + Base.CallAsmName + ".setting";

        // ツールバーの部品.
        private ToolStripSeparator _Separator;
        private ToolStripSeparator _Separator2;
        private ToolStripSeparator _Separator3;
        private ToolStripSeparator _Separator4;
        private ToolStripComboBox _PresetComboBox;
        private ToolStripButton _ButtonAdd;
        private ToolStripButton _ButtonUpdate;
        private ToolStripButton _ButtonDelete;
        private ToolStripButton _ButtonDirectInput;
        private ToolStripButton _ButtonSaveWavTxt;

        public string Name { get { return "動画編集拡張"; } }
        public string Version { get { return "1.0.0"; } }
        public string Caption { get { return "動画編集に有用な拡張機能を追加するプラグインです。"; } }
        public ISettingFormData SettingFormData { get { return _SettingFormData; } }

        public void Begin()
        {
            _Settings = new Settings_preset(this);
            _Settings.Load(_SettingFile);
            _SettingFormData = new SettingFormData_preset(_Settings);

            // 部品の初期化と追加.
            _Separator = new ToolStripSeparator();
            Pub.ToolStrip.Items.Add(_Separator);

            _ButtonDirectInput = new ToolStripButton("✏️ 数値入力");
            _ButtonDirectInput.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _ButtonDirectInput.ToolTipText = "パラメーター (音量・速度・音程)を数値で直接入力します。";
            _ButtonDirectInput.Click += ButtonDirectInput_Click;
            Pub.ToolStrip.Items.Add(_ButtonDirectInput);

            _Separator2 = new ToolStripSeparator();
            Pub.ToolStrip.Items.Add(_Separator2);

            _PresetComboBox = new ToolStripComboBox();
            _PresetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _PresetComboBox.ToolTipText = "プリセットをプルダウンから選択します。";
            _PresetComboBox.SelectedIndexChanged += PresetComboBox_SelectedIndexChanged;
            Pub.ToolStrip.Items.Add(_PresetComboBox);

            _Separator3 = new ToolStripSeparator();
            Pub.ToolStrip.Items.Add(_Separator3);

            _ButtonAdd = new ToolStripButton("＋ 追加");
            _ButtonAdd.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _ButtonAdd.ToolTipText = "現在の画面の設定をプリセットに保存します。";
            _ButtonAdd.Click += ButtonAdd_Click;
            Pub.ToolStrip.Items.Add(_ButtonAdd);

            _ButtonUpdate = new ToolStripButton("↻ 更新");
            _ButtonUpdate.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _ButtonUpdate.ToolTipText = "現在選択しているプリセットに、現在の画面の設定を上書き保存します。";
            _ButtonUpdate.Click += ButtonUpdate_Click;
            Pub.ToolStrip.Items.Add(_ButtonUpdate);

            _ButtonDelete = new ToolStripButton("－ 削除");
            _ButtonDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _ButtonDelete.ToolTipText = "現在選択しているプリセットを削除します。";
            _ButtonDelete.Click += ButtonDelete_Click;
            Pub.ToolStrip.Items.Add(_ButtonDelete);

            _Separator4 = new ToolStripSeparator();
            Pub.ToolStrip.Items.Add(_Separator4);

            _ButtonSaveWavTxt = new ToolStripButton("💾 書出");
            _ButtonSaveWavTxt.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _ButtonSaveWavTxt.ToolTipText = "音声(.wav)とテキスト(.txt)を書き出します。";
            _ButtonSaveWavTxt.Click += ButtonSaveWavTxt_Click;
            Pub.ToolStrip.Items.Add(_ButtonSaveWavTxt);

            // 保存されているプリセットを反映.
            RefreshComboBox();
        }

        public void End()
        {
            _Settings.Save(_SettingFile);

            _PresetComboBox.SelectedIndexChanged -= PresetComboBox_SelectedIndexChanged;
            _ButtonAdd.Click -= ButtonAdd_Click;
            _ButtonUpdate.Click -= ButtonUpdate_Click;
            _ButtonDelete.Click -= ButtonDelete_Click;
            _ButtonDirectInput.Click -= ButtonDirectInput_Click;
            _ButtonSaveWavTxt.Click -= ButtonSaveWavTxt_Click;

            Pub.ToolStrip.Items.Remove(_PresetComboBox);
            Pub.ToolStrip.Items.Remove(_ButtonAdd);
            Pub.ToolStrip.Items.Remove(_ButtonUpdate);
            Pub.ToolStrip.Items.Remove(_ButtonDelete);
            Pub.ToolStrip.Items.Remove(_ButtonDirectInput);
            Pub.ToolStrip.Items.Remove(_ButtonSaveWavTxt);
            Pub.ToolStrip.Items.Remove(_Separator);
            Pub.ToolStrip.Items.Remove(_Separator2);
            Pub.ToolStrip.Items.Remove(_Separator3);
            Pub.ToolStrip.Items.Remove(_Separator4);

            CleanupButtonTimer(_ButtonAdd);
            CleanupButtonTimer(_ButtonUpdate);
            CleanupButtonTimer(_ButtonDelete);
            CleanupButtonTimer(_ButtonSaveWavTxt);

            _PresetComboBox.Dispose();
            _ButtonAdd.Dispose();
            _ButtonUpdate.Dispose();
            _ButtonDelete.Dispose();
            _ButtonDirectInput.Dispose();
            _ButtonSaveWavTxt.Dispose();
            _Separator.Dispose();
            _Separator2.Dispose();
            _Separator3.Dispose();
            _Separator4.Dispose();
        }

        // コントロール用メソッド.
        private void RefreshComboBox()
        {
            _PresetComboBox.SelectedIndexChanged -= PresetComboBox_SelectedIndexChanged;

            string currentSelection = _PresetComboBox.SelectedItem?.ToString();
            _PresetComboBox.Items.Clear();
            _PresetComboBox.Items.Add("選択なし");

            foreach (var preset in _Settings.Presets)
            {
                _PresetComboBox.Items.Add(preset.Name);
            }

            if (!string.IsNullOrEmpty(currentSelection) && _PresetComboBox.Items.Contains(currentSelection))
            {
                _PresetComboBox.SelectedItem = currentSelection;
            }
            else
            {
                _PresetComboBox.SelectedIndex = 0;
            }

            _PresetComboBox.SelectedIndexChanged += PresetComboBox_SelectedIndexChanged;
        }

        private string ShowInputDialog(string text, string caption)
        {
            using (Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false
            })
            {
                Label textLabel = new Label() { Left = 10, Top = 20, Text = text, Width = 260 };
                TextBox textBox = new TextBox() { Left = 10, Top = 50, Width = 260 };
                Button confirmation = new Button() { Text = "OK", Left = 190, Top = 80, Width = 80, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "キャンセル", Left = 100, Top = 80, Width = 80, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;
                prompt.ActiveControl = textBox;

                return prompt.ShowDialog(Pub.FormMain) == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void ShowTemporaryStatus(ToolStripButton button, string temporaryText, string originalText, int durationMs)
        {
            CleanupButtonTimer(button);

            button.Text = temporaryText;
            button.Enabled = false;

            Timer statusTimer = new Timer();
            statusTimer.Interval = durationMs;
            statusTimer.Tick += (s, args) =>
            {
                if (!button.IsDisposed)
                {
                    button.Text = originalText;
                    button.Enabled = true;
                }

                statusTimer.Stop();
                statusTimer.Dispose();
                if (button.Tag == statusTimer)
                {
                    button.Tag = null;
                }
            };

            button.Tag = statusTimer;
            statusTimer.Start();
        }

        private void CleanupButtonTimer(ToolStripButton button)
        {
            if (button.Tag is Timer oldTimer)
            {
                oldTimer.Stop();
                oldTimer.Dispose();
                button.Tag = null;
            }
        }

        // イベント設定.
        private void PresetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_PresetComboBox.SelectedIndex <= 0) return;

            string selectedPresetName = _PresetComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedPresetName)) return;
            PresetItem preset = _Settings.Presets.Find(p => p.Name == selectedPresetName);

            if (preset != null)
            {
                Pub.FormMain.comboBoxVoiceType.SelectedIndex = preset.VoiceType;
                Pub.FormMain.trackBarVolume.Value = preset.Volume;
                Pub.FormMain.trackBarSpeed.Value = preset.Speed;
                Pub.FormMain.trackBarTone.Value = preset.Tone;
            }
        }

        // 追加ボタン.
        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            string name = ShowInputDialog("新しいプリセットの名前を入力してください: ", "プリセットの追加");

            if (string.IsNullOrEmpty(name) || name.Trim() == "") return;
            name = name.Trim();
            if (string.Equals(name, "選択なし", StringComparison.Ordinal)) return;

            if (_Settings.Presets.Exists(p => string.Equals(p.Name, name, StringComparison.Ordinal)))
            {
                DialogResult result = MessageBox.Show(Pub.FormMain, string.Format("プリセット「{0}」は既に存在します。上書きしますか？", name), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
            }

            _Settings.Presets.RemoveAll(p => string.Equals(p.Name, name, StringComparison.Ordinal));

            PresetItem newItem = new PresetItem()
            {
                Name = name,
                VoiceType = Pub.FormMain.comboBoxVoiceType.SelectedIndex,
                Volume = Pub.FormMain.trackBarVolume.Value,
                Speed = Pub.FormMain.trackBarSpeed.Value,
                Tone = Pub.FormMain.trackBarTone.Value
            };

            _Settings.Presets.Add(newItem);
            RefreshComboBox();
            _PresetComboBox.SelectedItem = name;

            _Settings.Save(_SettingFile);

            ShowTemporaryStatus(_ButtonAdd, "✓ 完了", "＋ 追加", _Settings.DisplayTimeMs);
        }

        // 更新ボタン.
        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            if (_PresetComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("上書き保存するプリセットをプルダウンから選択してください。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = _PresetComboBox.SelectedItem.ToString();
            PresetItem preset = _Settings.Presets.Find(p => string.Equals(p.Name, name, StringComparison.Ordinal));

            if (preset != null)
            {
                preset.VoiceType = Pub.FormMain.comboBoxVoiceType.SelectedIndex;
                preset.Volume = Pub.FormMain.trackBarVolume.Value;
                preset.Speed = Pub.FormMain.trackBarSpeed.Value;
                preset.Tone = Pub.FormMain.trackBarTone.Value;

                _Settings.Save(_SettingFile);

                ShowTemporaryStatus(_ButtonUpdate, "✓ 完了", "↻ 更新", _Settings.DisplayTimeMs);
            }
        }

        // 削除ボタン.
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (_PresetComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("削除するプリセットをプルダウンから選択してください。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int targetIndex = _PresetComboBox.SelectedIndex - 1;

            string name = _PresetComboBox.SelectedItem.ToString();
            DialogResult result = MessageBox.Show(string.Format("プリセット「{0}」を削除しますか？", name), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _Settings.Presets.RemoveAll(p => string.Equals(p.Name, name, StringComparison.Ordinal));
                RefreshComboBox();

                if (_Settings.SelectPreviousOnDelete && targetIndex >= 0 && targetIndex < _PresetComboBox.Items.Count)
                {
                    _PresetComboBox.SelectedIndex = targetIndex;
                }

                _Settings.Save(_SettingFile);

                ShowTemporaryStatus(_ButtonDelete, "✓ 完了", "－ 削除", _Settings.DisplayTimeMs);
            }
        }

        // 数値入力ボタン.
        private void ButtonDirectInput_Click(object sender, EventArgs e)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 300;
                prompt.Height = 200;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "パラメーター数値入力";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                int originalVol = Pub.FormMain.trackBarVolume.Value;
                int originalSpeed = Pub.FormMain.trackBarSpeed.Value;
                int originalTone = Pub.FormMain.trackBarTone.Value;

                Label lblVol = new Label() { Left = 20, Top = 20, Text = "音量", Width = 50 };
                NumericUpDown numVol = new NumericUpDown()
                {
                    Left = 80,
                    Top = 18,
                    Width = 80,
                    Minimum = Pub.FormMain.trackBarVolume.Minimum,
                    Maximum = Pub.FormMain.trackBarVolume.Maximum,
                    Value = originalVol
                };

                Label lblSpeed = new Label() { Left = 20, Top = 55, Text = "速度", Width = 50 };
                NumericUpDown numSpeed = new NumericUpDown()
                {
                    Left = 80,
                    Top = 53,
                    Width = 80,
                    Minimum = Pub.FormMain.trackBarSpeed.Minimum,
                    Maximum = Pub.FormMain.trackBarSpeed.Maximum,
                    Value = originalSpeed
                };

                Label lblTone = new Label() { Left = 20, Top = 90, Text = "音程", Width = 50 };
                NumericUpDown numTone = new NumericUpDown()
                {
                    Left = 80,
                    Top = 88,
                    Width = 80,
                    Minimum = Pub.FormMain.trackBarTone.Minimum,
                    Maximum = Pub.FormMain.trackBarTone.Maximum,
                    Value = originalTone
                };

                Button btnTest = new Button() { Text = "▶ 試聴", Left = 175, Top = 18, Width = 85, Height = 90 };

                Button btnOk = new Button() { Text = "適用", Left = 40, Top = 130, Width = 90, DialogResult = DialogResult.OK };
                Button btnCancel = new Button() { Text = "キャンセル", Left = 150, Top = 130, Width = 90, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(lblVol);
                prompt.Controls.Add(numVol);
                prompt.Controls.Add(lblSpeed);
                prompt.Controls.Add(numSpeed);
                prompt.Controls.Add(lblTone);
                prompt.Controls.Add(numTone);
                prompt.Controls.Add(btnTest);
                prompt.Controls.Add(btnOk);
                prompt.Controls.Add(btnCancel);

                prompt.AcceptButton = btnOk;
                prompt.CancelButton = btnCancel;

                btnTest.Click += (s, args) =>
                {
                    Pub.FormMain.trackBarVolume.Value = (int)numVol.Value;
                    Pub.FormMain.trackBarSpeed.Value = (int)numSpeed.Value;
                    Pub.FormMain.trackBarTone.Value = (int)numTone.Value;

                    Pub.AddTalkTask("音声のテストです。");
                };

                prompt.FormClosing += (s, args) =>
                {
                    if (prompt.DialogResult == DialogResult.Cancel)
                    {
                        Pub.FormMain.trackBarVolume.Value = originalVol;
                        Pub.FormMain.trackBarSpeed.Value = originalSpeed;
                        Pub.FormMain.trackBarTone.Value = originalTone;
                    }
                };

                prompt.ActiveControl = numVol;

                if (prompt.ShowDialog(Pub.FormMain) == DialogResult.OK)
                {
                    Pub.FormMain.trackBarVolume.Value = (int)numVol.Value;
                    Pub.FormMain.trackBarSpeed.Value = (int)numSpeed.Value;
                    Pub.FormMain.trackBarTone.Value = (int)numTone.Value;
                }
            }
        }

        // 書き出しボタン.
        private void ButtonSaveWavTxt_Click(object sender, EventArgs e)
        {
            string rawText = Pub.FormMain.textBoxSource.Text;
            if (string.IsNullOrEmpty(rawText))
            {
                MessageBox.Show("テキストが入力されていません。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filePath = _Settings.FilePath;
            if (string.IsNullOrEmpty(filePath) || filePath.Trim() == "")
            {
                MessageBox.Show("保存先フォルダが未設定です。\nプラグイン設定画面から「ファイルの保存場所」を指定してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(filePath))
            {
                try
                {
                    Directory.CreateDirectory(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存先フォルダの自動作成に失敗しました：\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string presetName = "選択なし";
            if (_PresetComboBox.SelectedIndex > 0)
            {
                presetName = _PresetComboBox.SelectedItem.ToString();
            }
            else
            {
                presetName = Pub.FormMain.comboBoxVoiceType.SelectedItem?.ToString() ?? "Default";
            }

            string bodyText = rawText.Replace("\r", "").Replace("\n", " ").Trim();

            string fileNameText = bodyText;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileNameText = fileNameText.Replace(c, '_');
            }

            System.Globalization.StringInfo si = new System.Globalization.StringInfo(fileNameText);
            if (si.LengthInTextElements > 5)
            {
                fileNameText = si.SubstringByTextElements(0, 5);
            }

            string serialStr = _Settings.NextSerialNumber.ToString("D3");
            string fileName = string.Format("{0}_{1}_{2}", serialStr, presetName, fileNameText);

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            try
            {
                string txtPath = Path.Combine(filePath, fileName + ".txt");
                string wavPath = Path.Combine(filePath, fileName + ".wav");

                if (File.Exists(txtPath) || File.Exists(wavPath))
                {
                    DialogResult overwriteResult = MessageBox.Show(Pub.FormMain,
                        string.Format("ファイル「{0}」は既に存在します。\n上書きしますか？", fileName),
                        "上書きの確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (overwriteResult != DialogResult.Yes) return;
                }

                using (StreamWriter sw = new StreamWriter(txtPath, false, new UTF8Encoding(false)))
                {
                    sw.Write(bodyText);
                }

                int volume = Pub.FormMain.trackBarVolume.Value;
                int speed = Pub.FormMain.trackBarSpeed.Value;
                int tone = Pub.FormMain.trackBarTone.Value;
                VoiceType voicetype = (VoiceType)Pub.FormMain.comboBoxVoiceType.SelectedIndex;

                Pub.AddTalkTask(bodyText, speed, tone, volume, voicetype, wavPath);

                _Settings.NextSerialNumber++;
                _Settings.Save(_SettingFile);

                ShowTemporaryStatus(_ButtonSaveWavTxt, "✓ 完了", "💾 書出", _Settings.DisplayTimeMs);
            }

            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("保存先のフォルダに書き込むための権限がありません。\n" +
                                "設定画面から「ファイルの保存場所」を変更してください。",
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ファイルの保存中にエラーが発生しました：\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 設定関連.
        public class Settings_preset : SettingsBase
        {
            public List<PresetItem> Presets = new List<PresetItem>();
            public int DisplayTimeMs { get; set; }
            public bool SelectPreviousOnDelete { get; set; }

            public string FilePath { get; set; }
            public int NextSerialNumber { get; set; }

            [System.Xml.Serialization.XmlIgnore]
            internal Plugin_VideoExtension Plugin;
            public Settings_preset()
            {
                DisplayTimeMs = 750;
                SelectPreviousOnDelete = true;
                FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BouyomiChan_Output");
                NextSerialNumber = 1;
            }
            public Settings_preset(Plugin_VideoExtension plugin)
            {
                Plugin = plugin;
                DisplayTimeMs = 750;
                SelectPreviousOnDelete = true;
                FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BouyomiChan_Output");
                NextSerialNumber = 1;
            }
            public override void ReadSettings() { }
            public override void WriteSettings() { }
        }

        public class SettingFormData_preset : ISettingFormData
        {
            Settings_preset _Setting;
            public string Title { get { return _Setting.Plugin.Name; } }
            public bool ExpandAll { get { return false; } }
            public SettingsBase Setting { get { return _Setting; } }
            public SBase PBase;
            public SettingFormData_preset(Settings_preset setting) { _Setting = setting; PBase = new SBase(_Setting); }
            public class SBase : ISettingPropertyGrid
            {
                Settings_preset _Setting;
                public SBase(Settings_preset setting) { _Setting = setting; }
                public string GetName() { return "プリセット設定"; }

                [System.ComponentModel.Category("動作設定")]
                [System.ComponentModel.DisplayName("完了表示時間 (ミリ秒)")]
                [System.ComponentModel.Description("各ボタンを押した後に「✓ 完了」と表示する時間です。デフォルトは750ミリ秒です。")]
                public int DisplayTime
                {
                    get { return _Setting.DisplayTimeMs; }
                    set
                    {
                        if (value < 0) _Setting.DisplayTimeMs = 0;
                        else _Setting.DisplayTimeMs = value;

                    }
                }

                [System.ComponentModel.Category("動作設定")]
                [System.ComponentModel.DisplayName("削除時に1つ前のプリセットを選択")]
                [System.ComponentModel.Description("プリセット削除時、自動的に1つ前のプリセットを選択するようにします。デフォルトは有効です。")]
                public bool SelectPreviousOnDelete
                {
                    get { return _Setting.SelectPreviousOnDelete; }
                    set { _Setting.SelectPreviousOnDelete = value; }
                }

                [System.ComponentModel.Category("動作設定")]
                [System.ComponentModel.DisplayName("ファイル保存先")]
                [System.ComponentModel.Description("音声及びテキストファイルの保存先を指定します。")]
                public string FilePath
                {
                    get { return _Setting.FilePath; }
                    set { _Setting.FilePath = value; }
                }

                [System.ComponentModel.Category("動作設定")]
                [System.ComponentModel.DisplayName("次の連番")]
                [System.ComponentModel.Description("次に生成されるファイルの連番を指定します。")]
                public int NextSerialNumber
                {
                    get { return _Setting.NextSerialNumber; }
                    set { _Setting.NextSerialNumber = value < 1 ? 1 : value; }
                }

                [System.ComponentModel.Category("操作方法 (数値入力)")]
                [System.ComponentModel.DisplayName("数値入力")]
                [System.ComponentModel.Description("「✏️ 数値入力」を押すと、パラメーターを直接入力するためのウィンドウが表示されます。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info1 { get { return "「✏️ 数値入力」を押すと、パラメーターを直接入力するためのウィンドウが表示されます。"; } }

                [System.ComponentModel.Category("操作方法 (プリセット)")]
                [System.ComponentModel.DisplayName("手順1")]
                [System.ComponentModel.Description("パラメーター (声質・音量・速度・音程)を変更して、好みの声色に設定します。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info2 { get { return "パラメーター (声質・音量・速度・音程)を変更して、好みの声色に設定します。"; } }

                [System.ComponentModel.Category("操作方法 (プリセット)")]
                [System.ComponentModel.DisplayName("手順2")]
                [System.ComponentModel.Description("「＋ 追加」を押してプリセット名を入力すると、現在のパラメーターがプリセットとして保存されます。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info3 { get { return "「＋ 追加」を押してプリセット名を入力すると、現在のパラメーターがプリセットとして保存されます。"; } }

                [System.ComponentModel.Category("操作方法 (プリセット)")]
                [System.ComponentModel.DisplayName("手順3")]
                [System.ComponentModel.Description("プルダウンからプリセットを選択すると、選択したプリセットが反映されます。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info4 { get { return "プルダウンからプリセットを選択すると、選択したプリセットが反映されます。"; } }

                [System.ComponentModel.Category("操作方法 (プリセット)")]
                [System.ComponentModel.DisplayName("手順4")]
                [System.ComponentModel.Description("「↻ 更新」を押すと、選択中のプリセットに現在のパラメーターが上書きされます。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info5 { get { return "「↻ 更新」を押すと、選択中のプリセットに現在のパラメーターが上書きされます。"; } }

                [System.ComponentModel.Category("操作方法 (プリセット)")]
                [System.ComponentModel.DisplayName("手順5")]
                [System.ComponentModel.Description("「－ 削除」を押すと、選択中のプリセットが削除されます。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info6 { get { return "「－ 削除」を押すと、選択中のプリセットが削除されます。"; } }

                [System.ComponentModel.Category("操作方法 (音声・テキスト同時書き出し)")]
                [System.ComponentModel.DisplayName("音声・テキスト同時書き出し")]
                [System.ComponentModel.Description("「💾 書出」を押すと、音声及びテキストが同時に出力されます。")]
                [System.ComponentModel.ReadOnly(true)]
                public string Info7 { get { return "「💾 書出」を押すと、音声及びテキストが同時に出力されます。"; } }
            }
        }
    }
}