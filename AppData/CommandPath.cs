using System;

namespace AppData
{
    class CommandPath : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Path <packagefamilyname> [options] - Retrieves path(s)
options:
  -a, --all         = All application data stores [default]
  -l, --local       = Local application data store
  -r, --roaming     = Roaming application data store
  -t, --temporary   = Temporary application data store
{0}
EXAMPLES:
  appdata path contosso.games.solitaire_1234567890abc --local
    Display the path to the local folder

appdata path contosso.games.solitaire_1234567890abc
    Display the paths to the local, roaming and temporary folders
";
#pragma warning restore

        [Flags]
        enum Locality
        {
            All = -1,
            Local = Windows.Storage.ApplicationDataLocality.Local,
            Roaming = Windows.Storage.ApplicationDataLocality.Roaming,
            Temporary = Windows.Storage.ApplicationDataLocality.Temporary
        };
        Locality locality = Locality.All;

        public CommandPath(string[] options)
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
                locality = Locality.All;
            else if (arg.Equals("-l", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--local", StringComparison.InvariantCultureIgnoreCase))
                locality = Locality.Local;
            else if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--roaming", StringComparison.InvariantCultureIgnoreCase))
                locality = Locality.Roaming;
            else if (arg.Equals("-t", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--temporary", StringComparison.InvariantCultureIgnoreCase))
                locality = Locality.Temporary;
            else
                base.Parse(arg);
        }

        public override void Execute()
        {
            PrintLineVerbose("open");
            OpenApplicationData();

            PrintLineVerbose("execute");
            if (this.locality == Locality.All || this.locality == Locality.Local)
                PrintLineFormat("    Local: {0}", appdata.LocalFolder.Path);
            if (this.locality == Locality.All || this.locality == Locality.Roaming)
                PrintLineFormat("  Roaming: {0}", appdata.RoamingFolder.Path);
            if (this.locality == Locality.All || this.locality == Locality.Temporary)
                PrintLineFormat("Temporary: {0}", appdata.TemporaryFolder.Path);
        }
    }
}
