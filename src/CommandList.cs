// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using Windows.Management.Deployment;

namespace AppData
{
    class CommandList : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData List <packagefamilyname> [options] - List package families with application data
options:
  -a, --all       = All package types [default]
  -m, --main      = List main packages
  -o, --optional  = List optional packages

EXAMPLES:
  appdata list *network* --main
    Lists Main package families with application data and 'network' in their name

appdata list *
    Lists all packages with application data
";
#pragma warning restore

        PackageTypes m_packageTypes = PackageTypes.Main | PackageTypes.Optional;

        public CommandList(string[] options)
        {
            Parse(options);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == GetRequiredArgumentsCount());
            this.packageFamilyName = this.requiredArguments[0];
        }

        void DisplayHelp()
        {
            DisplayHelp(this);
        }

        public override void Parse(string arg)
        {
            if (arg.Equals("-a", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--all", StringComparison.InvariantCultureIgnoreCase))
                m_packageTypes = PackageTypes.Main | PackageTypes.Optional;
            else if (arg.Equals("-m", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--main", StringComparison.InvariantCultureIgnoreCase))
                m_packageTypes = PackageTypes.Main;
            else if (arg.Equals("-o", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--optional", StringComparison.InvariantCultureIgnoreCase))
                m_packageTypes = PackageTypes.Optional;
            else
                base.Parse(arg);
        }

        public override void Execute()
        {
            PrintLineVerbose("execute");
            try
            {
                var packageManager = new PackageManager();
                foreach (var item in packageManager.FindPackagesForUserWithPackageTypes("", m_packageTypes))
                {
                    var familyName = item.Id.FamilyName;
                    if (Glob.Match(this.packageFamilyName, familyName))
                    {
                        PrintLine($"{item.Id.FamilyName}");
                    }
                }
            }
            catch (Exception ex) { FatalError(ex.ToString()); }
        }
    }
}
