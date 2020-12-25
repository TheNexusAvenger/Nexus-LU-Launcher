/*
 * TheNexusAvenger
 *
 * Tests the GitHub Manifest.
 */

using System;
using System.IO;
using NLUL.Core.Server.Util;
using NUnit.Framework;

namespace NLUL.Core.Test.Util
{
    [TestFixture]
    public class GitHubManifestTest
    {
        public string testDirectory;
        public GitHubManifest testManifest;
        
        /*
         * Sets up the test.
         */
        [SetUp]
        public void SetUp()
        {
            // Create the temporary directory.
            this.testDirectory = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
            Console.WriteLine("Test directory: " + this.testDirectory);
            Directory.CreateDirectory(testDirectory);
            
            // Create the test manifest.
            this.testManifest = new GitHubManifest(Path.Combine(this.testDirectory,"testManifest.json"));
        }
        
        /*
         * Tests getting and using entries.
         */
        [Test]
        public void TestGetEntry()
        {
            // Get the entries and assert they are correct.
            var entry1 = this.testManifest.GetEntry("TheBotAvenger/Initialized-Dummy-Repository",Path.Combine(this.testDirectory,"TestDirectory1"));
            var entry2 = this.testManifest.GetEntry("TheBotAvenger/Uninitialized-Dummy-Repository",Path.Combine(this.testDirectory,"TestDirectory1"));
            var entry3 = this.testManifest.GetEntry("TheBotAvenger/Initialized-Dummy-Repository",Path.Combine(this.testDirectory,"TestDirectory1"));
            var entry4 = this.testManifest.GetEntry("TheBotAvenger/Initialized-Dummy-Repository",Path.Combine(this.testDirectory,"TestDirectory2"));
            Assert.AreNotSame(entry1,entry2);
            Assert.AreSame(entry1,entry3);
            Assert.AreNotSame(entry1,entry4);
            Assert.AreNotSame(entry2,entry4);
            Assert.AreNotSame(entry3,entry4);
            
            // Assert getting the latest commits are valid.
            Assert.AreEqual(entry1.GetLatestCommit("master"),"3472794efb1707dd73a5ce3a8d3ef9b3ece228fe");
            Assert.AreEqual(entry1.GetLatestCommit("dummy-branch-1"),"6688c7880399e02ce1957729e133fda5435583a3");
            Assert.AreEqual(entry1.GetLatestCommit("dummy-branch-2"),"4c8807d396792c345bfdfe341c56ef77e78c100b");
            Assert.AreEqual(entry2.GetLatestCommit("master"),null);
            
            // Assert that the last branch updates are valid for not being fetched.
            Assert.IsFalse(entry1.IsBranchUpToDate("master"));
            Assert.IsFalse(entry1.IsBranchUpToDate("dummy-branch-1"));
            Assert.IsFalse(entry1.IsBranchUpToDate("dummy-branch-2"));
            Assert.IsTrue(entry2.IsBranchUpToDate("master"));
            Assert.IsFalse(entry4.IsBranchUpToDate("master"));
            
            // Assert that the last tags are valid.
            Assert.AreEqual(entry1.GetLatestTag(),"3472794efb1707dd73a5ce3a8d3ef9b3ece228fe");
            Assert.AreEqual(entry2.GetLatestTag(),null);
            
            // Fetch from the releases and commits and assert the files are correct.
            entry1.FetchLatestBranch("dummy-branch-2");
            entry4.FetchLatestTag();
            
            // Assert the files are correct.
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile1")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile2")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile3")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile4")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile5")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile6")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile1")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile2")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile3")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile4")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile5")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile6")));
            
            // Assert that the manifest file contains the correct contents.
            var manifestContents = File.ReadAllText(Path.Combine(this.testDirectory,"testManifest.json"));
            Assert.IsTrue(manifestContents.Contains("Initialized-Dummy-Repository"));
            Assert.IsFalse(manifestContents.Contains("6688c7880399e02ce1957729e133fda5435583a3"));
            Assert.IsTrue(manifestContents.Contains("3472794efb1707dd73a5ce3a8d3ef9b3ece228fe"));
            Assert.IsTrue(manifestContents.Contains("4c8807d396792c345bfdfe341c56ef77e78c100b"));
            
            // Update the branch and assert the files changed.
            entry1.FetchLatestBranch("dummy-branch-1");
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile1")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile2")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile3")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile4")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile5")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory1","DummyFile6")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile1")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile2")));
            Assert.IsTrue(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile3")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile4")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile5")));
            Assert.IsFalse(File.Exists(Path.Combine(this.testDirectory,"TestDirectory2","DummyFile6")));
            
            // Assert that the manifest file contains the correct contents.
            manifestContents = File.ReadAllText(Path.Combine(this.testDirectory,"testManifest.json"));
            Assert.IsTrue(manifestContents.Contains("Initialized-Dummy-Repository"));
            Assert.IsTrue(manifestContents.Contains("6688c7880399e02ce1957729e133fda5435583a3"));
            Assert.IsTrue(manifestContents.Contains("3472794efb1707dd73a5ce3a8d3ef9b3ece228fe"));
            Assert.IsFalse(manifestContents.Contains("4c8807d396792c345bfdfe341c56ef77e78c100b"));
            
            // Reload the manifest and assert the entries are valid.
            var newManifest = new GitHubManifest(Path.Combine(this.testDirectory,"testManifest.json"));
            var newEntry1 = newManifest.GetEntry("TheBotAvenger/Initialized-Dummy-Repository",Path.Combine(this.testDirectory,"TestDirectory1"));
            var newEntry4 = newManifest.GetEntry("TheBotAvenger/Initialized-Dummy-Repository",Path.Combine(this.testDirectory,"TestDirectory2"));
            Assert.IsFalse(newEntry1.IsBranchUpToDate("master"));
            Assert.IsTrue(newEntry1.IsBranchUpToDate("dummy-branch-1"));
            Assert.IsFalse(newEntry1.IsBranchUpToDate("dummy-branch-2"));
            Assert.IsFalse(newEntry1.IsTagUpToDate());
            Assert.IsTrue(newEntry4.IsBranchUpToDate("master"));
            Assert.IsFalse(newEntry4.IsBranchUpToDate("dummy-branch-1"));
            Assert.IsFalse(newEntry4.IsBranchUpToDate("dummy-branch-2"));
            Assert.IsTrue(newEntry4.IsTagUpToDate());
        }
    }
}