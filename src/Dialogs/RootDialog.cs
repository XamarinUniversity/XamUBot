﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Collections.Generic;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class RootDialog : BaseDialog
	{
		enum PickerIds
		{
			Topic
		}

		protected async override Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity activity)
		{
			switch (activity.Type)
			{
				case ActivityTypes.Message:
				case ActivityTypes.ConversationUpdate:
					ShowTopics(context);
					return false;
				
				default:
					return true;
			}
		}
		
		void ShowTopics(IDialogContext context)
		{
			ShowPicker(context, (int)PickerIds.Topic, "What would you like to know? Note: you can always say 'panic' if you are lost.", new[] { "Team", "Tracks", "Test LUIS", "Cards", "QandA" });
		}

		protected async override Task<bool> OnPickerSelectedAsync(IDialogContext context, int pickerId, string selectedChoice)
		{
			await context.PostAsync("You selected '" + selectedChoice + "'...");

			if (pickerId == (int)PickerIds.Topic)
			{
				if (selectedChoice.ToLowerInvariant().Contains("tracks"))
				{
					context.Call(new TracksDialog(), null);
				}
				else if (selectedChoice.ToLowerInvariant().Contains("team"))
				{
					context.Call(new TeamDialog(), null);
				}
				else if (selectedChoice.ToLowerInvariant().Contains("test luis"))
				{
					context.Call(new TestLuisDialog(), null);
				}
				else if (selectedChoice.ToLowerInvariant().Contains("cards"))
				{
					context.Call(new TestCardsDialog(), null);
				}
				else if (selectedChoice.ToLowerInvariant().Contains("qanda"))
				{
					context.Call(new TestQandADialog(), null);
				}

				return false;
			}
			else
			{
				await context.PostAsync($"Unfortunately I cannot help you with that.");
				ShowTopics(context);
				return true;
			}
		}
	}
}