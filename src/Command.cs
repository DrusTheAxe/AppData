// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Text;

namespace AppData
{
    abstract class Command
    {
        public enum DisplayLevel { Quiet = -1, Brief = 0, Verbose = 1 };
        public DisplayLevel displayLevel { get; set; }

        public string packageFamilyName { get; set; }

        public string[] requiredArguments;

        protected Windows.Storage.ApplicationData appdata;

        public string externalLocation { get; set; }

        public void Print(DisplayLevel level, string s)
        {
            if (level <= this.displayLevel)
                Console.Write(s);
        }
        public void PrintFormat(DisplayLevel level, string format, object arg0) { Print(level, String.Format(format, arg0)); }
        public void PrintFormat(DisplayLevel level, string format, object arg0, object arg1) { Print(level, String.Format(format, arg0, arg1)); }
        public void PrintFormat(DisplayLevel level, string format, object arg0, object arg1, object arg2) { Print(level, String.Format(format, arg0, arg1, arg2)); }
        public void PrintFormat(DisplayLevel level, string format, params object[] args) { Print(level, String.Format(format, args)); }

        public void PrintLine(DisplayLevel level)
        {
            if (level <= this.displayLevel)
                Console.WriteLine();
        }
        public void PrintLine(DisplayLevel level, string s)
        {
            if (level <= this.displayLevel)
                Console.WriteLine(s);
        }
        public void PrintLineFormat(DisplayLevel level, string format, object arg0) { PrintLine(level, String.Format(format, arg0)); }
        public void PrintLineFormat(DisplayLevel level, string format, object arg0, object arg1) { PrintLine(level, String.Format(format, arg0, arg1)); }
        public void PrintLineFormat(DisplayLevel level, string format, object arg0, object arg1, object arg2) { PrintLine(level, String.Format(format, arg0, arg1, arg2)); }
        public void PrintLineFormat(DisplayLevel level, string format, params object[] args) { PrintLine(level, String.Format(format, args)); }

        public void PrintVerbose(string s) { Print(DisplayLevel.Verbose, s); }

        public void PrintLineVerbose() { PrintLine(DisplayLevel.Verbose); }
        public void PrintLineVerbose(string s) { PrintLine(DisplayLevel.Verbose, s); }
        public void PrintLineVerboseFormat(string format, object arg0) { PrintLineFormat(DisplayLevel.Verbose, format, arg0); }

        public void Print(string s) { Print(DisplayLevel.Brief, s); }
        public void PrintFormat(string format, object arg0, object arg1) { Print(DisplayLevel.Brief, String.Format(format, arg0, arg1)); }
        public void PrintFormat(string format, object arg0, object arg1, object arg2) { Print(DisplayLevel.Brief, String.Format(format, arg0, arg1, arg2)); }
        public void PrintFormat(string format, params object[] args) { Print(DisplayLevel.Brief, String.Format(format, args)); }

        public void PrintLine() { PrintLine(DisplayLevel.Brief); }
        public void PrintLine(string s) { PrintLine(DisplayLevel.Brief, s); }
        public void PrintLineFormat(string format, object arg0) { PrintLineFormat(DisplayLevel.Brief, format, arg0); }
        public void PrintLineFormat(string format, object arg0, object arg1) { PrintLineFormat(DisplayLevel.Brief, format, arg0, arg1); }
        public void PrintLineFormat(string format, object arg0, object arg1, object arg2) { PrintLineFormat(DisplayLevel.Brief, format, arg0, arg1, arg2); }

        public static Command CreateInstance(string[] args)
        {
            if (args.Length < 1)
            {
                DisplayHelp();
            }

            string commandName = args[0];
            string[] options = new string[args.Length > 1 ? args.Length - 1 : 0];
            if (args.Length > 1)
                Array.Copy(args, 1, options, 0, options.Length);

            Command command;
            if (commandName.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandClear(options);
            else if (commandName.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandDelete(options);
            else if (commandName.Equals("export", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandExport(options);
            else if (commandName.Equals("get", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandGet(options);
            else if (commandName.Equals("import", StringComparison.InvariantCultureIgnoreCase))
            {
                FatalError("Coming soon...");
                return null;
            }
            else if (commandName.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandPath(options);
            else if (commandName.Equals("quota", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandQuota(options);
            else if (commandName.Equals("set", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandSet(options);
            else if (commandName.Equals("signaldatachanged", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandSignalDataChanged(options);
            else if (commandName.Equals("size", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandSize(options);
            else if (commandName.Equals("version", StringComparison.InvariantCultureIgnoreCase))
                command = new CommandVersion(options);
            else
            {
                FatalError("Unknown command", args[0]);
                throw new Exception("This can never happen: FatalError() never returns");
            }
            return command;
        }

        protected static void FatalError(string message)
        {
            FatalError(message, null);
        }

        protected static void FatalError(string message, string data)
        {
            if (data == null)
                Console.WriteLine("ERROR: {0}", message);
            else
                Console.WriteLine("ERROR: {0} ({1})", message, data);
            Environment.Exit(1);
        }

        protected static void FatalError(Exception ex)
        {
            FatalError("EXCEPTION", ex.ToString());
        }

        private static void DisplayHeader()
        {
            var assembly = Assembly.GetEntryAssembly();
            var name = assembly.GetName();
            var description = ((AssemblyDescriptionAttribute)assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            var company = ((AssemblyCompanyAttribute)assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company;
            var copyright = ((AssemblyCopyrightAttribute)assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            Console.WriteLine("{0} v{1} - {2} - {3} {4}", name.Name, name.Version.ToString(), description, copyright, company);
        }

#pragma warning disable 414
        static string Usage =
@"USAGE: AppData <command> <packagefamilyname> [options...]
Commands:
  CLEAR | GET | SET | DELETE | IMPORT | EXPORT |
  PATH | QUOTA | SIGNALDATACHANGED | SIZE | VERSION

Return Code:
  0 = Successful
  1 = Failed

For help on a specific command:
  AppData <command> --help

Examples:
  AppData CLEAR --help
  AppData GET --help
  AppData SET --help
  AppData DELETE --help
  AppData IMPORT --help
  AppData EXPORT --help
  AppData PATH --help
  AppData QUOTA --help
  AppData SIGNALDATACHANGED --help
  AppData SIZE --help
  AppData VERSION --help
";
#pragma warning restore

        static string[] CommonOptions = new string[] { "-v, --verbose", "Verbose", "--brief", "Brief", "--quiet", "Quiet", "-?, --help", "Display help information" };

        static void DisplayHelp()
        {
            DisplayHelp(null);
        }

        public static void DisplayHelp(Command command)
        {
            var type = command == null ? typeof(Command) : command.GetType();
            var field = type.GetField("Usage", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            string usage = (string)field.GetValue(null);

            if (command != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < CommonOptions.Length; i += 2)
                {
                    sb.AppendFormat("  {0,-17} = {1}\n", CommonOptions[i], CommonOptions[i + 1]);
                }
                usage = String.Format(usage, sb.ToString());
            }

            DisplayHeader();
            Console.WriteLine(usage);
            Environment.Exit(1);
        }

        public virtual void Parse(string arg)
        {
            if (arg.Equals("-?") || arg.Equals("--help"))
                DisplayHelp();
            else if (arg.Equals("-v", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("--verbose", StringComparison.InvariantCultureIgnoreCase))
                this.displayLevel = DisplayLevel.Verbose;
            else if (arg.Equals("--brief", StringComparison.InvariantCultureIgnoreCase))
                this.displayLevel = DisplayLevel.Brief;
            else if (arg.Equals("--quiet", StringComparison.InvariantCultureIgnoreCase))
                this.displayLevel = DisplayLevel.Quiet;
            else
                FatalError(String.Format("Unknown parameter ({0}); use 'APPDATA {1} --help' for usage", arg, GetCommandName()));
        }

        public void Parse(string[] args)
        {
            if (args.Length > 0 && (args[0].Equals("-?") || args[0].Equals("--help")))
                DisplayHelp(this);

            int count = GetRequiredArgumentsCount();
            if (args.Length < count)
                FatalError(String.Format("Missing required parameter(s); use 'APPDATA {0} --help' for usage", GetCommandName()));
            for (int i = 1; i < count; ++i)
                if (args[i].Equals("-?") || args[i].Equals("--help"))
                    DisplayHelp(this);
            if (count > 0)
            {
                this.requiredArguments = new string[count];
                Array.Copy(args, this.requiredArguments, count);

                for (int i = count; i < args.Length; ++i)
                {
                    string arg = args[i];
                    Parse(arg);
                }
            }
        }

        protected virtual int GetRequiredArgumentsCount() { return 1; }

        public virtual string GetCommandName()
        {
            string s = this.GetType().Name;
            System.Diagnostics.Debug.Assert(s.StartsWith("Command"));
            s = s.Substring("Command".Length);
            if (s.Length == 0)
                return null;
            return s.ToUpper();
        }

        public abstract void Execute();

        protected void OpenApplicationData()
        {
            if (packageFamilyName.IsEmpty())
                DisplayHelp(this);
            try
            {
                appdata = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(packageFamilyName);
            }
            catch (System.IO.FileNotFoundException)
            {
                FatalError("Package Family not found", packageFamilyName);
            }
            catch (Exception ex)
            {
                FatalError(ex);
            }
        }
    }
}
