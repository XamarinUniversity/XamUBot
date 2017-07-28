using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Cognitive.LUIS;
using System.Linq;
using XamUBot.Utterances;
using System.Threading;

namespace XamUBot.Dialogs
{
	/// <summary>
	/// Containsbase functionality to simplify common dialog tasks.
	/// </summary>
	[Serializable]
	public abstract class BaseDialog : IDialog<object>
	{
		/// <summary>
		/// "Yes" constant for a yes/no style dialog.
		/// </summary>
		public const string ChoiceYes = "yes";

		/// <summary>
		/// "No" constant for a yes/no style dialog.
		/// </summary>
		public const string ChoiceNo = "no";

		int _pendingDialogId;

		/// <summary>
		/// Creates options for displaying a selection of options.
		/// </summary>
		/// <param name="title">title of the prompt</param>
		/// <param name="numRetries">number of times to ask if user provides an invalid answer (0 = don't ask again)</param>
		/// <param name="buttons">options to display</param>
		/// <returns></returns>
		protected static PromptOptions<string> CreateDefaultPromptOptions(string title, int numRetries, params string[] buttons) => new PromptOptions<string>(prompt: title, retry: "", tooManyAttempts: "", options: new List<string>(buttons), attempts: numRetries, promptStyler: null);

		/// <summary>
		/// Makes the dialog wait for the next incoming message.
		/// </summary>
		/// <param name="context"></param>
		protected void WaitForNextMessage(IDialogContext context)
		{
			context.Wait(OnInnerMessageReceivedAsync);
		}

		public async Task StartAsync(IDialogContext context)
		{
			await OnInitializeAsync(context);
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
			if (activity != null)
			{
				if (Keywords.IsHelpKeyword(activity.Text))
				{
					await OnHelpReceivedAsync(context, activity);
				}
				else
				{
					await OnMessageReceivedAsync(context, activity);
				}
			}
		}

		/// <summary>
		/// Can be overridden to run one time initialization when the dialog is getting constructed.
		/// This does nothing in the base class except waiting for the next incoming message.
		/// </summary>
		/// <param name="context"></param>
		protected virtual Task OnInitializeAsync(IDialogContext context)
		{
			WaitForNextMessage(context);
			// Does nothing in base.
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets called if a message was received. This is a convenience method which removes the IAwaitable and directly delivers the activity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="msgActivity"></param>
		protected abstract Task OnMessageReceivedAsync(IDialogContext context, Activity msgActivity);

		/// <summary>
		/// Gets called if the dialog detected that the user is seeking help.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="msgActivity"></param>
		protected abstract Task OnHelpReceivedAsync(IDialogContext context, Activity msgActivity);

		/// <summary>
		/// Pops the current dialog of the stack and optionally sends a return value back to the previous dialog.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="returnValue"></param>
		protected void PopDialog(IDialogContext context, object returnValue = null)
		{
			context.Done(returnValue);
		}
		

		/// <summary>
		/// Helper method to deal with situations where the bot does not understand input.
		/// The optional message is shown before the picker.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="optionalMessage"></param>
		/// <returns></returns>
		protected async Task ShowDefaultNotUnderstoodPicker(IDialogContext context, string optionalMessage = null)
		{
			if(!string.IsNullOrWhiteSpace(optionalMessage))
			{
				await context.PostAsync(optionalMessage);
			}

			PromptDialog.Choice(
				context,
				OnNotUnderstoodSelected,
				CreateDefaultPromptOptions("Sorry, I did not understand. Are you looking for somehing else?", 1, NotUnderstood_PickerOption_BackToMain, NotUnderstood_PickerOption_GoToQandA, NotUnderstood_PickerOption_KeepTrying));

		}

		async Task OnNotUnderstoodSelected(IDialogContext context, IAwaitable<object> result)
		{
			var selectedChoice = await result.GetValueAsync<string>();
			switch (selectedChoice)
			{
				case NotUnderstood_PickerOption_BackToMain:
					PopToRootDialog(context);
					break;

				case NotUnderstood_PickerOption_GoToQandA:
					PopToRootDialog(context);
					await context.Forward(new QandADialog(), null, (IMessageActivity)context.Activity, CancellationToken.None);
					break;

				case NotUnderstood_PickerOption_KeepTrying:
				default:
					// Just wait for next message.
					await context.PostAsync("Ok!");
					WaitForNextMessage(context);
					break;
			}
		}

		const string NotUnderstood_PickerOption_BackToMain = "Show all options";
		const string NotUnderstood_PickerOption_GoToQandA = "Check Q&A";
		const string NotUnderstood_PickerOption_KeepTrying = "Stay here";

		/// <summary>
		/// Pops to the root dialog.
		/// </summary>
		/// <param name="context"></param>
		protected void PopToRootDialog(IDialogContext context)
		{
			PopDialog(context, InternalMessage_PopToRoot);
		}

		const string InternalMessage_PopToRoot = "InternalMessage_PopToRoot";
	}
}