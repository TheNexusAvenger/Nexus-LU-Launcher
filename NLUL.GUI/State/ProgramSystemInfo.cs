/*
 * TheNexusAvenger
 *
 * Contains the system info.
 */

using System;
using System.IO;
using NLUL.Core;

namespace NLUL.GUI.State
{
    public class ProgramSystemInfo
    {
        public static SystemInfo SystemInfo;

        static ProgramSystemInfo()
        {
            var programData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".nlul");
            SystemInfo = new SystemInfo(programData,Path.Combine(programData,"Client"));
        }
    }
}