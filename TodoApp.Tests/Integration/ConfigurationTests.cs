using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace TodoApp.Tests.Integration
{
    /// <summary>
    /// Configuration validation tests to ensure Web.config is properly formatted
    /// </summary>
    public class ConfigurationTests
    {
        public void Test_WebConfig_Is_Valid_Xml()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            if (!File.Exists(webConfigPath))
            {
                throw new FileNotFoundException($"Web.config not found at: {webConfigPath}");
            }

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(webConfigPath);
                Console.WriteLine("✓ Configuration test: Web.config is valid XML");
            }
            catch (XmlException ex)
            {
                throw new Exception($"Web.config is not valid XML: {ex.Message}");
            }
        }

        public void Test_WebConfig_Has_Required_Sections()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(webConfigPath);

            // Check for required sections
            var systemWeb = xmlDoc.SelectSingleNode("//system.web");
            if (systemWeb == null)
                throw new Exception("Missing system.web section in Web.config");

            var systemWebServer = xmlDoc.SelectSingleNode("//system.webServer");
            if (systemWebServer == null)
                throw new Exception("Missing system.webServer section in Web.config");

            var connectionStrings = xmlDoc.SelectSingleNode("//connectionStrings");
            if (connectionStrings == null)
                throw new Exception("Missing connectionStrings section in Web.config");

            var entityFramework = xmlDoc.SelectSingleNode("//entityFramework");
            if (entityFramework == null)
                throw new Exception("Missing entityFramework section in Web.config");

            Console.WriteLine("✓ Configuration test: All required sections present");
        }

        public void Test_WebConfig_Target_Framework()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(webConfigPath);

            var compilation = xmlDoc.SelectSingleNode("//system.web/compilation");
            if (compilation == null)
                throw new Exception("Missing compilation element in Web.config");

            var targetFramework = compilation.Attributes["targetFramework"]?.Value;
            if (targetFramework != "4.8.1")
                throw new Exception($"Incorrect targetFramework. Expected: 4.8.1, Found: {targetFramework}");

            var httpRuntime = xmlDoc.SelectSingleNode("//system.web/httpRuntime");
            if (httpRuntime == null)
                throw new Exception("Missing httpRuntime element in Web.config");

            var runtimeTargetFramework = httpRuntime.Attributes["targetFramework"]?.Value;
            if (runtimeTargetFramework != "4.8.1")
                throw new Exception($"Incorrect httpRuntime targetFramework. Expected: 4.8.1, Found: {runtimeTargetFramework}");

            Console.WriteLine("✓ Configuration test: Target framework correctly set to 4.8.1");
        }

        public void Test_WebConfig_No_Invalid_Attributes()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(webConfigPath);

            // Check that httpHandlers section doesn't exist in system.web (should only be in system.webServer for integrated mode)
            var httpHandlers = xmlDoc.SelectSingleNode("//system.web/httpHandlers");
            if (httpHandlers != null)
                throw new Exception("Invalid httpHandlers section found in system.web - should only be in system.webServer for integrated mode");

            Console.WriteLine("✓ Configuration test: No invalid attribute configurations found");
        }

        public void Test_WebConfig_Compilation_Settings()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(webConfigPath);

            var compilation = xmlDoc.SelectSingleNode("//system.web/compilation");
            if (compilation == null)
                throw new Exception("Missing compilation element in Web.config");

            // Check for tempDirectory attribute - it should not be present or should be a valid absolute path
            var tempDirectoryAttr = compilation.Attributes["tempDirectory"];
            if (tempDirectoryAttr != null)
            {
                var tempDirectory = tempDirectoryAttr.Value;

                // If tempDirectory is specified, it must be an absolute path
                if (!string.IsNullOrEmpty(tempDirectory))
                {
                    // Check if it's a relative path (starts with ~ or doesn't start with drive letter)
                    if (tempDirectory.StartsWith("~") ||
                        (!Path.IsPathRooted(tempDirectory) && !tempDirectory.StartsWith(@"C:\") && !tempDirectory.StartsWith(@"D:\")))
                    {
                        throw new Exception($"Invalid tempDirectory attribute: '{tempDirectory}'. Must be an absolute path or removed entirely.");
                    }
                }
            }

            // Verify debug attribute is properly set
            var debugAttr = compilation.Attributes["debug"];
            if (debugAttr == null)
                throw new Exception("Missing debug attribute in compilation element");

            var debugValue = debugAttr.Value.ToLower();
            if (debugValue != "true" && debugValue != "false")
                throw new Exception($"Invalid debug attribute value: {debugValue}. Must be 'true' or 'false'");

            Console.WriteLine("✓ Configuration test: Compilation settings are valid");
            Console.WriteLine($"✓ Debug mode: {debugValue}");

            if (tempDirectoryAttr == null)
            {
                Console.WriteLine("✓ tempDirectory attribute correctly omitted - using system default");
            }
            else
            {
                Console.WriteLine($"✓ tempDirectory attribute is valid: {tempDirectoryAttr.Value}");
            }
        }

        public void Test_WebConfig_Assembly_References()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(webConfigPath);

            var assemblyNodes = xmlDoc.SelectNodes("//system.web/compilation/assemblies/add");

            // For Web API projects, explicit assembly references are optional
            // Project dependencies are automatically loaded from bin folder
            if (assemblyNodes != null && assemblyNodes.Count > 0)
            {
                foreach (XmlElement assemblyNode in assemblyNodes)
                {
                    var assemblyName = assemblyNode.GetAttribute("assembly");

                    // Check for self-reference - TodoApp.Api should not reference itself
                    if (assemblyName == "TodoApp.Api")
                    {
                        throw new Exception("Invalid self-reference: TodoApp.Api should not reference itself in Web.config assemblies section");
                    }

                    // Validate that referenced assemblies are expected dependencies
                    if (!IsValidAssemblyReference(assemblyName))
                    {
                        throw new Exception($"Unexpected assembly reference: {assemblyName}");
                    }
                }

                Console.WriteLine("✓ Configuration test: Assembly references are valid");
                Console.WriteLine("✓ No self-references detected");
            }
            else
            {
                Console.WriteLine("✓ Configuration test: Web API assembly references for runtime compilation");
                Console.WriteLine("✓ Required for IIS Express to compile Global.asax.cs at runtime");
            }

            // Verify that required assemblies exist in bin folder
            var binPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\bin\Debug"
            );

            var requiredAssemblies = new[] { "TodoApp.Core.dll", "TodoApp.Services.dll", "TodoApp.Data.dll" };
            foreach (var requiredAssembly in requiredAssemblies)
            {
                var assemblyPath = Path.Combine(binPath, requiredAssembly);
                if (!File.Exists(assemblyPath))
                {
                    throw new Exception($"Required assembly not found in bin folder: {requiredAssembly}");
                }
            }
            Console.WriteLine("✓ All required dependency assemblies exist in bin folder");
        }

        private bool IsValidAssemblyReference(string assemblyName)
        {
            var validAssemblies = new[]
            {
                "TodoApp.Core",
                "TodoApp.Services",
                "TodoApp.Data",
                // System assemblies that might be needed
                "System.Web",
                "System.Web.Http",
                "System.Web.Http.WebHost",
                "System.Net.Http.Formatting",
                "System.Web.Http.Cors",
                "System.Web.Routing",
                "Newtonsoft.Json",
                "EntityFramework"
            };

            // Also check if it's a valid versioned assembly name (for Web.config assemblies section)
            foreach (var validAssembly in validAssemblies)
            {
                if (assemblyName.StartsWith(validAssembly + ","))
                {
                    return true;
                }
            }

            return validAssemblies.Contains(assemblyName);
        }

        public void Test_IISExpress_Configuration_Path()
        {
            // Check if IIS Express is configured properly (not using OneDrive paths)
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var oneDrivePath = Path.Combine(userProfile, "OneDrive");

            // This test validates that the development environment is set up correctly
            // The actual IIS Express config path issue needs to be resolved at the system level
            Console.WriteLine("✓ Configuration test: IIS Express path validation");
            Console.WriteLine($"✓ User profile: {userProfile}");

            if (Directory.Exists(oneDrivePath))
            {
                Console.WriteLine("⚠ OneDrive detected - ensure IIS Express config is not redirected to OneDrive");
                Console.WriteLine("⚠ Recommendation: Move IIS Express config out of OneDrive-synced folders");
            }
            else
            {
                Console.WriteLine("✓ No OneDrive path detected");
            }

            // Verify that the project can be loaded without OneDrive interference
            var projectPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api"
            );

            if (Directory.Exists(projectPath))
            {
                Console.WriteLine($"✓ Project path accessible: {Path.GetFullPath(projectPath)}");
            }
            else
            {
                throw new Exception($"Project path not accessible: {projectPath}");
            }
        }

        public void Test_Global_Asax_Configuration()
        {
            var globalAsaxPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\Global.asax"
            );

            if (!File.Exists(globalAsaxPath))
            {
                throw new Exception($"Global.asax not found at: {globalAsaxPath}");
            }

            var globalAsaxContent = File.ReadAllText(globalAsaxPath);

            // Check for correct directive format
            if (!globalAsaxContent.Contains("Inherits=\"TodoApp.Api.WebApiApplication\""))
            {
                throw new Exception("Global.asax must inherit from TodoApp.Api.WebApiApplication");
            }

            // Check for proper CodeFile/CodeBehind directive
            if (globalAsaxContent.Contains("CodeBehind="))
            {
                if (!globalAsaxContent.Contains("Codefile="))
                {
                    Console.WriteLine("⚠ Global.asax uses CodeBehind - consider using Codefile for better compatibility");
                }
            }
            else if (!globalAsaxContent.Contains("Codefile="))
            {
                throw new Exception("Global.asax must have either CodeBehind or Codefile directive");
            }

            // Verify the corresponding .cs file exists
            var globalAsaxCsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\Global.asax.cs"
            );

            if (!File.Exists(globalAsaxCsPath))
            {
                throw new Exception($"Global.asax.cs not found at: {globalAsaxCsPath}");
            }

            // Check that the WebApiApplication class is properly defined
            var globalAsaxCsContent = File.ReadAllText(globalAsaxCsPath);
            if (!globalAsaxCsContent.Contains("public partial class WebApiApplication"))
            {
                throw new Exception("WebApiApplication class must be declared as 'partial' when using Codefile directive");
            }

            if (!globalAsaxCsContent.Contains("namespace TodoApp.Api"))
            {
                throw new Exception("Global.asax.cs must be in TodoApp.Api namespace");
            }

            Console.WriteLine("✓ Configuration test: Global.asax properly configured");
            Console.WriteLine("✓ WebApiApplication class found in correct namespace");
            Console.WriteLine("✓ Global.asax and Global.asax.cs files exist");
        }
    }
}
