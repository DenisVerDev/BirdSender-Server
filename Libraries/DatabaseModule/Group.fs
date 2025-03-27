namespace DatabaseModule

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<Table("Groups")>]
type Group() =
    [<Key>]
    [<Column("id")>]
    member val Id : int = 0 with get, set

    [<Required>]
    [<Column("Name")>]
    member val Name : string = null with get, set

    [<Column("Owner")>]
    member val Owner : Nullable<int> = Nullable() with get, set

    [<Column("IsPublic")>]
    member val IsPublic : Nullable<bool> = Nullable() with get, set

    [<Column("CreationDate")>]
    member val CreationDate : Nullable<DateTime> = Nullable() with get, set
