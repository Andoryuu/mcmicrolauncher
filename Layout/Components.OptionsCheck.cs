using System;
using System.Drawing;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Utils;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        internal Control InitializeOptionsCheck()
        {
            var fullWidth = this.FullSize.Width - 20;

            var descriptionLabel = new Label
            {
                Text = "No local option files detected",
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Width = fullWidth
            };

            var importDefaultButton = new Button
            {
                Text = "Import from default MC",
                BackColor = Color.White,
                Location = new Point(10, 40),
                Size = new Size(fullWidth, 30)
            };

            var importSpecificButton = new Button
            {
                Text = "Import specific files",
                BackColor = Color.White,
                Location = new Point(10, 80),
                Size = new Size(fullWidth, 30)
            };

            var generateDefaultButton = new Button
            {
                Text = "Let MC generate default options",
                BackColor = Color.White,
                Location = new Point(10, 120),
                Size = new Size(fullWidth, 30)
            };

            var container = new ContainerControl
            {
                Size = this.FullSize,
                Visible = false
            };

            importDefaultButton.Click += (object sender, EventArgs e) =>
            {
                container.Enabled = false;

                var fileNames = OptionsImporter.GetDefaultOptionsPaths();

                var result = OptionsImporter.ImportOptions(fileNames)
                    ? Trigger.OptionsResolved
                    : Trigger.OptionsMissing;

                this.StateMachine.Call(result);
            };

            importSpecificButton.Click += (object sender, EventArgs e) =>
            {
                container.Enabled = false;

                var fileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = $"MC options ({Constants.OptionsPattern})|"
                        + Constants.OptionsPattern
                };

                var result = fileDialog.ShowDialog() == DialogResult.OK
                    && fileDialog.FileNames is var fileNames
                    && fileNames.Length > 0
                    && OptionsImporter.ImportOptions(fileNames);

                this.StateMachine.Call(result
                    ? Trigger.OptionsResolved
                    : Trigger.OptionsMissing);
            };

            generateDefaultButton.Click
                += (object sender, EventArgs e)
                => this.StateMachine.Call(Trigger.OptionsResolved);

            container.Controls.AddRange(new Control[]
            {
                descriptionLabel,
                importDefaultButton,
                importSpecificButton,
                generateDefaultButton
            });

            this.StateMachine.OnEntry(State.OptionsCheck, () =>
            {
                if (OptionsImporter.HasLocalOptions())
                {
                    this.StateMachine.Call(Trigger.OptionsResolved);
                    return;
                }

                var hasDefaultOptions = OptionsImporter.HasDefaultOptions();

                container.UI(() =>
                {
                    importDefaultButton.Enabled = hasDefaultOptions;

                    container.Enabled = true;
                    container.Visible = true;
                });
            });

            this.StateMachine.OnLeave(
                State.OptionsCheck,
                () => container.UI(() => container.Visible = false));

            return container;
        }
    }
}
