namespace SimpleWinForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSick = new Button();
            lblGoodJob = new Label();
            SuspendLayout();
            // 
            // btnSick
            // 
            btnSick.Location = new Point(339, 130);
            btnSick.Name = "btnSick";
            btnSick.Size = new Size(75, 23);
            btnSick.TabIndex = 0;
            btnSick.Text = "Sick Button";
            btnSick.UseVisualStyleBackColor = true;
            btnSick.Click += btnSick_Click;
            // 
            // lblGoodJob
            // 
            lblGoodJob.AutoSize = true;
            lblGoodJob.Location = new Point(358, 205);
            lblGoodJob.Name = "lblGoodJob";
            lblGoodJob.Size = new Size(38, 15);
            lblGoodJob.TabIndex = 1;
            lblGoodJob.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblGoodJob);
            Controls.Add(btnSick);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSick;
        private Label lblGoodJob;
    }
}