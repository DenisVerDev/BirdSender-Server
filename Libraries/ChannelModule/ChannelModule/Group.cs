using MessageModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChannelModule
{
    public class Group : Channel
    {
        public string GroupName { get; set; }

        public string AdminName { get; set; }

        public Group(string address) : base(address, true)
        {

        }

        public Group(string username, string groupname, string address) : base()
        {
            AdminName = username;
            AddUser(username);
            this.GroupName = groupname;
            this.IsGroup = true;
            this.Address = address;
            Directory.CreateDirectory(String.Format("ServerData/Groups/{0}", Address));
            Directory.CreateDirectory(String.Format("ServerData/Groups/{0}/files", Address));
        }

        public void ChangeName(string name)
        {
            Message msg = new Message();
            msg.Address = this.Address;
            msg.SendTime = DateTime.Now.ToString();
            msg.Username = "Bot";
            msg.Text = String.Format("[service type=chupdate, groupname={0}]{1} was renamed to {2}", name, GroupName, name);
            GroupName = name;
            SendMessage(msg);
        }

        public void NotifyUsers(string username)
        {
            Message msg = new Message();
            msg.Address = this.Address;
            msg.SendTime = DateTime.Now.ToString();
            msg.Username = "Bot";
            msg.Text = String.Format("[service type=ivitation, admin={0}, name={1}]{2} welcome to the group", AdminName, GroupName, username);
            SendMessage(msg);
        }

        public void AddUser(string username)
        {
            this.Users.Add(username);
            this.UsersCount = this.Users.Count;
        }

        public void KickUser(string username)
        {
            this.Users.Remove(username);
            this.UsersCount = this.Users.Count;
            this.ClearCheckList(username);
            Message msg = new Message();
            msg.Address = this.Address;
            msg.SendTime = DateTime.Now.ToString();
            msg.Username = "Bot";
            msg.Text = String.Format("[service type=grkick]{0}", username);
            SendServiceMessage(msg);
        }

        public override void ReadConfig()
        {
            try
            {
                FileStream fs = new FileStream(String.Format("ServerData/Groups/{0}/info.txt", this.Address), FileMode.Open, FileAccess.Read);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    GroupName = br.ReadString();
                    AdminName = br.ReadString();
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
                FileStream fs = new FileStream(String.Format("ServerData/Groups/{0}/info.txt", this.Address), FileMode.OpenOrCreate, FileAccess.Write);
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(GroupName);
                    bw.Write(AdminName);
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
                if (!File.Exists(String.Format("ServerData/Groups/{0}/history.mxml", this.Address))) mode = FileMode.OpenOrCreate;
                fs = new FileStream(String.Format("ServerData/Groups/{0}/history.mxml", this.Address), mode, FileAccess.Write);
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
                Message msg = serializer.GetMessageById(String.Format("ServerData/Groups/{0}/history.mxml", Address), i);
                messages.Add(msg);
            }
            lastmsgid[user] = pos;

            if (this.Total - pos == 0) return true;

            return false;
        }

        public override void RemoveAllData()
        {
            //deleting files
            string[] files = Directory.GetFiles(String.Format("ServerData/Groups/{0}/files", Address));
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(String.Format("ServerData/Groups/{0}/files", Address));
            //deleting group folder
            File.Delete(String.Format("ServerData/Groups/{0}/history.mxml", Address));
            File.Delete(String.Format("ServerData/Groups/{0}/info.txt", Address));
            if (File.Exists(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", Address))) File.Delete(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", Address));
            Directory.Delete(String.Format("ServerData/Groups/{0}", Address));

            Message msg = new Message();
            msg.Address = this.Address;
            msg.Username = "Bot";
            msg.SendTime = DateTime.Now.ToString();
            msg.Text = "[service type=chdelete]";

            SendServiceMessage(msg);
        }
    }
}
