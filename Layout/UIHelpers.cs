using System;
using System.Windows.Forms;

namespace MCMicroLauncher.Layout
{
    internal static class UIHelpers
    {
        internal static void UI(this Control control, Action updates)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(updates));
            }
            else
            {
                updates();
            }
        }
    }
}
