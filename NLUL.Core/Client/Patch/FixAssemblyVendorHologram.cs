/*
 * TheNexusAvenger
 *
 * Patch for fixing the Assembly vendor hologram.
 * For some reason, the path is set incorrectly, and
 * it only is a problem in unpacked clients.
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLUL.Core.Client.Patch
{
    public class FixAssemblyVendorHologram : IPatch
    {
        private static readonly byte[] InvalidAnimPath = Encoding.ASCII.GetBytes("\x01" + "9\x00\x00\x00Z:\\lwo\\4_game\\client\\res\\mesh\\3DUI\\Assembly_Logo_Sign.nif"); // Has to be split since \x019 becomes 0b25 instead of 0b01 + '9'.
        private static readonly byte[] ValidAnimPath = Encoding.ASCII.GetBytes("\x01(\x00\x00\x00.\\..\\..\\mesh\\3DUI\\Assembly_Logo_Sign.nif");
        
        private string assemblySignFileLocation;
     
        /*
         * Creates the patch.
         */
        public FixAssemblyVendorHologram(SystemInfo systemInfo)
        {
            this.assemblySignFileLocation = Path.Combine(systemInfo.ClientLocation, "res", "animations", "3dui", "assembly_sign_anim_sm.kfm");
        }
        
        /*
         * Returns if an update is available.
         */
        public bool IsUpdateAvailable()
        {
            return false;
        }
        
        /*
         * Returns if the patch is installed.
         */
        public bool IsInstalled()
        {
            return !File.Exists(this.assemblySignFileLocation) || !((IList) File.ReadAllBytes(this.assemblySignFileLocation)).Contains((byte) ':');
        }
        
        /*
         * Installs the patch.
         */
        public void Install()
        {
            ReplaceByteContents(this.assemblySignFileLocation, InvalidAnimPath, ValidAnimPath);
        }
        
        /*
         * Uninstalls the patch.
         */
        public void Uninstall()
        {
            ReplaceByteContents(this.assemblySignFileLocation, ValidAnimPath, InvalidAnimPath);
        }
        
        /*
         * Replaces the byte contents of the given file.
         * Only replaces 1 occurence.
         */
        private static void ReplaceByteContents(string filePath, byte[] find, byte[] replace)
        {
            // Return if the file doesn't exist.
            if (!File.Exists(filePath))
            {
                return;
            }
            
            // Find the start of the matching bytes. 
            var animFileData = File.ReadAllBytes(filePath);
            var findIndex = -1;
            for (var i = 0; i < animFileData.Length; i++)
            {
                if (animFileData[i] != find[0]) continue;
                for (var j = 0; i < find.Length; j++)
                {
                    if (animFileData[i + j] != find[j]) break;
                    if (j != find.Length - 1) continue;
                    findIndex = i;
                    break;
                }
                if (findIndex != -1)
                {
                    break;
                }
            }

            // Return if no index was found.
            if (findIndex == -1)
            {
                return;
            }
            
            // Create the new bytes to write, replacing the new bytes.
            var newBytes = new List<byte>();
            for (var i = 0; i < findIndex; i++)
            {
                newBytes.Add(animFileData[i]);
            }
            foreach (var t in replace)
            {
                newBytes.Add(t);
            }
            for (var i = findIndex + find.Length; i < animFileData.Length; i++)
            {
                newBytes.Add(animFileData[i]);
            }
            
            // Write the bytes.
            File.WriteAllBytes(filePath, newBytes.ToArray());
        }
    }
}