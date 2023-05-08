// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using Windows.Storage;

namespace AppData
{
    class CommandGet : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Get <packagefamilyname> <path> [options] - Display application data
where:
  path              = Path to the data, in the form of ROOT\Container
                        ROOT = [Local | Roaming]
                        Container = The full name of the container under the selected ROOT
options:
  --find:case       = Search comparisons are case sensitive.
  --find:nocase     = Search comparisons are case insensitive. [Default]
  --find:container=NAME = Search for containers with matching names. [Default=""*""]
  --find:value=KEY  = Search for values with matching keys. [Default=""*""]
  -r, --recurse     = Get all containers and subkeys recursively (like dir /s)
  --type=TYPE       = Specify the value data type. Valid types are:
                        EMPTY, BOOLEAN, CHAR16, STRING, UINT8, INT16, UINT16,
                        INT32, UINT32, INT64, UINT64, SINGLE, DOUBLE, DATETIME,
                        TIMESPAN, GUID, POINT, SIZE, RECT,
                        BOOLEAN[], CHAR16[], STRING[], UINT8[],
                        INT16[], UINT16[], INT32[], UINT32[], INT64[],
                        UINT64[], SINGLE[], DOUBLE[], DATETIME[], TIMESPAN[],
                        GUID[], POINT[], SIZE[], RECT[]
  --value=KEY       = Get a specific application data value
{0}
EXAMPLES:
  appdata get contosso.games.solitaire_1234567890abc local\people
    Displays all values in the Local\people container

  appdata get contosso.games.solitaire_1234567890abc local\people --recurse
    Displays all values and subcontainers in the Local\people container

  appdata get contosso.games.solitaire_1234567890abc local\people --type=GUID
    Displays values in the Local\people container whose type is GUID

  appdata get contosso.games.solitaire_1234567890abc local\people --find:value=*ward*
    Displays values in the Local\people container whose key contains ""ward""

  appdata get contosso.games.solitaire_1234567890abc local\people --find:value=J?n
    Displays values in the Local\people container whose key equals ""J?n""

  appdata get contosso.games.solitaire_1234567890abc local\people --find:value=J?n --recurse
    Displays values in the Local\people container and subcontainers whose key equals ""J?n""

  appdata get contosso.games.solitaire_1234567890abc local\people --find:container=Places --recurse
    Displays all values in all containers named ""Places"" under the Local\people container
";
#pragma warning restore

        [Flags]
        enum Locality
        {
            Local = Windows.Storage.ApplicationDataLocality.Local,
            Roaming = Windows.Storage.ApplicationDataLocality.Roaming
        };
        Locality locality = Locality.Roaming;

        bool recurse = false;

        AppDataType.Type matchType = AppDataType.Type.Unknown;
        string matchValueKey;

        Wildcard.WildcardOptions findOptions = Wildcard.WildcardOptions.IgnoreCase;
        string findContainerName;
        Wildcard findContainerNameWildcard;
        string findValueKey;
        Wildcard findValueKeyWildcard;

        string path = "";

        public CommandGet(string[] options)
        {
            Parse(options);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == GetRequiredArgumentsCount());
            this.packageFamilyName = this.requiredArguments[0];
            ParsePathParameter();

            if (!this.findContainerName.IsEmpty())
                this.findContainerNameWildcard = new Wildcard(this.findContainerName, this.findOptions);
            if (!this.findValueKey.IsEmpty())
                this.findValueKeyWildcard = new Wildcard(this.findValueKey, this.findOptions);
        }

        void DisplayHelp()
        {
            DisplayHelp(this);
        }

        public override void Parse(string arg)
        {
            if (arg.StartsWith("--find:case", StringComparison.InvariantCultureIgnoreCase))
            {
                this.findOptions = Wildcard.WildcardOptions.None;
            }
            else if (arg.StartsWith("--find:nocase", StringComparison.InvariantCultureIgnoreCase))
            {
                this.findOptions = Wildcard.WildcardOptions.IgnoreCase;
            }
            else if (arg.StartsWith("--find:container=", StringComparison.InvariantCultureIgnoreCase))
            {
                string argValue = arg.Substring("--find:container=".Length);
                if (argValue.IsEmpty())
                    FatalError(String.Format("Invalid name ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
                this.findContainerName = argValue;
            }
            else if (arg.StartsWith("--find:value=", StringComparison.InvariantCultureIgnoreCase))
            {
                string argValue = arg.Substring("--find:value=".Length);
                if (argValue.IsEmpty())
                    FatalError(String.Format("Invalid key ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
                this.findValueKey = argValue;
            }
            else if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--recurse", StringComparison.InvariantCultureIgnoreCase))
            {
                this.recurse = true;
            }
            else if (arg.StartsWith("--type=", StringComparison.InvariantCultureIgnoreCase))
            {
                string argValue = arg.Substring("--type=".Length);
                try
                {
                    this.matchType = AppDataType.ToAppDataType(argValue);
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
                this.matchValueKey = argValue;
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

            if (this.path.IsEmpty())
                Walk(settings);
            else
            {
                ApplicationDataContainer root;
                try
                {
                    root = settings.CreateContainer(this.path, ApplicationDataCreateDisposition.Existing);
                }
                catch (Exception ex)
                {
                    if (ex.HResult == Win32Errors.ERROR_OBJECT_NOT_FOUND)
                        FatalError("Container path not found", this.path);
                    else
                        FatalError(ex.ToString());
                    return;
                }
                Walk(root, this.path);
            }
        }

        private void ParsePathParameter()
        {
            System.Diagnostics.Debug.Assert(this.requiredArguments != null);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == 2);
            string arg = this.requiredArguments[1];
            string[] rootAndPath = arg.Split(new char[] { '\\' }, 2, StringSplitOptions.None);
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

        private void Walk(ApplicationDataContainer container)
        {
            Walk(container, "");
        }

        private void Walk(ApplicationDataContainer container, string path)
        {
            System.Diagnostics.Debug.Assert(container != null);

            if (this.findContainerNameWildcard == null || this.findContainerNameWildcard.IsMatch(container.Name))
            {
                bool showContainerName = true;
                var values = container.Values;
                foreach (var key in container.Values.Keys)
                {
                    // Are we looking for a specific value (key)?
                    if (this.matchValueKey != null && !this.matchValueKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (this.findValueKeyWildcard != null && !this.findValueKeyWildcard.IsMatch(key))
                        continue;

                    var value = values[key];

                    var type = value.GetAppDataType();

                    // Are we looking for a specific type?
                    if (this.matchType != AppDataType.Type.Unknown && type != this.matchType)
                        continue;

                    // Match!
                    if (showContainerName)
                    {
                        DumpContainer(container, path);
                        showContainerName = false;
                    }
                    DumpValue(key, value, type);
                }
                if (showContainerName && this.matchValueKey == null && this.findValueKeyWildcard == null && this.matchType == AppDataType.Type.Unknown)
                    DumpContainer(container, path);
            }

            if (this.recurse)
            {
                foreach (var childName in container.Containers.Keys)
                {
                    var child = container.Containers[childName];
                    Walk(child, path + @"\" + child.Name);
                }
            }
        }

        private void DumpContainer(ApplicationDataContainer container, string path)
        {
            PrintLine();
            PrintLine(AppDataExtensions.BuildPath(container, path));
        }

        private void DumpValue(string key, object value, AppDataType.Type type)
        {
            if (type == AppDataType.Type.ApplicationDataCompositeValue)
                DumpComposite(key, (ApplicationDataCompositeValue)value);
            else
                PrintLineFormat("    {0}    {1}    {2}", key.ToString(), type.ToString(), value.ToString());
        }

        private void DumpComposite(string compositeKey, ApplicationDataCompositeValue composite)
        {
            PrintLineFormat("    {0}    {1}", compositeKey.ToString(), AppDataType.Type.ApplicationDataCompositeValue.ToString());

            var values = composite.Values;
            foreach (var key in composite.Keys)
            {
                // Are we looking for a specific value (key)?
                if (this.matchValueKey != null && !this.matchValueKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (this.findValueKeyWildcard != null && !this.findValueKeyWildcard.IsMatch(key))
                    continue;

                var value = composite[key];

                var type = value.GetAppDataType();
                System.Diagnostics.Debug.Assert(type != AppDataType.Type.ApplicationDataCompositeValue);

                // Are we looking for a specific type?
                if (this.matchType != AppDataType.Type.Unknown && type != this.matchType)
                    continue;

                // Match!
                PrintLineFormat("        {0}    {1}    {2}", key.ToString(), type.ToString(), value.ToString());

            }
        }
    }
}
