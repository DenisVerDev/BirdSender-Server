using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MessangerServer
{
    public abstract class Channel
    {
        public delegate void ChannelHandler(Message msg,Channel channel, bool sync);
        public event ChannelHandler MessageCameOut;

        public delegate void StreamHandler(Channel channel,byte[] data, bool isend, string name);
        public event StreamHandler ImageCameOut;
        public event StreamHandler AudioCameOut;

        public string Address { get; set; }

        public List<string> Users { get; set; }

        public bool IsGroup { get; protected set; }

        public long Total { get; set; }

        public int UsersCount { get; set; }

        public Dictionary<string, long> lastmsgid { get; set; }

        public bool IsCallStarted { get; set; }

        public string StreamerName { get; set; }

        public Channel()
        {
            lastmsgid = new Dictionary<string, long>();
            this.Users = new List<string>();
            this.UsersCount = 0;
            this.Total = 0;
            IsCallStarted = false;
        }

        public Channel(string[] users)
        {
            lastmsgid = new Dictionary<string, long>();
            this.Users = new List<string>();
            this.Users.AddRange(users);
            this.UsersCount = this.Users.Count;
            this.Total = 0;
            IsCallStarted = false;
        }

        public Channel(string address, bool IsGroup)
        {
            lastmsgid = new Dictionary<string, long>();
            this.Address = address;
            this.IsGroup = IsGroup;
            IsCallStarted = false;
            ReadConfig();
        }

        public void StartCall(string caller)
        {
            Message msg = new Message();
            msg.Address = this.Address;
            msg.SendTime = DateTime.Now.ToString();
            msg.Username = "Bot";
            msg.Text = String.Format("[service type=stream]{0}",Address);
            SendServiceMessage(msg);
            IsCallStarted = true;
            StreamerName = caller;
        }

        public void StopCall()
        {
                Message msg = new Message();
                msg.Address = this.Address;
                msg.SendTime = DateTime.Now.ToString();
                msg.Username = "Bot";
                msg.Text = String.Format("[service type=stream]{0}", Address);
                SendServiceMessage(msg);
                IsCallStarted = false;
                StreamerName = String.Empty;
        }

        public void AudioCame(byte[] data, string name)
        {
            AudioCameOut(this, data, false,name);
        }

        public void ImageCame(byte[] data, bool isend, string name)
        {
            ImageCameOut(this, data, isend,name);
        }

        public abstract void ReadConfig();


        public abstract void SaveMessageInHistory(Message msg);

        public abstract void SaveConfigurations();

        public virtual void  SendServiceMessage(Message msg)
        {
            msg.Id = 0;
            MessageCameOut(msg, this, true);
        }

        public virtual void SendMessage(Message msg)
        {
                msg.Id = Total;
                Total++;
                MessageCameOut(msg,this,true);
                SaveMessageInHistory(msg);
                SaveConfigurations();
        }

        public void ClearCheckList(string user)
        {
            if (lastmsgid.ContainsKey(user))
            {
                lastmsgid.Remove(user);
            }
        }

        public void AddToCheckList(string user, long id)
        {
            if(!lastmsgid.ContainsKey(user))
            {
                lastmsgid.Add(user, id);
            }
        }


        public abstract void RemoveAllData();

        public long GetNewMessagesCount(string user)
        {
            long newtotal = this.Total - lastmsgid[user];
            return newtotal;
        }

        public abstract bool GetNewMessages(string user, out List<Message> messages);
       
    }
}
