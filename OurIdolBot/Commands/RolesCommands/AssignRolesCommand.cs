using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using OurIdolBot.Attributes;
using OurIdolBot.Database.Models;
using OurIdolBot.Database.Models.DynamicDB;
using OurIdolBot.Helpers;
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

            var assignRoles = _assignRolesService.GetRoles(ctx.Guild.Id);

            if (assignRoles.Count == 0)
            {
                await ctx.RespondAsync("There are no roles on this server that you can assign.");
            }
            else
            {
                // Get server roles.
                var serverRoles = ctx.Guild.Roles;

                List<DiscordRole> discordRoles = new List<DiscordRole>();
                foreach (ulong roleId in assignRoles)
                {
                    discordRoles.Add(serverRoles.Where(p => p.Value.Id == roleId).FirstOrDefault().Value);
                }

                List<DiscordRole> sortedRoles = discordRoles.OrderBy(o => o.Name).ToList();
                await PostLongMessageHelper.PostLongMessage(ctx, sortedRoles.Select(p => p.Name).ToList(), "**The roles available on the server are:**\n", ", ");
            }
        }

        [Command("assignRole")]
        [Description("Assign role to you from the role list.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.None)]
        public async Task GiveRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            var serverRoles = ctx.Guild.Roles;
            var role = serverRoles.Select(p => p).Where(q => q.Value.Name == message).FirstOrDefault();

            if (role.Value == null)
            {
                await ctx.RespondAsync("Given role doesn't exist.");
                return;
            }

            if (HasUserRole(ctx.Member, role.Value))
            {
                await ctx.RespondAsync("You already have this role.");
                return;
            }

            if (_assignRolesService.IsRoleOnList(role.Value.Id))
            {
                if (!CanBotModifyThisRole(role.Value, ctx.Guild.CurrentMember.Roles.ToList()))
                {
                    await ctx.RespondAsync("My roles are too low for me to give this role.");
                    return;
                }

                await ctx.Member.GrantRoleAsync(role.Value, "Role has been assigned by bot using assign role system. Action has been triggered by user.");
                await ctx.RespondAsync("Role has been assigned.");
            }
            else
            {
                await ctx.RespondAsync("Role is not on the list.");
            }
        }

        [Command("removeRole")]
        [Description("Remove role from you from the role list.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.None)]
        public async Task RemoveRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            var serverRoles = ctx.Guild.Roles;
            var role = serverRoles.Select(p => p).Where(q => q.Value.Name == message).FirstOrDefault();

            if (role.Value == null)
            {
                await ctx.RespondAsync("Given role doesn't exist.");
                return;
            }

            if (!HasUserRole(ctx.Member, role.Value))
            {
                await ctx.RespondAsync("You do not have this role.");
                return;
            }

            if (_assignRolesService.IsRoleOnList(role.Value.Id))
            {
                if (!CanBotModifyThisRole(role.Value, ctx.Guild.CurrentMember.Roles.ToList()))
                {
                    await ctx.RespondAsync("My roles are too low for me to remove this role.");
                    return;
                }
                await ctx.Member.RevokeRoleAsync(role.Value, "Role has been removed by bot using assign role system. Action has been triggered by user.");
                await ctx.RespondAsync("Role has been removed.");
            }
            else
            {
                await ctx.RespondAsync("Role is not on the list.");
            }
        }

        [Command("addRole")]
        [Description("Adds a role to the role list that server members can assign.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            var serverRoles = ctx.Guild.Roles;
            var role = serverRoles.Select(p => p).Where(q => q.Value.Name == message).FirstOrDefault();

            if (role.Value == null)
            {
                await ctx.RespondAsync("Given role doesn't exist.");
                return;
            }

            if (_assignRolesService.IsRoleOnList(role.Value.Id))
            {
                await ctx.RespondAsync("The role is already on the list.");
                return;
            }

            // User who triggered is owner, we can add role without problem or user who triggered isn't owner, we need to check if role is lower than the highest role he has
            var userTheHighestRolePosition = GetTheHighestRolePosition(ctx.Member.Roles.ToList());
            if (ctx.User == ctx.Guild.Owner || role.Value.Position < userTheHighestRolePosition)
            {
                // Add role to database
                _assignRolesService.AddRoleToDatabase(ctx.Guild.Id, role.Value.Id);
                await ctx.RespondAsync("Role added to the role list.");
            }
            else
            {
                await ctx.RespondAsync("You can not add this role because it is equal or higher than your highest role.");
            }
        }

        [Command("deleteRole")]
        [Description("Removes a role from the role list that can be assigned by server members.")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task DeleteRole(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            var serverRoles = ctx.Guild.Roles;
            var role = serverRoles.Select(p => p).Where(q => q.Value.Name == message).FirstOrDefault();

            if (role.Value == null)
            {
                await ctx.RespondAsync("The given role does not exist.");
                return;
            }

            if (!_assignRolesService.IsRoleOnList(role.Value.Id))
            {
                await ctx.RespondAsync("The role is not on the list.");
                return;
            }

            // User who triggered is owner, we can add role without problem or user who triggered isn't owner, we need to check if role is lower than the highest role he has
            var userTheHighestRolePosition = GetTheHighestRolePosition(ctx.Member.Roles.ToList());
            if (ctx.User == ctx.Guild.Owner || role.Value.Position < userTheHighestRolePosition)
            {
                // Add role to database
                _assignRolesService.RemoveRoleFromDatabase(ctx.Guild.Id, role.Value.Id);
                await ctx.RespondAsync("Role removed from the role list.");
            }
            else
            {
                await ctx.RespondAsync("You can not remove this role because it is equal or higher than your highest role.");
            }
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

        private bool CanBotModifyThisRole(DiscordRole role, List<DiscordRole> botRoles)
        {
            int highestBotRole = GetTheHighestRolePosition(botRoles);
            if (role.Position < highestBotRole)
            {
                return true;
            }
            else
            {
                return false;
            }
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
    }
}
