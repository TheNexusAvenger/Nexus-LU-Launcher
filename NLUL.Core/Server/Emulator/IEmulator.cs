/*
 * TheNexusAvenger
 *
 * Interface for a server emulator.
 */

using System.Collections.Generic;
using NLUL.Core.Server.Prerequisite;

namespace NLUL.Core.Server.Emulator
{
    public interface IEmulator
    {
        /*
         * Returns the prerequisites for the server.
         */
        public List<IPrerequisite> GetPrerequisites();
        
        /*
         * Installs the server. Used for both initializing
         * the first time and updating.
         */
        public void Install();
        
        /*
         * Starts the server.
         */
        public void Start();
        
        /*
         * Stops the server.
         */
        public void Stop();
    }
}