﻿using System.Windows;
using System.Windows.Controls;

namespace zombiesnu.DayZeroLauncher.App.Ui
{
	/// <summary>
	///     Interaction logic for SettingsView.xaml
	/// </summary>
	public partial class SettingsView : UserControl
	{
		public SettingsView()
		{
			InitializeComponent();
		}

		protected SettingsViewModel ViewModel
		{
			get { return (SettingsViewModel) DataContext; }
		}

		private void BrowseA2_Click(object sender, RoutedEventArgs e)
		{
			string foundDir = ViewModel.DisplayDirectoryPrompt(Window.GetWindow(Parent), false, ViewModel.Arma2Directory,
				"Locate ArmA2 game directory");
			if (foundDir != null)
				ViewModel.Arma2Directory = foundDir;
		}

		private void BrowseA2OA_Click(object sender, RoutedEventArgs e)
		{
			string foundDir = ViewModel.DisplayDirectoryPrompt(Window.GetWindow(Parent), false, ViewModel.Arma2OADirectory,
				"Locate Operation Arrowhead game directory");
			if (foundDir != null)
				ViewModel.Arma2OADirectory = foundDir;
		}

		private void BrowseAddons_Click(object sender, RoutedEventArgs e)
		{
			string foundDir = ViewModel.DisplayDirectoryPrompt(Window.GetWindow(Parent), true, ViewModel.AddonsDirectory,
				"Directory in which mods will be installed");
			if (foundDir != null)
				ViewModel.AddonsDirectory = foundDir;
		}

		private void Done_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Done();
		}
	}
}