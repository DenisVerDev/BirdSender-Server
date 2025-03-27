using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.IO;
using dm = DatabaseModule;
using clm = ClientModule;
using cnm = ChannelModule;
using mm = MessageModule;

namespace ServiceModule
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MSGService : IMSGService
    {
        public static List<cnm::Channel> Channels { get; set; }
        public static List<clm::User> Users { get; set; }

        #region RegistrationLogin
        public clm::ResultCodes Login(string email, string password, out clm::User user)
        {
            try
            {
                using (dm::MessangerDataContext db = new dm::MessangerDataContext())
                {
                    dm::User userlogin = db.Users.Where(x => x.Email == email).FirstOrDefault();
                    if (userlogin != null)
                    {
                        if (userlogin.Password != password)
                        {
                            user = null;
                            return clm::ResultCodes.IncorrectPassword;
                        }
                        user = Users.Where(x => x.Username == userlogin.Username).First();
                    }
                    else
                    {
                        user = null;
                        return clm::ResultCodes.IncorrectEmail;
                    }
                }
            }
            catch (Exception ex)
            {
                user = null;
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes Registration(dm::User user)
        {
            try
            {
                using (dm::MessangerDataContext db = new dm::MessangerDataContext())
                {
                    if (!db.Users.Where(x => x.Username == user.Username).Any())
                    {
                        if (!db.Users.Where(x => x.Email == user.Email).Any())
                        {
                            db.Users.Add(user);
                            db.SaveChanges();
                            Users.Add(new clm::User(user));
                        }
                        else return clm::ResultCodes.IncorrectEmail;
                    }
                    else return clm::ResultCodes.IncorrectUserName;
                }
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }
        #endregion

        #region SearchUsers

        public clm::ResultCodes SearchUserByUserName(string username, out clm::User user)
        {
            try
            {
                using (dm::MessangerDataContext db = new dm::MessangerDataContext())
                {
                    dm::User users = db.Users.Where(x => x.Username == username).FirstOrDefault();
                    if (users == null)
                    {
                        user = null;
                        return clm::ResultCodes.NoMatching;
                    }
                    user = new clm::User(users);
                }
            }
            catch (Exception ex)
            {
                user = null;
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }
        #endregion

        #region WorkWithImage
        public clm::ResultCodes UploadUserAvatar(string username, byte[] img, bool isnew)
        {
            try
            {
                using (dm::MessangerDataContext db = new dm::MessangerDataContext())
                {
                    if (db.Users.Where(x => x.Username == username).Any())
                    {
                        if (isnew) File.Delete(String.Format("ServerData/UsersAvatars/{0}_avatar.jpg", username));
                        using (FileStream fs = new FileStream(String.Format("ServerData/UsersAvatars/{0}_avatar.jpg", username), FileMode.Append, FileAccess.Write))
                        {
                            fs.Write(img, 0, img.Length);
                            fs.Close();
                        }
                    }
                    else return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetUserAvatar(string username, long position, out byte[] img)
        {
            try
            {
                if (File.Exists(String.Format("ServerData/UsersAvatars/{0}_avatar.jpg", username)))
                {
                    using (FileStream fs = new FileStream(String.Format("ServerData/UsersAvatars/{0}_avatar.jpg", username), FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = position;
                        img = new byte[65000];
                        fs.Read(img, 0, img.Length);
                        long length = fs.Length;
                        position = fs.Position;
                        fs.Close();
                        if (position >= length) return clm::ResultCodes.Success;
                        else return clm::ResultCodes.InProccess;
                    }
                }
                else
                {
                    img = null;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                img = null;
                return clm::ResultCodes.ServerError;
            }
        }

        public clm::ResultCodes UploadGroupAvatar(string address, byte[] img, bool isnew)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    if (isnew) File.Delete(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", address));
                    using (FileStream fs = new FileStream(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", address), FileMode.Append, FileAccess.Write))
                    {
                        fs.Write(img, 0, img.Length);
                        fs.Close();
                    }
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetGroupAvatar(string address, long position, out byte[] img)
        {
            try
            {
                if (File.Exists(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", address)))
                {
                    using (FileStream fs = new FileStream(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", address), FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = position;
                        img = new byte[65000];
                        fs.Read(img, 0, img.Length);
                        long length = fs.Length;
                        position = fs.Position;
                        fs.Close();
                        if (position >= length) return clm::ResultCodes.Success;
                        else return clm::ResultCodes.InProccess;
                    }
                }
                else
                {
                    img = null;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                img = null;
                return clm::ResultCodes.ServerError;
            }
        }
        #endregion

        #region WorkWithChannels

        public clm::ResultCodes GetUserOnlineStatus(string username, out bool isonline, out Nullable<DateTime> dateTime)
        {
            try
            {
                clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                if (user != null)
                {
                    isonline = user.IsOline;
                    if (!isonline) dateTime = user.LastOnline;
                    else dateTime = null;
                }
                else
                {
                    dateTime = null;
                    isonline = false;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                dateTime = null;
                isonline = false;
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }


        public clm::ResultCodes GetGroupUsersCount(string address, out int count)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    count = channel.UsersCount;
                }
                else
                {
                    count = 0;
                    return clm::ResultCodes.ServerError;
                }
            }
            catch (Exception ex)
            {
                count = 0;
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes AddChat(string firstuser, string seconduser)
        {
            try
            {
                cnm::Channel ch = Channels.Where(x => x.Users.Count == 2 && x.Users.Contains(firstuser) && x.Users.Contains(seconduser)).FirstOrDefault();
                if (ch == null)
                {
                    clm::User first = Users.Where(x => x.Username == firstuser).FirstOrDefault();
                    clm::User second = Users.Where(x => x.Username == seconduser).FirstOrDefault();
                    if (first != null && second != null)
                    {
                        string[] users = new string[2] { firstuser, seconduser };
                        cnm::Chat chat = new cnm::Chat(users);
                        first.SubscribeToChannel(chat, true);
                        second.SubscribeToChannel(chat, false);
                        Channels.Add(chat);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.ElementIsAlreadyCreated;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes AddGroup(string groupname, string username, string address)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel == null)
                {
                    clm::User admin = Users.Where(x => x.Username == username).FirstOrDefault();
                    if (admin != null)
                    {
                        cnm::Group group = new cnm::Group(username, groupname, address);
                        admin.SubscribeToChannel(group, true);
                        group.NotifyUsers(admin.Username);
                        Channels.Add(group);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }


        public clm::ResultCodes GetGroupInfo(string address, out string admin, out string groupname)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    cnm::Group group = channel as cnm::Group;
                    admin = group.AdminName;
                    groupname = group.GroupName;
                }
                else
                {
                    admin = null;
                    groupname = null;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                admin = null;
                groupname = null;
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes InviteToGroup(string address, string username)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                    if (user != null)
                    {
                        (channel as cnm::Group).AddUser(user.Username);
                        user.SubscribeToChannel(channel, false);
                        (channel as cnm::Group).NotifyUsers(user.Username);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes RemoveChannel(string username, string address)
        {
            try
            {
                cnm::Channel ch = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (ch != null)
                {
                    if (ch.IsGroup && (ch as cnm::Group).AdminName == username || !ch.IsGroup)
                    {
                        ch.RemoveAllData();
                        foreach (string name in ch.Users)
                        {
                            clm::User user = Users.Where(x => x.Username == name).FirstOrDefault();
                            if (user != null) user.UnSubscribeFromChannel(ch);
                        }
                        Channels.Remove(ch);
                    }
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes LeaveGroup(string username, string address)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null && channel.IsGroup)
                {
                    clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                    if (user != null)
                    {
                        (channel as cnm::Group).KickUser(username);
                        user.UnSubscribeFromChannel(channel);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetGroupUsers(string address, string lastname, out int count, out string[] user)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    count = channel.UsersCount;
                    int index = channel.Users.IndexOf(lastname);
                    if (index == -1) index = 0;
                    if (channel.UsersCount >= 10) user = channel.Users.GetRange(index, 10).ToArray();
                    else user = channel.Users.GetRange(index, channel.UsersCount).ToArray();
                    if (index + 10 < channel.UsersCount) return clm::ResultCodes.InProccess;
                }
                else
                {
                    count = 0;
                    user = null;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                user = null;
                count = 0;
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }


        public clm::ResultCodes RenameGroup(string address, string newname)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null && channel.IsGroup)
                {
                    (channel as cnm::Group).ChangeName(newname);
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes StartCall(string username, string address)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.StartCall(username);
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetCall(string address, out bool isstreaming)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    isstreaming = channel.IsCallStarted;
                }
                else
                {
                    isstreaming = false;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                isstreaming = false;
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes StopCall(string username, string address)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.StopCall();
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes SendAudio(string address, byte[] data, string name)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.AudioCame(data, name);
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes SendImages(string address, byte[] data, bool isend, string name)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.ImageCame(data, isend, name);
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }


        public clm::ResultCodes ConnectToCall(string username, string address)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                    if (user != null)
                    {
                        user.IsConnectedToStream = new KeyValuePair<string, bool>(address, true);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes DisconnectFromCall(string username, string address)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                    if (user != null)
                    {
                        user.IsConnectedToStream = new KeyValuePair<string, bool>(String.Empty, false);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        #endregion

        #region SendMessages

        public clm::ResultCodes SendMessage(mm::Message msg)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == msg.Address).FirstOrDefault();
                if (channel != null)
                {
                    if (channel.Users.Contains(msg.Username))
                    {
                        channel.SendMessage(msg);
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetTotalNewMessages(string user, string address, out long total)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    clm::User us = Users.Where(x => x.Username == user).FirstOrDefault();
                    if (us != null)
                    {
                        if (us.CheckChannel(address)) total = channel.GetNewMessagesCount(user);
                        else
                        {
                            total = 0;
                            return clm::ResultCodes.NoMatching;
                        }
                    }
                    else
                    {
                        total = 0;
                        return clm::ResultCodes.NoMatching;
                    }
                }
                else
                {
                    total = 0;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                total = 0;
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetNewMessages(string username, string address)
        {
            try
            {
                clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                if (user != null)
                {
                    return user.GetNewMessages(address);
                }
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes GetInvites(string username, out string[] address)
        {
            try
            {
                clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                if (user != null)
                {
                    address = user.Invites.ToArray();
                    if (address.Length > 0)
                    {
                        user.Invites.Clear();
                        user.SaveAllChannels();
                    }
                    else return clm::ResultCodes.NoMatching;
                }
                else
                {
                    address = null;
                    return clm::ResultCodes.NoMatching;
                }

            }
            catch (Exception ex)
            {
                address = null;
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes SendFile(string filename, string address, long position, byte[] file)
        {
            try
            {
                string mode = "Chats";
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    if (channel.IsGroup) mode = "Groups";
                    string path = String.Format("ServerData/{0}/{1}/files/{2}", mode, address, filename);
                    using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                    {
                        fs.SetLength(position);
                        fs.Position = position;
                        fs.Write(file, 0, file.Length);
                        fs.Close();
                    }
                }
                else return clm::ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public clm::ResultCodes DownloadFile(string filename, string address, long position, out byte[] file)
        {
            try
            {
                cnm::Channel channel = Channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    string mode = "Chats";
                    if (channel.IsGroup) mode = "Groups";
                    string path = String.Format("ServerData/{0}/{1}/files/{2}", mode, address, filename);
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = position;
                        file = new byte[65000];
                        fs.Read(file, 0, file.Length);
                        fs.Close();
                    }
                }
                else
                {
                    file = null;
                    return clm::ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                file = null;
                return clm::ResultCodes.ServerError;
            }

            return clm::ResultCodes.Success;
        }


        #endregion

        public clm::ResultCodes SubscribeToUser(string username)
        {
            try
            {
                clm::User us = Users.Where(x => x.Username == username).FirstOrDefault();
                if (us != null)
                {
                    us.ConnectToUser();
                }
                else throw new Exception();
            }
            catch (Exception ex)
            {
                return clm::ResultCodes.ServerError;
            }
            return clm::ResultCodes.Success;
        }

        public void Disconnect(string username)
        {
            try
            {
                clm::User user = Users.Where(x => x.Username == username).FirstOrDefault();
                user.DisconnectFromUser();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
