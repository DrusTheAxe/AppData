// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TrollCaveEnterprises;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace AppData
{
    class CommandExport : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Export <packagefamilyname> <target> [options] - Export application data
where:
  target             = Path to the exported data.
                         This is a directory, if --target:format=FILESYSTEM
                         This is a filename, if --target:format=ZIP
options:
  --target:format=FMT= Export format. Valid formats are: FILESYSTEM, ZIP
                         [Default=ZIP]
  --settings:format=FMT = Export format for settings. Valid formats are:
                            XML, JSON [Default=XML]
  --overwrite:prompt= Prompt to confirm overwriting an existing file [Default]
  --overwrite:yes   = Overwrite an existing file
  --overwrite:no    = Never overwrite an existing file
  --order:none      = Emit settings in the order as retrieved [Default]
  --order:sorted    = Emit settings in sorted order

Locality options:
  --all             = All application data stores [Default]
  --local           = Local application data stores (files and settings)
  --local:files     = Local application data files
  --local:settings  = Local application data settings
  --localcache:files= LocalCache application data files
  --roaming         = Roaming application data store (files and settings)
  --roaming:files   = Roaming application data files
  --roaming:settings= Roaming application data settings
  --temporary       = Temporary application data store
Any combination is supported, e.g. ""--local:settings --roaming:settings"" to export local and roaming settings. Append ""-"": to disable the option, e.g. ""--all --temporary-"" to export all except temporary data.

ZIP options (if --target:format=ZIP):
  -0, --compress:none  = Use no compression
  -1, --compress:fast  = Use fastest compression
  -5, --compress:normal= Use normal compression [Default]
  -9, --compress:small = Use optimal compression
  --direntries         = Add directory entries [Default]
  --nodirentries       = Do not add directory entries
{0}
EXAMPLES:
  appdata export contosso.games.solitaire_1234567890abc d:\temp\solitaire.zip
    Export all application data to d:\temp\solitaire.zip

  appdata export contosso.games.solitaire_1234567890abc d:\temp\solitaire.zip --all- --local
    Export all local application data to d:\temp\solitaire.zip

  appdata export contosso.games.solitaire_1234567890abc d:\temp\solitaire.zip --all- --local:settings --roaming:settings
    Export all settings to d:\temp\solitaire.zip

  appdata export contosso.games.solitaire_1234567890abc d:\temp\solitaire.zip --compress:small
    Export local application data to d:\temp\solitaire.zip with maximum compression
";
#pragma warning restore

        string target = "";

        enum TargetFormat
        {
            FileSystem,
            Zip
        }
        TargetFormat targetFormat = TargetFormat.Zip;

        SettingsFormat settingsFormat = SettingsFormat.XML;

        Overwrite overwrite = Overwrite.Prompt;

        [Flags]
        enum Locality
        {
            LocalFiles = 0x0001,
            LocalSettings = 0x0002,
            Local = LocalFiles | LocalSettings,
            RoamingFiles = 0x0010,
            RoamingSettings = 0x0020,
            Roaming = RoamingFiles | RoamingSettings,
            Temporary = 0x0100,
            LocalCacheFiles = 0x1000,
            LocalCache = LocalCacheFiles,
            All = Local | Roaming | Temporary | LocalCacheFiles
        };
        int locality = (int)Locality.All;

        enum Order
        {
            None, Sorted
        }
        Order order = Order.None;

        CompressionLevel compressionLevel = CompressionLevel.Optimal;
        bool zipDirectoryEntries = true;

        public CommandExport(string[] options)
        {
            Parse(options);
            System.Diagnostics.Debug.Assert(this.requiredArguments.Length == GetRequiredArgumentsCount());
            this.packageFamilyName = this.requiredArguments[0];
            this.target = this.requiredArguments[1];
        }

        void DisplayHelp()
        {
            DisplayHelp(this);
        }

        public override void Parse(string arg)
        {
            if (arg.Equals("--target:format=filesystem", StringComparison.InvariantCultureIgnoreCase))
            {
                this.targetFormat = TargetFormat.FileSystem;
            }
            else if (arg.Equals("--target:format=zip", StringComparison.InvariantCultureIgnoreCase))
            {
                this.targetFormat = TargetFormat.Zip;
            }
            else if (arg.Equals("--settings:format=json", StringComparison.InvariantCultureIgnoreCase))
            {
                this.settingsFormat = SettingsFormat.JSON;
            }
            else if (arg.Equals("--settings:format=xml", StringComparison.InvariantCultureIgnoreCase))
            {
                this.settingsFormat = SettingsFormat.XML;
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
            else if (arg.Equals("--order:none", StringComparison.InvariantCultureIgnoreCase))
            {
                this.order = Order.None;
            }
            else if (arg.Equals("--order:sorted", StringComparison.InvariantCultureIgnoreCase))
            {
                this.order = Order.Sorted;
            }
            else if (arg.Equals("-0", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--compress:none", StringComparison.InvariantCultureIgnoreCase))
            {
                this.compressionLevel = CompressionLevel.NoCompression;
            }
            else if (arg.Equals("-1", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--compress:fast", StringComparison.InvariantCultureIgnoreCase))
            {
                this.compressionLevel = CompressionLevel.Fastest;
            }
            else if (arg.Equals("-5", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--compress:normal", StringComparison.InvariantCultureIgnoreCase))
            {
                this.compressionLevel = CompressionLevel.Optimal;
            }
            else if (arg.Equals("-9", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--compress:small", StringComparison.InvariantCultureIgnoreCase))
            {
                this.compressionLevel = CompressionLevel.SmallestSize;
            }
            else if (arg.Equals("--direntries", StringComparison.InvariantCultureIgnoreCase))
            {
                this.zipDirectoryEntries = true;
            }
            else if (arg.Equals("--nodirentries", StringComparison.InvariantCultureIgnoreCase))
            {
                this.zipDirectoryEntries = false;
            }
            else if (!TryParseLocality(arg))
                base.Parse(arg);
        }

        private static readonly string[] LocalityArgs = new string[] { "--all",
            "--local", "--local:files", "--local:settings",
            "--roaming", "--roaming:files", "--roaming:settings",
            "--temporary",
            "--localcache:files" };
        private static readonly Locality[] LocalityValues = new Locality[]{ Locality.All,
            Locality.Local, Locality.LocalFiles, Locality.LocalSettings,
            Locality.Roaming, Locality.RoamingFiles, Locality.RoamingSettings,
            Locality.Temporary,
            Locality.LocalCacheFiles };

        private bool TryParseLocality(string arg)
        {
            for (int i = 0; i < LocalityArgs.Length; ++i)
            {
                if (arg.Equals(LocalityArgs[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    this.locality |= (int)LocalityValues[i];
                    return true;
                }
                else if (arg.Equals(LocalityArgs[i] + "-", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.locality &= (int)~LocalityValues[i];
                    return true;
                }
            }
            return false;
        }

        protected override int GetRequiredArgumentsCount() { return 2; }

        public override void Execute()
        {
            PrintLineVerbose("open");
            OpenApplicationData();

            PrintLineVerbose("execute");
            System.Diagnostics.Debug.Assert(this.targetFormat == TargetFormat.FileSystem || this.targetFormat == TargetFormat.Zip);
            if (this.targetFormat == TargetFormat.FileSystem)
                ExportToFileSystem();
            else if (this.targetFormat == TargetFormat.Zip)
                ExportToZipFile();
        }

        public void ExportToFileSystem()
        {
            var targetRoot = Path.IsPathRooted(this.target) ? this.target : Path.Combine(Directory.GetCurrentDirectory(), this.target);
            if (Directory.Exists(targetRoot))
            {
                var dirs = Directory.GetDirectories(targetRoot);
                var files = Directory.GetFiles(targetRoot);
                if (dirs.Length > 0 || files.Length > 0)
                {
                    if (this.overwrite == Overwrite.No)
                        FatalError("The operation was canceled (--overwrite:no).");
                    else if (this.overwrite == Overwrite.Prompt)
                    {
                        bool ok = PromptForOverwrite(targetRoot);
                        if (!ok)
                            FatalError("The operation was canceled by the user.");
                    }

                    PrintLineVerboseFormat("Clearing {0}", targetRoot);
                    try
                    {
                        foreach (var dir in dirs)
                            FileSystem.DeleteDirectory(dir, true);
                        foreach (var file in files)
                            FileSystem.DeleteFile(file);
                    }
                    catch (Exception ex)
                    {
                        FatalError(ex);
                    }
                }
            }
            else
            {
                PrintLineVerboseFormat("Creating {0}", targetRoot);
                Directory.CreateDirectory(targetRoot);
            }

            try
            {
                PrintLineVerboseFormat("Creating {0}", this.target);
                if ((this.locality & (int)Locality.LocalFiles) != 0)
                    FileSystemAddFolder(targetRoot, Locality.LocalFiles, appdata.LocalFolder.Path);
                if ((this.locality & (int)Locality.LocalCacheFiles) != 0)
                    FileSystemAddFolder(targetRoot, Locality.LocalCacheFiles, appdata.LocalCacheFolder.Path);
                if ((this.locality & (int)Locality.RoamingFiles) != 0)
                    FileSystemAddFolder(targetRoot, Locality.RoamingFiles, appdata.RoamingFolder.Path);
                if ((this.locality & (int)Locality.Temporary) != 0)
                    FileSystemAddFolder(targetRoot, Locality.Temporary, appdata.TemporaryFolder.Path);

                if ((this.locality & (int)Locality.LocalSettings) != 0)
                    FileSystemAddSettings(targetRoot, Locality.LocalSettings, appdata.LocalSettings);
                if ((this.locality & (int)Locality.RoamingSettings) != 0)
                    FileSystemAddSettings(targetRoot, Locality.RoamingSettings, appdata.RoamingSettings);
            }
            catch (Exception ex)
            {
                FatalError(ex);
            }
        }

        public void ExportToZipFile()
        {
            if (File.Exists(this.target))
            {
                if (this.overwrite == Overwrite.No)
                    FatalError("The operation was canceled (--overwrite:no).");
                else if (this.overwrite == Overwrite.Prompt)
                {
                    bool ok = PromptForOverwrite(this.target);
                    if (!ok)
                        FatalError("The operation was canceled by the user.");
                }
            }

            try
            {
                PrintLineVerboseFormat("Creating {0}", this.target);
                using (FileStream zipFileStream = new FileStream(this.target, FileMode.Create, FileAccess.Write, FileShare.Write | FileShare.Delete))
                {
                    using (ZipArchive zip = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                    {
                        if ((this.locality & (int)Locality.LocalFiles) != 0)
                            ZipAddFolder(zip, Locality.LocalFiles, appdata.LocalFolder.Path);
                        if ((this.locality & (int)Locality.LocalCacheFiles) != 0)
                            ZipAddFolder(zip, Locality.LocalCacheFiles, appdata.LocalCacheFolder.Path);
                        if ((this.locality & (int)Locality.RoamingFiles) != 0)
                            ZipAddFolder(zip, Locality.RoamingFiles, appdata.RoamingFolder.Path);
                        if ((this.locality & (int)Locality.Temporary) != 0)
                            ZipAddFolder(zip, Locality.Temporary, appdata.TemporaryFolder.Path);

                        if ((this.locality & (int)Locality.LocalSettings) != 0)
                            ZipAddSettings(zip, Locality.LocalSettings, appdata.LocalSettings);
                        if ((this.locality & (int)Locality.RoamingSettings) != 0)
                            ZipAddSettings(zip, Locality.RoamingSettings, appdata.RoamingSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                FatalError(ex);
            }
        }

        private uint FileSystemAddFolder(string path, Locality locality, string source)
        {
            PrintLineFormat("Scanning {0}", locality.ToString());

            string parent = Path.GetDirectoryName(source);
            string saveCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(parent);

            uint count = 0;
            string sourceRoot = Path.GetFileName(source);
            foreach (var fse in Directory.EnumerateFileSystemEntries(sourceRoot, "*", SearchOption.AllDirectories))
            {
                string targetFilename = Path.Combine(path, fse);
                ShowProgressAddingFileSystemEntry(fse);
                if ((File.GetAttributes(fse) & System.IO.FileAttributes.Directory) != 0)
                    Directory.CreateDirectory(targetFilename);
                else
                    File.Copy(fse, targetFilename);
                ++count;
            }

            Directory.SetCurrentDirectory(saveCurrentDirectory);

            if (this.displayLevel < DisplayLevel.Verbose && count > 0)
                PrintLine();

            return count;
        }

        private uint ZipAddFolder(ZipArchive zip, Locality locality, string source)
        {
            PrintLineFormat("Scanning {0}", locality.ToString());

            string parent = Path.GetDirectoryName(source);
            string saveCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(parent);

            uint count = 0;
            string sourceRoot = Path.GetFileName(source);
            foreach (var fse in Directory.EnumerateFileSystemEntries(sourceRoot, "*", SearchOption.AllDirectories))
            {
                string f = fse.Replace('\\', '/');
                if ((File.GetAttributes(f) & System.IO.FileAttributes.Directory) != 0)
                {
                    if (this.zipDirectoryEntries)
                    {
                        ShowProgressAddingFileSystemEntry(fse);
                        zip.CreateEntry(f + @"/");
                        ++count;
                    }
                }
                else
                {
                    ShowProgressAddingFileSystemEntry(fse);
                    zip.CreateEntryFromFile(fse, f, this.compressionLevel);
                    ++count;
                }
            }

            Directory.SetCurrentDirectory(saveCurrentDirectory);

            if (this.displayLevel < DisplayLevel.Verbose && count > 0)
                PrintLine();

            return count;
        }

        private void ShowProgressAddingFileSystemEntry(string fse)
        {
            if (this.displayLevel >= DisplayLevel.Verbose)
                PrintLineVerboseFormat("...Adding {0}", fse);
            else
                Print(".");
        }

        private uint FileSystemAddSettings(string path, Locality locality, ApplicationDataContainer root)
        {
            PrintLineFormat("Scanning {0}", locality.ToString());

            string filename = Path.Combine(path, locality.ToString() + "." + this.settingsFormat.ToString().ToLowerInvariant());
            var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write | FileShare.Delete);

            uint countValues = 0;
            uint countContainers = 0;
            using (SettingsWriter writer = CreateSettingsWriter(fileStream))
            {
                WriteSettings(writer, locality, root);
            }

            return countValues;
        }

        private uint ZipAddSettings(ZipArchive zip, Locality locality, ApplicationDataContainer root)
        {
            PrintLineFormat("Scanning {0}", locality.ToString());

            var entry = zip.CreateEntry(locality.ToString() + "." + this.settingsFormat.ToString().ToLowerInvariant(), this.compressionLevel);

            uint countValues = 0;
            uint countContainers = 0;
            using (SettingsWriter writer = CreateSettingsWriter(entry.Open()))
            {
                WriteSettings(writer, locality, root);
            }

            return countValues;
        }

        private void WriteSettings(SettingsWriter writer, Locality locality, ApplicationDataContainer root)
        {
            writer.Start();
            writer.StartContainers(LocalityToApplicationDataLocality(locality));
            WalkSettingsContainer(writer, root, true);
            writer.EndContainers();
            writer.End();
        }

        private SettingsWriter CreateSettingsWriter(Stream stream)
        {
            switch (this.settingsFormat)
            {
                case SettingsFormat.XML: return new SettingsWriterXML(stream);
                case SettingsFormat.JSON: return new SettingsWriterJSON(stream);
                default: System.Diagnostics.Debug.Fail("This can never happen: Unknown order"); return null;
            }
        }

        private uint WalkSettingsContainer(SettingsWriter writer, ApplicationDataContainer container, bool isLastAtThisNestingLevel)
        {
            if (container.Values.Count == 0 && container.Containers.Count == 0)
                return 0;

            writer.StartContainer(container);
            WalkSettingsValues(writer, container.Values, container.Containers.Count == 0);
            uint count = 0;
            var containers = container.Containers;
            var keys = SortKeys(containers.Keys).ToList();
            foreach (var key in keys)
            {
                ++count;
                var child = containers[key];
                WalkSettingsContainer(writer, child, count == keys.Count);
            }
            writer.EndContainer(isLastAtThisNestingLevel);

            if (this.displayLevel < DisplayLevel.Verbose && count > 0)
                PrintLine();

            return count;
        }

        private IEnumerable<T> SortKeys<T>(IEnumerable<T> keys)
        {
            System.Diagnostics.Debug.Assert(keys != null);

            switch (this.order)
            {
                case Order.None: return keys;
                case Order.Sorted: return keys.OrderBy(item => item);
                default: System.Diagnostics.Debug.Fail("This can never happen: Unknown order"); return null;
            }
        }

        private ICollection<string> SortKeys(ICollection<string> keys)
        {
            System.Diagnostics.Debug.Assert(keys != null);

            switch (this.order)
            {
                case Order.None: return keys;
                case Order.Sorted: var sortedKeys = new List<string>(keys); sortedKeys.Sort(); return sortedKeys;
                default: System.Diagnostics.Debug.Fail("This can never happen: Unknown order"); return null;
            }
        }

        private uint WalkSettingsValues(SettingsWriter writer, IPropertySet values, bool isLastAtThisNestingLevel)
        {
            if (values.Count == 0)
                return 0;

            writer.StartValues();
            uint count = 0;
            var keys = SortKeys(values.Keys);
            foreach (var key in keys)
            {
                ++count;
                var value = values[key];
                ShowProgressAddingSettingsValue(key);
                var type = value.GetAppDataType();
                if (type == AppDataType.Type.ApplicationDataCompositeValue)
                {
                    WalkSettingsComposite(writer, key, (ApplicationDataCompositeValue)value, count == keys.Count);
                }
                else
                    writer.WriteValue(key, value, count == keys.Count);
            }
            writer.EndValues(isLastAtThisNestingLevel);

            if (this.displayLevel < DisplayLevel.Verbose && count > 0)
                PrintLine();

            return count;
        }

        private uint WalkSettingsComposite(SettingsWriter writer, string name, ApplicationDataCompositeValue composite, bool isLastAtThisNestingLevel)
        {
            if (composite.Count == 0)
                return 0;

            writer.StartComposite(name);
            uint count = 0;
            var keys = SortKeys(composite.Keys);
            var values = composite.Values;
            foreach (var key in composite.Keys)
            {
                ++count;
                var value = composite[key];

                writer.WriteValue(key, value, count == keys.Count);
            }
            writer.EndComposite(isLastAtThisNestingLevel);

            System.Diagnostics.Debug.Assert(composite.Count >= 0);
            return (uint)composite.Count;
        }

        private void ShowProgressAddingSettingsValue(string key)
        {
            if (this.displayLevel >= DisplayLevel.Verbose)
                PrintLineVerboseFormat("...Adding value {0}", key);
            else
                Print(".");
        }

        private bool PromptForOverwrite(string target)
        {
            return Stdio.Prompt(String.Format("Export target {0} is not empty, overwrite (Yes/No)? ", target));
        }

        private ApplicationDataLocality LocalityToApplicationDataLocality(Locality locality)
        {
            switch (locality)
            {
                case Locality.Local:
                case Locality.LocalFiles:
                case Locality.LocalSettings:
                    return ApplicationDataLocality.Local;
                case Locality.Roaming:
                case Locality.RoamingFiles:
                case Locality.RoamingSettings:
                    return ApplicationDataLocality.Roaming;
                case Locality.Temporary:
                    return ApplicationDataLocality.Temporary;
                case Locality.LocalCacheFiles:
                    return ApplicationDataLocality.LocalCache;
                default:
                    System.Diagnostics.Debug.Fail("This can never happen: Locality cannot be converted");
                    throw new Exception("This can never happen: Locality cannot be converted");
            }
        }
    }
}
