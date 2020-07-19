using System;
using System.Windows.Forms;

namespace MCMicroLauncher.Layout
{
    internal static class UIHelpers
    {
        internal static void UI(this Control control, Action updates)
        {
            control.Invoke(new MethodInvoker(updates));
        }
    }
}
