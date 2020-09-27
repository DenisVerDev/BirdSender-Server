using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;
using System.IO;

namespace MessangerServer
{
    [DataContract]
    public class User
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public DateTime LastOnline { get; set; }

        private bool status;

        [DataMember]
        public bool IsOline 
        { 
            get
            {
                try
                {
                    if (callback.HasConnectionToClient() == ResultCodes.Success)
                    {
                        status = true;
                    }
                }
                catch(Exception ex)
                {
                    if (callback != null) DisconnectFromUser();
                    status = false;
                }
                return status;
            }
            set
            {
                status = value;
            }
        }

        public KeyValuePair<string,bool> IsConnectedToStream { get; set; }

        private List<string> AllChannels = new List<string>();
        public List<string> Invites = new List<string>();

        public IClientResponse callback = null;

        public Channel.ChannelHandler ChannelHandler = null;

        private bool Sync { get; set; }

        public User(Users user)
        {
            IsConnectedToStream = new KeyValuePair<string, bool>(String.Empty, false);
            this.id = user.id;
            this.Username = user.Username;
            this.LastOnline = user.LastOnline.GetValueOrDefault();
            this.IsOline = false;
            Sync = true;
            ReadChannels();
        }

        public void SubscribeToChannel(Channel channel, bool iscreator)
        {
            ChannelHandler = new Channel.ChannelHandler(NewMessage);
            channel.MessageCameOut += ChannelHandler;
            channel.AudioCameOut += NewAudio;
            channel.ImageCameOut += NewImage;
            if (!AllChannels.Contains(channel.Address) && !iscreator && !IsOline) Invites.Add(channel.Address);
            if (!AllChannels.Contains(channel.Address)) AllChannels.Add(channel.Address);
            SaveAllChannels();
        }

        public bool CheckChannel(string address)
        {
            return AllChannels.Contains(address);
        }

        private void NewAudio(Channel channel, byte[] data, bool isend, string name)
        {
            if(IsOline && IsConnectedToStream.Key.Equals(channel.Address))
            {
                if(Username != name)
                {
                    callback.ReceiveAudio(data);
                }
            }
        }

        private void NewImage(Channel channel, byte[] data, bool isend, string name)
        {
            if (IsOline && IsConnectedToStream.Key.Equals(channel.Address))
            {
                if (Username != name)
                {
                    callback.ReceiveImage(data,isend);
                }
            }
        }
        
        private void NewMessage(Message msg,Channel channel, bool sync)
        {

            if (IsOline)
            {
                if (Username != msg.Username && Sync == sync)
                {
                    callback.ReceiveMessage(msg);
                }
            }
            else if(MessageSerializer.IsCorrectServiceMessage(msg.Text))
            {
                channel.AddToCheckList(this.Username, msg.Id);
            }
                
        }

        public ResultCodes GetNewMessages(string address)
        {
            try
            {
                Sync = false;
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    List<Message> messages = null;
                    bool isend = channel.GetNewMessages(this.Username, out messages);
                    foreach (Message msg in messages)
                    {
                        NewMessage(msg, channel, false);
                    }
                    channel.SaveConfigurations();
                    if (!isend) return ResultCodes.InProccess;
                    else
                    {
                        Sync = true;
                        channel.ClearCheckList(this.Username);
                    }
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                Sync = true;
                return ResultCodes.ServerError;
            }
            
            return ResultCodes.Success;
        }

        private void ReadChannels()
        {
            try
            {
                if (File.Exists(String.Format("ServerData/UsersChannels/{0}.txt", Username)))
                {
                    FileStream fs = new FileStream(String.Format("ServerData/UsersChannels/{0}.txt", Username), FileMode.Open, FileAccess.Read);
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int count = br.ReadInt32();
                        for(int i = 0; i < count; i++)
                        {
                            string channel = br.ReadString();
                            AllChannels.Add(channel);
                        }
                        count = br.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            string channel = br.ReadString();
                            Invites.Add(channel);
                        }
                        br.Close();
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void SaveAllChannels()
        {
            try
            {
                FileStream fs = new FileStream(String.Format("ServerData/UsersChannels/{0}.txt", Username), FileMode.OpenOrCreate, FileAccess.Write);
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(AllChannels.Count);
                    for(int i = 0; i < AllChannels.Count; i++)
                    {
                        bw.Write(AllChannels[i]);
                    }
                    bw.Write(Invites.Count);
                    for(int i = 0; i < Invites.Count; i++)
                    {
                        bw.Write(Invites[i]);
                    }
                    bw.Close();
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void UnSubscribeFromChannel(Channel channel)
        {
            channel.MessageCameOut -= ChannelHandler;
            channel.AudioCameOut -= NewAudio;
            channel.ImageCameOut -= NewImage;
            if (AllChannels.Contains(channel.Address))
            {
                AllChannels.Remove(channel.Address);
                if(Invites.Contains(channel.Address))Invites.Remove(channel.Address);
                SaveAllChannels();
            }
        }


        public void ConnectToUser()
        {
            callback = OperationContext.Current.GetCallbackChannel<IClientResponse>();
            IsOline = true;
            //informing all chats, that we are online
            foreach (string address in AllChannels)
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null && channel.IsGroup == false)
                {
                    Message msg = new Message();
                    msg.Address = channel.Address;
                    msg.SendTime = DateTime.Now.ToString();
                    msg.Username = this.Username;
                    msg.Text = "[service type=user]";
                    channel.SendServiceMessage(msg);
                }
            }
        }

        public void DisconnectFromUser()
        {
            IsConnectedToStream = new KeyValuePair<string, bool>(String.Empty, false);

            callback = null;
            using (MessangerData db = new MessangerData())
            {
                IsOline = false;
                LastOnline = DateTime.Now;
                Users user = db.Users.Where(x => x.Username == this.Username).FirstOrDefault();
                if (user != null)
                {
                    user.LastOnline = this.LastOnline;
                    db.SaveChanges();
                }
            }
            //informing all chats, that we are offline
            foreach (string address in AllChannels)
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null && channel.IsGroup == false)
                {
                    Message msg = new Message();
                    msg.Address = channel.Address;
                    msg.SendTime = DateTime.Now.ToString();
                    msg.Username = this.Username;
                    msg.Text = "[service type=user]";
                    channel.SendServiceMessage(msg);
                }
            }
        }
    }
}
