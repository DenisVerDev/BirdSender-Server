namespace DatabaseModule

open System.Data.Entity

type MessangerDataContext() =
    inherit DbContext("MessangerDataSource")

    [<DefaultValue>]
    val mutable users : DbSet<User>
    member this.Users
        with get() = this.users
        and set v = this.users <- v

    [<DefaultValue>]
    val mutable groups : DbSet<Group>
    member this.Groups
        with get() = this.groups
        and set v = this.groups <- v
