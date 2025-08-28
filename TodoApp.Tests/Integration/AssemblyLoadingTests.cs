using System;
using System.IO;
using System.Reflection;

namespace TodoApp.Tests.Integration
{
    /// <summary>
    /// Assembly loading tests to diagnose Global.asax type loading issues
    /// </summary>
    public class AssemblyLoadingTests
    {
        public void Test_WebApiApplication_Type_Can_Be_Loaded()
        {
            try
            {
                // Get the path to the API assembly
                var apiAssemblyPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\TodoApp.Api\bin\Debug\TodoApp.Api.dll"
                );

                if (!File.Exists(apiAssemblyPath))
                {
                    throw new FileNotFoundException($"TodoApp.Api.dll not found at: {apiAssemblyPath}");
                }

                // Load the assembly
                var assembly = Assembly.LoadFrom(apiAssemblyPath);

                // Try to get the WebApiApplication type
                var webApiApplicationType = assembly.GetType("TodoApp.Api.WebApiApplication");

                if (webApiApplicationType == null)
                {
                    throw new Exception("WebApiApplication type not found in TodoApp.Api.dll");
                }

                // Verify it inherits from HttpApplication
                //if (!typeof(System.Web.HttpApplication).IsAssignableFrom(webApiApplicationType))
                //{
                //    throw new Exception("WebApiApplication does not inherit from HttpApplication");
                //}

                Console.WriteLine("✓ Assembly test: WebApiApplication type loads successfully");
                Console.WriteLine($"  - Assembly path: {apiAssemblyPath}");
                Console.WriteLine($"  - Type full name: {webApiApplicationType.FullName}");
                Console.WriteLine($"  - Base type: {webApiApplicationType.BaseType?.FullName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load WebApiApplication type: {ex.Message}", ex);
            }
        }

        public void Test_All_Referenced_Assemblies_Load()
        {
            try
            {
                var apiAssemblyPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\TodoApp.Api\bin\Debug\TodoApp.Api.dll"
                );

                var assembly = Assembly.LoadFrom(apiAssemblyPath);

                // Try to load all referenced assemblies
                var referencedAssemblies = assembly.GetReferencedAssemblies();

                foreach (var refAssembly in referencedAssemblies)
                {
                    try
                    {
                        Assembly.Load(refAssembly);
                        Console.WriteLine($"✓ Referenced assembly loaded: {refAssembly.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠ Warning: Could not load {refAssembly.Name}: {ex.Message}");
                    }
                }

                Console.WriteLine("✓ Assembly test: Referenced assemblies checked");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check referenced assemblies: {ex.Message}", ex);
            }
        }

        public void Test_Web_Config_Assembly_Settings()
        {
            var webConfigPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\TodoApp.Api\web.config"
            );

            if (!File.Exists(webConfigPath))
            {
                throw new FileNotFoundException($"Web.config not found at: {webConfigPath}");
            }

            var configContent = File.ReadAllText(webConfigPath);

            // Check for common assembly binding issues
            if (!configContent.Contains("runtime"))
            {
                Console.WriteLine("⚠ Warning: No runtime section found in Web.config");
            }

            if (!configContent.Contains("assemblyBinding"))
            {
                Console.WriteLine("⚠ Warning: No assemblyBinding section found in Web.config");
            }

            if (!configContent.Contains("targetFramework=\"4.8.1\""))
            {
                throw new Exception("Web.config does not specify targetFramework 4.8.1");
            }

            Console.WriteLine("✓ Assembly test: Web.config settings validated");
        }
    }
}
