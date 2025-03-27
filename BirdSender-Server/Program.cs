using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using dm = DataModule;
using sm = ServiceModule;
using clm = ClientModule;
using LoggerModule;

namespace MessangerServer
{

    class Program
    {
        static void Main(string[] args)
        {
            dm::DataBank db = new dm.DataBank();
            db.LoadUsers();
            db.LoadChannels();

            sm::MSGService.Channels = db.Channels;
            sm::MSGService.Users = db.Users;

            clm::User.GlobalChannels = db.Channels;

            ServiceHost sh = new ServiceHost(typeof(sm::MSGService));
            sh.Open();
            Logger.Log("Server started at " + DateTime.Now.ToString(), Logger.MessageType.Attention);
            Console.ReadLine();

        }
    }
}
