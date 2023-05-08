// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
