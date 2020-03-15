using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using OurIdolBot.Database.Models;
using OurIdolBot.Database.Models.DynamicDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Services.RolesServices
{
    public class AssignRolesService
    {
        public List<string> ShowRoles(ulong serverId, IReadOnlyDictionary<ulong, DiscordRole> serverRoles)
        {
            List<string> result = new List<string>();

            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, serverId);

                // If there's no roles, send message and exit.
                if (dbServer.AssignRoles.Count == 0)
                {
                    result.Add("There are no roles on this server that you can assign.");
                    return result;
                }
                // Prepare message.
                result.Add("**The roles available on the server are:**\n");
                // Get server roles.

                List<DiscordRole> discordRoles = new List<DiscordRole>();
                foreach (AssignRole assignRole in dbServer.AssignRoles)
                {
                    discordRoles.Add(serverRoles.Where(p => p.Value.Id.ToString() == assignRole.RoleID).FirstOrDefault().Value);
                }

                List<DiscordRole> sortedRoles = discordRoles.OrderBy(o => o.Name).ToList();
                foreach (DiscordRole sortedRole in sortedRoles)
                {
                    if (sortedRole != null)
                    {
                        result.Add(sortedRole.Name);
                    }
                }
            }
            return result;
        }

        public (string, DiscordRole) AssignRole(ulong serverId, IReadOnlyDictionary<ulong, DiscordRole> serverRoles, DiscordMember member, string message)
        {
            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, serverId);

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Value.Name == message)
                    {
                        // Check if role is already in database
                        if (IsRoleInDatabase(dbServer, serverRole.Value.Id))
                        {
                            // Check if user already has this role
                            if (HasUserRole(member, serverRole.Value))
                            {
                                return ("You already have this role.", null);
                            }
                            return ("Role has been assigned.", serverRole.Value);
                        }
                    }
                }
                return ("Role is not on the list.", null);
            }
        }

        public (string, DiscordRole) RemoveRole(ulong serverId, IReadOnlyDictionary<ulong, DiscordRole> serverRoles, DiscordMember member, string message)
        {
            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, serverId);

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Value.Name == message)
                    {
                        // Check if role is already in database
                        if (IsRoleInDatabase(dbServer, serverRole.Value.Id))
                        {
                            // Check if user already has this role
                            if (!HasUserRole(member, serverRole.Value))
                            {
                                return ("You do not have this role.", null);
                            }

                            return ("Role has been removed.", serverRole.Value);
                        }

                    }
                }
                return ("Role is not on the list.", null);
            }
        }

        public string AddRole(ulong serverId, IReadOnlyDictionary<ulong, DiscordRole> serverRoles, DiscordMember owner, DiscordMember member, string message)
        {
            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, serverId);
                

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Value.Name == message)
                    {
                        // Check if role is already in database
                        if (IsRoleInDatabase(dbServer, serverRole.Value.Id))
                        {
                            return "The role is already on the list.";
                        }

                        // User who triggered is owner, we can add role without problem
                        if (member == owner)
                        {
                            AssignRole assingRole = new AssignRole(serverRole.Value.Id);
                            assingRole.Server = dbServer;
                            databaseContext.Add(assingRole);
                            databaseContext.SaveChanges();
                            return "Role added to the role list.";
                        }
                        // User who triggered isn't owner, we need to check if role is lower than the highest role he has
                        else
                        {
                            var userTheHighestRolePosition = GetTheHighestRolePosition(member.Roles.ToList());
                            // Role is lower than the highest role user has
                            if (serverRole.Value.Position < userTheHighestRolePosition)
                            {
                                AssignRole assingRole = new AssignRole(serverRole.Value.Id);
                                assingRole.Server = dbServer;
                                databaseContext.Add(assingRole);
                                databaseContext.SaveChanges();
                                return "Role added to the role list.";
                            }
                            // Role is equal or higher than the highest role user has
                            else
                            {
                                return "You can not add this role because it is equal or higher than your highest role.";
                            }

                        }
                    }
                }
                return "The given role does not exist.";
            }
        }

        public string DeleteRole(ulong serverId, IReadOnlyDictionary<ulong, DiscordRole> serverRoles, DiscordMember owner, DiscordMember member, string message)
        {
            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, serverId);
               

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Value.Name == message)
                    {

                        // Check if role is already in database
                        if (!IsRoleInDatabase(dbServer, serverRole.Value.Id))
                        {
                            return "The role is not on the list.";
                        }

                        // User who triggered is owner, we can add role without problem
                        if (member == owner)
                        {
                            dbServer.AssignRoles.RemoveAll(p => p.RoleID == serverRole.Value.Id.ToString());
                            databaseContext.SaveChanges();
                            return "Role removed from the role list.";
                        }
                        // User who triggered isn't owner, we need to check if role is lower than the highest role he has
                        else
                        {
                            var userTheHighestRolePosition = GetTheHighestRolePosition(member.Roles.ToList());
                            // Role is lower than the highest role user has
                            if (serverRole.Value.Position < userTheHighestRolePosition)
                            {
                                dbServer.AssignRoles.RemoveAll(p => p.RoleID == serverRole.Value.Id.ToString());
                                databaseContext.SaveChanges();
                                return "Role removed from the role list.";
                            }
                            // Role is equal or higher than the highest role user has
                            else
                            {
                                return "You can not remove this role because it is equal or higher than your highest role.";
                            }

                        }
                    }
                }
                return "The given role does not exist.";
            }
        }

        private Server GetServerFromDatabase(DynamicDBContext databaseContext, ulong GuildId)
        {
            Server dbServer = databaseContext.Servers.Where(p => p.ServerID == GuildId.ToString()).Include(p => p.AssignRoles).FirstOrDefault();

            //If server is not present in database add it.
            if (dbServer == null)
            {
                dbServer = new Server(GuildId);
                dbServer.AssignRoles = new List<AssignRole>();
                databaseContext.Add(dbServer);
                databaseContext.SaveChanges();
            }
            return dbServer;
        }

        private bool HasUserRole(DiscordMember member, DiscordRole role)
        {
            foreach (var memberRole in member.Roles)
            {
                if (memberRole == role)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetTheHighestRolePosition(List<DiscordRole> roles)
        {
            int position = 0;
            foreach (var role in roles)
            {
                if (role.Position > position)
                {
                    position = role.Position;
                }
            }

            return position;
        }

        private bool IsRoleInDatabase(Server server, ulong roleId)
        {
            if (server.AssignRoles == null || server.AssignRoles.Count == 0)
            {
                return false;
            }
            if (server.AssignRoles.FirstOrDefault(p => p.RoleID == roleId.ToString()) == null)
            {
                return false;
            }
            return true;
        }
    }
}
