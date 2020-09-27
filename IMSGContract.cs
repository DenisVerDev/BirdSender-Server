using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace MessangerServer
{
    [ServiceContract(CallbackContract = typeof(IClientResponse))]
    public interface IMSGService
    {
        //Registration and login
        [OperationContract]
        ResultCodes Registration(Users user);

        [OperationContract]
        ResultCodes Login(string email, string password,out User user);
        //----------------------------------------------------------------------

        //Search for users
        [OperationContract]
        ResultCodes SearchUserByUserName(string username, out User user);
        //----------------------------------------------------------------------

        //WorkWithImage
        [OperationContract]
        ResultCodes UploadUserAvatar(string username, byte[] img, bool isnew);

        [OperationContract]
        ResultCodes UploadGroupAvatar(string address, byte[] img, bool isnew);

        [OperationContract]
        ResultCodes GetUserAvatar(string username, long position, out byte[] img);

        [OperationContract]
        ResultCodes GetGroupAvatar(string address, long position, out byte[] img);

        //----------------------------------------------------------------------

        //WorkWithChannels

        [OperationContract]
        ResultCodes GetUserOnlineStatus(string username, out bool isonline, out Nullable<DateTime> dateTime);

        [OperationContract]
        ResultCodes AddChat(string firstuser, string seconduser);

        [OperationContract]
        ResultCodes AddGroup(string groupname, string username, string address);

        [OperationContract]
        ResultCodes GetGroupUsersCount(string address, out int count);

        [OperationContract]
        ResultCodes GetGroupInfo(string address, out string admin, out string groupname);

        [OperationContract]
        ResultCodes InviteToGroup(string address, string username);

        [OperationContract]
        ResultCodes RemoveChannel(string username, string address);

        [OperationContract]
        ResultCodes LeaveGroup(string username, string address);

        [OperationContract]
        ResultCodes GetGroupUsers(string address, string lastname, out int count, out string[] user);

        [OperationContract]
        ResultCodes RenameGroup(string address, string newname);

        [OperationContract]
        ResultCodes StartCall(string username, string address);

        [OperationContract]
        ResultCodes GetCall(string address, out bool isstreaming);

        [OperationContract]
        ResultCodes StopCall(string username, string address);

        [OperationContract]
        ResultCodes SendAudio(string address, byte[] data,string username);

        [OperationContract]
        ResultCodes SendImages(string address, byte[] data, bool isend, string name);

        [OperationContract]
        ResultCodes ConnectToCall(string username, string address);

        [OperationContract]
        ResultCodes DisconnectFromCall(string username, string address);
        //----------------------------------------------------------------------

        //Messages
        [OperationContract]
        ResultCodes SendMessage(Message msg);

        [OperationContract]
        ResultCodes SendFile(string filename, string address, long position, byte[] file);

        [OperationContract]
        ResultCodes DownloadFile(string filename, string address, long position, out byte[] file);

        [OperationContract]
        ResultCodes GetTotalNewMessages(string user,string address, out long total);

        [OperationContract]
        ResultCodes GetNewMessages(string username, string address);

        [OperationContract]
        ResultCodes GetInvites(string username, out string[] address);
        //----------------------------------------------------------------------

        //SubscribeToUser
        [OperationContract]
        ResultCodes SubscribeToUser(string username);

        [OperationContract(IsOneWay =true)]
        void Disconnect(string username);
        //----------------------------------------------------------------------
    }
}
