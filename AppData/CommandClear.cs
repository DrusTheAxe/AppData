// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace AppData
{
    class CommandClear : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Clear <packagefamilyname> [options] - Clear application data
options:
  -a, --all       = All application data stores [default]
  -l, --local     = Local application data store
  -r, --roaming   = Roaming application data store
  -t, --temporary = Temporary application data store
{0}
EXAMPLES:
  appdata clear contosso.games.solitaire_1234567890abc --local
    Clears the local folder and settings

appdata clear contosso.games.solitaire_1234567890abc
    Clears all folders and settings
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

        public CommandClear(string[] options)
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
            var result = ExecuteAsync();

            PrintLineVerbose("wait");
            result.Wait();

            PrintLineVerbose("done");
            ///***SEEME***
            //Console.WriteLine(result.ToString());
            //Console.WriteLine(result.AsyncState.ToString());
            //Console.WriteLine(result.Exception.ToString());
            //Console.WriteLine(result.Id);
            //Console.WriteLine(result.IsCanceled);
            //Console.WriteLine(result.IsCompleted);
            //Console.WriteLine(result.IsFaulted);
            PrintLineVerboseFormat("Status: {0}", result.Status);
        }

        async Task ExecuteAsync()
        {
            try
            {
                switch (locality)
                {
                    case Locality.All: await appdata.ClearAsync(); break;
                    case Locality.Local: await appdata.ClearAsync(Windows.Storage.ApplicationDataLocality.Local); break;
                    case Locality.Roaming: await appdata.ClearAsync(Windows.Storage.ApplicationDataLocality.Roaming); break;
                    case Locality.Temporary: await appdata.ClearAsync(Windows.Storage.ApplicationDataLocality.Temporary); break;
                }
            }
            catch (Exception ex) { FatalError(ex.ToString()); }
        }
    }
}
