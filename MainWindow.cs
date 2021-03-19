using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Authentication;
using MCMicroLauncher.Utils;

namespace MCMicroLauncher
{
    public partial class MainWindow : Form
    {
        private readonly StateMachine<State, Trigger> StateMachine;

        private readonly AuthClient AuthClient;

        private readonly DataStore DataStore;

        private readonly JavaCaller JavaCaller;

        private readonly AssetsLoader AssetsLoader;

        public MainWindow()
        {
            this.DataStore = new DataStore();

            this.StateMachine = new StateMachine<State, Trigger>
            {
                { (State.Closed, Trigger.Start), State.Validation },

                { (State.Validation, Trigger.ValidationSuccess), State.LoadingAssets },
                { (State.Validation, Trigger.ValidationFailed), State.Refresh },

                { (State.Refresh, Trigger.RefreshSuccess), State.LoadingAssets },
                { (State.Refresh, Trigger.RefreshFailed), State.Login },

                { (State.Login, Trigger.LoginSuccess), State.LoadingAssets },
                { (State.Login, Trigger.LoginFailed), State.Login },

                { (State.LoadingAssets, Trigger.AssetsLoaded), State.OptionsCheck },

                { (State.OptionsCheck, Trigger.OptionsResolved), State.Launcher },
                { (State.OptionsCheck, Trigger.OptionsMissing), State.OptionsCheck },

                { (State.Launcher, Trigger.MinecraftLaunched), State.MinecraftRunning },

                { (State.MinecraftRunning, Trigger.MinecraftStopped), State.Launcher }
            };

            this.AuthClient = new AuthClient(this.DataStore);
            this.JavaCaller = new JavaCaller(this.StateMachine, this.DataStore);
            this.AssetsLoader = new AssetsLoader(this.DataStore);

            InitializeComponent();

            this.StateMachine.Call(Trigger.Start);
        }
    }
}
