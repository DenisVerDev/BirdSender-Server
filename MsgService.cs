using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Net.Http.Headers;

namespace MessangerServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MsgService : IMSGService
    {


        #region RegistrationLogin
        public ResultCodes Login(string email, string password, out User user)
        {
            try
            {
                using (MessangerData db = new MessangerData())
                {
                    Users userlogin = db.Users.Where(x => x.Email == email).FirstOrDefault();
                    if (userlogin != null)
                    {
                        if (userlogin.Password != password)
                        {
                            user = null;
                            return ResultCodes.IncorrectPassword;
                        }
                        user = Program.users.Where(x => x.Username == userlogin.Username).First();
                    }
                    else
                    {
                        user = null;
                        return ResultCodes.IncorrectEmail;
                    }
                }
            }
            catch (Exception ex)
            {
                user = null;
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }

        public ResultCodes Registration(Users user)
        {
            try
            {
                using (MessangerData db = new MessangerData())
                {
                    if (!db.Users.Where(x => x.Username == user.Username).Any())
                    {
                        if (!db.Users.Where(x => x.Email == user.Email).Any())
                        {
                            db.Users.Add(user);
                            db.SaveChanges();
                            Program.users.Add(new User(user));
                        }
                        else return ResultCodes.IncorrectEmail;
                    }
                    else return ResultCodes.IncorrectUserName;
                }
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }
        #endregion

        #region SearchUsers

        public ResultCodes SearchUserByUserName(string username, out User user)
        {
            try 
            {
                using (MessangerData db = new MessangerData())
                {
                    Users users = db.Users.Where(x => x.Username == username).FirstOrDefault();
                    if (users == null)
                    {
                        user = null;
                        return ResultCodes.NoMatching;
                    }
                    user = new User(users);
                }
            }
            catch(Exception ex)
            {
                user = null;
                return ResultCodes.ServerError;
            }
            
            return ResultCodes.Success;
        }
        #endregion

        #region WorkWithImage
        public ResultCodes UploadUserAvatar(string username, byte[] img,bool isnew)
        {
            try
            {
                using (MessangerData db = new MessangerData())
                {
                    if (db.Users.Where(x => x.Username == username).Any())
                    {
                        if (isnew) File.Delete(String.Format("ServerData/UsersAvatars/{0}_avatar.jpg", username));
                        using (FileStream fs = new FileStream(String.Format("ServerData/UsersAvatars/{0}_avatar.jpg", username),FileMode.Append,FileAccess.Write))
                        {
                            fs.Write(img, 0, img.Length);
                            fs.Close();
                        }
                    }
                    else return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }

        public ResultCodes GetUserAvatar(string username, long position, out byte[] img)
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
                        if (position >= length) return ResultCodes.Success;
                        else return ResultCodes.InProccess;
                    }
                }
                else
                {
                    img = null;
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                img = null;
                return ResultCodes.ServerError;
            }
        }

        public ResultCodes UploadGroupAvatar(string address, byte[] img, bool isnew)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    if (isnew) File.Delete(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", address));
                    using (FileStream fs = new FileStream(String.Format("ServerData/Groups/{0}/{0}_avatar.jpg", address), FileMode.Append, FileAccess.Write))
                    {
                        fs.Write(img, 0, img.Length);
                        fs.Close();
                    }
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }

        public ResultCodes GetGroupAvatar(string address, long position, out byte[] img)
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
                        if (position >= length) return ResultCodes.Success;
                        else return ResultCodes.InProccess;
                    }
                }
                else
                {
                    img = null;
                    return ResultCodes.NoMatching;
                }
            }
            catch (Exception ex)
            {
                img = null;
                return ResultCodes.ServerError;
            }
        }
        #endregion

        #region WorkWithChannels

        public ResultCodes GetUserOnlineStatus(string username, out bool isonline, out Nullable<DateTime> dateTime)
        {
            try
            {
                User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                if(user != null)
                {
                    isonline = user.IsOline;
                    if (!isonline) dateTime = user.LastOnline;
                    else dateTime = null;
                }
                else
                {
                    dateTime = null;
                    isonline = false;
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                dateTime = null;
                isonline = false;
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }


        public ResultCodes GetGroupUsersCount(string address, out int count)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    count = channel.UsersCount;
                }
                else
                {
                    count = 0;
                    return ResultCodes.ServerError;
                }
            }
            catch (Exception ex)
            {
                count = 0;
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }

        public ResultCodes AddChat(string firstuser, string seconduser)
        {
            try
            {
                Channel ch = Program.channels.Where(x =>x.Users.Count == 2 && x.Users.Contains(firstuser) && x.Users.Contains(seconduser)).FirstOrDefault();
                if (ch == null)
                {
                    User first = Program.users.Where(x => x.Username == firstuser).FirstOrDefault();
                    User second = Program.users.Where(x => x.Username == seconduser).FirstOrDefault();
                    if (first != null && second != null)
                    {
                        string[] users = new string[2] { firstuser, seconduser };
                        Chat chat = new Chat(users);
                        first.SubscribeToChannel(chat,true);
                        second.SubscribeToChannel(chat,false);
                        Program.channels.Add(chat);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.ElementIsAlreadyCreated;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success; 
        }

        public ResultCodes AddGroup(string groupname, string username, string address)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel == null)
                {
                    User admin = Program.users.Where(x => x.Username == username).FirstOrDefault();
                    if (admin != null)
                    {
                        Group group = new Group(username,groupname,address);
                        admin.SubscribeToChannel(group, true);
                        group.NotifyUsers(admin.Username);
                        Program.channels.Add(group);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }


        public ResultCodes GetGroupInfo(string address, out string admin, out string groupname)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if(channel != null)
                {
                    Group group = channel as Group;
                    admin = group.AdminName;
                    groupname = group.GroupName;
                }
                else
                {
                    admin = null;
                    groupname = null;
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                admin = null;
                groupname = null;
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes InviteToGroup(string address, string username)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                    if (user != null)
                    {
                        (channel as Group).AddUser(user.Username);
                        user.SubscribeToChannel(channel, false);
                        (channel as Group).NotifyUsers(user.Username);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }

        public ResultCodes RemoveChannel(string username, string address)
        {
            try
            {
                Channel ch = Program.channels.Where(x =>x.Address == address).FirstOrDefault();
                if (ch != null)
                {
                    if (ch.IsGroup && (ch as Group).AdminName == username || !ch.IsGroup)
                    {
                        ch.RemoveAllData();
                        foreach (string name in ch.Users)
                        {
                            User user = Program.users.Where(x => x.Username == name).FirstOrDefault();
                            if (user != null) user.UnSubscribeFromChannel(ch);
                        }
                        Program.channels.Remove(ch);
                    }
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }

        public ResultCodes LeaveGroup(string username, string address)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null && channel.IsGroup)
                {
                    User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                    if(user != null)
                    {
                        (channel as Group).KickUser(username);
                        user.UnSubscribeFromChannel(channel);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes GetGroupUsers(string address, string lastname, out int count, out string[] user)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    count = channel.UsersCount;
                    int index = channel.Users.IndexOf(lastname);
                    if (index == -1) index = 0;
                    if(channel.UsersCount >= 10) user = channel.Users.GetRange(index, 10).ToArray();
                    else user = channel.Users.GetRange(index, channel.UsersCount).ToArray();
                    if (index + 10 < channel.UsersCount) return ResultCodes.InProccess;
                }
                else
                {
                    count = 0;
                    user = null;
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                user = null;
                count = 0;
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }


        public ResultCodes RenameGroup(string address, string newname)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null && channel.IsGroup)
                {
                    (channel as Group).ChangeName(newname);
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes StartCall(string username, string address)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.StartCall(username);
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes GetCall(string address, out bool isstreaming)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    isstreaming = channel.IsCallStarted;
                }
                else
                {
                    isstreaming = false;
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                isstreaming = false;
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes StopCall(string username, string address)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.StopCall();
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes SendAudio(string address, byte[] data, string name)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.AudioCame(data, name);
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes SendImages(string address, byte[] data, bool isend, string name)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    channel.ImageCame(data, isend,name);
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }


        public ResultCodes ConnectToCall(string username, string address)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                    if(user != null)
                    {
                        user.IsConnectedToStream = new KeyValuePair<string, bool>(address, true);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes DisconnectFromCall(string username, string address)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if (channel != null)
                {
                    User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                    if (user != null)
                    {
                        user.IsConnectedToStream = new KeyValuePair<string, bool>(String.Empty, false);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.NoMatching;
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        #endregion

        public ResultCodes SubscribeToUser(string username)
        {
            try
            {
                User us = Program.users.Where(x => x.Username == username).FirstOrDefault();
                if (us != null)
                {
                    us.ConnectToUser();
                }
                else throw new Exception();
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public void Disconnect(string username)
        {
            try
            {
                User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                user.DisconnectFromUser();
            }
            catch(Exception ex)
            {

            }
        }

        #region SendMessages

        public ResultCodes SendMessage(Message msg)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == msg.Address).FirstOrDefault();
                if (channel != null)
                {
                    if (channel.Users.Contains(msg.Username))
                    {
                        channel.SendMessage(msg);
                    }
                    else return ResultCodes.NoMatching;
                }
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes GetTotalNewMessages(string user, string address, out long total)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if(channel != null)
                {
                    User us = Program.users.Where(x => x.Username == user).FirstOrDefault();
                    if (us != null)
                    {
                        if(us.CheckChannel(address)) total = channel.GetNewMessagesCount(user);
                        else
                        {
                            total = 0;
                            return ResultCodes.NoMatching;
                        }
                    }
                    else
                    {
                        total = 0;
                        return ResultCodes.NoMatching;
                    }
                }
                else
                {
                    total = 0;
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                total = 0;
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes GetNewMessages(string username,string address)
        {
            try
            {
                User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                if (user != null)
                {
                    return user.GetNewMessages(address);
                }
            }
            catch (Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes GetInvites(string username,  out string[] address)
        {
            try
            {
                User user = Program.users.Where(x => x.Username == username).FirstOrDefault();
                if (user != null)
                {
                    address = user.Invites.ToArray();
                    if (address.Length > 0)
                    {
                        user.Invites.Clear();
                        user.SaveAllChannels();
                    }
                    else return ResultCodes.NoMatching;
                }
                else
                {
                    address = null;
                    return ResultCodes.NoMatching;
                }

            }
            catch(Exception ex)
            {
                address = null;
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes SendFile(string filename, string address, long position, byte[] file)
        {
            try
            {
                string mode = "Chats";
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
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
                else return ResultCodes.NoMatching;
            }
            catch(Exception ex)
            {
                return ResultCodes.ServerError;
            }
            return ResultCodes.Success;
        }

        public ResultCodes DownloadFile(string filename, string address, long position, out byte[] file)
        {
            try
            {
                Channel channel = Program.channels.Where(x => x.Address == address).FirstOrDefault();
                if(channel != null)
                {
                    string mode = "Chats";
                    if (channel.IsGroup) mode = "Groups";
                    string path = String.Format("ServerData/{0}/{1}/files/{2}",mode,address,filename);
                    using(FileStream fs = new FileStream(path,FileMode.Open,FileAccess.Read))
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
                    return ResultCodes.NoMatching;
                }
            }
            catch(Exception ex)
            {
                file = null;
                return ResultCodes.ServerError;
            }

            return ResultCodes.Success;
        }


        #endregion
    }

}
