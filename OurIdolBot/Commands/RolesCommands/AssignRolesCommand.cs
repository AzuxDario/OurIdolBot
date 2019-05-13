using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using OurIdolBot.Attributes;
using OurIdolBot.Database.Models;
using OurIdolBot.Database.Models.DynamicDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.RolesCommands
{
    [CommandsGroup("Roles")]
    class AssignRolesCommand : BaseCommandModule
    {
        [Command("showRoles")]
        [Description("Shows the roles that you can assign yourself on this server.")]
        public async Task ShowRoles(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, ctx.Guild.Id);

                // If there's no roles, send message and exit.
                if (dbServer.AssignRoles.Count == 0)
                {
                    await ctx.RespondAsync("There are no roles on this server that you can assign.");
                    return;
                }
                // Prepare message.
                string message = "**The roles available on the server are:**\n";
                // Get server roles.
                var serverRoles = ctx.Guild.Roles;

                List<DiscordRole> discordRoles = new List<DiscordRole>();
                foreach (AssignRole assignRole in dbServer.AssignRoles)
                {
                    discordRoles.Add(serverRoles.Where(p => p.Id.ToString() == assignRole.RoleID).FirstOrDefault());

                }

                List<DiscordRole> sortedRoles = discordRoles.OrderBy(o => o.Name).ToList();
                foreach (DiscordRole sortedRole in sortedRoles)
                {
                    if (sortedRole != null)
                    {
                        if (sortedRole != sortedRoles[0])
                        {
                            message += ", ";
                        }
                        message += sortedRole.Name;
                    }
                    if (message.Length > 1800)
                    {
                        await ctx.RespondAsync(message);
                        message = "";
                    }
                }
                if (message.Length > 0)
                {
                    await ctx.RespondAsync(message);
                }
            }
        }

        [Command("assignRole")]
        [Description("Assign role to you from the role list.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.None)]
        public async Task AssignRole(CommandContext ctx, params string[] message)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, ctx.Guild.Id);

                var serverRoles = ctx.Guild.Roles;

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Name == ctx.RawArgumentString)
                    {
                        // Check if role is already in database
                        if (IsRoleInDatabase(dbServer, serverRole.Id))
                        {
                            // Check if user already has this role
                            if (HasUserRole(ctx.Member, serverRole))
                            {
                                await ctx.RespondAsync("You already have this role.");
                                return;
                            }

                            // User who triggered is owner, we can add role without problem
                            if (ctx.User == ctx.Guild.Owner)
                            {
                                await ctx.Member.GrantRoleAsync(serverRole, "Role has been assigned by bot using assign role system. Action has been triggered by user.");
                                await ctx.RespondAsync("Role has been assigned.");
                            }
                            // User who triggered isn't owner, we need to check if role is lower than the highest role he has
                            else
                            {
                                var userTheHighestRolePosition = GetTheHighestRolePosition(ctx.Member.Roles.ToList());
                                // Role is lower than the highest role user has
                                if (serverRole.Position < userTheHighestRolePosition)
                                {
                                    await ctx.Member.GrantRoleAsync(serverRole, "Role has been assigned by bot using assign role system. Action has been triggered by user.");
                                    await ctx.RespondAsync("Role has been assigned.");
                                }
                                // Role is equal or higher than the highest role user has
                                else
                                {
                                    await ctx.RespondAsync("You can not give yourself this role because it is equal or higher than your highest role.");
                                }

                            }
                            return;
                        }

                    }
                }
                await ctx.RespondAsync("Role is not on the list.");
            }

        }

        [Command("removeRole")]
        [Description("Remove role from you from the role list.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.None)]
        public async Task RemoveRole(CommandContext ctx, params string[] message)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, ctx.Guild.Id);

                var serverRoles = ctx.Guild.Roles;

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Name == ctx.RawArgumentString)
                    {
                        // Check if role is already in database
                        if (IsRoleInDatabase(dbServer, serverRole.Id))
                        {
                            // Check if user already has this role
                            if (!HasUserRole(ctx.Member, serverRole))
                            {
                                await ctx.RespondAsync("You do not have this role.");
                                return;
                            }

                            // User who triggered is owner, we can add role without problem
                            if (ctx.User == ctx.Guild.Owner)
                            {
                                await ctx.Member.RevokeRoleAsync(serverRole, "Role has been removed by bot using assign role system. Action has been triggered by user.");
                                await ctx.RespondAsync("Role has been removed.");
                            }
                            // User who triggered isn't owner, we need to check if role is lower than the highest role he has
                            else
                            {
                                var userTheHighestRolePosition = GetTheHighestRolePosition(ctx.Member.Roles.ToList());
                                // Role is lower than the highest role user has
                                if (serverRole.Position < userTheHighestRolePosition)
                                {
                                    await ctx.Member.RevokeRoleAsync(serverRole, "Role has been removed by bot using assign role system. Action has been triggered by user.");
                                    await ctx.RespondAsync("Role has been removed.");
                                }
                                // Role is the highest role user has
                                else
                                {
                                    await ctx.RespondAsync("You can not remove this role because it's your highest role.");
                                }

                            }
                            return;
                        }

                    }
                }
                await ctx.RespondAsync("Role is not on the list.");
            }

        }

        [Command("addRole")]
        [Description("Adds a role to the role list that server members can assign.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddRole(CommandContext ctx, params string[] message)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, ctx.Guild.Id);

                // Get server roles.
                var serverRoles = ctx.Guild.Roles;

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Name == ctx.RawArgumentString)
                    {
                        // Check if role is already in database
                        if (IsRoleInDatabase(dbServer, serverRole.Id))
                        {
                            await ctx.RespondAsync("The role is already on the list.");
                            return;
                        }

                        // User who triggered is owner, we can add role without problem
                        if (ctx.User == ctx.Guild.Owner)
                        {
                            AssignRole assingRole = new AssignRole(serverRole.Id);
                            assingRole.Server = dbServer;
                            databaseContext.Add(assingRole);
                            databaseContext.SaveChanges();
                            await ctx.RespondAsync("Role added to the role list.");
                        }
                        // User who triggered isn't owner, we need to check if role is lower than the highest role he has
                        else
                        {
                            var userTheHighestRolePosition = GetTheHighestRolePosition(ctx.Member.Roles.ToList());
                            // Role is lower than the highest role user has
                            if (serverRole.Position < userTheHighestRolePosition)
                            {
                                AssignRole assingRole = new AssignRole(serverRole.Id);
                                assingRole.Server = dbServer;
                                databaseContext.Add(assingRole);
                                databaseContext.SaveChanges();
                                await ctx.RespondAsync("Role added to the role list.");
                            }
                            // Role is equal or higher than the highest role user has
                            else
                            {
                                await ctx.RespondAsync("You can not add this role because it is equal or higher than your highest role.");
                            }

                        }

                        return;
                    }
                }
                await ctx.RespondAsync("The given role does not exist.");
            }
        }

        [Command("deleteRole")]
        [Description("Removes a role from the role list that can be assigned by server members.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task DeleteRole(CommandContext ctx, params string[] message)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = GetServerFromDatabase(databaseContext, ctx.Guild.Id);

                // Get server roles.
                var serverRoles = ctx.Guild.Roles;

                foreach (var serverRole in serverRoles)
                {
                    if (serverRole.Name == ctx.RawArgumentString)
                    {

                        // Check if role is already in database
                        if (!IsRoleInDatabase(dbServer, serverRole.Id))
                        {
                            await ctx.RespondAsync("Roli is not on the list.");
                            return;
                        }

                        // User who triggered is owner, we can add role without problem
                        if (ctx.User == ctx.Guild.Owner)
                        {
                            dbServer.AssignRoles.RemoveAll(p => p.RoleID == serverRole.Id.ToString());
                            databaseContext.SaveChanges();
                            await ctx.RespondAsync("Role removed from the role list.");
                        }
                        // User who triggered isn't owner, we need to check if role is lower than the highest role he has
                        else
                        {
                            var userTheHighestRolePosition = GetTheHighestRolePosition(ctx.Member.Roles.ToList());
                            // Role is lower than the highest role user has
                            if (serverRole.Position < userTheHighestRolePosition)
                            {
                                dbServer.AssignRoles.RemoveAll(p => p.RoleID == serverRole.Id.ToString());
                                databaseContext.SaveChanges();
                                await ctx.RespondAsync("Role removed from the role list.");
                            }
                            // Role is equal or higher than the highest role user has
                            else
                            {
                                await ctx.RespondAsync("You can not remove this role because it is equal or higher than your highest role.");
                            }

                        }

                        return;
                    }
                }
                await ctx.RespondAsync("The given role does not exist.");
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
