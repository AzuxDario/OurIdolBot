# Our Idol bot
<p align="center">
<a href="https://travis-ci.org/AzuxDario/OurIdolBot"><img src="https://travis-ci.org/AzuxDario/OurIdolBot.svg?branch=master" alt="Build status"></img></a>
<a href="https://github.com/AzuxDario/OurIdolBot/pulls?q=is%3Apr+is%3Aclosed"><img src="https://img.shields.io/github/issues-pr-closed-raw/AzuxDario/OurIdolBot" alt="Closed pull requests"></img></a>
<a href="https://github.com/AzuxDario/OurIdolBot/blob/master/LICENSE"><img src="https://img.shields.io/github/license/AzuxDario/OurIdolBot" alt="License"></img></a>
</p>

Simple Discord bot which can send  info about currently playing song by AnisonFM, J-Pop Project Radio,  Blue Anime Ivana. The bot can shows info once or auto update it on chosen channel every 15 seconds. Bot also shows links to these radios.

# Origin
Bot was made by me for one of servers where I am. I was asked to write it because bot we use to play radios doesn't shows current song longer.
Feel free to add it to your server if you want.

# Commands
## Music
  * $enableNowPlaying (alias: $enableNP) - Enable auto generated messages about current playing song every 15 seconds.
  * $disableNowPlaying (alias: $disableNP) - Disable auto generated messages about current playing song.
  * $isEnabled - Shows whether auto generated messages are enabled on this channel.
  * $nowPlaying (alias: $np) - Check current playing song.
  * $countUpdatingChannels - Shows on how much channels idols updating information.
## Conversion
  * $kmToMiles - Convert kilometers to miles.
  * $milesToKm - Convert miles to kilometers.
  * $cToF - Convert Celsius to Fahrenheit.
  * $fToC - Convert Fahrenheit to Celsius.
## Roles
  * $showRoles - Shows the roles that you can assign yourself on server.
  * $assignRole - Assign role to you from the role list.
  * $removeRole - Remove role from you from the role list.
  * $addRole - Adds a role to the role list that server members can assign.
  * $deleteRole - Removes a role from the role list that can be assigned by server members.
## Images
  * $8ball - random 8ball images
  * $avatar - Shows avatar of user.
  * $baka - random baka images
  * $catgirl - random catgirl images
  * $cuddle - random cuddle images
  * $feed - random feed images
  * $hug - random hug images
  * $inu - random inu images
  * $kiss - random kiss images
  * $kitsune - random kitsune images
  * $lizard - random lizard images
  * $neko - random neko images
  * $pat - random pat images
  * $pfp - random pfp images
  * $poke - random poke images
  * $slap - random slap images
  * $smug - random smug images
  * $tickle - random tickle images
  * $wallpaper - random wallpaper images
## Help
  * $help - Shows help.
  * $help command - Shows help about specific command.
## Special
  * $ping - Check ping
 
# Invitation link
 https://discordapp.com/oauth2/authorize?&client_id=446754812177022998&scope=bot&permissions=268504064

# Used libraries
  * [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus)
  * [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
  * [WebSocket4Net](https://github.com/kerryjiang/WebSocket4Net)

# Running
If you want to run this bot you need to do following steps.
## API keys
You need to create 2 files 'release.json' and 'debug.json' in main directory, using 'config.example.json' as template. These files are used to run in release and debug mode respectively. They contains Discord Bot token, id of creator, and prefix for commands.
## Database
You need to create file called 'DynamicDatabase.sqlite' in main directory, using 'DynamicDatabase.example.sqlite' as template.