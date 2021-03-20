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
                Text = "Downloading assets, please wait...",
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 60),
                Width = this.FullSize.Width - 20,
                Height = 30
            };

            var countLabel = new Label
            {
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 90),
                Width = this.FullSize.Width - 20,
                Height = 30
            };

            var container = new ContainerControl
            {
                Size = this.FullSize,
                Visible = false
            };

            container.Controls.AddRange(new Control[]
            {
                descriptionLabel,
                countLabel
            });

            this.StateMachine.OnEntry(State.LoadingAssets, async () =>
            {
                if (await this.DataStore.GetAssetsPreparedAsync())
                {
                    this.StateMachine.Call(Trigger.AssetsLoaded);
                    return;
                }

                container.UI(() => container.Visible = true);

                var (count, progress)
                    = await this.AssetsLoader.PrepareAssetsAsync();

                if (count < 0)
                {
                    Application.Exit();
                }

                var progCount = 0;
                await foreach (var item in progress)
                {
                    if (!item)
                    {
                        Application.Exit();
                    }

                    progCount++;

                    container.UI(() =>
                        countLabel.Text = $"{progCount} / {count}");
                }

                await this.DataStore.SetAssetsPreparedAsync(true);
                this.StateMachine.Call(Trigger.AssetsLoaded);
            });

            this.StateMachine.OnLeave(
                State.LoadingAssets,
                () => container.UI(() => container.Visible = false));

            return container;
        }
    }
}
