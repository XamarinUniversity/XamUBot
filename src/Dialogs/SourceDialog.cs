using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class SourceDialog : IDialog<object>
	{
		public Task StartAsync(IDialogContext context)
		{
			WaitForNextMessage(context);
			return Task.CompletedTask;
		}

		protected void WaitForNextMessage(IDialogContext context)
		{
			context.Wait(OnInnerMessageReceivedAsync);
		}

		protected static PromptOptions<string> CreateDefaultPromptOptions(string title, int numRetries, params string[] buttons)
		{
			return new PromptOptions<string>(
				prompt: title,
				retry: title,
				tooManyAttempts: "",
				options: new List<string>(buttons),
				attempts: numRetries,
				promptStyler: null);
		}

		/// <summary>
		/// Handles messages posted to the dialog.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		async Task OnInnerMessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;

			if (activity.Type == ActivityTypes.Message)
			{

				await context.PostAsync("You said " + activity.Text);

				PromptDialog.Choice(
					context,
					OnChoiceMade,
					CreateDefaultPromptOptions("Select", 1, "Yes", "No"));
			}
			else
			{
				WaitForNextMessage(context);
			}
		}

		async Task OnChoiceMade(IDialogContext context, IAwaitable<object> result)
		{
			var v = await result as string;
			await context.Forward(new TargetDialog(), OnResumeAfterDialog, null);
			WaitForNextMessage(context);
		}

		async Task OnResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
		{
			await context.PostAsync("Back to source dialog.");
		}
	}
}