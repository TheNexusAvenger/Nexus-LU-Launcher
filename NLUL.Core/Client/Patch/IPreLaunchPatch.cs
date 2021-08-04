namespace NLUL.Core.Client.Patch
{
    public interface IPreLaunchPatch : IPatch
    {
        /// <summary>
        /// Performs and operations between setting the
        /// boot.cfg and launching the client. This will
        /// yield launching the client.
        /// </summary>
        public void OnClientRequestLaunch();
    }
}