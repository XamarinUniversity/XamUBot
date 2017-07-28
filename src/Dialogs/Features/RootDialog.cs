using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class RootDialog : BaseDialog
	{
		bool _firstVisit = true;
				
		protected async override Task OnHelpReceivedAsync(IDialogContext context, Activity msgActivity)
		{
			await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootHelp));
			WaitForNextMessage(context);
		}

		protected async override Task OnMessageReceivedAsync(IDialogContext context, Activity activity)
		{
			switch (activity.Type)
			{
				case ActivityTypes.ConversationUpdate:
					await ShowTopics(context);
					break;

				default:
					WaitForNextMessage(context);
					break;
			}
		}

		async Task ShowTopics(IDialogContext context)
		{
			if (_firstVisit)
			{
				await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.Welcome));
				_firstVisit = false;
			}

			PromptDialog.Choice(
				context,
				OnMainMenuItemSelected,
				CreateDefaultPromptOptions(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt),
				0,
				"Team", "QandA", "Support"));
		}

		async Task OnMainMenuItemSelected(IDialogContext context, IAwaitable<object> result)
		{
			string selectedChoice = await result.GetValueAsync<string>();

			if (selectedChoice == null)
			{
				// User exceeded maximum retries and always provided a value that's not part of the picker.
				// Show main menu picker again.
				await context.PostAsync("Looks like we have a small communication problem. If you need support, please say 'help' or pick one of the available options.");
				await ShowTopics(context);
				return;
			}

			if (selectedChoice.ToLowerInvariant().Contains("team"))
			{
				await context.Forward(new TeamDialog(), OnResumeDialog, null, CancellationToken.None);
			}
			else if (selectedChoice.ToLowerInvariant().Contains("qanda"))
			{
				await context.Forward(new QandADialog(), OnResumeDialog, null, CancellationToken.None);
			}
			else if (selectedChoice.ToLowerInvariant().Contains("support"))
			{
				await context.Forward(new SupportDialog(), OnResumeDialog, null, CancellationToken.None);
			}
			else
			{
				WaitForNextMessage(context);
			}
		}

		async Task OnResumeDialog(IDialogContext context, IAwaitable<object> result)
		{
			await context.PostAsync("You're back to the main menu!");
			await ShowTopics(context);
		}
	}
}