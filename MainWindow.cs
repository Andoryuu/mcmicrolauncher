﻿using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Authentication;

namespace MCMicroLauncher
{
    public partial class MainWindow : Form
    {
        private readonly StateMachine<State, Trigger> StateMachine;

        private readonly AuthClient AuthClient;

        private readonly DataStore DataStore;

        private readonly JavaCaller JavaCaller;

        public MainWindow()
        {
            this.DataStore = new DataStore();

            this.StateMachine = new StateMachine<State, Trigger>
            {
                { (State.Closed, Trigger.Start), State.Validation },

                { (State.Validation, Trigger.ValidationSuccess), State.Launcher },
                { (State.Validation, Trigger.ValidationFailed), State.Refresh },

                { (State.Refresh, Trigger.RefreshSuccess), State.Launcher },
                { (State.Refresh, Trigger.RefreshFailed), State.Login },

                { (State.Login, Trigger.LoginSuccess), State.Launcher },
                { (State.Login, Trigger.LoginFailed), State.Login },

                { (State.Launcher, Trigger.MinecraftLaunched), State.MinecraftRunning },

                { (State.MinecraftRunning, Trigger.MinecraftStopped), State.Launcher }
            };

            this.AuthClient = new AuthClient(this.DataStore);
            this.JavaCaller = new JavaCaller(this.StateMachine, this.DataStore);

            InitializeComponent();

            this.StateMachine.Call(Trigger.Start);
        }
    }
}
