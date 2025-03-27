using MessageModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChannelModule
{
    public class Chat : Channel
    {
        public Chat(string address) : base(address, false)
        {

        }

        public Chat(string[] users) : base(users)
        {
            Address = Users[0] + "_" + Users[1];
            this.IsGroup = false;
            Directory.CreateDirectory(String.Format("ServerData/Chats/{0}", Address));
            Directory.CreateDirectory(String.Format("ServerData/Chats/{0}/files", Address));
        }

        public override void ReadConfig()
        {
            try
            {
                FileStream fs = new FileStream(String.Format("ServerData/Chats/{0}/info.txt", this.Address), FileMode.Open, FileAccess.Read);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    Total = br.ReadInt64();
                    UsersCount = br.ReadInt32();
                    Users = new List<string>();
                    for (int i = 0; i < UsersCount; i++)
                    {
                        Users.Add(br.ReadString());
                    }
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string key = br.ReadString();
                        long value = br.ReadInt64();
                        lastmsgid.Add(key, value);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void SaveConfigurations()
        {
            try
            {
                FileStream fs = new FileStream(String.Format("ServerData/Chats/{0}/info.txt", this.Address), FileMode.OpenOrCreate, FileAccess.Write);
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(Total);
                    bw.Write(UsersCount);
                    foreach (string user in Users)
                    {
                        bw.Write(user);
                    }
                    bw.Write(lastmsgid.Count);
                    foreach (KeyValuePair<string, long> pair in lastmsgid)
                    {
                        bw.Write(pair.Key);
                        bw.Write(pair.Value);
                    }
                }
                fs.Close();
                fs.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        public override void SaveMessageInHistory(Message msg)
        {
            try
            {
                MessageSerializer ser = new MessageSerializer();
                FileStream fs = null;
                FileMode mode = FileMode.Append;
                if (!File.Exists(String.Format("ServerData/Chats/{0}/history.mxml", this.Address))) mode = FileMode.OpenOrCreate;
                fs = new FileStream(String.Format("ServerData/Chats/{0}/history.mxml", this.Address), mode, FileAccess.Write);
                ser.Serialize(fs, msg);
                fs.Close();
                fs.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        public override bool GetNewMessages(string user, out List<Message> messages)
        {
            MessageSerializer serializer = new MessageSerializer();
            messages = new List<Message>();
            long pos = 0;
            if (this.Total - lastmsgid[user] >= 20) pos = lastmsgid[user] + 20;
            else
            {
                pos = this.Total - lastmsgid[user];
                pos += lastmsgid[user];
            }
            for (long i = lastmsgid[user]; i < pos; i++)
            {
                Message msg = serializer.GetMessageById(String.Format("ServerData/Chats/{0}/history.mxml", Address), i);
                messages.Add(msg);
            }
            lastmsgid[user] = pos;

            if (this.Total - pos == 0) return true;

            return false;
        }

        public override void RemoveAllData()
        {
            //deleting files
            string[] files = Directory.GetFiles(String.Format("ServerData/Chats/{0}_{1}/files", Users[0], Users[1]));
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(String.Format("ServerData/Chats/{0}_{1}/files", Users[0], Users[1]));
            //deleting chat folder
            File.Delete(String.Format("ServerData/Chats/{0}_{1}/history.mxml", Users[0], Users[1]));
            File.Delete(String.Format("ServerData/Chats/{0}_{1}/info.txt", Users[0], Users[1]));
            Directory.Delete(String.Format("ServerData/Chats/{0}_{1}", Users[0], Users[1]));

            Message msg = new Message();
            msg.Address = this.Address;
            msg.Username = "Bot";
            msg.SendTime = DateTime.Now.ToString();
            msg.Text = "[service type=chdelete]";

            SendServiceMessage(msg);
        }
    }
}
