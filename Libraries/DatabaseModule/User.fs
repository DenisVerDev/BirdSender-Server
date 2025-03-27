namespace DatabaseModule

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<Table("Users")>]
type User() =
    [<Key>]
    [<Column("id")>]
    member val Id : int = 0 with get, set

    [<Column("Username")>]
    member val Username : string = null with get, set

    [<Column("Password")>]
    member val Password : string = null with get, set

    [<Column("Email")>]
    member val Email : string = null with get, set

    [<Column("CreationDate")>]
    member val CreationDate : Nullable<DateTime> = Nullable() with get, set

    [<Column("LastOnline")>]
    member val LastOnline : Nullable<DateTime> = Nullable() with get, set
