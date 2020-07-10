using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MCMicroLauncher
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = new Container();

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
            var nameLabel = new Label
            {
                Text = "Account email",
                ForeColor = Color.White,
                Location = new Point(10, 10)
            }

            // Name box
            this.nameBox = new TextBox
            {
                Location = new Point(10, 40),
                Width = 200
            };

            // Pass label
            var passLabel = new Label
            {
                Text = "Password",
                ForeColor = Color.White,
                Location = new Point(10, 80)
            };

            // Pass box
            this.passBox = new TextBox
            {
                UseSystemPasswordChar = true,
                Location = new Point(10, 110),
                Width = 200
            };

            // Login button
            this.loginButton = new Button
            {
                Text = "Login",
                BackColor = Color.White,
                Location = new Point(60, 150),
                Size = new Size(100, 30),
                TabStop = false,
                Click += loginButton_Click
            };

            // Form
            this.Text = "MC μLauncher";
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(220, 190);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ShowIcon = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.DarkOliveGreen;

            this.Controls.AddRange(new Control[]
            {
                nameLabel,
                this.nameBox,
                passLabel,
                this.passBox,
                this.loginButton
            });
            this.AcceptButton = loginButton;

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

