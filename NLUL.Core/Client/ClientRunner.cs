/*
 * TheNexusAvenger
 *
 * Downloads, patches, and launches the client.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace NLUL.Core.Client
{
    public class ClientRunner
    {
        private SystemInfo SystemInfo;
        private string DownloadLocation;
        private string ExtractLocation;
        
        /*
         * Creates a Client instance.
         */
        public ClientRunner(SystemInfo systemInfo)
        {
            this.SystemInfo = systemInfo;
            this.DownloadLocation = Path.Combine(systemInfo.SystemFileLocation,"client.zip");
            this.ExtractLocation = Path.Combine(systemInfo.SystemFileLocation,"ClientExtract");
        }
        
        /*
         * Downloads the client.
         */
        public void DownloadClient(bool force)
        {
            // Return if the client exists and isn't forced.
            if (!force && File.Exists(this.DownloadLocation))
            {
                Console.WriteLine("Client was already downloaded.");
                return;
            }
            
            // Delete the existing download if it exists.
            if (File.Exists(this.DownloadLocation))
            {
                File.Delete(this.DownloadLocation);
            }
            
            // Download the client.
            Console.WriteLine("Downloading the Lego Universe client.");
            Directory.CreateDirectory(Path.GetDirectoryName(this.SystemInfo.ClientLocation));
            var client = new WebClient();
            client.DownloadFile("https://s3.amazonaws.com/luclient/luclient.zip",this.DownloadLocation);
        }
        
        /*
         * Extracts the client files.
         */
        private void ExtractClient(bool force)
        {
            // Clean the client if forced.
            if (force && Directory.Exists(this.ExtractLocation))
            {
                Directory.Delete(this.ExtractLocation,true);
            }
            if (force && Directory.Exists(this.SystemInfo.ClientLocation))
            {
                Directory.Delete(this.SystemInfo.ClientLocation,true);
            }
            
            // Extract the files.
            Console.WriteLine("Extracting the client files.");
            if (!Directory.Exists(this.ExtractLocation))
            {
                ZipFile.ExtractToDirectory(this.DownloadLocation,this.ExtractLocation);
            }
            
            // Move the files.
            Directory.Move(Path.Combine(this.ExtractLocation,"LCDR Unpacked"),this.SystemInfo.ClientLocation);
            Directory.Delete(this.ExtractLocation);
        }
        
        /*
         * Tries to extract the client files. If it fails,
         * the client is re-downloaded.
         */
        public void TryExtractClient(bool force)
        {
            // Return if the client was already extracted.
            if (!force && Directory.Exists(this.SystemInfo.ClientLocation))
            {
                Console.WriteLine("Client was already extracted.");
                return;
            }
            
            // Download the client if not done already.
            this.DownloadClient(false);
            
            // Extract the files.
            try
            {
                Console.WriteLine("Trying to extract client files.");
                this.ExtractClient(force);
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Failed to extract the files (download corrupted); retrying download.");
                this.DownloadClient(true);
                this.ExtractClient(force);
            }
        }
        
        /*
         * Patches the client files.
         */
        public void PatchClient()
        {
            // Download the TcpUdp mod.
            Console.WriteLine("Adding the TcpUdp mod.");
            var tcpUdpModZipLocation = Path.Combine(this.SystemInfo.SystemFileLocation,"TcpUdp.zip");
            var tcpUdpModExtractLocation = Path.Combine(this.SystemInfo.SystemFileLocation,"TcpUdp");
            if (!File.Exists(tcpUdpModZipLocation))
            {
                var client = new WebClient();
                client.DownloadFile("https://bitbucket.org/lcdr/raknet_shim_dll/downloads/shim_dll.zip",tcpUdpModZipLocation);
            }
            if (!Directory.Exists(tcpUdpModExtractLocation))
            {
                ZipFile.ExtractToDirectory(tcpUdpModZipLocation,tcpUdpModExtractLocation);
            }
            
            // Copy the TcpUdp files.
            if (!File.Exists(Path.Join(this.SystemInfo.SystemFileLocation,"Client","dinput8.dll")))
            {
                File.Copy(Path.Join(this.SystemInfo.SystemFileLocation,"TcpUdp","dinput8.dll"),Path.Join(this.SystemInfo.SystemFileLocation,"Client","dinput8.dll"));
            }
            if (!File.Exists(Path.Join(this.SystemInfo.SystemFileLocation,"Client","mods","raknet_shim","mod.dll")))
            {
                Directory.CreateDirectory(Path.Join(this.SystemInfo.SystemFileLocation,"Client","mods","raknet_shim"));
                File.Copy(Path.Join(this.SystemInfo.SystemFileLocation,"TcpUdp","mods","raknet_shim","mod.dll"), Path.Join(this.SystemInfo.SystemFileLocation,"Client","mods","raknet_shim","mod.dll"));
            }
            
            // Clear the files.
            File.Delete(tcpUdpModZipLocation);
            Directory.Delete(tcpUdpModExtractLocation,true);
        }
        
        /*
         * Launches the client.
         */
        public void Launch(string host)
        {
            // Modify the boot file.
            Console.WriteLine("Setting to connect to \"" + host + "\"");
            var bootConfigLocation = Path.Combine(this.SystemInfo.ClientLocation,"boot.cfg");
            var newBootContents = "";
            foreach (var line in File.ReadLines(bootConfigLocation))
            {
                if (line.StartsWith("AUTHSERVERIP="))
                {
                    newBootContents += "AUTHSERVERIP=0:" + host + ",\n";
                }
                else
                {
                    newBootContents += line + "\n";
                }
            }
            File.WriteAllText(bootConfigLocation,newBootContents.Substring(0,newBootContents.Length - 1));
            
            // Launch the client.
            Console.WriteLine("Launching the client.");
            var clientProcess = new Process();
            clientProcess.StartInfo.WorkingDirectory = this.SystemInfo.ClientLocation;
            clientProcess.StartInfo.FileName = Path.Combine(this.SystemInfo.ClientLocation,"legouniverse.exe");
            clientProcess.StartInfo.CreateNoWindow = true;
            clientProcess.Start();
            clientProcess.WaitForExit();
            Console.WriteLine("Client closed.");
        }
    }
}