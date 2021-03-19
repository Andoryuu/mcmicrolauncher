using System.Drawing;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        internal Control InitializeAssetsLoading()
        {
            var descriptionLabel = new Label
            {
                Text = "Loading assets, please wait...",
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
                descriptionLabel
            });

            this.StateMachine.OnEntry(State.LoadingAssets, async () =>
            {
                if (!await this.DataStore.GetAssetsPreparedAsync())
                {
                    container.UI(() => container.Visible = true);

                    if (!await this.AssetsLoader.PrepareAssetsAsync())
                    {
                        Application.Exit();
                    }
                }

                this.StateMachine.Call(Trigger.AssetsLoaded);
            });

            this.StateMachine.OnLeave(
                State.LoadingAssets,
                () => container.UI(() => container.Visible = false));

            return container;
        }
    }
}
