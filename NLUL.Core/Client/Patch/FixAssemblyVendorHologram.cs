using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLUL.Core.Client.Patch
{
    public class FixAssemblyVendorHologram : IPatch
    {
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name => "Fix Assembly Vendor Hologram";
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        public string Description => "Fixes the Assembly vendor at Nimbus Station showing a Missing NIF error.";

        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum => ClientPatchName.FixAssemblyVendorHologram;
        
        /// <summary>
        /// Data of the invalid path.
        /// </summary>
        private static readonly byte[] InvalidAnimPath = Encoding.ASCII.GetBytes("\x01" + "9\x00\x00\x00Z:\\lwo\\4_game\\client\\res\\mesh\\3DUI\\Assembly_Logo_Sign.nif"); // Has to be split since \x019 becomes 0b25 instead of 0b01 + '9'.
        
        /// <summary>
        /// Data of the valid path.
        /// </summary>
        private static readonly byte[] ValidAnimPath = Encoding.ASCII.GetBytes("\x01(\x00\x00\x00.\\..\\..\\mesh\\3DUI\\Assembly_Logo_Sign.nif");
        
        /// <summary>
        /// System info of the client.
        /// </summary>
        private readonly SystemInfo systemInfo;
        
        /// <summary>
        /// Location of the Assembly sign in Nimbus Station.
        /// </summary>
        private string AssemblySignFileLocation => Path.Combine(this.systemInfo.ClientLocation, "res", "animations", "3dui", "assembly_sign_anim_sm.kfm");
     
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable => false;

        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed => File.Exists(this.AssemblySignFileLocation) && !((IList) File.ReadAllBytes(this.AssemblySignFileLocation)).Contains((byte) ':');
        
        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        public FixAssemblyVendorHologram(SystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
        }
        
        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install()
        {
            ReplaceByteContents(this.AssemblySignFileLocation, InvalidAnimPath, ValidAnimPath);
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall()
        {
            ReplaceByteContents(this.AssemblySignFileLocation, ValidAnimPath, InvalidAnimPath);
        }
        
        /// <summary>
        /// Replaces the byte contents of the given file.
        /// Only replaces 1 occurence.
        /// </summary>
        /// <param name="filePath">File path to change.</param>
        /// <param name="find">Bytes to find.</param>
        /// <param name="replace">Bytes to replace.</param>
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