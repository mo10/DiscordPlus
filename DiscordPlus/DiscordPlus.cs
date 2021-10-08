using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using Discord;
using HarmonyLib;
using ModHelper;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace DiscordPlus
{
    public static class DiscordPlus
    {
		public static List<long> ClientIds = new List<long>()
		{
			820344593357996092L, // 0 - 40
			896044493839679498L, // 41 - 
		};
		public static int ClientSelected = 0;

        public static void DoPatching()
        {
            var harmony = new Harmony("com.github.mo10.discordplus");

            var discord = AccessTools.Constructor(typeof(Discord.Discord),new Type[] { typeof(long), typeof(ulong) });
            var discordPrefix = AccessTools.Method(typeof(DiscordPlus), "DiscordPrefix");
            harmony.Patch(discord, new HarmonyMethod(discordPrefix));

            var setUpdateActivity = AccessTools.Method(typeof(DiscordManager), "SetUpdateActivity", new Type[] { typeof(bool), typeof(string) });
            var setUpdateActivityPrefix = AccessTools.Method(typeof(DiscordPlus), "SetUpdateActivityPrefix");
            harmony.Patch(setUpdateActivity, new HarmonyMethod(setUpdateActivityPrefix));
        }

		public static void DiscordPrefix(ref long clientId, ref ulong flags)
        {
            clientId = ClientIds[ClientSelected];
        }
		
		public static bool SetUpdateActivityPrefix(ref bool isPlaying,ref string levelInfo, ref DiscordManager __instance)
		{
			if (__instance.activityManager == null || __instance.applicationManager == null)
			{
				return false;
			}

			string musicUid = Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>();
			string musicPackage = Singleton<DataManager>.instance["Account"]["SelectedAlbumUid"].GetResult<string>();
			string musicLevel = Singleton<DataManager>.instance["Account"]["SelectedMusicLevel"].GetResult<string>();
			int diffculty = Singleton<DataManager>.instance["Account"]["SelectedDifficulty"].GetResult<int>();
			
			string diffcultyStr = string.Empty;
            switch (diffculty)
            {
				case 1:
					diffcultyStr = Singleton<ConfigManager>.instance.GetConfigStringValue("tip", 0, "diffcultyEasy");
					break;
				case 2:
					diffcultyStr = Singleton<ConfigManager>.instance.GetConfigStringValue("tip", 0, "diffcultyHard");
					break;
				case 3:
					diffcultyStr = Singleton<ConfigManager>.instance.GetConfigStringValue("tip", 0, "diffcultyMaster");
					break;
				default:
					diffcultyStr = "???";
					break;
			}

			string coverName = "random_song_cover";
			Activity activity;
			if (isPlaying)
			{
				// Switch to another discord client id, when music_package id > 40.
				int clientIdx = 0;
				if (int.Parse(musicUid.Split('-')[0]) >= 41)
					clientIdx = 1;
				if (clientIdx != ClientSelected)
				{
					// Need re-init discord sdk
					ModLogger.Debug($"Switch to {clientIdx}");
					ClientSelected = clientIdx;
					ReInitDiscord(__instance);
				}

				// 33-12 39-8 cover is not exist
				if (musicPackage != "music_package_999"
					&& musicUid != "33-12"
					&& musicUid != "39-8")
				{
					coverName = musicUid;
				}

				activity = new Activity
				{
					State = levelInfo,
					Details = $"{diffcultyStr} - Lvl.{musicLevel}",
					Assets = new ActivityAssets()
					{
						LargeImage = coverName,
						LargeText = levelInfo,
						SmallImage = "image_logo",
						SmallText = "Muse Dash",
					}
				};
			}
			else
			{
				activity = new Activity
				{
					State = "In Menu",
					Assets = new ActivityAssets()
					{
						LargeImage = "image_logo",
					}
				};
			}
			// Upadte activity
			__instance.activityManager.UpdateActivity(activity, delegate (Result result)
			{
				if (result != Result.Ok)
				{
					ModLogger.Debug("Discord Update Activity Failed!");
				}
			});
			return false;
		}

		public static void ReInitDiscord(DiscordManager instance)
		{
			// Dispose old instant
			instance.applicationManager = null;
			instance.activityManager = null;
			instance.discord.Dispose();
			// Init Discord SDK
			instance.discord = new Discord.Discord(ClientIds[ClientSelected], 1UL);
			if (instance.discord.IsInit == Result.Ok)
			{
				instance.activityManager = instance.discord.GetActivityManager();
				instance.applicationManager = instance.discord.GetApplicationManager();
			}
		}
	}
}
