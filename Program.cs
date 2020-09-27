using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessangerServer
{
    public enum ResultCodes
    {
        ServerConnectionLost = 0,
        IncorrectPassword = 1,
        IncorrectEmail = 2,
        IncorrectUserName = 3,
        ServerError = 4,
        NoMatching = 5,
        ElementIsAlreadyCreated=6,
        Success = 7,
        ClientConnectionLost = 8,
        InProccess = 9
    };

    class Program
    {
        static public List<Channel> channels = new List<Channel>();
        static public List<User> users = new List<User>();


        static void Main(string[] args)
        {
            LoadUsers();
            LoadChannels();
            ServiceHost sh = new ServiceHost(typeof(MsgService));
            sh.Open();
            Logger.Log("Server started at " + DateTime.Now.ToString(),Logger.MessageType.Attention);
            Console.ReadLine(); 

        }

        static public void LoadUsers()
        {
            using(MessangerData db = new MessangerData())
            {
                foreach(Users user in db.Users)
                {
                    users.Add(new User(user));
                }
            }
        }

        static public void LoadChannels()
        {
            //chats load
            try
            {
                string[] chats = Directory.GetDirectories("ServerData/Chats/");
                User first = null;
                User second = null;
                foreach (string chat in chats)
                {
                    Chat ch = new Chat(Path.GetFileName(chat));
                    first = Program.users.Where(x => x.Username == ch.Users[0]).FirstOrDefault();
                    second = Program.users.Where(x => x.Username == ch.Users[1]).FirstOrDefault();
                    if (first != null && second != null)
                    {
                        channels.Add(ch);
                        first.SubscribeToChannel(ch, false);
                        second.SubscribeToChannel(ch,false);
                        Logger.Log(ch.Users[0] + " and " + ch.Users[1], Logger.MessageType.Usual);
                    }
                }

                string[] groups = Directory.GetDirectories("ServerData/Groups/");
                foreach (string group in groups)
                {
                    Group gr = new Group(Path.GetFileName(group));
                    channels.Add(gr);
                    foreach(string username in gr.Users)
                    {
                        User user = users.Where(x => x.Username == username).FirstOrDefault();
                        if(user != null)
                        {
                            user.SubscribeToChannel(gr, false);
                        }
                    }
                    Logger.Log(gr.Address + " is loaded", Logger.MessageType.Usual);
                }
            }
            catch(Exception ex)
            {

            }
        }
    }

    public static class Logger
    {
        public  enum MessageType { Usual=0,Attention=1,Warning=2};
        public static void Log(string msg, MessageType type)
        {
            switch(type)
            {
                case MessageType.Usual:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MessageType.Attention:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                default: break;
            }
            Console.WriteLine(msg); 
        }
    }
}
