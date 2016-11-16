using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenerationSample.Json;
using CodeGenerationSample.Json.Exceptions;
using System.Diagnostics;
using System.Security.Cryptography;

namespace CodeGenerationSample.Scripts.Test
{
    [TestClass]
    public class JsonToCsvFixture
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        public const string ADLToolsFolderPath = "C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\Common7\\IDE\\Extensions\\Microsoft\\Microsoft Azure Data Lake Tools for Visual Studio 2015\\2.2.2100.0";
        public static string USQLScriptsPath = Path.GetFullPath("..\\..\\..\\CodeGenerationSample.Scripts");
        public static string USQLDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local\\USQLDataRoot");        
        public static string ProcessedDestinationPath = Path.Combine(USQLDataRoot, "Samples\\Output");
        
        private string GetFileHash(string filename)
        {
            var hash = new SHA1Managed();
            var clearBytes = File.ReadAllBytes(filename);
            var hashedBytes = hash.ComputeHash(clearBytes);
            return ConvertBytesToHex(hashedBytes);
        }

        private string ConvertBytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x"));
            }
            return sb.ToString();
        }

        private void RunAnalyticsJob(string scriptPath)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = Path.Combine(ADLToolsFolderPath, "LocalRunSDK", "LocalRunHelper.exe");
            processStartInfo.Arguments = string.Format("run -Script \"{0}\" -DataRoot \"{1}\" -Verbose -WorkDir \"C:\\temp\" -CppSDK \"{2}\"", scriptPath, USQLDataRoot, Path.Combine(ADLToolsFolderPath, "CppSDK"));
            processStartInfo.CreateNoWindow = false;
            processStartInfo.UseShellExecute = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            using (Process exeProcess = new Process())
            {
                exeProcess.StartInfo = processStartInfo;
                exeProcess.EnableRaisingEvents = true;
                exeProcess.Start();

                exeProcess.WaitForExit();
            }

        }
        
        [TestMethod]
        [DeploymentItem("ExpectedResults")]
        public void TestAdvancedPaths()
        {
            string expectedResultsFile = "produce.csv";
            string uSqlScriptName = "JsonToCsv.usql";
            string destinationFile = Path.Combine(ProcessedDestinationPath, expectedResultsFile);
            string scriptPath = Path.Combine(USQLScriptsPath, uSqlScriptName);

            // Delete the destination file so we know it is from the test
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            RunAnalyticsJob(scriptPath);

            var originalHash = GetFileHash(expectedResultsFile);
            var destinationHash = GetFileHash(destinationFile);

            Assert.AreEqual(destinationHash, originalHash, string.Format("Script: {0} failed to match the output file!", uSqlScriptName));
        }
        

    }
}
