using System;
using System.Collections.Generic;
using System.Text;

namespace OurIdolBot.Database.Models.DynamicDB
{
    class Server
    {
        public Server() { }
        public Server(ulong id)
        {
            ServerID = id.ToString();
        }

        public int ID { get; set; }
        public string ServerID { get; set; }
        
        public virtual List<AssignRole> AssignRoles { get; set; }
    }
}
