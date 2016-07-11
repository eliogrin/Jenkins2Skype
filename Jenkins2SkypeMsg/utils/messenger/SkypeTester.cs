using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenkins2SkypeMsg.utils.messenger
{
    class SkypeTester
    {
        static SkypeConnector skype;

        public static void start()
        {
            Console.WriteLine();
            Console.WriteLine("There are 2 posobility of using Skype chat:");
            Console.WriteLine("1 - Personal messages, in this case you just put your Skype login in config file.");
            Console.WriteLine("2 - Group messages, in this case you need P2P group chat with blob id.");
            Console.WriteLine("Here you can create or test Skype group chat.");
            Console.WriteLine();
            Console.WriteLine("You can do it manualy in 2 steps:");
            Console.WriteLine("* '/createmoderatedchat' - command to create new P2P chat");
            Console.WriteLine("* '/get blob' - command to get blob id of created group chat");

            bool exit = false;
            bool showMenu = true;
            skype = new SkypeConnector();
            do
            {
                if (showMenu)
                {
                    Console.WriteLine();
                    Console.WriteLine("Or you can use next menu:");
                    Console.WriteLine("1 - to create new P2P chat;");
                    Console.WriteLine("2 - to test existed blob id;");
                    Console.WriteLine("0 - if you want back to main menu.");
                }
                Console.Write("=>");
                ConsoleKeyInfo menuItem = Console.ReadKey();
                Console.WriteLine("");

                if (menuItem.Key == ConsoleKey.D1)
                {
                    String blob = createGroupChat();
                    useGroupChatByBlob(blob);
                    showMenu = true;
                }
                else if (menuItem.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("Enter blob id for test group chat:");
                    String blob = Console.ReadLine();
                    useGroupChatByBlob(blob);
                    showMenu = true;
                }
                else if (menuItem.Key == ConsoleKey.D0)
                {
                    exit = true;
                }
                else
                {
                    showMenu = false;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
            }
            while (!exit);
        }

        private static String createGroupChat()
        {
            Console.WriteLine("To create group chat Skype required users from contact list.");
            Console.WriteLine("Enter user Skype login of user (it will be a group admin):");
            String groupAdmin = Console.ReadLine();

            String blob = skype.createGroup(groupAdmin);

            skype.sendMessage(blob, "/setrole " + groupAdmin + " MASTER");
            Console.WriteLine("New chat blob id is '" + blob + "'");

            return blob;
        }

        private static void useGroupChatByBlob(String blob)
        {
            String text = "";
            do
            {
                Console.WriteLine();
                Console.WriteLine("Enter message to send (leave empty to exit):");
                text = Console.ReadLine();
                if (!String.IsNullOrEmpty(text))
                {
                    skype.sendMessage(blob, text);
                }
            }
            while (!String.IsNullOrEmpty(text));
        }
    }
}
