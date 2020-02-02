/*
 * TheNexusAvenger
 *
 * Interface for a prerequisite, such as a file
 * to setup or an application to install.
 */

namespace NLUL.Core.Server.Prerequisite
{
    public interface IPrerequisite
    {
        /*
         * Returns the name of the prerequisite.
         */
        public string GetName();
        
        /*
         * Returns the error message for the
         * prerequisite not being met.
         */
        public string GetErrorMessage();
        
        /*
         * Handles setting up the prerequisite.
         * Returns if it was completed successfully,
         * and false if it wasn't.
         */
        public bool SetupPrerequisite();
        
        /*
         * Returns if the prerequisite was met.
         */
        public bool IsMet();
    }
}