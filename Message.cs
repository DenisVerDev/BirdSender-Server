﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MessangerServer
{
    [DataContract, Serializable]
    public class Message
    {
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string SendTime { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public long Id { get; set; }

        public Message(string username, string address,string text="empty")
        {
            this.Username = username;
            this.Address = address;
            this.Text = text;
            SendTime = DateTime.Now.ToLongDateString();
        }

        public Message(string username, string address, string text,string date, long id)
        {
            this.Username = username;
            this.Address = address;
            this.Text = text;
            this.SendTime = date;
            this.Id = id;
        }

        public Message()
        {
            Address = Username = Text = SendTime = String.Empty;
            Id = 0;
        }
    }
}
