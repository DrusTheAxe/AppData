﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData
{
    class Stdio
    {
        public static bool Prompt(string message)
        {
            do
            {
                Console.Write(message);
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
    }
}
