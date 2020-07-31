using System;
using System.Collections.Generic;

namespace GenesysCloudChatClients
{
    class Program
    {

 

        static void Main(string[] args)
        {
            int option = 0;
            ChatsHandler chatsHandler = new ChatsHandler();
            Console.WriteLine("Welcome to the Genesys Cloud Chats Handler!");
            Console.WriteLine();
            do
            {
                Console.WriteLine("Main Menu");
                Console.WriteLine("-");
                Console.WriteLine("Press between the following options: ");
                Console.WriteLine("1 - Create a new Chat");
                Console.WriteLine("2 - Send a message in a Chat Conversation");
                Console.WriteLine("3 - List Chat conversations");
                Console.WriteLine("4 - Exit");
                option = Convert.ToInt32(Console.ReadLine());

                switch (option) {
                    case 1:
                        chatsHandler.CreateChat();
                        break;
                    case 2:
                        chatsHandler.SendChatMessage();
                        Console.WriteLine();
                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;
                    case 3:
                        chatsHandler.ListChats();
                        Console.WriteLine();
                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;
                    case 4:
                        Console.WriteLine("Thank you for using Genesys Cloud Chat!");
                        break;
                    default:
                        Console.WriteLine("Incorrect option. Please, select between 1 to 4.");
                        Console.WriteLine();
                        break;
                }
            } while (option != 4);
            
        }
    }
}
