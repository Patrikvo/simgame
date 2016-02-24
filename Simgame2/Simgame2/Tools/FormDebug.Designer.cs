namespace Simgame2.Tools
{
    partial class FormDebug
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBarYaw = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.trackBarPitch = new System.Windows.Forms.TrackBar();
            this.trackBarRoll = new System.Windows.Forms.TrackBar();
            this.labelLightvalues = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.tabPageLight = new System.Windows.Forms.TabPage();
            this.tabPageShadow = new System.Windows.Forms.TabPage();
            this.labelRenderer = new System.Windows.Forms.Label();
            this.comboBoxRenderer = new System.Windows.Forms.ComboBox();
            this.textBoxNormalBias = new System.Windows.Forms.TextBox();
            this.labelNormalBias = new System.Windows.Forms.Label();
            this.textBoxShadowBias = new System.Windows.Forms.TextBox();
            this.labelShadowBias = new System.Windows.Forms.Label();
            this.textBoxShadowFarPlane = new System.Windows.Forms.TextBox();
            this.labelShadowFarPlane = new System.Windows.Forms.Label();
            this.textBoxlShadowTargetZ = new System.Windows.Forms.TextBox();
            this.labellShadowTargetZ = new System.Windows.Forms.Label();
            this.textBoxlShadowTargetY = new System.Windows.Forms.TextBox();
            this.labellShadowTargetY = new System.Windows.Forms.Label();
            this.textBoxlShadowTargetX = new System.Windows.Forms.TextBox();
            this.labelShadowTargetX = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxShadowPosZ = new System.Windows.Forms.TextBox();
            this.labelShadowPosZ = new System.Windows.Forms.Label();
            this.textBoxShadowPosY = new System.Windows.Forms.TextBox();
            this.labelShadowPosY = new System.Windows.Forms.Label();
            this.textBoxShadowPosX = new System.Windows.Forms.TextBox();
            this.labelShadowPosX = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarYaw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRoll)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.tabPageLight.SuspendLayout();
            this.tabPageShadow.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(30, 46);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox.Size = new System.Drawing.Size(164, 95);
            this.textBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Main Light Orientation";
            // 
            // trackBarYaw
            // 
            this.trackBarYaw.Location = new System.Drawing.Point(43, 68);
            this.trackBarYaw.Maximum = 359;
            this.trackBarYaw.Name = "trackBarYaw";
            this.trackBarYaw.Size = new System.Drawing.Size(225, 45);
            this.trackBarYaw.TabIndex = 2;
            this.trackBarYaw.Scroll += new System.EventHandler(this.trackBarMainLight_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Yaw";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Pitch";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 160);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Roll";
            // 
            // trackBarPitch
            // 
            this.trackBarPitch.Location = new System.Drawing.Point(46, 105);
            this.trackBarPitch.Maximum = 359;
            this.trackBarPitch.Name = "trackBarPitch";
            this.trackBarPitch.Size = new System.Drawing.Size(222, 45);
            this.trackBarPitch.TabIndex = 6;
            this.trackBarPitch.Scroll += new System.EventHandler(this.trackBarMainLight_Scroll);
            // 
            // trackBarRoll
            // 
            this.trackBarRoll.Location = new System.Drawing.Point(46, 141);
            this.trackBarRoll.Maximum = 359;
            this.trackBarRoll.Name = "trackBarRoll";
            this.trackBarRoll.Size = new System.Drawing.Size(222, 45);
            this.trackBarRoll.TabIndex = 7;
            this.trackBarRoll.Scroll += new System.EventHandler(this.trackBarMainLight_Scroll);
            // 
            // labelLightvalues
            // 
            this.labelLightvalues.AutoSize = true;
            this.labelLightvalues.Location = new System.Drawing.Point(9, 39);
            this.labelLightvalues.Name = "labelLightvalues";
            this.labelLightvalues.Size = new System.Drawing.Size(83, 13);
            this.labelLightvalues.TabIndex = 8;
            this.labelLightvalues.Text = "labelLightvalues";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageGeneral);
            this.tabControl1.Controls.Add(this.tabPageLight);
            this.tabControl1.Controls.Add(this.tabPageShadow);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(284, 570);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.textBox);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(276, 544);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // tabPageLight
            // 
            this.tabPageLight.Controls.Add(this.trackBarYaw);
            this.tabPageLight.Controls.Add(this.labelLightvalues);
            this.tabPageLight.Controls.Add(this.label1);
            this.tabPageLight.Controls.Add(this.trackBarRoll);
            this.tabPageLight.Controls.Add(this.label2);
            this.tabPageLight.Controls.Add(this.trackBarPitch);
            this.tabPageLight.Controls.Add(this.label3);
            this.tabPageLight.Controls.Add(this.label4);
            this.tabPageLight.Location = new System.Drawing.Point(4, 22);
            this.tabPageLight.Name = "tabPageLight";
            this.tabPageLight.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLight.Size = new System.Drawing.Size(276, 544);
            this.tabPageLight.TabIndex = 1;
            this.tabPageLight.Text = "Light";
            this.tabPageLight.UseVisualStyleBackColor = true;
            // 
            // tabPageShadow
            // 
            this.tabPageShadow.Controls.Add(this.labelRenderer);
            this.tabPageShadow.Controls.Add(this.comboBoxRenderer);
            this.tabPageShadow.Controls.Add(this.textBoxNormalBias);
            this.tabPageShadow.Controls.Add(this.labelNormalBias);
            this.tabPageShadow.Controls.Add(this.textBoxShadowBias);
            this.tabPageShadow.Controls.Add(this.labelShadowBias);
            this.tabPageShadow.Controls.Add(this.textBoxShadowFarPlane);
            this.tabPageShadow.Controls.Add(this.labelShadowFarPlane);
            this.tabPageShadow.Controls.Add(this.textBoxlShadowTargetZ);
            this.tabPageShadow.Controls.Add(this.labellShadowTargetZ);
            this.tabPageShadow.Controls.Add(this.textBoxlShadowTargetY);
            this.tabPageShadow.Controls.Add(this.labellShadowTargetY);
            this.tabPageShadow.Controls.Add(this.textBoxlShadowTargetX);
            this.tabPageShadow.Controls.Add(this.labelShadowTargetX);
            this.tabPageShadow.Controls.Add(this.label9);
            this.tabPageShadow.Controls.Add(this.textBoxShadowPosZ);
            this.tabPageShadow.Controls.Add(this.labelShadowPosZ);
            this.tabPageShadow.Controls.Add(this.textBoxShadowPosY);
            this.tabPageShadow.Controls.Add(this.labelShadowPosY);
            this.tabPageShadow.Controls.Add(this.textBoxShadowPosX);
            this.tabPageShadow.Controls.Add(this.labelShadowPosX);
            this.tabPageShadow.Controls.Add(this.label5);
            this.tabPageShadow.Location = new System.Drawing.Point(4, 22);
            this.tabPageShadow.Name = "tabPageShadow";
            this.tabPageShadow.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageShadow.Size = new System.Drawing.Size(276, 544);
            this.tabPageShadow.TabIndex = 2;
            this.tabPageShadow.Text = "Shadow";
            this.tabPageShadow.UseVisualStyleBackColor = true;
            // 
            // labelRenderer
            // 
            this.labelRenderer.AutoSize = true;
            this.labelRenderer.Location = new System.Drawing.Point(8, 142);
            this.labelRenderer.Name = "labelRenderer";
            this.labelRenderer.Size = new System.Drawing.Size(51, 13);
            this.labelRenderer.TabIndex = 21;
            this.labelRenderer.Text = "Renderer";
            // 
            // comboBoxRenderer
            // 
            this.comboBoxRenderer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRenderer.FormattingEnabled = true;
            this.comboBoxRenderer.Location = new System.Drawing.Point(11, 158);
            this.comboBoxRenderer.Name = "comboBoxRenderer";
            this.comboBoxRenderer.Size = new System.Drawing.Size(121, 21);
            this.comboBoxRenderer.TabIndex = 20;
            this.comboBoxRenderer.SelectedIndexChanged += new System.EventHandler(this.comboBoxRenderer_SelectedIndexChanged);
            // 
            // textBoxNormalBias
            // 
            this.textBoxNormalBias.Location = new System.Drawing.Point(158, 159);
            this.textBoxNormalBias.Name = "textBoxNormalBias";
            this.textBoxNormalBias.Size = new System.Drawing.Size(100, 20);
            this.textBoxNormalBias.TabIndex = 19;
            this.textBoxNormalBias.Leave += new System.EventHandler(this.textBoxNormalBias_Leave);
            // 
            // labelNormalBias
            // 
            this.labelNormalBias.AutoSize = true;
            this.labelNormalBias.Location = new System.Drawing.Point(155, 143);
            this.labelNormalBias.Name = "labelNormalBias";
            this.labelNormalBias.Size = new System.Drawing.Size(60, 13);
            this.labelNormalBias.TabIndex = 18;
            this.labelNormalBias.Text = "NormalBias";
            // 
            // textBoxShadowBias
            // 
            this.textBoxShadowBias.Location = new System.Drawing.Point(158, 117);
            this.textBoxShadowBias.Name = "textBoxShadowBias";
            this.textBoxShadowBias.Size = new System.Drawing.Size(100, 20);
            this.textBoxShadowBias.TabIndex = 17;
            this.textBoxShadowBias.Leave += new System.EventHandler(this.textBoxShadowBias_Leave);
            // 
            // labelShadowBias
            // 
            this.labelShadowBias.AutoSize = true;
            this.labelShadowBias.Location = new System.Drawing.Point(155, 101);
            this.labelShadowBias.Name = "labelShadowBias";
            this.labelShadowBias.Size = new System.Drawing.Size(83, 13);
            this.labelShadowBias.TabIndex = 16;
            this.labelShadowBias.Text = "1 / ShadowBias";
            // 
            // textBoxShadowFarPlane
            // 
            this.textBoxShadowFarPlane.Location = new System.Drawing.Point(11, 117);
            this.textBoxShadowFarPlane.Name = "textBoxShadowFarPlane";
            this.textBoxShadowFarPlane.Size = new System.Drawing.Size(100, 20);
            this.textBoxShadowFarPlane.TabIndex = 15;
            this.textBoxShadowFarPlane.Leave += new System.EventHandler(this.textBoxShadowFarPlane_Leave);
            // 
            // labelShadowFarPlane
            // 
            this.labelShadowFarPlane.AutoSize = true;
            this.labelShadowFarPlane.Location = new System.Drawing.Point(8, 101);
            this.labelShadowFarPlane.Name = "labelShadowFarPlane";
            this.labelShadowFarPlane.Size = new System.Drawing.Size(88, 13);
            this.labelShadowFarPlane.TabIndex = 14;
            this.labelShadowFarPlane.Text = "ShadowFarPlane";
            // 
            // textBoxlShadowTargetZ
            // 
            this.textBoxlShadowTargetZ.Location = new System.Drawing.Point(198, 75);
            this.textBoxlShadowTargetZ.Name = "textBoxlShadowTargetZ";
            this.textBoxlShadowTargetZ.Size = new System.Drawing.Size(60, 20);
            this.textBoxlShadowTargetZ.TabIndex = 13;
            // 
            // labellShadowTargetZ
            // 
            this.labellShadowTargetZ.AutoSize = true;
            this.labellShadowTargetZ.Location = new System.Drawing.Point(178, 78);
            this.labellShadowTargetZ.Name = "labellShadowTargetZ";
            this.labellShadowTargetZ.Size = new System.Drawing.Size(14, 13);
            this.labellShadowTargetZ.TabIndex = 12;
            this.labellShadowTargetZ.Text = "Z";
            // 
            // textBoxlShadowTargetY
            // 
            this.textBoxlShadowTargetY.Location = new System.Drawing.Point(113, 75);
            this.textBoxlShadowTargetY.Name = "textBoxlShadowTargetY";
            this.textBoxlShadowTargetY.Size = new System.Drawing.Size(60, 20);
            this.textBoxlShadowTargetY.TabIndex = 11;
            // 
            // labellShadowTargetY
            // 
            this.labellShadowTargetY.AutoSize = true;
            this.labellShadowTargetY.Location = new System.Drawing.Point(93, 78);
            this.labellShadowTargetY.Name = "labellShadowTargetY";
            this.labellShadowTargetY.Size = new System.Drawing.Size(14, 13);
            this.labellShadowTargetY.TabIndex = 10;
            this.labellShadowTargetY.Text = "Y";
            // 
            // textBoxlShadowTargetX
            // 
            this.textBoxlShadowTargetX.Location = new System.Drawing.Point(28, 75);
            this.textBoxlShadowTargetX.Name = "textBoxlShadowTargetX";
            this.textBoxlShadowTargetX.Size = new System.Drawing.Size(60, 20);
            this.textBoxlShadowTargetX.TabIndex = 9;
            this.textBoxlShadowTargetX.Leave += new System.EventHandler(this.textBoxlShadowTarget_Leave);
            // 
            // labelShadowTargetX
            // 
            this.labelShadowTargetX.AutoSize = true;
            this.labelShadowTargetX.Location = new System.Drawing.Point(8, 78);
            this.labelShadowTargetX.Name = "labelShadowTargetX";
            this.labelShadowTargetX.Size = new System.Drawing.Size(14, 13);
            this.labelShadowTargetX.TabIndex = 8;
            this.labelShadowTargetX.Text = "X";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 59);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "ShadowLightTarget";
            // 
            // textBoxShadowPosZ
            // 
            this.textBoxShadowPosZ.Location = new System.Drawing.Point(198, 29);
            this.textBoxShadowPosZ.Name = "textBoxShadowPosZ";
            this.textBoxShadowPosZ.Size = new System.Drawing.Size(60, 20);
            this.textBoxShadowPosZ.TabIndex = 6;
            this.textBoxShadowPosZ.Leave += new System.EventHandler(this.textBoxShadowPos_Leave);
            // 
            // labelShadowPosZ
            // 
            this.labelShadowPosZ.AutoSize = true;
            this.labelShadowPosZ.Location = new System.Drawing.Point(178, 32);
            this.labelShadowPosZ.Name = "labelShadowPosZ";
            this.labelShadowPosZ.Size = new System.Drawing.Size(14, 13);
            this.labelShadowPosZ.TabIndex = 5;
            this.labelShadowPosZ.Text = "Z";
            // 
            // textBoxShadowPosY
            // 
            this.textBoxShadowPosY.Location = new System.Drawing.Point(113, 29);
            this.textBoxShadowPosY.Name = "textBoxShadowPosY";
            this.textBoxShadowPosY.Size = new System.Drawing.Size(60, 20);
            this.textBoxShadowPosY.TabIndex = 4;
            this.textBoxShadowPosY.Leave += new System.EventHandler(this.textBoxShadowPos_Leave);
            // 
            // labelShadowPosY
            // 
            this.labelShadowPosY.AutoSize = true;
            this.labelShadowPosY.Location = new System.Drawing.Point(93, 32);
            this.labelShadowPosY.Name = "labelShadowPosY";
            this.labelShadowPosY.Size = new System.Drawing.Size(14, 13);
            this.labelShadowPosY.TabIndex = 3;
            this.labelShadowPosY.Text = "Y";
            // 
            // textBoxShadowPosX
            // 
            this.textBoxShadowPosX.Location = new System.Drawing.Point(28, 29);
            this.textBoxShadowPosX.Name = "textBoxShadowPosX";
            this.textBoxShadowPosX.Size = new System.Drawing.Size(60, 20);
            this.textBoxShadowPosX.TabIndex = 2;
            this.textBoxShadowPosX.Leave += new System.EventHandler(this.textBoxShadowPos_Leave);
            // 
            // labelShadowPosX
            // 
            this.labelShadowPosX.AutoSize = true;
            this.labelShadowPosX.Location = new System.Drawing.Point(8, 32);
            this.labelShadowPosX.Name = "labelShadowPosX";
            this.labelShadowPosX.Size = new System.Drawing.Size(14, 13);
            this.labelShadowPosX.TabIndex = 1;
            this.labelShadowPosX.Text = "X";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "ShadowLightPosition";
            // 
            // FormDebug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 570);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDebug";
            this.Text = "FormDebug";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarYaw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRoll)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.tabPageLight.ResumeLayout(false);
            this.tabPageLight.PerformLayout();
            this.tabPageShadow.ResumeLayout(false);
            this.tabPageShadow.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBarYaw;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar trackBarPitch;
        private System.Windows.Forms.TrackBar trackBarRoll;
        private System.Windows.Forms.Label labelLightvalues;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageLight;
        private System.Windows.Forms.TabPage tabPageShadow;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxShadowPosZ;
        private System.Windows.Forms.Label labelShadowPosZ;
        private System.Windows.Forms.TextBox textBoxShadowPosY;
        private System.Windows.Forms.Label labelShadowPosY;
        private System.Windows.Forms.TextBox textBoxShadowPosX;
        private System.Windows.Forms.Label labelShadowPosX;
        private System.Windows.Forms.TextBox textBoxlShadowTargetZ;
        private System.Windows.Forms.Label labellShadowTargetZ;
        private System.Windows.Forms.TextBox textBoxlShadowTargetY;
        private System.Windows.Forms.Label labellShadowTargetY;
        private System.Windows.Forms.TextBox textBoxlShadowTargetX;
        private System.Windows.Forms.Label labelShadowTargetX;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxShadowFarPlane;
        private System.Windows.Forms.Label labelShadowFarPlane;
        private System.Windows.Forms.TextBox textBoxShadowBias;
        private System.Windows.Forms.Label labelShadowBias;
        private System.Windows.Forms.TextBox textBoxNormalBias;
        private System.Windows.Forms.Label labelNormalBias;
        private System.Windows.Forms.Label labelRenderer;
        private System.Windows.Forms.ComboBox comboBoxRenderer;



    }
}