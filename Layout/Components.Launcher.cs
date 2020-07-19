using System;
using System.Drawing;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        internal Control InitializeLauncher()
        {
            // Name label
            var nameLabel = new Label
            {
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Width = this.FullSize.Width - 20
            };

            // Regular windowed
            var regWinOpt = new RadioButton
            {
                Text = "Regular windowed",
                ForeColor = Color.White,
                Location = new Point(10, 20),
                Width = this.FullSize.Width - 40
            };

            // Fullscreen windowed
            var fullWinOpt = new RadioButton
            {
                Text = "Borderless fullscreen",
                ForeColor = Color.White,
                Location = new Point(10, 50),
                Width = this.FullSize.Width - 40
            };

            // Settings group
            var optionsGroup = new GroupBox
            {
                Text = "Windowed mode style",
                ForeColor = Color.White,
                Location = new Point(10, 50),
                Height = 80
            };

            optionsGroup.Controls.Add(regWinOpt);
            optionsGroup.Controls.Add(fullWinOpt);

            // Launch button
            var launchButton = new Button
            {
                BackColor = Color.White,
                Location = new Point(130, 150),
                Size = new Size(80, 30),
            };

            launchButton.Click
                += (object sender, EventArgs e)
                => this.AuthClient.LaunchMinecraft(fullWinOpt.Checked);

            var container = new ContainerControl
            {
                Size = this.FullSize,
                Visible = false
            };

            container.Controls.AddRange(new Control[]
            {
                nameLabel,
                optionsGroup,
                launchButton
            });

            this.StateMachine.OnEntry(
                State.Launcher,
                () =>
                {
                    var userName = this.AuthClient.GetName();
                    container.UI(() =>
                    {
                        launchButton.Text = "Launch";
                        launchButton.Enabled = true;
                        nameLabel.Text = $"Logged in as {userName}";
                        container.Visible = true;
                    });
                });

            this.StateMachine.OnEntry(
                State.MinecraftRunning,
                () => container.UI(() =>
                {
                    launchButton.Text = "Running";
                    launchButton.Enabled = false;
                    container.Visible = true;
                }));

            this.StateMachine.OnLeave(
                State.Launcher,
                () => container.UI(() => container.Visible = false));

            return container;
        }
    }
}
