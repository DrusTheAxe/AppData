// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;

namespace AppData
{
    class CommandVersion : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Version <packagefamilyname> [options] - Retrieve the version number
options:
{0}
EXAMPLES:
  appdata version contosso.games.solitaire_1234567890abc
    Display the version of the application data
";
#pragma warning restore

        public CommandVersion(string[] options)
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
            Console.WriteLine("open");
            OpenApplicationData();

            Console.WriteLine("Version: {0}", appdata.Version);
        }
    }
}
