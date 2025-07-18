using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessageModule
{
    public class MessageSerializer
    {
        int prcount = 0;

        Type type;
        PropertyInfo[] properties = null;

        public MessageSerializer()
        {
            type = typeof(Message);
            properties = type.GetProperties();
            prcount = properties.Length + 2;
        }

        public void Serialize(Stream stream, Message msg)
        {
            string str = "<Message>\r\n";
            foreach (PropertyInfo p in properties)
            {
                str += String.Format("<{0}>{1}</{0}>\r\n", p.Name, p.GetValue(msg).ToString());
            }
            str += "</Message>\r\n";
            byte[] data = Encoding.UTF8.GetBytes(str);
            stream.Write(data, 0, data.Length);
        }

        public Message GetMessageById(string path, long id)
        {
            long position = prcount * id;
            string[] msgprop = File.ReadLines(path).Skip((int)position).Take(prcount).ToArray();

            string username = msgprop.Where(x => x.Contains("<Username>")).First();
            username = username.Substring(username.IndexOf('>') + 1, username.IndexOf('/') - 2 - username.IndexOf('>'));

            string address = msgprop.Where(x => x.Contains("<Address>")).First();
            address = address.Substring(address.IndexOf('>') + 1, address.IndexOf('/') - 2 - address.IndexOf('>'));

            string text = msgprop.Where(x => x.Contains("<Text>")).First();
            text = text.Substring(text.IndexOf('>') + 1, text.IndexOf('/') - 2 - text.IndexOf('>'));

            string time = msgprop.Where(x => x.Contains("<SendTime>")).First();
            time = time.Substring(time.IndexOf('>') + 1, time.IndexOf('/') - 2 - time.IndexOf('>'));

            Message msg = new Message(username,
                address, text, time, id);
            return msg;
        }

        public static bool IsCorrectServiceMessage(string text)
        {
            if (text.StartsWith("[service type=user") || text.StartsWith("[service type=chdelete") || text.StartsWith("[service type=grkick") || text.StartsWith("[service type=stream"))
                return false;

            return true;
        }
    }
}
