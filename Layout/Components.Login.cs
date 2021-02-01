using System;
using System.Drawing;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        internal Control InitializeLogin()
        {
            // Name label
            var nameLabel = new Label
            {
                Text = "Account email",
                ForeColor = Color.White,
                Location = new Point(10, 10)
            };

            // Name box
            var nameBox = new TextBox
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
            var passBox = new TextBox
            {
                UseSystemPasswordChar = true,
                Location = new Point(10, 110),
                Width = 200
            };

            // Login button
            var loginButton = new Button
            {
                BackColor = Color.White,
                Location = new Point(60, 150),
                Size = new Size(100, 30),
                TabStop = false
            };

            nameBox.KeyPress += (object sender, KeyPressEventArgs e) =>
            {
                if (e.KeyChar == 13)
                {
                    loginButton.PerformClick();
                }
            };

            passBox.KeyPress += (object sender, KeyPressEventArgs e) =>
            {
                if (e.KeyChar == 13)
                {
                    loginButton.PerformClick();
                }
            };

            loginButton.Click += async (object sender, EventArgs e) =>
            {
                nameBox.Enabled = false;
                passBox.Enabled = false;
                loginButton.Enabled = false;
                loginButton.Text = "Authenticating";

                await this.DataStore.SetLoginName(nameBox.Text);

                this.StateMachine.Call(await this.AuthClient
                        .AuthenticateAsync(nameBox.Text, passBox.Text)
                    ? Trigger.LoginSuccess
                    : Trigger.LoginFailed);
            };

            var container = new ContainerControl
            {
                Size = this.FullSize,
                Visible = false
            };

            container.Controls.AddRange(new Control[]
            {
                nameLabel,
                nameBox,
                passLabel,
                passBox,
                loginButton
            });

            this.StateMachine.OnEntry(
                State.Login,
                async () =>
                {
                    var loginName = await this.DataStore.GetLoginName();

                    container.UI(() =>
                    {
                        loginButton.Text = "Login";
                        loginButton.Enabled = true;
                        nameBox.Text = loginName;
                        nameBox.Enabled = true;
                        passBox.Text = "";
                        passBox.Enabled = true;
                        container.Visible = true;

                        (string.IsNullOrWhiteSpace(loginName)
                            ? nameBox
                            : passBox)
                            .Focus();
                    });
                });

            this.StateMachine.OnLeave(
                State.Login,
                () => container.UI(() => container.Visible = false));

            return container;
        }
    }
}
