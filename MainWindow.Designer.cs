using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MCMicroLauncher.Layout;

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
            this.StateMachine.Dispose();

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
            var fullSize = new Size(220, 190);

            this.SuspendLayout();

            // Form
            this.Text = "MC μLauncher";
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = fullSize;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ShowIcon = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.DarkOliveGreen;

            var cmpt = new Components(
                this.StateMachine,
                this.AuthClient,
                this.DataStore,
                this.JavaCaller,
                fullSize);

            this.Controls.Add(cmpt.InitializeLoginCheck());
            this.Controls.Add(cmpt.InitializeLogin());
            this.Controls.Add(cmpt.InitializeOptionsCheck());
            this.Controls.Add(cmpt.InitializeLauncher());

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}

