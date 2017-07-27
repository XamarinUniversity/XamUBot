using System;
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
				
		protected async override Task<bool> OnHelpReceivedAsync(IDialogContext context, Activity msgActivity, int repetitions)
		{
			await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootHelp));
			return true;
		}

		protected async override Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity activity, int repetitions)
		{
			switch (activity.Type)
			{
				case ActivityTypes.ConversationUpdate:
					await ShowTopics(context);
					return false;
				default:
					return true;
			}
		}

		async Task ShowTopics(IDialogContext context)
		{
			if (_firstVisit)
			{
				await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.Welcome));
				_firstVisit = false;
			}

			// Workaround for an issue in bot framework that prevents using a picker when starting the conversation.
			var rootPickerDialog = new CustomPromptDialog(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt), "Team", "QandA", "Support");
			PushDialog(context, (int)DialogIds.RootPickerDialog, rootPickerDialog);
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
				if (selectedChoice.ToLowerInvariant().Contains("team"))
				{
					PushDialog(context, (int)DialogIds.TeamDialog, new TeamDialog());
				}
				else if (selectedChoice.ToLowerInvariant().Contains("qanda"))
				{
					PushDialog(context, (int)DialogIds.QandADialog, new QandADialog());
				}
				else if (selectedChoice.ToLowerInvariant().Contains("support"))
				{
					PushDialog(context, (int)DialogIds.SupportDialog, new SupportDialog());
				}
			}

			// We're navigating to a new dialog, so don't wait for next message.
			return false;
		}


		protected async override Task<bool> OnGetDialogReturnValueAsync(IDialogContext context, int dialogId, object result)
		{
			await base.OnGetDialogReturnValueAsync(context, dialogId, result);

			if(dialogId == (int)DialogIds.RootPickerDialog)
			{
				await OnPickerSelectedAsync(context, (int)PickerIds.MainTopic, (string)result);
				return false;
			}

			return true;
		}
	}
}