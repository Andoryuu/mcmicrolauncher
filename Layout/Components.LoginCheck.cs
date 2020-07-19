using System.Drawing;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Authentication;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        internal Control InitializeLoginCheck()
        {
            // State label
            var stateLabel = new Label
            {
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 70),
                Width = this.FullSize.Width - 20,
                Height = 50
            };

            var container = new ContainerControl
            {
                Size = this.FullSize,
                Visible = false
            };

            container.Controls.AddRange(new Control[]
            {
                stateLabel
            });

            this.StateMachine.OnEntry(State.Validation, async () =>
            {
                container.UI(() =>
                {
                    stateLabel.Text = "Validating login token\n...";
                    container.Visible = true;
                });

                this.StateMachine.Call(await this.AuthClient.Validate()
                    ? Trigger.ValidationSuccess
                    : Trigger.ValidationFailed);
            });

            this.StateMachine.OnEntry(State.Refresh, async () =>
            {
                container.UI(() =>
                {
                    stateLabel.Text = "Refreshing login token\n...";
                    container.Visible = true;
                });

                this.StateMachine.Call(await this.AuthClient.Refresh()
                    ? Trigger.RefreshSuccess
                    : Trigger.RefreshFailed);
            });

            this.StateMachine.OnLeave(
                State.Validation,
                () => container.UI(() => container.Visible = false));

            this.StateMachine.OnLeave(
                State.Refresh,
                () => container.UI(() => container.Visible = false));

            return container;
        }
    }
}
