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
        MinecraftLaunched,
        MinecraftStopped
    }
}
