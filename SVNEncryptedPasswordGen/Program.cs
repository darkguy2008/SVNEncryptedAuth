using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVNEncryptedPasswordGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.Write("Type a password: ");

            String pass = String.Empty;
            ConsoleKeyInfo key;

            // http://stackoverflow.com/questions/3404421/password-masking-console-application
            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); Console.WriteLine();
            Console.WriteLine("key:" + SVNEncryptedAuthHelper.StringCipher.Encrypt(pass, "p4$$w0rd"));
        }
    }
}
