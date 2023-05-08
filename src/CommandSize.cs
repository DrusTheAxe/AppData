// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.UI.StartScreen;

namespace AppData
{
    class CommandSize : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData Size <packagefamilyname> [options] - Retrieves data sizes
options:
  --all              = All application data stores [default]
  --local            = Local application data stores (files and settings)
  --local:files      = Local application data files
  --local:settings   = Local application data settings
  --localcache:files = LocalCache application data files
  --roaming          = Roaming application data store (files and settings)
  --roaming:files    = Roaming application data files
  --roaming:settings = Roaming application data settings
  --temporary        = Temporary application data store
  -b                 = Display sizes in Bytes [default]
  -k                 = Display sizes in KB
  -m                 = Display sizes in MB
  -g                 = Display sizes in GB
  --format:list      = Display results in list format
  --format:table     = Display results in table format [default]
Any combination is supported, e.g. ""--local:settings --roaming:settings"" to export local and roaming settings. Append ""-"": to disable the option, e.g. ""--all --temporary-"" to export all except temporary data.
{0}
EXAMPLES:
  appdata size contosso.games.solitaire_1234567890abc --local:files
    Display the size of the local folder

appdata size contosso.games.solitaire_1234567890abc --all --local:settings- --roaming:settings-
    Display the sizes to the local, roaming and temporary folders
";
#pragma warning restore

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
            All = Local | Roaming | Temporary | LocalCacheFiles,
            None = 0
        };
        Locality locality = Locality.None;

        enum SizeFormat
        {
            Bytes, KB, MB, GB
        };
        SizeFormat sizeFormat = SizeFormat.Bytes;

        private string FormatBytes(ulong bytes)
        {
            if (this.sizeFormat == SizeFormat.Bytes)
            {
                return $"{bytes:N0}";
            }

            ulong n = 0;
            uint nn = 0;
            string suffix = "";
            if (this.sizeFormat == SizeFormat.KB)
            {
                n = bytes / 1024;
                double fraction = (double)(bytes % 1024) / 1024;
                nn = (uint)(fraction * 10);
                if (n == 0 && nn == 0)
                    nn = 1;
                suffix = "KB";
            }
            else if (this.sizeFormat == SizeFormat.MB)
            {
                n = bytes / (1024 * 1024);
                double fraction = (double)(bytes % (1024 * 1024)) / (1024 * 1024);
                nn = (uint)(fraction * 10);
                if (n == 0 && nn == 0)
                    nn = 1;
                suffix = "MB";
            }
            else if (this.sizeFormat == SizeFormat.GB)
            {
                n = bytes / (1024 * 1024 * 1024);
                double fraction = (double)(bytes % (1024 * 1024 * 1024)) / (1024 * 1024 * 1024);
                nn = (uint)(fraction * 10);
                if (n == 0 && nn == 0)
                    nn = 1;
                suffix = "GB";
            }

            return $"{n:N0}.{nn}{suffix}";
        }

        enum OutputFormat
        {
            List, Table
        };
        OutputFormat outputFormat = OutputFormat.List;

        void OutputFilesHeader()
        {
            if (this.outputFormat == OutputFormat.Table)
            {
                PrintLine($"Type    Locality   Dirs       Files      File Size  Disk Space\n" +
                           "------- ---------- ---------- ---------- ---------- ------------");
            }

        }

        void OutputFiles(string title, ulong dirCount, ulong fileCount, ulong fileSize, ulong diskSize)
        {
            if (this.outputFormat == OutputFormat.List)
            {
                PrintLine($"{title,10}: Dirs:{dirCount:N0} Files:{fileCount:N0} Size:{FormatBytes(fileSize)} DiskSpace:{FormatBytes(diskSize)}");
            }
            else if (this.outputFormat == OutputFormat.Table)
            {
                PrintLine($"Folder  {title,-10} {dirCount,10:N0} {fileCount,10:N0} {FormatBytes(fileSize),10} {FormatBytes(diskSize),12}");
            }
        }

        void OutputSettings(string title, ulong containerCount, ulong valueCount)
        {
            if (this.outputFormat == OutputFormat.List)
            {
                PrintLine($"{title,10}: Containers:{containerCount:N0} Values:{valueCount:N0}");
            }
            else if (this.outputFormat == OutputFormat.Table)
            {
                PrintLine($"Setting {title,-10} {containerCount,10:N0} {valueCount,10:N0}");
            }
        }

        public CommandSize(string[] options)
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
            if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
            {
                this.sizeFormat = SizeFormat.Bytes;
            }
            else if (arg.Equals("-k", StringComparison.InvariantCultureIgnoreCase))
            {
                this.sizeFormat = SizeFormat.KB;
            }
            else if (arg.Equals("-m", StringComparison.InvariantCultureIgnoreCase))
            {
                this.sizeFormat = SizeFormat.MB;
            }
            else if (arg.Equals("-g", StringComparison.InvariantCultureIgnoreCase))
            {
                this.sizeFormat = SizeFormat.GB;
            }
            else if (arg.Equals("--format:list", StringComparison.InvariantCultureIgnoreCase))
            {
                this.outputFormat = OutputFormat.List;
            }
            else if (arg.Equals("--format:table", StringComparison.InvariantCultureIgnoreCase))
            {
                this.outputFormat = OutputFormat.Table;
            }
            else if (!TryParseLocality(arg))
            {
                base.Parse(arg);
            }
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
                    this.locality |= LocalityValues[i];
                    return true;
                }
                else if (arg.Equals(LocalityArgs[i] + "-", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.locality &= ~LocalityValues[i];
                    return true;
                }
            }
            return false;
        }

        protected override int GetRequiredArgumentsCount() { return 1; }

        public override void Execute()
        {
            if (this.locality == Locality.None)
                this.locality = Locality.All;

            PrintLineVerbose("open");
            OpenApplicationData();

            PrintLineVerbose("execute");
            OutputFilesHeader();
            if ((this.locality & Locality.LocalFiles) == Locality.LocalFiles)
                FileUsage(appdata.LocalFolder.Path, "Local");
            if ((this.locality & Locality.LocalCacheFiles) == Locality.LocalCacheFiles)
                FileUsage(appdata.LocalCacheFolder.Path, "LocalCache");
            if ((this.locality & Locality.RoamingFiles) == Locality.RoamingFiles)
                FileUsage(appdata.RoamingFolder.Path, "Roaming");
            if ((this.locality & Locality.Temporary) == Locality.Temporary)
                FileUsage(appdata.TemporaryFolder.Path, "Temporary");

            if ((this.locality & Locality.LocalSettings) == Locality.LocalSettings)
                SettingUsage(appdata.LocalSettings, "Local");
            if ((this.locality & Locality.RoamingSettings) == Locality.RoamingSettings)
                SettingUsage(appdata.RoamingSettings, "Roaming");
        }

        private void FileUsage(string path, string title)
        {
            ulong dirCount = 0;
            ulong fileCount = 0;
            ulong fileSize = 0;
            ulong diskSize = 0;

            if (!Directory.Exists(path))
            {
                PrintLine($"WARNING: {path} is missing!");
                return;
            }

            var rootPath = Path.GetPathRoot(path);
            DISK_SPACE_INFORMATION diskSpaceInfo;
            int hr = GetDiskSpaceInformationW(rootPath, out diskSpaceInfo);
            if (hr < 0)
            {
                PrintLine($"ERROR: GetDiskSpaceInformationW({rootPath}) = 0x{hr:08X}");
                Environment.Exit(1);
            }
            ulong blockSize = (ulong)diskSpaceInfo.SectorsPerAllocationUnit * (ulong)diskSpaceInfo.BytesPerSector;

            var q = new Queue<string>();
            var dir = path;
            do
            {
                dirCount++;

                foreach (var d in Directory.GetDirectories(dir))
                    q.Enqueue(d);

                foreach (var fi in new DirectoryInfo(dir).GetFiles())
                {
                    fileCount++;
                    fileSize += (ulong)fi.Length;
                }
            } while (q.TryDequeue(out dir));

            OutputFiles(title, dirCount - 1, fileCount, fileSize, diskSize);
        }

        struct DISK_SPACE_INFORMATION
        {
            public ulong ActualTotalAllocationUnits;
            public ulong ActualAvailableAllocationUnits;
            public ulong ActualPoolUnavailableAllocationUnits;
            public ulong CallerTotalAllocationUnits;
            public ulong CallerAvailableAllocationUnits;
            public ulong CallerPoolUnavailableAllocationUnits;
            public ulong UsedAllocationUnits;
            public ulong TotalReservedAllocationUnits;
            public ulong VolumeStorageReserveAllocationUnits;
            public ulong AvailableCommittedAllocationUnits;
            public ulong PoolAvailableAllocationUnits;
            public uint SectorsPerAllocationUnit;
            public uint BytesPerSector;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetDiskSpaceInformationW(string rootPath, out DISK_SPACE_INFORMATION diskSpaceInfo);

        private void SettingUsage(ApplicationDataContainer settings, string title)
        {
            ulong containerCount = 0;
            ulong valueCount = 0;

            var q = new Queue<ApplicationDataContainer>();
            var container = settings;
            do
            {
                containerCount++;

                foreach (var c in container.Containers)
                    q.Enqueue(c.Value);

                foreach (var ps in container.Values)
                {
                    valueCount++;
                }
            } while (q.TryDequeue(out container));

            OutputSettings(title, containerCount - 1, valueCount);
        }
    }
}
