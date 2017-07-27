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
		bool _firstVisit = true;

		protected async override Task<bool> OnInitializeAsync(IDialogContext context)
		{
			await base.OnInitializeAsync(context);
			await ShowTopics(context);
			return false;
		}

        protected async override Task<bool> OnHelpReceivedAsync(IDialogContext context, Activity msgActivity, int repetitions)
        {
//            await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootHelp));
            return false;
        }

        protected async override Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity activity, int repetitions)
		{
			return true;
			/*
			switch (activity.Type)
			{
				case ActivityTypes.Message:
				case ActivityTypes.ConversationUpdate:
					await ShowTopics(context);
					return false;
				default:
					return true;
			}
			*/
		}

		async Task ShowTopics(IDialogContext context)
		{
			if (_firstVisit)
			{
				await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.Welcome));
				_firstVisit = false;
			}
            ShowPicker(context, (int)PickerIds.MainTopic, ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt), new[] { "Team", "Tracks", "QandA", "Help" });
        }

		protected async override Task<bool> OnPickerSelectedAsync(IDialogContext context, int pickerId, string selectedChoice)
		{
			if (selectedChoice == null)
			{
				// User exceeded maximum retries and always provided a value that's not part of the picker.
				await context.PostAsync("Looks like we have a small communication problem. If you need support, please say 'help' or pick one of the available options.");
				await ShowTopics(context);
				// Never wait for incoming messages when presenting a picker.
				return false;
			}


			//await context.PostAsync("You selected '" + selectedChoice + "'...");

			if (pickerId == (int)PickerIds.MainTopic)
			{
				if (selectedChoice.ToLowerInvariant().Contains("tracks"))
				{
                    await context.PostAsync("Great, let's talk about the classes and tracks available at Xamarin University.");
                    GoToDialog(context, (int)DialogIds.TracksDialog, new TracksDialog());
				}
				else if (selectedChoice.ToLowerInvariant().Contains("team"))
				{
                    await context.PostAsync("Super, let's talk about the awesome team at Xamarin University");
                    GoToDialog(context, (int)DialogIds.TeamDialog, new TeamDialog());
				}
				else if (selectedChoice.ToLowerInvariant().Contains("qanda"))
				{
                    await context.PostAsync("Cool, you want to check out our questions and answers.");
                    GoToDialog(context, (int)DialogIds.QandADialog, new QandADialog());
				}
				else if (selectedChoice.ToLowerInvariant().Contains("support"))
				{
					GoToDialog(context, (int)DialogIds.SupportDialog, new SupportDialog());
				}
                else if (selectedChoice.ToLowerInvariant().Contains("help"))
                {
                    await OnHelpReceivedAsync(context, null, 0);
//                    GoToDialog(context, (int)DialogIds.SupportDialog, new SupportDialog());
                }
            }

            // We're navigating to a new dialog, so don't wait for next message.
            return false;
		}


        protected async override Task<bool> OnGetDialogReturnValueAsync(IDialogContext context, int dialogId, object result)
		{
			return await base.OnGetDialogReturnValueAsync(context, dialogId, result);
		}
	}
}