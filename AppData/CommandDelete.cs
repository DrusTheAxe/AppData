// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using Windows.Storage;

namespace AppData
{
    class CommandDelete : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Delete <packagefamilyname> <path> [options] - Alter application data
where:
  path            = Path to the data, in the form of ROOT\Container
                      ROOT = [Local | Roaming]
                      Container = The full name of the container under the selected ROOT
options:
  --value=KEY     = Delete a specific application data value
  --value:all     = Delete all values in this container
  --force         = Force the deletion without prompt
{0}
EXAMPLES:
  appdata delete contosso.games.solitaire_1234567890abc local\configuration\startup
      Deletes the container local\configuration\startup and all its subcontainers and values

  appdata delete contosso.games.solitaire_1234567890abc local\configuration --value=greet
      Deletes the value greet in the container local\configuration

  appdata delete contosso.games.solitaire_1234567890abc local\configuration --value:all
      Deletes all values in the container local\configuration

  appdata delete contosso.games.solitaire_1234567890abc local\configuration --value:all --force
      Deletes all values in the container local\configuration without prompting for confirmation
";
#pragma warning restore

        [Flags]
        enum Locality
        {
            Local = Windows.Storage.ApplicationDataLocality.Local,
            Roaming = Windows.Storage.ApplicationDataLocality.Roaming
        };
        Locality locality = Locality.Roaming;

        string valueKey;
        bool deleteAllValuesInContainer;
        bool force = false;

        string path = "";

        public CommandDelete(string[] options)
        {
            Parse(options);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == GetRequiredArgumentsCount());
            this.packageFamilyName = this.requiredArguments[0];
            ParsePathParameter();
        }

        void DisplayHelp()
        {
            DisplayHelp(this);
        }

        public override void Parse(string arg)
        {
            if (arg.Equals("--force", StringComparison.InvariantCultureIgnoreCase))
            {
                this.force = true;
            }
            else if (arg.StartsWith("--value=", StringComparison.InvariantCultureIgnoreCase))
            {
                string argValue = arg.Substring("--value=".Length);
                if (argValue.IsEmpty())
                    FatalError(String.Format("Invalid key ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
                this.valueKey = argValue;
            }
            else if (arg.Equals("--value:all", StringComparison.InvariantCultureIgnoreCase))
            {
                this.deleteAllValuesInContainer = true;
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
            if (this.valueKey.IsEmpty() && !this.deleteAllValuesInContainer)
            {
                System.Diagnostics.Debug.Assert(!this.path.IsEmpty());
                if (!this.force && !Stdio.Prompt(String.Format("Permanently delete the container {0} (Yes/No)? ", AppDataExtensions.BuildPath(settings, this.path))))
                    FatalError("The operation was canceled by the user.");
                try
                {
                    settings.DeleteContainer(this.path);
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
            else
            {
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

                PrintLineVerbose("delete");
                if (this.deleteAllValuesInContainer)
                {
                    DeleteValues(container, AppDataExtensions.BuildPath(container, this.path));
                }
                else
                {
                    DeleteValue(container, this.valueKey);
                }
            }

            PrintLine("The operation completed successfully.");
        }

        private void DeleteValues(ApplicationDataContainer container, string containerPath)
        {
            System.Diagnostics.Debug.Assert(container != null);

            if (container.Values.Count > 0)
            {
                if (!this.force)
                {
                    bool ok = PromptForDelete(container.Values, containerPath);
                    if (!ok)
                        FatalError("The operation was canceled by the user.");
                }
                foreach (var key in container.Values.Keys)
                    container.Values.Remove(key);
            }
        }

        private void DeleteValue(ApplicationDataContainer container, string key)
        {
            System.Diagnostics.Debug.Assert(container != null);
            System.Diagnostics.Debug.Assert(!key.IsEmpty());

            if (container.Values.ContainsKey(key))
            {
                if (!this.force)
                {
                    bool ok = PromptForDelete(key);
                    if (!ok)
                        FatalError("The operation was canceled by the user.");
                }
                PrintLineVerboseFormat("Delete value {0}", key);
                container.Values.Remove(key);
            }
        }

        private bool PromptForDelete(Windows.Foundation.Collections.IPropertySet values, string containerPath)
        {
            return Stdio.Prompt(String.Format("Delete all {0} {1} in the container {2} (Yes/No)? ", values.Count, "value".Plural(values.Count), containerPath));
        }

        private bool PromptForDelete(string key)
        {
            return Stdio.Prompt(String.Format("Delete the value {0} (Yes/No)? ", key));
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
