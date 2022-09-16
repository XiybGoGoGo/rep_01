namespace TcIntelligentTechnologyUI.CForms
{
    partial class formParameter
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
            this.buttonCameraCalibrate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonMultipleLaser = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCameraCalibrate
            // 
            this.buttonCameraCalibrate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCameraCalibrate.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonCameraCalibrate.Location = new System.Drawing.Point(6, 23);
            this.buttonCameraCalibrate.Name = "buttonCameraCalibrate";
            this.buttonCameraCalibrate.Size = new System.Drawing.Size(173, 37);
            this.buttonCameraCalibrate.TabIndex = 7;
            this.buttonCameraCalibrate.Text = "相机内外参标定";
            this.buttonCameraCalibrate.UseVisualStyleBackColor = true;
            this.buttonCameraCalibrate.Click += new System.EventHandler(this.buttonCameraCalibrate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.propertyGrid1);
            this.groupBox1.Controls.Add(this.buttonMultipleLaser);
            this.groupBox1.Controls.Add(this.buttonCameraCalibrate);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(1293, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(185, 700);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Tag = "3";
            this.groupBox1.Text = "功能按钮";
            // 
            // buttonMultipleLaser
            // 
            this.buttonMultipleLaser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMultipleLaser.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonMultipleLaser.Location = new System.Drawing.Point(6, 66);
            this.buttonMultipleLaser.Name = "buttonMultipleLaser";
            this.buttonMultipleLaser.Size = new System.Drawing.Size(173, 37);
            this.buttonMultipleLaser.TabIndex = 11;
            this.buttonMultipleLaser.Text = "激光点计算";
            this.buttonMultipleLaser.UseVisualStyleBackColor = true;
            this.buttonMultipleLaser.Click += new System.EventHandler(this.buttonMultipleLaser_Click);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Location = new System.Drawing.Point(6, 184);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(179, 469);
            this.propertyGrid1.TabIndex = 12;
            // 
            // formParameter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1478, 700);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "formParameter";
            this.Text = "m_formCalibration";
            this.Load += new System.EventHandler(this.m_formVisionControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Button buttonCameraCalibrate;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Button buttonMultipleLaser;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
    }
}