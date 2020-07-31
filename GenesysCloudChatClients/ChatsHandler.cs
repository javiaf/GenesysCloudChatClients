using System;
using System.Collections.Generic;
using System.Text;

namespace GenesysCloudChatClients
{
    class ChatsHandler
    {
        private Dictionary<string, ChatClient> chats = null;
        
        public ChatsHandler() {
            chats = new Dictionary<string, ChatClient>();
        }




        private Dictionary<string, string> GetKVPs(int nKvps)
        {
            Dictionary<string, string> kvps = new Dictionary<string, string>();
            string key, value;
            key = value = string.Empty;
            for (int i = 0; i < nKvps; i++)
            {
                Console.WriteLine("KVP " + i + ": Type the KEY:");
                key = Console.ReadLine();
                Console.WriteLine("KVP " + i + ": Type the VALUE:");
                value = Console.ReadLine();
            }

            return kvps;
        }

        public void ListChats() {
            List<string> conversationIds = new List<string>(this.chats.Keys);
            foreach (string conversationId in conversationIds) {
                Console.WriteLine("Conversation ID: " + conversationId);
            }
        }

        public void SendChatMessage()
        {
            ChatClient chatClient;
            string chatId, message;
            chatId = message = string.Empty;

            Console.WriteLine("Chat Message - Listing Conversation IDs: ");
            Console.WriteLine("-");
            ListChats();
            Console.WriteLine();

            Console.WriteLine("Chat Message - Enter the conversation ID of the chat: ");
            chatId = Console.ReadLine();
            if (chats.TryGetValue(chatId, out chatClient))
            {
                Console.WriteLine("Chat Message - Type your Message: ");
                message = Console.ReadLine();       
                chatClient.SendMessage(message);
            }
            else {
                Console.WriteLine("Chat Message - Error: Wrong Conversation ID");
            }

        }

        public void CreateChat()
        {
            ChatClient chatClient = new ChatClient();
            
            
            
            Dictionary<string, string> kvps = new Dictionary<string, string>();
            string name, lastName, displayName;
            name = lastName = displayName = string.Empty;

            int nKvps = 0;



                Console.WriteLine("Chat Creation - Type your First Name: " );
                name = Console.ReadLine();
                Console.WriteLine("Chat Creation - Type your Last Name: ");
                lastName = Console.ReadLine();
                displayName = name + " " + lastName;
                Console.WriteLine("Chat Creation - Type the number of Key Value Pairs you want to add: ");
                nKvps = Convert.ToInt32(Console.ReadLine());
                kvps = GetKVPs(nKvps);
                chatClient.ConversationStarted += new EventHandler(ConversationStarted_Handler);
                chatClient.CreateChat(displayName, name, lastName, kvps);
                
        }

        private void ConversationStarted_Handler(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ChatClient)) {
                ChatClient chatClient = (ChatClient)sender;
                chats.TryAdd(chatClient.ConversationId, chatClient);
                Console.WriteLine("Chat with ID: " + chatClient.ConversationId + " Started!");
            }
        }
    }
}
