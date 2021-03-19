namespace MCMicroLauncher.ApplicationState
{
    internal enum Trigger
    {
        Start,
        ValidationSuccess,
        ValidationFailed,
        RefreshSuccess,
        RefreshFailed,
        LoginSuccess,
        LoginFailed,
        AssetsLoaded,
        OptionsResolved,
        OptionsMissing,
        MinecraftLaunched,
        MinecraftStopped
    }
}
