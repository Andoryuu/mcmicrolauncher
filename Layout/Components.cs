using System.Drawing;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Authentication;
using MCMicroLauncher.Utils;

namespace MCMicroLauncher.Layout
{
    internal partial class Components
    {
        private readonly StateMachine<State, Trigger> StateMachine;
        private readonly Size FullSize;
        private readonly AuthClient AuthClient;
        private readonly DataStore DataStore;
        private readonly JavaCaller JavaCaller;
        private readonly AssetsLoader AssetsLoader;

        internal Components(
            StateMachine<State, Trigger> stateMachine,
            AuthClient authClient,
            DataStore dataStore,
            JavaCaller javaCaller,
            AssetsLoader assetsLoader,
            Size fullSize)
        {
            this.StateMachine = stateMachine;
            this.AuthClient = authClient;
            this.FullSize = fullSize;
            this.DataStore = dataStore;
            this.JavaCaller = javaCaller;
            this.AssetsLoader = assetsLoader;
        }
    }
}
