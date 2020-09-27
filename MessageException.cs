using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerServer
{
    public class MessageException: Exception
    {
        public string User { get; private set; }
        public long Id { get; private set; }

        public MessageException(string user, long id) :base("User has no connection to server")
        {
            User = user;
            Id = id;
        }
    }
}
