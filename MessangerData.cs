﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Data.Entity;

namespace MessangerServer
{

    public class MessangerData : DbContext
    {
        public MessangerData():base("UsersEntities")
        {

        }

        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
    }

}
