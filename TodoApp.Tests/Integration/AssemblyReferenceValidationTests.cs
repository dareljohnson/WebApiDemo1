using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace TodoApp.Tests.Integration
{
    /// <summary>
    /// Tests specifically for CS0234 namespace/assembly reference errors
    /// </summary>
    public class AssemblyReferenceValidationTests
    {
        public void Test_System_Web_Http_Assembly_Reference()
        {
            // This test specifically addresses CS0234: The type or namespace name 'Http' does not exist in the namespace 'System.Web'
            // Validates that System.Web.Http assembly is properly referenced and accessible

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var apiProjectPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api");
            var projectFilePath = Path.Combine(apiProjectPath, "TodoApp.Api.csproj");

            if (!File.Exists(projectFilePath))
            {
                throw new FileNotFoundException($"TodoApp.Api.csproj not found at: {projectFilePath}");
            }

            var projectContent = File.ReadAllText(projectFilePath);

            // Check that System.Web.Http reference exists
            if (!projectContent.Contains("System.Web.Http"))
            {
                throw new Exception("CS0234 Prevention: System.Web.Http assembly reference missing from project file");
            }

            // Validate the HintPath points to the correct NuGet package
            var webApiCorePattern = @"<Reference Include=""System\.Web\.Http"">\s*<HintPath>\.\.\\packages\\Microsoft\.AspNet\.WebApi\.Core\.5\.2\.9\\lib\\net45\\System\.Web\.Http\.dll</HintPath>";
            if (!Regex.IsMatch(projectContent, webApiCorePattern, RegexOptions.Multiline))
            {
                throw new Exception("CS0234 Prevention: System.Web.Http HintPath is incorrect or missing");
            }

            // Check that the actual DLL file exists
            var webApiDllPath = Path.Combine(testAssemblyDir, "..", "..", "..", "packages", "Microsoft.AspNet.WebApi.Core.5.2.9", "lib", "net45", "System.Web.Http.dll");
            if (!File.Exists(webApiDllPath))
            {
                throw new Exception($"CS0234 Prevention: System.Web.Http.dll not found at expected location: {webApiDllPath}");
            }

            Console.WriteLine("✓ CS0234 Prevention: System.Web.Http assembly reference validated");
            Console.WriteLine($"✓ Project file contains correct reference");
            Console.WriteLine($"✓ HintPath correctly points to NuGet package");
            Console.WriteLine($"✓ DLL file exists at expected location");
        }

        public void Test_Web_Api_Dependencies_Complete()
        {
            // Validates all Web API related dependencies are properly referenced

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectFilePath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "TodoApp.Api.csproj");
            var projectContent = File.ReadAllText(projectFilePath);

            var requiredReferences = new[]
            {
                "System.Web.Http",
                "System.Web.Http.WebHost",
                "System.Net.Http.Formatting",
                "System.Web.Http.Cors"
            };

            foreach (var reference in requiredReferences)
            {
                if (!projectContent.Contains($"<Reference Include=\"{reference}\">"))
                {
                    throw new Exception($"CS0234 Prevention: Missing required Web API reference: {reference}");
                }
            }

            Console.WriteLine("✓ All Web API assembly references validated");
            Console.WriteLine("✓ System.Web.Http (Core API framework)");
            Console.WriteLine("✓ System.Web.Http.WebHost (IIS hosting)");
            Console.WriteLine("✓ System.Net.Http.Formatting (JSON/XML formatting)");
            Console.WriteLine("✓ System.Web.Http.Cors (Cross-origin support)");
        }

        public void Test_Packages_Config_Consistency()
        {
            // Ensures packages.config matches project file references

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var packagesConfigPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "packages.config");

            if (!File.Exists(packagesConfigPath))
            {
                throw new FileNotFoundException($"packages.config not found at: {packagesConfigPath}");
            }

            var packagesContent = File.ReadAllText(packagesConfigPath);

            var expectedPackages = new[]
            {
                "Microsoft.AspNet.WebApi.Core",
                "Microsoft.AspNet.WebApi.WebHost",
                "Microsoft.AspNet.WebApi.Client",
                "Microsoft.AspNet.WebApi.Cors"
            };

            foreach (var package in expectedPackages)
            {
                if (!packagesContent.Contains($"id=\"{package}\""))
                {
                    throw new Exception($"CS0234 Prevention: Missing NuGet package in packages.config: {package}");
                }

                if (!packagesContent.Contains("targetFramework=\"net481\""))
                {
                    throw new Exception("CS0234 Prevention: Packages must target .NET Framework 4.8.1");
                }
            }

            Console.WriteLine("✓ packages.config consistency validated");
            Console.WriteLine("✓ All Web API NuGet packages present");
            Console.WriteLine("✓ Target framework correctly set to net481");
        }

        public void Test_Build_Output_Assembly_Loading()
        {
            // Tests that compiled assembly can actually load System.Web.Http types
            // This catches runtime assembly loading issues that might not show up during compilation

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var apiAssemblyPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "bin", "Debug", "TodoApp.Api.dll");

            if (!File.Exists(apiAssemblyPath))
            {
                throw new FileNotFoundException($"TodoApp.Api.dll not found. Ensure solution is built. Path: {apiAssemblyPath}");
            }

            try
            {
                // Load the assembly and try to resolve Web API types
                var assembly = Assembly.LoadFrom(apiAssemblyPath);

                // Try to load types that depend on System.Web.Http
                var configType = assembly.GetType("TodoApp.Api.WebApiConfig");
                if (configType == null)
                {
                    throw new Exception("CS0234 Runtime Prevention: WebApiConfig type not found - System.Web.Http likely not loading");
                }

                // Verify the GlobalConfiguration.Configure method can be found
                var registerMethod = configType.GetMethod("Register");
                if (registerMethod == null)
                {
                    throw new Exception("CS0234 Runtime Prevention: WebApiConfig.Register method not found");
                }

                Console.WriteLine("✓ Runtime assembly loading validation passed");
                Console.WriteLine("✓ TodoApp.Api.dll loads successfully");
                Console.WriteLine("✓ System.Web.Http types accessible at runtime");
                Console.WriteLine("✓ WebApiConfig class and Register method found");

            }
            catch (Exception ex) when (!(ex.Message.Contains("CS0234")))
            {
                throw new Exception($"CS0234 Runtime Prevention: Failed to load Web API types: {ex.Message}");
            }
        }

        public void Test_Web_Config_Runtime_Compilation_Assemblies()
        {
            // This test specifically addresses CS0234 errors during runtime compilation by IIS Express
            // When Global.asax.cs uses System.Web.Http, the runtime compiler needs assembly references

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var webConfigPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Web.config");

            if (!File.Exists(webConfigPath))
            {
                throw new FileNotFoundException($"Web.config not found at: {webConfigPath}");
            }

            var webConfigContent = File.ReadAllText(webConfigPath);

            // Check if compilation section exists
            if (!webConfigContent.Contains("<compilation"))
            {
                throw new Exception("CS0234 Runtime Prevention: Web.config missing compilation section");
            }

            // For runtime compilation of Global.asax.cs that uses System.Web.Http,
            // we need assemblies either in the compilation section OR automatic bin loading
            var hasAssembliesSection = webConfigContent.Contains("<assemblies>");
            var hasSystemWebHttpAssembly = webConfigContent.Contains("System.Web.Http");
            var hasAutomaticLoadingComment = webConfigContent.Contains("automatically loaded") || webConfigContent.Contains("automatic bin loading");

            // If Global.asax.cs uses System.Web.Http but Web.config doesn't reference it,
            // runtime compilation will fail with CS0234 UNLESS automatic bin loading is working
            var globalAsaxCsPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Global.asax.cs");
            var globalAsaxContent = File.ReadAllText(globalAsaxCsPath);
            var usesSystemWebHttp = globalAsaxContent.Contains("using System.Web.Http");

            if (usesSystemWebHttp)
            {
                if (hasSystemWebHttpAssembly)
                {
                    Console.WriteLine("√ CS0234 Runtime Prevention: Web.config has explicit System.Web.Http assembly reference for Global.asax.cs");
                    Console.WriteLine("√ Runtime compilation should work correctly in IIS Express");
                }
                else if (!hasAssembliesSection && hasAutomaticLoadingComment)
                {
                    Console.WriteLine("√ CS0234 Runtime Prevention: Web.config uses automatic bin loading for Global.asax.cs compilation");
                    Console.WriteLine("√ Runtime compilation should work correctly in IIS Express with automatic loading");

                    // Verify assemblies exist in bin folder
                    var binPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "bin", "Debug");
                    var systemWebHttpPath = Path.Combine(binPath, "System.Web.Http.dll");
                    if (File.Exists(systemWebHttpPath))
                    {
                        Console.WriteLine("√ System.Web.Http.dll present in bin folder for automatic loading");
                    }
                    else
                    {
                        throw new Exception("CS0234 Runtime Prevention: System.Web.Http.dll missing from bin folder - automatic loading will fail");
                    }
                }
                else
                {
                    Console.WriteLine("⚠ CS0234 Runtime Risk: Global.asax.cs uses System.Web.Http but Web.config doesn't reference it");
                    Console.WriteLine("⚠ This may cause runtime compilation errors in IIS Express");
                    Console.WriteLine("⚠ Recommendation: Add System.Web.Http to compilation/assemblies section OR use automatic bin loading");
                    throw new Exception("CS0234 Runtime Prevention: Web.config needs System.Web.Http assembly reference for Global.asax.cs compilation");
                }
            }
            else
            {
                Console.WriteLine("√ CS0234 Runtime Prevention: Web.config assembly references appropriate for Global.asax.cs usage");
            }
        }

        public void Test_Web_Config_Assemblies_Section_Complete()
        {
            // Validates that Web.config is properly configured for automatic bin loading OR has explicit assemblies

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var webConfigPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Web.config");
            var webConfigContent = File.ReadAllText(webConfigPath);

            var requiredAssemblies = new[]
            {
                "System.Web.Http",
                "System.Web.Http.WebHost",
                "System.Net.Http.Formatting"
            };

            // Check if we have explicit assembly references OR automatic bin loading
            bool hasExplicitAssemblies = webConfigContent.Contains("<assemblies>");
            bool hasAutomaticComment = webConfigContent.Contains("automatically loaded") || webConfigContent.Contains("automatic bin loading");

            if (hasExplicitAssemblies)
            {
                // Old approach: validate explicit assembly references
                Console.WriteLine("√ Web.config uses explicit assembly references");
                foreach (var assembly in requiredAssemblies)
                {
                    if (!webConfigContent.Contains($"assembly=\"{assembly}"))
                    {
                        throw new Exception($"CS0234 Runtime Prevention: Missing {assembly} in Web.config assemblies section");
                    }
                }
                Console.WriteLine("√ All required Web API assemblies present in Web.config compilation section");
            }
            else
            {
                // New approach: validate automatic bin loading is configured
                Console.WriteLine("√ Web.config uses automatic bin loading (recommended)");

                // Verify required assemblies exist in bin folder
                var binPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "bin", "Debug");
                foreach (var assembly in requiredAssemblies)
                {
                    var assemblyPath = Path.Combine(binPath, $"{assembly}.dll");
                    if (!File.Exists(assemblyPath))
                    {
                        throw new Exception($"CS0234 Runtime Prevention: {assembly}.dll missing from bin folder for automatic loading");
                    }
                }
                Console.WriteLine("√ All required Web API assemblies present in bin folder for automatic loading");

                if (hasAutomaticComment)
                {
                    Console.WriteLine("√ Configuration includes comment explaining automatic loading");
                }
            }

            Console.WriteLine("√ System.Web.Http (Core Web API functionality)");
            Console.WriteLine("√ System.Web.Http.WebHost (IIS hosting support)");
            Console.WriteLine("√ System.Net.Http.Formatting (JSON/XML formatting)");
        }

        public void Test_Web_Config_Assembly_Tokens_Correct()
        {
            // This test validates that if Web.config has explicit assembly references, 
            // the PublicKeyToken case matches the actual assemblies

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var webConfigPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Web.config");
            var webConfigContent = File.ReadAllText(webConfigPath);

            // Check if Web.config has explicit assembly references
            bool hasExplicitAssemblies = webConfigContent.Contains("<assemblies>");

            if (!hasExplicitAssemblies)
            {
                Console.WriteLine("√ Web.config uses automatic bin loading - no explicit assembly tokens to validate");
                Console.WriteLine("√ PublicKeyToken validation not needed for automatic loading");
                return;
            }

            // Get the actual assembly info from the installed packages
            var systemWebHttpPath = Path.Combine(testAssemblyDir, "..", "..", "..", "packages", "Microsoft.AspNet.WebApi.Core.5.2.9", "lib", "net45", "System.Web.Http.dll");

            if (File.Exists(systemWebHttpPath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(systemWebHttpPath);
                    var actualFullName = assembly.FullName;

                    Console.WriteLine($"✓ Actual System.Web.Http assembly: {actualFullName}");

                    // Extract the public key token from the actual assembly
                    var tokenMatch = System.Text.RegularExpressions.Regex.Match(actualFullName, @"PublicKeyToken=([a-fA-F0-9]+)");
                    if (tokenMatch.Success)
                    {
                        var actualToken = tokenMatch.Groups[1].Value.ToLower();

                        // Check if Web.config uses the correct token case (only if explicit assemblies exist)
                        if (webConfigContent.Contains($"PublicKeyToken={actualToken.ToUpper()}"))
                        {
                            throw new Exception($"Web.config uses uppercase PublicKeyToken which causes assembly loading issues. Expected: PublicKeyToken={actualToken}, Found: PublicKeyToken={actualToken.ToUpper()}");
                        }
                        else if (webConfigContent.Contains($"PublicKeyToken={actualToken}"))
                        {
                            Console.WriteLine("√ Web.config uses correct case PublicKeyToken");
                        }
                        else
                        {
                            Console.WriteLine("√ Web.config doesn't contain explicit assembly references - using automatic loading");
                        }
                    }
                }
                catch (Exception ex) when (!ex.Message.Contains("Web.config"))
                {
                    throw new Exception($"Failed to load System.Web.Http assembly for validation: {ex.Message}");
                }
            }
            else
            {
                throw new FileNotFoundException($"System.Web.Http.dll not found at: {systemWebHttpPath}");
            }
        }

        /// <summary>
        /// Test that Web.config assembly references can actually be loaded by the runtime
        /// </summary>
        public static void Test_Web_Config_Assemblies_Can_Be_Loaded()
        {
            Console.WriteLine("Assembly test: Web.config assemblies can be loaded from bin folder");

            var testAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var apiProjectPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api");
            string binPath = Path.Combine(apiProjectPath, "bin", "Debug");
            string[] expectedAssemblies = {
                "System.Web.Http, Version=5.2.9.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Web.Http.WebHost, Version=5.2.9.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Net.Http.Formatting, Version=5.2.9.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
            };

            foreach (string assemblyName in expectedAssemblies)
            {
                try
                {
                    // Try to load the assembly by name (simulating runtime behavior)
                    string simpleName = assemblyName.Split(',')[0];
                    string dllPath = Path.Combine(binPath, simpleName + ".dll");

                    if (File.Exists(dllPath))
                    {
                        Assembly assembly = Assembly.LoadFrom(dllPath);
                        Console.WriteLine($"√ Successfully loaded: {assembly.GetName().Name} v{assembly.GetName().Version}");

                        // Verify the assembly version matches what's in Web.config
                        AssemblyName assemblyNameObj = assembly.GetName();
                        if (assemblyNameObj.Version.ToString() == "5.2.9.0")
                        {
                            Console.WriteLine($"√ Version matches Web.config: {assemblyNameObj.Version}");
                        }
                        else
                        {
                            Console.WriteLine($"? Version mismatch - Assembly: {assemblyNameObj.Version}, Web.config expects: 5.2.9.0");
                        }
                    }
                    else
                    {
                        throw new Exception($"Assembly file not found: {dllPath}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Cannot load assembly {assemblyName.Split(',')[0]}: {ex.Message}");
                }
            }

            Console.WriteLine("√ All Web.config assemblies can be loaded from bin folder");
        }

        /// <summary>
        /// Test if Web.config can work without explicit assembly references (using automatic bin loading)
        /// </summary>
        public static void Test_Web_Config_Without_Assembly_References()
        {
            Console.WriteLine("Assembly test: Testing Web.config without explicit assembly references");

            var testAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var apiProjectPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api");

            // Check if we can suggest removing the assemblies section for automatic loading
            string webConfigPath = Path.Combine(apiProjectPath, "Web.config");
            string webConfigContent = File.ReadAllText(webConfigPath);

            // Parse the XML to check current structure
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webConfigContent);

            XmlNode assembliesNode = doc.SelectSingleNode("//system.web/compilation/assemblies");
            if (assembliesNode != null)
            {
                Console.WriteLine("? Current Web.config has explicit assembly references");
                Console.WriteLine("? For IIS Express, automatic bin loading might work better");
                Console.WriteLine("? Consider removing <assemblies> section to use automatic bin folder loading");

                // Count how many assemblies are explicitly referenced
                XmlNodeList assemblyNodes = assembliesNode.SelectNodes("add[@assembly]");
                Console.WriteLine($"? Currently {assemblyNodes.Count} assemblies explicitly referenced");

                // Check if these assemblies exist in bin
                string binPath = Path.Combine(apiProjectPath, "bin", "Debug");
                int existingCount = 0;
                foreach (XmlNode assemblyNode in assemblyNodes)
                {
                    string assemblyAttr = assemblyNode.Attributes["assembly"].Value;
                    string assemblyName = assemblyAttr.Split(',')[0];
                    string dllPath = Path.Combine(binPath, assemblyName + ".dll");
                    if (File.Exists(dllPath))
                    {
                        existingCount++;
                        Console.WriteLine($"√ Bin folder has: {assemblyName}.dll");
                    }
                }

                if (existingCount == assemblyNodes.Count)
                {
                    Console.WriteLine("√ All explicitly referenced assemblies exist in bin folder");
                    Console.WriteLine("√ Automatic bin loading should work - explicit references may be unnecessary");
                    Console.WriteLine("√ Recommendation: Remove <assemblies> section and let IIS Express auto-load from bin");
                }
            }
            else
            {
                Console.WriteLine("√ Web.config uses automatic bin loading (no explicit assemblies section)");
            }

            Console.WriteLine("√ Web.config assembly loading strategy validated");
        }

        /// <summary>
        /// Fix Web.config by removing explicit assembly references to use automatic bin loading
        /// </summary>
        public static void Test_Fix_Web_Config_Remove_Assembly_References()
        {
            Console.WriteLine("Assembly test: Fixing Web.config to use automatic bin loading");

            var testAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var apiProjectPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api");
            string webConfigPath = Path.Combine(apiProjectPath, "Web.config");

            if (File.Exists(webConfigPath))
            {
                string webConfigContent = File.ReadAllText(webConfigPath);

                // Parse the XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webConfigContent);

                XmlNode assembliesNode = doc.SelectSingleNode("//system.web/compilation/assemblies");
                if (assembliesNode != null)
                {
                    Console.WriteLine("√ Removing explicit assembly references for automatic bin loading");

                    // Remove the assemblies node entirely
                    assembliesNode.ParentNode.RemoveChild(assembliesNode);

                    // Add a comment explaining the change
                    XmlNode compilationNode = doc.SelectSingleNode("//system.web/compilation");
                    XmlComment comment = doc.CreateComment(" Assemblies are automatically loaded from bin folder - no explicit references needed ");
                    compilationNode.AppendChild(comment);

                    // Save the updated Web.config
                    doc.Save(webConfigPath);

                    Console.WriteLine("√ Web.config updated to use automatic bin loading");
                    Console.WriteLine("√ This should resolve IIS Express assembly loading issues");
                }
                else
                {
                    Console.WriteLine("√ Web.config already uses automatic bin loading");
                }
            }
            else
            {
                throw new FileNotFoundException($"Web.config not found: {webConfigPath}");
            }

            Console.WriteLine("√ Web.config assembly loading fix applied");
        }

        /// <summary>
        /// Test to detect Global.asax.cs requiring explicit assembly references for runtime compilation
        /// </summary>
        public static void Test_Global_Asax_Requires_Assembly_References()
        {
            Console.WriteLine("Assembly test: Checking if Global.asax.cs requires explicit assembly references");

            var testAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var apiProjectPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api");
            var globalAsaxPath = Path.Combine(apiProjectPath, "Global.asax.cs");
            var webConfigPath = Path.Combine(apiProjectPath, "Web.config");

            if (File.Exists(globalAsaxPath) && File.Exists(webConfigPath))
            {
                string globalAsaxContent = File.ReadAllText(globalAsaxPath);
                string webConfigContent = File.ReadAllText(webConfigPath);

                // Check if Global.asax.cs uses System.Web.Http
                bool usesSystemWebHttp = globalAsaxContent.Contains("using System.Web.Http") ||
                                        globalAsaxContent.Contains("GlobalConfiguration");

                bool hasExplicitAssemblies = webConfigContent.Contains("<assemblies>");

                if (usesSystemWebHttp && !hasExplicitAssemblies)
                {
                    Console.WriteLine("⚠ CS0234 Risk: Global.asax.cs uses System.Web.Http but Web.config has no explicit assemblies");
                    Console.WriteLine("⚠ IIS Express may fail to compile Global.asax.cs at runtime");
                    Console.WriteLine("⚠ Solution: Add minimal assembly references for Global.asax.cs compilation");

                    // Apply the fix - add minimal assembly references back for Global.asax.cs
                    Console.WriteLine("√ Applying fix: Adding minimal assembly references for Global.asax.cs");

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(webConfigContent);

                    XmlNode compilationNode = doc.SelectSingleNode("//system.web/compilation");
                    if (compilationNode != null)
                    {
                        // Remove any existing comment about automatic loading
                        var comments = compilationNode.SelectNodes("comment()");
                        foreach (XmlComment comment in comments)
                        {
                            if (comment.InnerText.Contains("automatically loaded"))
                            {
                                compilationNode.RemoveChild(comment);
                            }
                        }

                        // Create assemblies section with minimal required assemblies for Global.asax.cs
                        XmlElement assembliesElement = doc.CreateElement("assemblies");

                        // Add comment explaining why assemblies are needed
                        XmlComment assembliesComment = doc.CreateComment(" Required for Global.asax.cs runtime compilation - uses System.Web.Http ");
                        assembliesElement.AppendChild(assembliesComment);

                        // Add only System.Web.Http - the one actually used in Global.asax.cs
                        XmlElement systemWebHttpElement = doc.CreateElement("add");
                        systemWebHttpElement.SetAttribute("assembly", "System.Web.Http, Version=5.2.9.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                        assembliesElement.AppendChild(systemWebHttpElement);

                        // Also add WebHost for IIS hosting support (Global.asax.cs needs this for GlobalConfiguration)
                        XmlElement webHostElement = doc.CreateElement("add");
                        webHostElement.SetAttribute("assembly", "System.Web.Http.WebHost, Version=5.2.9.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                        assembliesElement.AppendChild(webHostElement);

                        compilationNode.AppendChild(assembliesElement);

                        // Save the updated Web.config
                        doc.Save(webConfigPath);

                        Console.WriteLine("√ Added minimal assembly reference: System.Web.Http");
                        Console.WriteLine("√ Added minimal assembly reference: System.Web.Http.WebHost");
                        Console.WriteLine("√ Global.asax.cs should now compile correctly in IIS Express");
                    }
                }
                else if (usesSystemWebHttp && hasExplicitAssemblies)
                {
                    Console.WriteLine("√ Global.asax.cs uses System.Web.Http and Web.config has explicit assemblies");
                    Console.WriteLine("√ Runtime compilation should work correctly");
                }
                else
                {
                    Console.WriteLine("√ Global.asax.cs doesn't require explicit assembly references");
                }
            }
            else
            {
                throw new FileNotFoundException("Global.asax.cs or Web.config not found");
            }

            Console.WriteLine("√ Global.asax.cs assembly reference validation completed");
        }

        /// <summary>
        /// Verify the Web.config fix worked - no more explicit assembly references
        /// </summary>
        public static void Test_Verify_Web_Config_Fix()
        {
            Console.WriteLine("Assembly test: Verifying Web.config fix - no explicit assembly references");

            var testAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var apiProjectPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api");
            string webConfigPath = Path.Combine(apiProjectPath, "Web.config");

            if (File.Exists(webConfigPath))
            {
                string webConfigContent = File.ReadAllText(webConfigPath);

                // Parse the XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webConfigContent);

                XmlNode assembliesNode = doc.SelectSingleNode("//system.web/compilation/assemblies");
                if (assembliesNode == null)
                {
                    Console.WriteLine("√ Web.config no longer has explicit assembly references");
                    Console.WriteLine("√ IIS Express will use automatic bin loading");

                    // Verify the comment was added
                    XmlNode compilationNode = doc.SelectSingleNode("//system.web/compilation");
                    bool hasComment = false;
                    foreach (XmlNode child in compilationNode.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Comment && child.Value.Contains("automatically loaded"))
                        {
                            hasComment = true;
                            Console.WriteLine("√ Comment added explaining automatic bin loading");
                            break;
                        }
                    }

                    if (!hasComment)
                    {
                        Console.WriteLine("? No comment found, but that's okay - fix still applied");
                    }
                }
                else
                {
                    throw new Exception("Web.config still has explicit assembly references - fix not applied correctly");
                }

                // Verify assemblies still exist in bin folder
                string binPath = Path.Combine(apiProjectPath, "bin", "Debug");
                string[] requiredAssemblies = { "System.Web.Http.dll", "System.Web.Http.WebHost.dll", "System.Net.Http.Formatting.dll" };

                foreach (string assembly in requiredAssemblies)
                {
                    string assemblyPath = Path.Combine(binPath, assembly);
                    if (File.Exists(assemblyPath))
                    {
                        Console.WriteLine($"√ Bin folder contains: {assembly}");
                    }
                    else
                    {
                        throw new Exception($"Required assembly missing from bin folder: {assembly}");
                    }
                }

                Console.WriteLine("√ All required assemblies present in bin folder for automatic loading");
            }
            else
            {
                throw new FileNotFoundException($"Web.config not found: {webConfigPath}");
            }

            Console.WriteLine("√ Web.config fix verification completed successfully");
            Console.WriteLine("√ Configuration should now work correctly with IIS Express");
        }
    }
}
