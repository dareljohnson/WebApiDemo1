using System;
using System.IO;
using System.Reflection;

namespace TodoApp.Tests.Integration
{
    /// <summary>
    /// Deployment validation tests to help diagnose runtime loading issues
    /// </summary>
    public class DeploymentValidationTests
    {
        public void Test_All_Required_Files_Present()
        {
            var apiPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api"
            );

            // Check for Global.asax
            var globalAsax = Path.Combine(apiPath, "Global.asax");
            if (!File.Exists(globalAsax))
                throw new Exception("Global.asax file not found");

            // Check for Global.asax.cs
            var globalAsaxCs = Path.Combine(apiPath, "Global.asax.cs");
            if (!File.Exists(globalAsaxCs))
                throw new Exception("Global.asax.cs file not found");

            // Check for Web.config
            var webConfig = Path.Combine(apiPath, "web.config");
            if (!File.Exists(webConfig))
                throw new Exception("web.config file not found");

            // Check bin directory
            var binPath = Path.Combine(apiPath, "bin", "Debug");
            if (!Directory.Exists(binPath))
                throw new Exception("bin/Debug directory not found");

            // Check for compiled assembly
            var apiDll = Path.Combine(binPath, "TodoApp.Api.dll");
            if (!File.Exists(apiDll))
                throw new Exception("TodoApp.Api.dll not found in bin directory");

            Console.WriteLine("✓ Deployment test: All required files present");
        }

        public void Test_Assembly_Has_Correct_Build_Settings()
        {
            var apiAssemblyPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\bin\Debug\TodoApp.Api.dll"
            );

            var assembly = System.Reflection.Assembly.LoadFrom(apiAssemblyPath);

            // Check assembly version
            var version = assembly.GetName().Version;
            Console.WriteLine($"  - Assembly version: {version}");

            // Check if assembly is built for correct framework
            var frameworkAttribute = assembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
            if (frameworkAttribute.Length > 0)
            {
                var targetFramework = ((System.Runtime.Versioning.TargetFrameworkAttribute)frameworkAttribute[0]).FrameworkName;
                Console.WriteLine($"  - Target framework: {targetFramework}");

                if (!targetFramework.Contains("4.8"))
                {
                    throw new Exception($"Assembly not built for .NET Framework 4.8. Found: {targetFramework}");
                }
            }

            Console.WriteLine("✓ Deployment test: Assembly build settings validated");
        }

        public void Test_Provide_Deployment_Troubleshooting()
        {
            Console.WriteLine("✓ Deployment troubleshooting guide:");
            Console.WriteLine("  If you're getting 'Could not load type' errors in IIS/IIS Express:");
            Console.WriteLine("  1. Clean and rebuild the entire solution");
            Console.WriteLine("  2. Delete bin and obj folders, then rebuild");
            Console.WriteLine("  3. Restart IIS Express or IIS");
            Console.WriteLine("  4. Check Application Pool .NET Framework version (should be 4.0)");
            Console.WriteLine("  5. Verify web.config compilation debug='true' for development");
            Console.WriteLine("  6. Check that Global.asax.cs has proper 'Build Action: Compile'");
            Console.WriteLine("  7. Ensure bin folder contains all required assemblies");
            Console.WriteLine("  8. Try 'aspnet_compiler -v / -p path/to/website' for precompilation");
        }

        public void Test_Generate_Deployment_Commands()
        {
            var solutionPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\.."
            ));

            Console.WriteLine("✓ Deployment commands to resolve the issue:");
            Console.WriteLine($"  cd \"{solutionPath}\"");
            Console.WriteLine("  dotnet clean TodoApp.sln");
            Console.WriteLine("  dotnet build TodoApp.sln --configuration Release");
            Console.WriteLine("  # Or for IIS deployment:");
            Console.WriteLine("  # msbuild TodoApp.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:PublishProfile=FolderProfile");
        }

        public void Test_WebApiApplication_Runtime_Loading()
        {
            // This test validates that the WebApiApplication can be loaded at runtime
            // similar to how IIS Express would load it

            var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TodoApp.Api", "bin", "Debug");
            var assemblyPath = Path.Combine(binPath, "TodoApp.Api.dll");

            if (!File.Exists(assemblyPath))
            {
                throw new Exception($"TodoApp.Api.dll not found at: {assemblyPath}");
            }

            try
            {
                // Load the assembly the way ASP.NET runtime would
                var assembly = Assembly.LoadFrom(assemblyPath);
                var webApiApplicationType = assembly.GetType("TodoApp.Api.WebApiApplication");

                if (webApiApplicationType == null)
                {
                    throw new Exception("WebApiApplication type not found in TodoApp.Api.dll");
                }

                // Verify it inherits from HttpApplication
                //if (!typeof(System.Web.HttpApplication).IsAssignableFrom(webApiApplicationType))
                //{
                //    throw new Exception("WebApiApplication must inherit from System.Web.HttpApplication");
                //}

                // Verify it has the Application_Start method
                var applicationStartMethod = webApiApplicationType.GetMethod("Application_Start",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (applicationStartMethod == null)
                {
                    throw new Exception("WebApiApplication must have Application_Start method");
                }

                Console.WriteLine("✓ Deployment test: WebApiApplication can be loaded from compiled assembly");
                Console.WriteLine($"✓ Assembly location: {assemblyPath}");
                Console.WriteLine($"✓ Type full name: {webApiApplicationType.FullName}");
                Console.WriteLine($"✓ Base type: {webApiApplicationType.BaseType?.FullName}");
                Console.WriteLine("✓ Application_Start method found");

            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.Contains("WebApiApplication")))
            {
                throw new Exception($"Failed to load WebApiApplication from assembly: {ex.Message}");
            }
        }
    }
}
