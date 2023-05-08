// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using Windows.Storage;

namespace AppData
{
    class CommandSet : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Set <packagefamilyname> <path> [options] - Alter application data
where:
  path              = Path to the data, in the form of ROOT\Container
                        ROOT = [Local | Roaming]
                        Container = The full name of the container under the selected ROOT
options:
  --value=KEY       = Set a specific application data value
  --type=TYPE       = Specify the value data type. Valid types are:
                        EMPTY, BOOLEAN, CHAR16, STRING, UINT8, INT16, UINT16,
                        INT32, UINT32, INT64, UINT64, SINGLE, DOUBLE
                        [Default=STRING]
  --data=DATA       = Specify the data to assign to the key
  --overwrite:prompt= Prompt to confirm overwriting an existing value [Default]
  --overwrite:yes   = Overwrite an existing value
  --overwrite:no    = Never overwrite an existing value
{0}
EXAMPLES:
  appdata set contosso.games.solitaire_1234567890abc local\configuration\startup
      Adds a container local\configuration\startup

  appdata set contosso.games.solitaire_1234567890abc local\configuration --value=greet --type=boolean --data=true
      Adds a value (container: local\configuration, type: BOOLEAN, value: greet, data: true)
";
#pragma warning restore

        static AppDataType.Type[] supportedAppDataTypes = new AppDataType.Type[]{
            AppDataType.Type.Empty, AppDataType.Type.Boolean, AppDataType.Type.Char16, AppDataType.Type.String,
            AppDataType.Type.UInt8, AppDataType.Type.Int16, AppDataType.Type.UInt16, AppDataType.Type.Int32,
            AppDataType.Type.UInt32, AppDataType.Type.Int64, AppDataType.Type.UInt64, AppDataType.Type.Single, AppDataType.Type.Double
        };

        [Flags]
        enum Locality
        {
            Local = Windows.Storage.ApplicationDataLocality.Local,
            Roaming = Windows.Storage.ApplicationDataLocality.Roaming
        };
        Locality locality = Locality.Roaming;

        string valueKey;
        AppDataType.Type dataType = AppDataType.Type.String;
        string dataAsString;
        object data;

        Overwrite overwrite = Overwrite.Prompt;

        string path = "";

        public CommandSet(string[] options)
        {
            Parse(options);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == GetRequiredArgumentsCount());
            this.packageFamilyName = this.requiredArguments[0];
            ParsePathParameter();

            try
            {
                this.data = AppDataType.Parse(this.dataType, this.dataAsString);
            }
            catch (FormatException)
            {
                FatalError(String.Format("Invalid data ({0}) for the type ({1}); use 'APPDATA {2} --help' for usage", this.dataAsString, AppDataType.ToString(this.dataType), GetCommandName()));
            }
        }

        void DisplayHelp()
        {
            DisplayHelp(this);
        }

        public override void Parse(string arg)
        {
            if (arg.StartsWith("--data=", StringComparison.InvariantCultureIgnoreCase))
            {
                this.dataAsString = arg.Substring("--data=".Length);
            }
            else if (arg.Equals("--overwrite:no", StringComparison.InvariantCultureIgnoreCase))
            {
                this.overwrite = Overwrite.No;
            }
            else if (arg.Equals("--overwrite:prompt", StringComparison.InvariantCultureIgnoreCase))
            {
                this.overwrite = Overwrite.Prompt;
            }
            else if (arg.Equals("--overwrite:yes", StringComparison.InvariantCultureIgnoreCase))
            {
                this.overwrite = Overwrite.Yes;
            }
            else if (arg.StartsWith("--type=", StringComparison.InvariantCultureIgnoreCase))
            {
                string argValue = arg.Substring("--type=".Length);
                try
                {
                    this.dataType = AppDataType.ToAppDataType(argValue, supportedAppDataTypes);
                }
                catch (NotSupportedException)
                {
                    FatalError(String.Format("Unknown type ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
                }
            }
            else if (arg.StartsWith("--value=", StringComparison.InvariantCultureIgnoreCase))
            {
                string argValue = arg.Substring("--value=".Length);
                if (argValue.IsEmpty())
                    FatalError(String.Format("Invalid key ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
                this.valueKey = argValue;
            }
            else
                base.Parse(arg);
        }

        protected override int GetRequiredArgumentsCount() { return 2; }

        public override void Execute()
        {
            PrintLineVerbose("open");
            OpenApplicationData();

            PrintLineVerbose("execute");
            System.Diagnostics.Debug.Assert(this.locality == Locality.Local || this.locality == Locality.Roaming);
            var settings = (this.locality == Locality.Local) ? appdata.LocalSettings : appdata.RoamingSettings;

            PrintLineVerbose("container");
            ApplicationDataContainer container;
            if (this.path.IsEmpty())
                container = settings;
            else
            {
                try
                {
                    container = settings.CreateContainer(this.path, ApplicationDataCreateDisposition.Always);
                }
                catch (Exception ex)
                {
                    if (ex.HResult == Win32Errors.ERROR_OBJECT_NOT_FOUND)
                        FatalError("Container path not found", this.path);
                    else
                        FatalError(ex.ToString());
                    return;
                }
            }

            if (!this.valueKey.IsEmpty())
            {
                PrintLineVerbose("value");
                if (container.Values.ContainsKey(this.valueKey))
                {
                    if (this.overwrite == Overwrite.No)
                        FatalError("The operation was canceled (--overwrite:no).");
                    else if (this.overwrite == Overwrite.Prompt)
                    {
                        bool ok = PromptForOverwrite(this.valueKey);
                        if (!ok)
                            FatalError("The operation was canceled by the user.");
                    }
                }
                container.Values[this.valueKey] = this.data;
            }

            PrintLine("The operation completed successfully.");
        }

        private bool PromptForOverwrite(string key)
        {
            do
            {
                Console.Write(String.Format("Value {0} exists, overwrite (Yes/No)? ", key));
                string input = Console.ReadLine();
                if (input.Length >= 1)
                {
                    if (input[0] == 'Y' || input[0] == 'y')
                        return true;
                    else if (input[0] == 'N' || input[0] == 'n')
                        return false;
                }
            } while (true);
        }

        private void ParsePathParameter()
        {
            System.Diagnostics.Debug.Assert(this.requiredArguments != null);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == 2);
            string arg = this.requiredArguments[1];
            string[] rootAndPath = arg.Split(new char[] { '\\', '/' }, 2, StringSplitOptions.None);
            if (rootAndPath == null || rootAndPath.Length < 1 || rootAndPath[0].IsEmpty())
                FatalError(String.Format("Invalid path ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
            string prefix = rootAndPath[0];
            if (prefix.Equals("local", StringComparison.InvariantCultureIgnoreCase))
                this.locality = Locality.Local;
            else if (prefix.Equals("roaming", StringComparison.InvariantCultureIgnoreCase))
                this.locality = Locality.Roaming;
            else
                FatalError(String.Format("Invalid locality in path ({0}); use 'APPDATA {1} --help' for usage", prefix, GetCommandName()));
            string suffix = rootAndPath.Length > 1 ? rootAndPath[1] : null;
            this.path = suffix.IsEmpty() ? null : suffix;
        }
    }
}
