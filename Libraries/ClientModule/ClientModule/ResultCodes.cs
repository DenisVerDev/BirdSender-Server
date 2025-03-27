using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModule
{
    public enum ResultCodes
    {
        ServerConnectionLost = 0,
        IncorrectPassword = 1,
        IncorrectEmail = 2,
        IncorrectUserName = 3,
        ServerError = 4,
        NoMatching = 5,
        ElementIsAlreadyCreated = 6,
        Success = 7,
        ClientConnectionLost = 8,
        InProccess = 9
    }
}
