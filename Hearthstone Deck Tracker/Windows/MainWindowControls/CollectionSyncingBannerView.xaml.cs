﻿using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public partial class CollectionSyncingBannerView : INotifyPropertyChanged
	{
		public CollectionSyncingBannerView()
		{
			InitializeComponent();
			HSReplayNetHelper.CollectionSynced += () => OnPropertyChanged(nameof(CollectionSynced));
			HSReplayNetOAuth.LoggedOut += () => OnPropertyChanged(nameof(CollectionSynced));
		}

		public bool CollectionSynced => HSReplayNetOAuth.IsAuthenticated && Account.Instance.CollectionState.Any();

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
