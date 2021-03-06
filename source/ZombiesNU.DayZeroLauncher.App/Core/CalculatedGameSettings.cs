﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace zombiesnu.DayZeroLauncher.App.Core
{
	public class LocatorInfo
	{
		[JsonProperty("installers")] public HashWebClient.RemoteFileInfo Installers = null;
		[JsonProperty("mods")] public HashWebClient.RemoteFileInfo Mods = null;
		[JsonProperty("motd")] public string MotdUrl = null;

		[JsonProperty("patches")] public HashWebClient.RemoteFileInfo Patches = null;
		[JsonProperty("servers")] public string ServerListUrl = null;

		public static LocatorInfo LoadFromString(string jsonText)
		{
			return JsonConvert.DeserializeObject<LocatorInfo>(jsonText);
		}
	}

	public class CalculatedGameSettings : BindableBase
	{
		private static CalculatedGameSettings _current;

		public static CalculatedGameSettings Current
		{
			get
			{
				if (_current == null)
				{
					_current = new CalculatedGameSettings();
					_current.Update();
				}
				return _current;
			}
		}

		public string Arma2Path { get; set; }
		public string Arma2OAPath { get; set; }
		public string AddonsPath { get; set; }
		public GameVersions Versions { get; set; }
		public string ModContentVersion { get; set; }

		public LocatorInfo Locator { get; set; }

		public void Update()
		{
			SetArma2Path();
			SetArma2OAPath();
			SetAddonsPath();
			SetGameVersions();
			SetModContentVersion();
		}

		public void SetArma2Path()
		{
			if (!string.IsNullOrWhiteSpace(UserSettings.Current.GameOptions.Arma2DirectoryOverride))
				Arma2Path = UserSettings.Current.GameOptions.Arma2DirectoryOverride;
			else
				Arma2Path = LocalMachineInfo.Current.Arma2Path;

			PropertyHasChanged("Arma2Path");
		}

		public void SetArma2OAPath()
		{
			if (!string.IsNullOrWhiteSpace(UserSettings.Current.GameOptions.Arma2OADirectoryOverride))
				Arma2OAPath = UserSettings.Current.GameOptions.Arma2OADirectoryOverride;
			else
				Arma2OAPath = LocalMachineInfo.Current.Arma2OAPath;

			PropertyHasChanged("Arma2OAPath");
		}

		public void SetAddonsPath()
		{
			if (!string.IsNullOrWhiteSpace(UserSettings.Current.GameOptions.AddonsDirectoryOverride))
				AddonsPath = UserSettings.Current.GameOptions.AddonsDirectoryOverride;
			else
				AddonsPath = Arma2OAPath;

			PropertyHasChanged("AddonsPath");
		}

		private void SetGameVersions()
		{
			if (!string.IsNullOrEmpty(Arma2OAPath))
				Versions = new GameVersions(Arma2OAPath);
			else
				Versions = null;

			PropertyHasChanged("Versions");
		}

		private void SetModContentVersion()
		{
			try
			{
				ModContentVersion = File.ReadAllText(UserSettings.ContentCurrentTagFile).Trim();
			}
			catch (Exception)
			{
				ModContentVersion = null;
			}

			PropertyHasChanged("ModContentVersion");
		}
	}
}