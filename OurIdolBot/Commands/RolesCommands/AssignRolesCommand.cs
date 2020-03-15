using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using OurIdolBot.Attributes;
using OurIdolBot.Database.Models;
using OurIdolBot.Database.Models.DynamicDB;
using OurIdolBot.Services.RolesServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.RolesCommands
{
    [CommandsGroup("Roles")]
    public class AssignRolesCommand : BaseCommandModule
    {
        private readonly AssignRolesService _assignRolesService;

        public AssignRolesCommand(AssignRolesService assignRolesService)
        {
            _assignRolesService = assignRolesService;
        }

        [Command("showRoles")]
        [Description("Shows the roles that you can assign yourself on this server.")]
        public async Task ShowRoles(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var message = _assignRolesService.ShowRoles(ctx.Guild.Id, ctx.Guild.Roles);
            StringBuilder messagePart = new StringBuilder();
            for(int i = 0; i < message.Count; i++)
            {
                if(messagePart.Length > 1800)
                {
                    await ctx.RespondAsync(messagePart.ToString());
                    messagePart.Clear(); ;
                }

                if(i == 0 || i == 1)
                {
                    messagePart.Append(message[i]);
                }
                else
                {
                    messagePart.Append(", ");
                    messagePart.Append(message[i]);
                }
            }
            if (messagePart.Length > 0)
            {
                await ctx.RespondAsync(messagePart.ToString());
            }
        }

        [Command("assignRole")]
        [Description("Assign role to you from the role list.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.None)]
        public async Task AssignRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();
            var result = _assignRolesService.AssignRole(ctx.Guild.Id, ctx.Guild.Roles, ctx.Member, message);
            await ctx.RespondAsync(result.Item1);
            if(result.Item2 != null)
            {
                await ctx.Member.GrantRoleAsync(result.Item2, "Role has been assigned by bot using assign role system. Action has been triggered by user.");
            }
        }

        [Command("removeRole")]
        [Description("Remove role from you from the role list.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.None)]
        public async Task RemoveRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();
            var result = _assignRolesService.RemoveRole(ctx.Guild.Id, ctx.Guild.Roles, ctx.Member, message);
            await ctx.RespondAsync(result.Item1);
            if (result.Item2 != null)
            {
                await ctx.Member.GrantRoleAsync(result.Item2, "Role has been removed by bot using assign role system. Action has been triggered by user.");
            }
        }

        [Command("addRole")]
        [Description("Adds a role to the role list that server members can assign.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();
            var result = _assignRolesService.AddRole(ctx.Guild.Id, ctx.Guild.Roles, ctx.Guild.Owner, ctx.Member, message);
            await ctx.RespondAsync(result);
        }

        [Command("deleteRole")]
        [Description("Removes a role from the role list that can be assigned by server members.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task DeleteRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();
            var result = _assignRolesService.DeleteRole(ctx.Guild.Id, ctx.Guild.Roles, ctx.Guild.Owner, ctx.Member, message);
            await ctx.RespondAsync(result);
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
