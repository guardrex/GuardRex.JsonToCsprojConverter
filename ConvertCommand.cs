// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Xml.Linq;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.Cli.Utils;
using Newtonsoft.Json;

namespace GuardRex.JsonToCsprojConverter
{
    public class ConvertCommand
    {
        private readonly string _projectPath;
        private readonly string _framework;
        private readonly string _configuration;

        public ConvertCommand(string framework, string configuration, string projectPath)
        {
            _projectPath = projectPath;
            _framework = framework;
            _configuration = configuration;
        }

        public int Run()
        {
            var applicationBasePath = GetApplicationBasePath();
            var projectJsonPath = Path.Combine(applicationBasePath, "project2.json");
            var projectContext = GetProjectContext(applicationBasePath, _framework);
            var appName = projectContext.ProjectFile.GetCompilerOptions(projectContext.TargetFramework, _configuration).OutputName;
            var appCsprojPath = Path.Combine(applicationBasePath, appName + ".csproj");

            string projectJsonContent = string.Empty;
            
            if (File.Exists(projectJsonPath))
            {
                
                try
                {
                    Reporter.Output.WriteLine($"Reading project.json file at '{projectJsonPath}'");
                    projectJsonContent = File.ReadAllText(projectJsonPath);

                    XDocument doc = (XDocument)JsonConvert.DeserializeXNode(projectJsonContent);

                    using (var f = new FileStream(appCsprojPath, FileMode.Create))
                    {
                        doc.Save(f);
                    }

                    Reporter.Output.WriteLine("Conversion of project.json successful");
                }
                catch (System.Exception ex)
                {
                    Reporter.Output.WriteLine($"Exception reading file: '{ex.Message}'");
                }
            }
            else
            {
                Reporter.Output.WriteLine($"No project.json found. Skipping '{projectJsonPath}'");
            }

            return 0;
        }

        private string GetApplicationBasePath()
        {
            if (!string.IsNullOrEmpty(_projectPath))
            {
                var fullProjectPath = Path.GetFullPath(_projectPath);

                return Path.GetFileName(fullProjectPath) == "project.json"
                    ? Path.GetDirectoryName(fullProjectPath)
                    : fullProjectPath;
            }

            return Directory.GetCurrentDirectory();
        }

        private static ProjectContext GetProjectContext(string applicationBasePath, string framework)
        {
            var project = ProjectReader.GetProject(Path.Combine(applicationBasePath, "project.json"));

            return new ProjectContextBuilder()
                .WithProject(project)
                .WithTargetFramework(framework)
                .Build();
        }
    }
}
