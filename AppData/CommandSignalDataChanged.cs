namespace AppData
{
    class CommandSignalDataChanged : Command
    {
#pragma warning disable 414
        static string Usage =
@"USAGE: AppData SignalDataChanged <packagefamilyname> [options] - Fire the DataChanged event
options:
{0}
EXAMPLES:
  appdata signaldatachanged contosso.games.solitaire_1234567890abc
";
#pragma warning restore

        public CommandSignalDataChanged(string[] options)
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

            PrintLineVerbose("execute");
            appdata.SignalDataChanged();
        }
    }
}
