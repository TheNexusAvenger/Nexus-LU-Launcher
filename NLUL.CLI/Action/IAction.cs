/*
 * TheNexusAvenger
 *
 * Performs an action.
 */

using System.Collections.Generic;
using NLUL.Core;

namespace NLUL.CLI.Action
{
    public interface IAction
    {
        /*
         * Returns the arguments for the action.
         */
        public string GetArguments();
        
        /*
         * Returns a description of what the action does.
         */
        public string GetDescription();
        
        /*
         * Performs the action.
         */
        public void Run(List<string> arguments,SystemInfo systemInfo);
    }
}