namespace ArtViewer
{

    /// <summary>
    /// This class stores the client ID and client secret for connecting to the DeviantArt API.
    /// On build, the GenerateSecrets.csx script will be called, overwriting this file and its
    /// contents, replacing the dummy values below with real data that it reads from the .env 
    /// file.
    /// 
    /// Any changes made to this file will be overwritten when the GenerateSecrets.csx script 
    /// is run, so if you need to modify this file in any way, do not do it here. Make your 
    /// changes in the .env file and/or the GenerateSecrets.csx script.
    /// 
    /// To ensure this file is not included in source control, it is recommended that you follow
    /// these steps before building the app:
    /// 
    /// 1. Temporarily remove this file from the .gitignore. 
    /// 
    /// 2. Ensure this file is pushed to your remote. If you are not using your own remote, skip
    /// this step.
    /// 
    /// 2. Put this file back into the .gitignore. Then run this command to ensure changes to 
    /// this file are ignored.
    /// 
    ///     git update-index --assume-unchanged ArtViewer/Secrets.cs
    /// </summary>
    public static class Secrets
    {
        public const string client_id = "dummy_value";
        public const string client_secret = "dummy_value";
    }
}
