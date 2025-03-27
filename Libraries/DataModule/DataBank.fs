namespace DataModule

open System
open System.IO
open System.Collections.Generic
open System.Linq
open DatabaseModule
open ChannelModule
open ClientModule
open LoggerModule

type DataBank() =
    // Equivalent to C#'s public List<Channel> Channels { get; } = new List<Channel>();
    member val Channels : List<Channel> = new List<Channel>() with get
    // Equivalent to public List<User> Users { get; } = new List<User>();
    member val Users : List<User> = new List<User>() with get

    member this.LoadUsers() =
        // Open a database context and populate the Users list.
        use db = new MessangerDataContext()
        for user in db.Users do
            // Create a new ClientModule.User (similar to clm::User) from the db user
            this.Users.Add(new ClientModule.User(user))

    member this.LoadChannels() =
        try
            // Load chats
            let chats = Directory.GetDirectories("ServerData/Chats/")
            for chat in chats do
                let ch = new Chat(Path.GetFileName(chat))
                // Using List<T>.Find to get matching users by username
                let first = this.Users.Find(fun x -> x.Username = ch.Users.[0])
                let second = this.Users.Find(fun x -> x.Username = ch.Users.[1])
                if first <> null && second <> null then
                    this.Channels.Add(ch)
                    first.SubscribeToChannel(ch, false)
                    second.SubscribeToChannel(ch, false)
                    Logger.Log(ch.Users.[0] + " and " + ch.Users.[1], Logger.MessageType.Usual)
            
            // Load groups
            let groups = Directory.GetDirectories("ServerData/Groups/")
            for group in groups do
                let gr = new Group(Path.GetFileName(group))
                this.Channels.Add(gr)
                for username in gr.Users do
                    let user = this.Users.Find(fun x -> x.Username = username)
                    if user <> null then
                        user.SubscribeToChannel(gr, false)
                Logger.Log(gr.Address + " is loaded", Logger.MessageType.Usual)
        with
        | _ -> ()  // Ignore exceptions as in the original code
