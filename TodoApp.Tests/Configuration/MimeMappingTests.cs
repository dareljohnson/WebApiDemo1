using System;
using System.IO;
using System.Xml;

namespace TodoApp.Tests.Configuration
{
    /// <summary>
    /// Tests for Web.config MIME mapping configuration to prevent HTTP 500.19 errors
    /// </summary>
    public class MimeMappingTests
    {
        public void Test_Web_Config_Mime_Mapping_No_Duplicates()
        {
            // Arrange
            var webConfigPath = GetWebConfigPath();

            // Act & Assert
            try
            {
                var doc = new XmlDocument();
                doc.Load(webConfigPath);

                var staticContentNodes = doc.SelectNodes("//staticContent");

                if (staticContentNodes != null && staticContentNodes.Count > 0)
                {
                    var staticContentNode = staticContentNodes[0];
                    var jsonMimeMapNodes = staticContentNode.SelectNodes("mimeMap[@fileExtension='.json']");
                    var jsonRemoveNodes = staticContentNode.SelectNodes("remove[@fileExtension='.json']");

                    // Should have exactly one remove and one add for .json
                    if (jsonMimeMapNodes != null && jsonMimeMapNodes.Count > 0)
                    {
                        if (jsonRemoveNodes == null || jsonRemoveNodes.Count == 0)
                        {
                            throw new Exception("MIME mapping for .json found without corresponding remove element - this will cause HTTP 500.19 duplicate entry error");
                        }

                        if (jsonMimeMapNodes.Count > 1)
                        {
                            throw new Exception($"Multiple MIME mappings for .json found: {jsonMimeMapNodes.Count} - this will cause HTTP 500.19 duplicate entry error");
                        }

                        // Verify the MIME type is correct
                        var mimeMapNode = jsonMimeMapNodes[0] as XmlElement;
                        var mimeType = mimeMapNode?.GetAttribute("mimeType");
                        if (mimeType != "application/json")
                        {
                            throw new Exception($"Incorrect MIME type for .json: {mimeType}, expected: application/json");
                        }

                        Console.WriteLine("✓ MIME mapping for .json is properly configured with remove/add pattern");
                        Console.WriteLine($"✓ Correct MIME type: {mimeType}");
                    }
                    else
                    {
                        Console.WriteLine("✓ No .json MIME mapping found - using IIS default");
                    }
                }
                else
                {
                    Console.WriteLine("✓ No staticContent section found - using IIS defaults");
                }
            }
            catch (XmlException ex)
            {
                throw new Exception($"Web.config XML parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"MIME mapping validation failed: {ex.Message}");
            }
        }

        public void Test_Web_Config_Static_Content_Configuration()
        {
            // Arrange
            var webConfigPath = GetWebConfigPath();

            // Act & Assert
            try
            {
                var doc = new XmlDocument();
                doc.Load(webConfigPath);

                var systemWebServerNode = doc.SelectSingleNode("//system.webServer");
                if (systemWebServerNode == null)
                {
                    throw new Exception("system.webServer section not found in Web.config");
                }

                var staticContentNode = systemWebServerNode.SelectSingleNode("staticContent");
                if (staticContentNode != null)
                {
                    // Validate that staticContent section is properly structured
                    var childNodes = staticContentNode.ChildNodes;
                    bool hasValidStructure = true;
                    string structureInfo = "";

                    foreach (XmlNode child in childNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (child.Name == "remove" || child.Name == "mimeMap")
                            {
                                var fileExtension = (child as XmlElement)?.GetAttribute("fileExtension");
                                structureInfo += $"{child.Name}[@fileExtension='{fileExtension}'] ";
                            }
                            else
                            {
                                hasValidStructure = false;
                                throw new Exception($"Invalid element in staticContent: {child.Name}");
                            }
                        }
                    }

                    if (hasValidStructure)
                    {
                        Console.WriteLine("✓ staticContent section has valid structure");
                        Console.WriteLine($"✓ Elements: {structureInfo.Trim()}");
                    }
                }
                else
                {
                    Console.WriteLine("✓ No staticContent section found - using IIS defaults");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Static content configuration validation failed: {ex.Message}");
            }
        }

        public void Test_Web_Config_No_Configuration_Errors()
        {
            // Arrange
            var webConfigPath = GetWebConfigPath();

            // Act & Assert
            try
            {
                var doc = new XmlDocument();
                doc.Load(webConfigPath);

                // Check for common configuration issues that cause HTTP 500.19
                var issues = new System.Collections.Generic.List<string>();

                // 1. Check for duplicate mimeMap entries
                var allMimeMapNodes = doc.SelectNodes("//mimeMap");
                if (allMimeMapNodes != null)
                {
                    var extensions = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (XmlElement mimeMap in allMimeMapNodes)
                    {
                        var ext = mimeMap.GetAttribute("fileExtension");
                        if (!string.IsNullOrEmpty(ext))
                        {
                            if (extensions.ContainsKey(ext))
                                extensions[ext]++;
                            else
                                extensions[ext] = 1;
                        }
                    }

                    foreach (var kvp in extensions)
                    {
                        if (kvp.Value > 1)
                        {
                            // Check if there's a corresponding remove element
                            var removeNodes = doc.SelectNodes($"//remove[@fileExtension='{kvp.Key}']");
                            if (removeNodes == null || removeNodes.Count == 0)
                            {
                                issues.Add($"Duplicate mimeMap for {kvp.Key} without remove element");
                            }
                        }
                    }
                }

                // 2. Check for malformed XML attributes
                var allElements = doc.SelectNodes("//*[@*]");
                if (allElements != null)
                {
                    foreach (XmlElement element in allElements)
                    {
                        if (element.Attributes != null)
                        {
                            foreach (XmlAttribute attr in element.Attributes)
                            {
                                if (string.IsNullOrEmpty(attr.Value))
                                {
                                    issues.Add($"Empty attribute value: {element.Name}@{attr.Name}");
                                }
                            }
                        }
                    }
                }

                if (issues.Count > 0)
                {
                    throw new Exception($"Configuration issues found: {string.Join(", ", issues)}");
                }

                Console.WriteLine("✓ No configuration errors detected that would cause HTTP 500.19");
                Console.WriteLine("✓ Web.config is properly formatted for IIS deployment");
            }
            catch (XmlException ex)
            {
                throw new Exception($"Web.config XML error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Configuration error validation failed: {ex.Message}");
            }
        }

        private string GetWebConfigPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var webConfigPath = Path.Combine(currentDir, "..", "TodoApp.Api", "Web.config");

            if (!File.Exists(webConfigPath))
            {
                throw new FileNotFoundException($"Web.config not found at: {webConfigPath}");
            }

            return webConfigPath;
        }
    }
}
