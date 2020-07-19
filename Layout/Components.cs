using System.Drawing;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Authentication;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        private readonly StateMachine<State, Trigger> StateMachine;
        private readonly Size FullSize;
        private readonly AuthClient AuthClient;

        internal Components(
            StateMachine<State, Trigger> stateMachine,
            AuthClient authClient,
            Size fullSize)
        {
            this.StateMachine = stateMachine;
            this.AuthClient = authClient;
            this.FullSize = fullSize;
        }
    }
}
