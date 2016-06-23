// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace GuardRex.JsonToCsprojConverter
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "guardrex-json-to-csproj-converter",
                FullName = "GuardRex JSON to CSPROJ Converter",
                Description = "Converts project.json into a .csproj file for the project",
            };
            app.HelpOption("-h|--help");

            var frameworkOption = app.Option("-f|--framework <FRAMEWORK>", "Target framework of application being published", CommandOptionType.SingleValue);
            var configurationOption = app.Option("-c|--configuration <CONFIGURATION>", "Target configuration of application being published", CommandOptionType.SingleValue);
            var projectPath = app.Argument("<PROJECT>", "The path to the project (project folder or project.json) being published. If empty the current directory is used.");

            app.OnExecute(() =>
            {
                var exitCode = new ConvertCommand(frameworkOption.Value(), configurationOption.Value(), projectPath.Value).Run();

                return exitCode;
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception e)
            {
                Reporter.Error.WriteLine(e.Message.Red());
                Reporter.Output.WriteLine(e.ToString().Yellow());
            }

            return 1;
        }
    }
}
