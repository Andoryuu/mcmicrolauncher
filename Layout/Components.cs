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
        private readonly DataStore DataStore;
        private readonly JavaCaller JavaCaller;

        internal Components(
            StateMachine<State, Trigger> stateMachine,
            AuthClient authClient,
            DataStore dataStore,
            JavaCaller javaCaller,
            Size fullSize)
        {
            this.StateMachine = stateMachine;
            this.AuthClient = authClient;
            this.FullSize = fullSize;
            this.DataStore = dataStore;
            this.JavaCaller = javaCaller;
        }
    }
}
