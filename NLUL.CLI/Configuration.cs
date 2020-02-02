/*
 * TheNexusAvenger
 *
 * Configuration for the CLI executable.
 */

using System;
using System.IO;

namespace NLUL
{
    public class Configuration
    {
        public static readonly string PROGRAM_DATA_LOCATION = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".nlul");
    }
}