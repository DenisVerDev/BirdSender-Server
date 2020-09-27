using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace MessangerServer
{
    public interface IClientResponse
    {
        [OperationContract(IsOneWay =true)]
        void ReceiveMessage(Message msg);

        [OperationContract]
        ResultCodes HasConnectionToClient();

        [OperationContract(IsOneWay = true)]
        void ReceiveAudio(byte[] data);

        [OperationContract(IsOneWay = true)]
        void ReceiveImage(byte[] data, bool isend);
    }
}
