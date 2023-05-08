// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

namespace AppData
{
    class CommandQuota : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Quota <packagefamilyname> [options] - Retrieve the roaming storage quota
options:
{0}
EXAMPLES:
  appdata quota contosso.games.solitaire_1234567890abc
    Display the maximum size of roaming data the can be synchronized to the cloud
";
#pragma warning restore

        public CommandQuota(string[] options)
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
            base.Parse(arg);
        }

        public override void Execute()
        {
            PrintLineVerbose("open");
            OpenApplicationData();

            PrintLineFormat("Quota: {0} KB", appdata.RoamingStorageQuota);
        }
    }
}
