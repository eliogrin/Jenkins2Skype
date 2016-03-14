using Jenkins2SkypeMsg.core;
using Jenkins2SkypeMsg.utils;
using Jenkins2SkypeMsg.utils.configuration;
using Jenkins2SkypeMsg.utils.messenger;
using System;
using System.Threading;

namespace Jenkins2SkypeMsg
{
    class Program
    {
        static void Main(string[] args)
        {
            TraceInit logger = new TraceInit();
            
            String configPath = null;
            if (args.Length > 0)
            {
                configPath = args[0];
            }

            if (String.IsNullOrEmpty(configPath))
            {
                ConsoleColor defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Yo can specify config name as argument to launch this app.");
                Console.WriteLine("\nSelect options to start app:");
                Console.WriteLine("[1] - start monitoring;");
                Console.WriteLine("[2] - to create new skype group chat and get group blob;");
                Console.WriteLine("[3] - to create new config file;");
                Console.WriteLine("[0] - to exit");

                bool readOptions = false;
                do
                {
                    Console.Write("=>");
                    ConsoleKeyInfo key = Console.ReadKey();
                    Console.WriteLine("");

                    if (key.Key == ConsoleKey.D1)
                    {
                        Console.WriteLine("Enter config name or path:");
                        configPath = Console.ReadLine();
                        
                        if (JobsConfigReader.checkConfigFile(configPath))
                        {
                            readOptions = true;
                        }
                    }
                    else if (key.Key == ConsoleKey.D2)
                    {
                        Console.WriteLine("Sorry, not Implemented yet");
                        Thread.Sleep(2000);
                        readOptions = true;
                    }
                    else if (key.Key == ConsoleKey.D3)
                    {
                        Console.WriteLine("Sorry, not Implemented yet");
                        Thread.Sleep(2000);
                        readOptions = true;
                    }
                    else if (key.Key == ConsoleKey.D0)
                    {
                        Console.WriteLine("Ok, bye-bye");
                        Thread.Sleep(2000);
                        readOptions = true;
                    }
                    else
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                }
                while (!readOptions);

                Console.ForegroundColor = defaultColor;
            }

            if (!String.IsNullOrEmpty(configPath))
            {
                bool skypeOk = false;
                
                Console.WriteLine("Checking Skype availability");
                try
                {
                    Messenger.Instance.getStatus();
                    skypeOk = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can't connect to Skype app, error happend: " + ex.Message);
                    Console.WriteLine("Closing app...");
                    Thread.Sleep(1000);
                }

                if (skypeOk)
                {
                    while (true)
                    {
                        if (TaskLauncher.isEnabled())
                        {
                            Thread.Sleep(10000);
                        }
                        else
                        {
                            Config.readConfig(configPath);
                            TaskLauncher.run();
                        }
                    }
                }
            }
        }
    }
}
