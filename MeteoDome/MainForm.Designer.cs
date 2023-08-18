namespace MeteoDome
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox_Meteo = new System.Windows.Forms.GroupBox();
            this.dome_weather_label = new System.Windows.Forms.Label();
            this.label_Obs_cond = new System.Windows.Forms.Label();
            this.label_Sun = new System.Windows.Forms.Label();
            this.label_Wind = new System.Windows.Forms.Label();
            this.label_Allsky_ext_STD = new System.Windows.Forms.Label();
            this.label_Seeing = new System.Windows.Forms.Label();
            this.label_Allsky_ext = new System.Windows.Forms.Label();
            this.label_Seeing_ext = new System.Windows.Forms.Label();
            this.label_SkyTempSTD = new System.Windows.Forms.Label();
            this.label_SkyTemp = new System.Windows.Forms.Label();
            this.groupBox_Dome = new System.Windows.Forms.GroupBox();
            this.checkBox_initflag = new System.Windows.Forms.CheckBox();
            this.numericUpDown_timeout_south = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_timeout_north = new System.Windows.Forms.NumericUpDown();
            this.label_timeout_south = new System.Windows.Forms.Label();
            this.label_timeout_north = new System.Windows.Forms.Label();
            this.label_butt_south = new System.Windows.Forms.Label();
            this.label_butt_north = new System.Windows.Forms.Label();
            this.checkBoxSouth = new System.Windows.Forms.CheckBox();
            this.checkBoxNorth = new System.Windows.Forms.CheckBox();
            this.label_Shutter_South = new System.Windows.Forms.Label();
            this.label_Shutter_North = new System.Windows.Forms.Label();
            this.label_Motor_North = new System.Windows.Forms.Label();
            this.label_Motor_South = new System.Windows.Forms.Label();
            this.label_Dome_Power = new System.Windows.Forms.Label();
            this.checkBox_AutoDome = new System.Windows.Forms.CheckBox();
            this.comboBox_Dome = new System.Windows.Forms.ComboBox();
            this.button_Dome_Run = new System.Windows.Forms.Button();
            this.timerSet = new System.Windows.Forms.Timer(this.components);
            this.logBox = new System.Windows.Forms.ListBox();
            this.contextMenuStripLogger = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSaveLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemClearLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox_Meteo.SuspendLayout();
            this.groupBox_Dome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDown_timeout_south)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDown_timeout_north)).BeginInit();
            this.contextMenuStripLogger.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Meteo
            // 
            this.groupBox_Meteo.Controls.Add(this.dome_weather_label);
            this.groupBox_Meteo.Controls.Add(this.label_Obs_cond);
            this.groupBox_Meteo.Controls.Add(this.label_Sun);
            this.groupBox_Meteo.Controls.Add(this.label_Wind);
            this.groupBox_Meteo.Controls.Add(this.label_Allsky_ext_STD);
            this.groupBox_Meteo.Controls.Add(this.label_Seeing);
            this.groupBox_Meteo.Controls.Add(this.label_Allsky_ext);
            this.groupBox_Meteo.Controls.Add(this.label_Seeing_ext);
            this.groupBox_Meteo.Controls.Add(this.label_SkyTempSTD);
            this.groupBox_Meteo.Controls.Add(this.label_SkyTemp);
            this.groupBox_Meteo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox_Meteo.Location = new System.Drawing.Point(0, 10);
            this.groupBox_Meteo.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox_Meteo.Name = "groupBox_Meteo";
            this.groupBox_Meteo.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox_Meteo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox_Meteo.Size = new System.Drawing.Size(286, 301);
            this.groupBox_Meteo.TabIndex = 0;
            this.groupBox_Meteo.TabStop = false;
            this.groupBox_Meteo.Text = "Weater";
            // 
            // dome_weather_label
            // 
            this.dome_weather_label.AutoSize = true;
            this.dome_weather_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.dome_weather_label.Location = new System.Drawing.Point(4, 253);
            this.dome_weather_label.Name = "dome_weather_label";
            this.dome_weather_label.Size = new System.Drawing.Size(136, 17);
            this.dome_weather_label.TabIndex = 11;
            this.dome_weather_label.Text = "Weather is unknown";
            // 
            // label_Obs_cond
            // 
            this.label_Obs_cond.AutoSize = true;
            this.label_Obs_cond.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Obs_cond.Location = new System.Drawing.Point(4, 270);
            this.label_Obs_cond.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Obs_cond.Name = "label_Obs_cond";
            this.label_Obs_cond.Size = new System.Drawing.Size(168, 17);
            this.label_Obs_cond.TabIndex = 9;
            this.label_Obs_cond.Text = "Obs conditions: Unknown";
            // 
            // label_Sun
            // 
            this.label_Sun.AutoSize = true;
            this.label_Sun.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Sun.Location = new System.Drawing.Point(3, 215);
            this.label_Sun.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Sun.Name = "label_Sun";
            this.label_Sun.Size = new System.Drawing.Size(206, 17);
            this.label_Sun.TabIndex = 7;
            this.label_Sun.Text = "Sun zenith distance (deg): NaN";
            // 
            // label_Wind
            // 
            this.label_Wind.AutoSize = true;
            this.label_Wind.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Wind.Location = new System.Drawing.Point(3, 184);
            this.label_Wind.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Wind.Name = "label_Wind";
            this.label_Wind.Size = new System.Drawing.Size(112, 17);
            this.label_Wind.TabIndex = 6;
            this.label_Wind.Text = "Wind (m/s): NaN";
            // 
            // label_Allsky_ext_STD
            // 
            this.label_Allsky_ext_STD.AutoSize = true;
            this.label_Allsky_ext_STD.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Allsky_ext_STD.Location = new System.Drawing.Point(3, 99);
            this.label_Allsky_ext_STD.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Allsky_ext_STD.Name = "label_Allsky_ext_STD";
            this.label_Allsky_ext_STD.Size = new System.Drawing.Size(218, 17);
            this.label_Allsky_ext_STD.TabIndex = 5;
            this.label_Allsky_ext_STD.Text = "AllSky extinction STD (mag): NaN";
            // 
            // label_Seeing
            // 
            this.label_Seeing.AutoSize = true;
            this.label_Seeing.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Seeing.Location = new System.Drawing.Point(3, 152);
            this.label_Seeing.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Seeing.Name = "label_Seeing";
            this.label_Seeing.Size = new System.Drawing.Size(144, 17);
            this.label_Seeing.TabIndex = 4;
            this.label_Seeing.Text = "Seeing (arcsec): NaN";
            // 
            // label_Allsky_ext
            // 
            this.label_Allsky_ext.AutoSize = true;
            this.label_Allsky_ext.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Allsky_ext.Location = new System.Drawing.Point(3, 76);
            this.label_Allsky_ext.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Allsky_ext.Name = "label_Allsky_ext";
            this.label_Allsky_ext.Size = new System.Drawing.Size(186, 17);
            this.label_Allsky_ext.TabIndex = 3;
            this.label_Allsky_ext.Text = "AllSky extinction (mag): NaN";
            // 
            // label_Seeing_ext
            // 
            this.label_Seeing_ext.AutoSize = true;
            this.label_Seeing_ext.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Seeing_ext.Location = new System.Drawing.Point(3, 130);
            this.label_Seeing_ext.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Seeing_ext.Name = "label_Seeing_ext";
            this.label_Seeing_ext.Size = new System.Drawing.Size(192, 17);
            this.label_Seeing_ext.TabIndex = 2;
            this.label_Seeing_ext.Text = "Seeing extinction (mag): NaN";
            // 
            // label_SkyTempSTD
            // 
            this.label_SkyTempSTD.AutoSize = true;
            this.label_SkyTempSTD.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_SkyTempSTD.Location = new System.Drawing.Point(3, 45);
            this.label_SkyTempSTD.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_SkyTempSTD.Name = "label_SkyTempSTD";
            this.label_SkyTempSTD.Size = new System.Drawing.Size(218, 17);
            this.label_SkyTempSTD.TabIndex = 1;
            this.label_SkyTempSTD.Text = "Sky temperature STD (deg): NaN";
            // 
            // label_SkyTemp
            // 
            this.label_SkyTemp.AutoSize = true;
            this.label_SkyTemp.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_SkyTemp.Location = new System.Drawing.Point(3, 22);
            this.label_SkyTemp.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_SkyTemp.Name = "label_SkyTemp";
            this.label_SkyTemp.Size = new System.Drawing.Size(186, 17);
            this.label_SkyTemp.TabIndex = 0;
            this.label_SkyTemp.Text = "Sky temperature (deg): NaN";
            // 
            // groupBox_Dome
            // 
            this.groupBox_Dome.Controls.Add(this.checkBox_initflag);
            this.groupBox_Dome.Controls.Add(this.numericUpDown_timeout_south);
            this.groupBox_Dome.Controls.Add(this.numericUpDown_timeout_north);
            this.groupBox_Dome.Controls.Add(this.label_timeout_south);
            this.groupBox_Dome.Controls.Add(this.label_timeout_north);
            this.groupBox_Dome.Controls.Add(this.label_butt_south);
            this.groupBox_Dome.Controls.Add(this.label_butt_north);
            this.groupBox_Dome.Controls.Add(this.checkBoxSouth);
            this.groupBox_Dome.Controls.Add(this.checkBoxNorth);
            this.groupBox_Dome.Controls.Add(this.label_Shutter_South);
            this.groupBox_Dome.Controls.Add(this.label_Shutter_North);
            this.groupBox_Dome.Controls.Add(this.label_Motor_North);
            this.groupBox_Dome.Controls.Add(this.label_Motor_South);
            this.groupBox_Dome.Controls.Add(this.label_Dome_Power);
            this.groupBox_Dome.Controls.Add(this.checkBox_AutoDome);
            this.groupBox_Dome.Controls.Add(this.comboBox_Dome);
            this.groupBox_Dome.Controls.Add(this.button_Dome_Run);
            this.groupBox_Dome.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.groupBox_Dome.Location = new System.Drawing.Point(288, 10);
            this.groupBox_Dome.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox_Dome.Name = "groupBox_Dome";
            this.groupBox_Dome.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox_Dome.Size = new System.Drawing.Size(355, 301);
            this.groupBox_Dome.TabIndex = 1;
            this.groupBox_Dome.TabStop = false;
            this.groupBox_Dome.Text = "Dome";
            // 
            // checkBox_initflag
            // 
            this.checkBox_initflag.AutoSize = true;
            this.checkBox_initflag.Enabled = false;
            this.checkBox_initflag.Location = new System.Drawing.Point(191, 95);
            this.checkBox_initflag.Name = "checkBox_initflag";
            this.checkBox_initflag.Size = new System.Drawing.Size(68, 21);
            this.checkBox_initflag.TabIndex = 21;
            this.checkBox_initflag.Text = "Initflag";
            this.checkBox_initflag.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_timeout_south
            // 
            this.numericUpDown_timeout_south.Location = new System.Drawing.Point(191, 209);
            this.numericUpDown_timeout_south.Maximum = new decimal(new int[] {1000, 0, 0, 0});
            this.numericUpDown_timeout_south.Name = "numericUpDown_timeout_south";
            this.numericUpDown_timeout_south.Size = new System.Drawing.Size(96, 23);
            this.numericUpDown_timeout_south.TabIndex = 20;
            this.numericUpDown_timeout_south.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericUpDown_timeout_south_KeyPress);
            // 
            // numericUpDown_timeout_north
            // 
            this.numericUpDown_timeout_north.Location = new System.Drawing.Point(191, 184);
            this.numericUpDown_timeout_north.Maximum = new decimal(new int[] {1000, 0, 0, 0});
            this.numericUpDown_timeout_north.Name = "numericUpDown_timeout_north";
            this.numericUpDown_timeout_north.Size = new System.Drawing.Size(96, 23);
            this.numericUpDown_timeout_north.TabIndex = 19;
            this.numericUpDown_timeout_north.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericUpDown_timeout_north_KeyPress);
            // 
            // label_timeout_south
            // 
            this.label_timeout_south.AutoSize = true;
            this.label_timeout_south.Location = new System.Drawing.Point(3, 211);
            this.label_timeout_south.Name = "label_timeout_south";
            this.label_timeout_south.Size = new System.Drawing.Size(155, 17);
            this.label_timeout_south.TabIndex = 17;
            this.label_timeout_south.Text = "Timeout south (s): NaN";
            // 
            // label_timeout_north
            // 
            this.label_timeout_north.AutoSize = true;
            this.label_timeout_north.Location = new System.Drawing.Point(3, 186);
            this.label_timeout_north.Name = "label_timeout_north";
            this.label_timeout_north.Size = new System.Drawing.Size(153, 17);
            this.label_timeout_north.TabIndex = 14;
            this.label_timeout_north.Text = "Timeout north (s): NaN";
            // 
            // label_butt_south
            // 
            this.label_butt_south.AutoSize = true;
            this.label_butt_south.Location = new System.Drawing.Point(3, 161);
            this.label_butt_south.Name = "label_butt_south";
            this.label_butt_south.Size = new System.Drawing.Size(152, 17);
            this.label_butt_south.TabIndex = 12;
            this.label_butt_south.Text = "Button south: unknown";
            // 
            // label_butt_north
            // 
            this.label_butt_north.AutoSize = true;
            this.label_butt_north.Location = new System.Drawing.Point(3, 139);
            this.label_butt_north.Name = "label_butt_north";
            this.label_butt_north.Size = new System.Drawing.Size(150, 17);
            this.label_butt_north.TabIndex = 11;
            this.label_butt_north.Text = "Button north: unknown";
            // 
            // checkBoxSouth
            // 
            this.checkBoxSouth.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSouth.AutoSize = true;
            this.checkBoxSouth.Checked = true;
            this.checkBoxSouth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSouth.Location = new System.Drawing.Point(281, 22);
            this.checkBoxSouth.Name = "checkBoxSouth";
            this.checkBoxSouth.Size = new System.Drawing.Size(64, 21);
            this.checkBoxSouth.TabIndex = 10;
            this.checkBoxSouth.Text = "South";
            this.checkBoxSouth.UseVisualStyleBackColor = true;
            // 
            // checkBoxNorth
            // 
            this.checkBoxNorth.AutoSize = true;
            this.checkBoxNorth.Checked = true;
            this.checkBoxNorth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNorth.Location = new System.Drawing.Point(191, 22);
            this.checkBoxNorth.Name = "checkBoxNorth";
            this.checkBoxNorth.Size = new System.Drawing.Size(62, 21);
            this.checkBoxNorth.TabIndex = 9;
            this.checkBoxNorth.Text = "North";
            this.checkBoxNorth.UseVisualStyleBackColor = true;
            // 
            // label_Shutter_South
            // 
            this.label_Shutter_South.AutoSize = true;
            this.label_Shutter_South.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Shutter_South.Location = new System.Drawing.Point(3, 112);
            this.label_Shutter_South.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Shutter_South.Name = "label_Shutter_South";
            this.label_Shutter_South.Size = new System.Drawing.Size(157, 17);
            this.label_Shutter_South.TabIndex = 7;
            this.label_Shutter_South.Text = "Shutter south: unknown";
            // 
            // label_Shutter_North
            // 
            this.label_Shutter_North.AutoSize = true;
            this.label_Shutter_North.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Shutter_North.Location = new System.Drawing.Point(3, 90);
            this.label_Shutter_North.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Shutter_North.Name = "label_Shutter_North";
            this.label_Shutter_North.Size = new System.Drawing.Size(155, 17);
            this.label_Shutter_North.TabIndex = 6;
            this.label_Shutter_North.Text = "Shutter north: unknown";
            // 
            // label_Motor_North
            // 
            this.label_Motor_North.AutoSize = true;
            this.label_Motor_North.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Motor_North.Location = new System.Drawing.Point(3, 45);
            this.label_Motor_North.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Motor_North.Name = "label_Motor_North";
            this.label_Motor_North.Size = new System.Drawing.Size(145, 17);
            this.label_Motor_North.TabIndex = 5;
            this.label_Motor_North.Text = "Motor north: unknown";
            // 
            // label_Motor_South
            // 
            this.label_Motor_South.AutoSize = true;
            this.label_Motor_South.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Motor_South.Location = new System.Drawing.Point(3, 67);
            this.label_Motor_South.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Motor_South.Name = "label_Motor_South";
            this.label_Motor_South.Size = new System.Drawing.Size(147, 17);
            this.label_Motor_South.TabIndex = 4;
            this.label_Motor_South.Text = "Motor south: unknown";
            // 
            // label_Dome_Power
            // 
            this.label_Dome_Power.AutoSize = true;
            this.label_Dome_Power.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.label_Dome_Power.Location = new System.Drawing.Point(3, 22);
            this.label_Dome_Power.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Dome_Power.Name = "label_Dome_Power";
            this.label_Dome_Power.Size = new System.Drawing.Size(111, 17);
            this.label_Dome_Power.TabIndex = 3;
            this.label_Dome_Power.Text = "Power: unknown";
            // 
            // checkBox_AutoDome
            // 
            this.checkBox_AutoDome.AutoSize = true;
            this.checkBox_AutoDome.Checked = true;
            this.checkBox_AutoDome.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoDome.Location = new System.Drawing.Point(191, 74);
            this.checkBox_AutoDome.Margin = new System.Windows.Forms.Padding(1);
            this.checkBox_AutoDome.Name = "checkBox_AutoDome";
            this.checkBox_AutoDome.Size = new System.Drawing.Size(142, 21);
            this.checkBox_AutoDome.TabIndex = 2;
            this.checkBox_AutoDome.Text = "Auto dome control\r\n";
            this.checkBox_AutoDome.UseVisualStyleBackColor = true;
            this.checkBox_AutoDome.CheckedChanged += new System.EventHandler(this.CheckBox_AutoDome_CheckedChanged);
            // 
            // comboBox_Dome
            // 
            this.comboBox_Dome.FormattingEnabled = true;
            this.comboBox_Dome.Items.AddRange(new object[] {"Open", "Close", "Stop"});
            this.comboBox_Dome.Location = new System.Drawing.Point(191, 47);
            this.comboBox_Dome.Margin = new System.Windows.Forms.Padding(1);
            this.comboBox_Dome.Name = "comboBox_Dome";
            this.comboBox_Dome.Size = new System.Drawing.Size(78, 24);
            this.comboBox_Dome.TabIndex = 1;
            // 
            // button_Dome_Run
            // 
            this.button_Dome_Run.Location = new System.Drawing.Point(273, 47);
            this.button_Dome_Run.Margin = new System.Windows.Forms.Padding(1);
            this.button_Dome_Run.Name = "button_Dome_Run";
            this.button_Dome_Run.Size = new System.Drawing.Size(60, 25);
            this.button_Dome_Run.TabIndex = 0;
            this.button_Dome_Run.Text = "Run";
            this.button_Dome_Run.UseVisualStyleBackColor = true;
            this.button_Dome_Run.Click += new System.EventHandler(this.Button_Dome_Run_Click);
            // 
            // timerSet
            // 
            this.timerSet.Enabled = false;
            this.timerSet.Interval = 1000;
            this.timerSet.Tick += new System.EventHandler(this.TimerSetTick);
            // 
            // logBox
            // 
            this.logBox.ContextMenuStrip = this.contextMenuStripLogger;
            this.logBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.logBox.FormattingEnabled = true;
            this.logBox.HorizontalScrollbar = true;
            this.logBox.ItemHeight = 16;
            this.logBox.Location = new System.Drawing.Point(0, 317);
            this.logBox.Name = "logBox";
            this.logBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.logBox.Size = new System.Drawing.Size(643, 100);
            this.logBox.TabIndex = 3;
            // 
            // contextMenuStripLogger
            // 
            this.contextMenuStripLogger.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {this.toolStripMenuItemSaveLogs, this.toolStripMenuItemClearLogs});
            this.contextMenuStripLogger.Name = "contextMenuStripLogger";
            this.contextMenuStripLogger.Size = new System.Drawing.Size(102, 48);
            // 
            // toolStripMenuItemSaveLogs
            // 
            this.toolStripMenuItemSaveLogs.Name = "toolStripMenuItemSaveLogs";
            this.toolStripMenuItemSaveLogs.Size = new System.Drawing.Size(101, 22);
            this.toolStripMenuItemSaveLogs.Text = "Save";
            this.toolStripMenuItemSaveLogs.Click += new System.EventHandler(this.toolStripMenuItemSaveLogs_Click);
            // 
            // toolStripMenuItemClearLogs
            // 
            this.toolStripMenuItemClearLogs.Name = "toolStripMenuItemClearLogs";
            this.toolStripMenuItemClearLogs.Size = new System.Drawing.Size(101, 22);
            this.toolStripMenuItemClearLogs.Text = "Clear";
            this.toolStripMenuItemClearLogs.Click += new System.EventHandler(this.toolStripMenuItemClearLogs_Click);
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {this.toolStripStatusLabel});
            this.statusStripMain.Location = new System.Drawing.Point(0, 417);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(643, 22);
            this.statusStripMain.TabIndex = 5;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(170, 17);
            this.toolStripStatusLabel.Text = "UTC: yyyy-MM-ddTHH-mm-ss";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 439);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.groupBox_Dome);
            this.Controls.Add(this.groupBox_Meteo);
            this.Controls.Add(this.statusStripMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "MainForm";
            this.Text = "MeteoDome";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox_Meteo.ResumeLayout(false);
            this.groupBox_Meteo.PerformLayout();
            this.groupBox_Dome.ResumeLayout(false);
            this.groupBox_Dome.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDown_timeout_south)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDown_timeout_north)).EndInit();
            this.contextMenuStripLogger.ResumeLayout(false);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;

        private System.Windows.Forms.StatusStrip statusStripMain;

        #endregion

        private System.Windows.Forms.GroupBox groupBox_Meteo;
        private System.Windows.Forms.GroupBox groupBox_Dome;
        private System.Windows.Forms.Label label_Allsky_ext_STD;
        private System.Windows.Forms.Label label_Seeing;
        private System.Windows.Forms.Label label_Allsky_ext;
        private System.Windows.Forms.Label label_Seeing_ext;
        private System.Windows.Forms.Label label_SkyTempSTD;
        private System.Windows.Forms.Label label_SkyTemp;
        private System.Windows.Forms.Label label_Wind;
        private System.Windows.Forms.Label label_Sun;
        private System.Windows.Forms.Timer timerSet;
        private System.Windows.Forms.ComboBox comboBox_Dome;
        private System.Windows.Forms.CheckBox checkBox_AutoDome;
        private System.Windows.Forms.Button button_Dome_Run;
        private System.Windows.Forms.Label label_Obs_cond;
        private System.Windows.Forms.Label label_Shutter_South;
        private System.Windows.Forms.Label label_Shutter_North;
        private System.Windows.Forms.Label label_Motor_North;
        private System.Windows.Forms.Label label_Motor_South;
        private System.Windows.Forms.Label label_Dome_Power;
        private System.Windows.Forms.Label dome_weather_label;
        private System.Windows.Forms.CheckBox checkBoxSouth;
        private System.Windows.Forms.CheckBox checkBoxNorth;
        private System.Windows.Forms.ListBox logBox;
        private System.Windows.Forms.Label label_butt_south;
        private System.Windows.Forms.Label label_butt_north;
        private System.Windows.Forms.Label label_timeout_north;
        private System.Windows.Forms.Label label_timeout_south;
        private System.Windows.Forms.NumericUpDown numericUpDown_timeout_south;
        private System.Windows.Forms.NumericUpDown numericUpDown_timeout_north;
        private System.Windows.Forms.CheckBox checkBox_initflag;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLogger;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveLogs;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearLogs;
    }
}

