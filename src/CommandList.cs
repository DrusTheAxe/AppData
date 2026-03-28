// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

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
{0}
EXAMPLES:
  appdata list *network* --main
    Lists Main package families with application data and 'network' in their name

appdata list *
    Lists all packages with application data
";
#pragma warning restore

        [Flags]
        enum PackageType
        {
            Main = Windows.Management.Deployment.PackageType.Main,
            Optional = Windows.Management.Deployment.PackageType.Optional,
            All = Main | Optional
        };
        PackageType packageType = PackageType.All;

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
                packageType = PackageType.All;
            else if (arg.Equals("-m", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--main", StringComparison.InvariantCultureIgnoreCase))
                packageType = PackageType.Local;
            else if (arg.Equals("-o", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--optional", StringComparison.InvariantCultureIgnoreCase))
                packageType = PackageType.Roaming;
            else
                base.Parse(arg);
        }

        public override void Execute()
        {
            PrintLineVerbose("open");
            OpenApplicationData();

            PrintLineVerbose("execute");
            var result = ExecuteAsync();
            try
            {
                //PrintLineFormat("     Local: {0}", appdata.LocalFolder.Path);
            }
            catch (Exception ex) { FatalError(ex.ToString()); }
        }
    }
}
