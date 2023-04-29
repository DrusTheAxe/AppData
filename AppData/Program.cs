namespace AppData
{

    class Program
    {
        static void Main(string[] args)
        {
            Command cmd = Command.CreateInstance(args);
            cmd.Execute();
        }
    }
}
