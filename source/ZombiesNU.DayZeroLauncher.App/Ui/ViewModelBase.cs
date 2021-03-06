﻿using zombiesnu.DayZeroLauncher.App.Core;

namespace zombiesnu.DayZeroLauncher.App.Ui
{
	public abstract class ViewModelBase : BindableBase
	{
		private bool _isSelected;
		private string _title;

		protected ViewModelBase()
		{
			App.Events.Subscribe(this);
		}

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				PropertyHasChanged("Title");
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				PropertyHasChanged("IsSelected");
			}
		}
	}
}