using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TodoApp.Tests.Integration
{
    /// <summary>
    /// Tests specifically for CS0260 partial class compilation errors
    /// </summary>
    public class PartialClassValidationTests
    {
        public void Test_Global_Asax_WebApiApplication_Partial_Declaration()
        {
            // This test specifically addresses CS0260: Missing partial modifier error
            // When using Codefile directive, the class must be marked as partial

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var globalAsaxCsPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Global.asax.cs");

            if (!File.Exists(globalAsaxCsPath))
            {
                throw new FileNotFoundException($"Global.asax.cs not found at: {globalAsaxCsPath}");
            }

            var content = File.ReadAllText(globalAsaxCsPath);

            // Check for partial keyword in class declaration
            var partialClassPattern = @"public\s+partial\s+class\s+WebApiApplication";
            if (!Regex.IsMatch(content, partialClassPattern))
            {
                throw new Exception("CS0260 Error Prevention: WebApiApplication must be declared as 'public partial class' when using Codefile directive in Global.asax");
            }

            // Verify it's in the correct namespace
            if (!content.Contains("namespace TodoApp.Api"))
            {
                throw new Exception("WebApiApplication must be in TodoApp.Api namespace");
            }

            // Verify inheritance
            if (!content.Contains(": System.Web.HttpApplication"))
            {
                throw new Exception("WebApiApplication must inherit from System.Web.HttpApplication");
            }

            Console.WriteLine("✓ CS0260 Prevention: WebApiApplication properly declared as partial class");
            Console.WriteLine("✓ Namespace validation: TodoApp.Api");
            Console.WriteLine("✓ Inheritance validation: System.Web.HttpApplication");
        }

        public void Test_Global_Asax_Codefile_Directive_Compatibility()
        {
            // Ensure Global.asax directive is compatible with partial class setup

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var globalAsaxPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Global.asax");

            if (!File.Exists(globalAsaxPath))
            {
                throw new FileNotFoundException($"Global.asax not found at: {globalAsaxPath}");
            }

            var content = File.ReadAllText(globalAsaxPath);

            // Check for Codefile directive (not CodeBehind)
            if (!content.Contains("Codefile=\"Global.asax.cs\""))
            {
                throw new Exception("Global.asax must use Codefile directive for partial class compatibility");
            }

            // Check inheritance declaration
            if (!content.Contains("Inherits=\"TodoApp.Api.WebApiApplication\""))
            {
                throw new Exception("Global.asax must inherit from TodoApp.Api.WebApiApplication");
            }

            Console.WriteLine("✓ Global.asax uses Codefile directive (partial class compatible)");
            Console.WriteLine("✓ Inheritance properly declared in Global.asax");
        }

        public void Test_Compilation_Error_Prevention()
        {
            // This test ensures the specific CS0260 error conditions are prevented

            var testAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            var globalAsaxCsPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Global.asax.cs");
            var globalAsaxPath = Path.Combine(testAssemblyDir, "..", "..", "..", "TodoApp.Api", "Global.asax");

            var csContent = File.ReadAllText(globalAsaxCsPath);
            var asaxContent = File.ReadAllText(globalAsaxPath);

            // CS0260 occurs when:
            // 1. Global.asax uses Codefile directive (creates auto-generated partial class)
            // 2. Global.asax.cs doesn't use partial keyword
            // 3. Both try to declare the same class name

            var usesCodefile = asaxContent.Contains("Codefile=");
            var hasPartialKeyword = csContent.Contains("public partial class");

            if (usesCodefile && !hasPartialKeyword)
            {
                throw new Exception("CS0260 Error Detected: When Global.asax uses Codefile directive, Global.asax.cs class must be marked as 'partial'");
            }

            Console.WriteLine("✓ CS0260 compilation error prevention validated");
            Console.WriteLine($"✓ Codefile directive: {usesCodefile}");
            Console.WriteLine($"✓ Partial class declaration: {hasPartialKeyword}");
        }
    }
}
