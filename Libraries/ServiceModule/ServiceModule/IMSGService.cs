using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using dm = DatabaseModule;
using clm = ClientModule;
using mm = MessageModule;

namespace ServiceModule
{
    [ServiceContract(CallbackContract = typeof(clm::IClientResponse))]
    public interface IMSGService
    {
        //Registration and login
        [OperationContract]
        clm::ResultCodes Registration(dm::User user);

        [OperationContract]
        clm::ResultCodes Login(string email, string password, out clm::User user);
        //----------------------------------------------------------------------

        //Search for users
        [OperationContract]
        clm::ResultCodes SearchUserByUserName(string username, out clm::User user);
        //----------------------------------------------------------------------

        //WorkWithImage
        [OperationContract]
        clm::ResultCodes UploadUserAvatar(string username, byte[] img, bool isnew);

        [OperationContract]
        clm::ResultCodes UploadGroupAvatar(string address, byte[] img, bool isnew);

        [OperationContract]
        clm::ResultCodes GetUserAvatar(string username, long position, out byte[] img);

        [OperationContract]
        clm::ResultCodes GetGroupAvatar(string address, long position, out byte[] img);

        //----------------------------------------------------------------------

        //WorkWithChannels

        [OperationContract]
        clm::ResultCodes GetUserOnlineStatus(string username, out bool isonline, out Nullable<DateTime> dateTime);

        [OperationContract]
        clm::ResultCodes AddChat(string firstuser, string seconduser);

        [OperationContract]
        clm::ResultCodes AddGroup(string groupname, string username, string address);

        [OperationContract]
        clm::ResultCodes GetGroupUsersCount(string address, out int count);

        [OperationContract]
        clm::ResultCodes GetGroupInfo(string address, out string admin, out string groupname);

        [OperationContract]
        clm::ResultCodes InviteToGroup(string address, string username);

        [OperationContract]
        clm::ResultCodes RemoveChannel(string username, string address);

        [OperationContract]
        clm::ResultCodes LeaveGroup(string username, string address);

        [OperationContract]
        clm::ResultCodes GetGroupUsers(string address, string lastname, out int count, out string[] user);

        [OperationContract]
        clm::ResultCodes RenameGroup(string address, string newname);

        [OperationContract]
        clm::ResultCodes StartCall(string username, string address);

        [OperationContract]
        clm::ResultCodes GetCall(string address, out bool isstreaming);

        [OperationContract]
        clm::ResultCodes StopCall(string username, string address);

        [OperationContract]
        clm::ResultCodes SendAudio(string address, byte[] data, string username);

        [OperationContract]
        clm::ResultCodes SendImages(string address, byte[] data, bool isend, string name);

        [OperationContract]
        clm::ResultCodes ConnectToCall(string username, string address);

        [OperationContract]
        clm::ResultCodes DisconnectFromCall(string username, string address);
        //----------------------------------------------------------------------

        //Messages
        [OperationContract]
        clm::ResultCodes SendMessage(mm::Message msg);

        [OperationContract]
        clm::ResultCodes SendFile(string filename, string address, long position, byte[] file);

        [OperationContract]
        clm::ResultCodes DownloadFile(string filename, string address, long position, out byte[] file);

        [OperationContract]
        clm::ResultCodes GetTotalNewMessages(string user, string address, out long total);

        [OperationContract]
        clm::ResultCodes GetNewMessages(string username, string address);

        [OperationContract]
        clm::ResultCodes GetInvites(string username, out string[] address);
        //----------------------------------------------------------------------

        //SubscribeToUser
        [OperationContract]
        clm::ResultCodes SubscribeToUser(string username);

        [OperationContract(IsOneWay = true)]
        void Disconnect(string username);
        //----------------------------------------------------------------------
    }
}
