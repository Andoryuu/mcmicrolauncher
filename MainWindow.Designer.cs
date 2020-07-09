using System;
using System.Drawing;
using System.Windows.Forms;

namespace MCMicroLauncher
{
    partial class MainWindow
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

        private TextBox nameBox;
        private TextBox passBox;
        private Button loginButton;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Name label
            var nameLabel = new Label();
            nameLabel.Text = "Account email";
            nameLabel.ForeColor = Color.White;
            nameLabel.Top = 10;
            nameLabel.Left = 10;

            // Name box
            this.nameBox = new TextBox();
            this.nameBox.Left = 10;
            this.nameBox.Top = 40;
            this.nameBox.Width = 200;

            // Pass label
            var passLabel = new Label();
            passLabel.Text = "Password";
            passLabel.ForeColor = Color.White;
            passLabel.Top = 80;
            passLabel.Left = 10;

            // Pass box
            this.passBox = new TextBox();
            this.passBox.UseSystemPasswordChar = true;
            this.passBox.Left = 10;
            this.passBox.Top = 110;
            this.passBox.Width = 200;

            // Login button
            this.loginButton = new Button();
            this.loginButton.Text = "Login";
            this.loginButton.ForeColor = Color.White;
            this.loginButton.Left = 60;
            this.loginButton.Top = 150;
            this.loginButton.Width = 100;
            this.loginButton.Height = 30;
            this.loginButton.TabStop = false;
            this.loginButton.Click += loginButton_Click;

            // Form
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(220, 190);
            this.BackColor = Color.DarkOliveGreen;
            this.ShowIcon = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Controls.AddRange(new Control[]
            {
                nameLabel,
                this.nameBox,
                passLabel,
                this.passBox,
                this.loginButton
            });
            this.AcceptButton = loginButton;
            this.Text = "MC μLauncher";

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            await AuthClient.Authenticate(this.nameBox.Text, this.passBox.Text);
            // Console.WriteLine($"click: {this.nameBox.Text} - {this.passBox.Text}");
        }

        #endregion
    }
}

