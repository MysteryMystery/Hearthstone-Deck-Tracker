﻿using System;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal static class HSReplayNetHelper
	{
		static HSReplayNetHelper()
		{
			ConfigWrapper.CollectionSyncingChanged += () => SyncCollection().Forget();
			CollectionHelper.OnCollectionChanged += () => SyncCollection().Forget();
		}

		public static event Action CollectionSynced;
		public static event Action<bool> Authenticating;

		public static async Task TryAuthenticate()
		{
			Authenticating?.Invoke(true);
			if(await HSReplayNetOAuth.Authenticate())
			{
				if(await HSReplayNetOAuth.UpdateAccountData())
					await SyncCollection();
				else
					ErrorManager.AddError("Could not load HSReplay.net account status", "Please try again later.");
			}
			else
				ErrorManager.AddError("Could not authenticate with HSReplay.net", "Please try again later.");
			Authenticating?.Invoke(false);
		}

		public static async Task UpdateAccount()
		{
			await HSReplayNetOAuth.UpdateAccountData();
			if(!Account.Instance.TokenClaimed.HasValue)
				await ApiWrapper.UpdateUploadTokenStatus();
			if(Account.Instance.TokenClaimed == false)
				await HSReplayNetOAuth.ClaimUploadToken(Account.Instance.UploadToken);
		}

		public static async Task SyncCollection()
		{
			if(!Config.Instance.SyncCollection || !HSReplayNetOAuth.IsAuthenticated)
				return;
			var collection = await CollectionHelper.GetCollection();
			if(collection == null)
				return;
			var hash = collection.GetHashCode();
			var account = collection.AccountHi + "-" + collection.AccountLo;
			if(Account.Instance.CollectionState.TryGetValue(account, out var state) && state.Hash == hash)
				return;
			if(await HSReplayNetOAuth.UpdateCollection(collection))
			{
				Account.Instance.CollectionState[account] = new Account.SyncState(hash);
				Account.Save();
				CollectionSynced?.Invoke();
			}
			else
				ErrorManager.AddError("HSReplay.net error", "Could not update your collection. Please try again later.");
		}


	}
}
