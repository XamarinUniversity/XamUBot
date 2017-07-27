using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Cognitive.LUIS;
using System.Configuration;
using System.Linq;
using XamUBot.Utterances;

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
		int _pendingPickerId;

		// Contains the last query (incoming message of the user)
		string _lastQuery;
		// Keeps track how many times the same query came in.
		int _lastQueryRepetitions = 0;

		// Keeps track how many times teh bot did not know an answer.
		int _notUnderstoodCounter = 0;

		public async Task StartAsync(IDialogContext context)
		{
			var waitForMessage = await OnInitializeAsync(context);
			if (waitForMessage)
			{
				context.Wait(OnInnerMessageReceivedAsync);
			}
		}

		async Task OnInnerMessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			bool waitForNextMessage = true;

			var activity = await result as Activity;
			if (activity != null)
			{
				if (activity.Text?.Trim().ToLowerInvariant() == _lastQuery)
				{
					_lastQueryRepetitions++;
				}
				else
				{
					_lastQueryRepetitions = 0;
					_lastQuery = activity.Text?.Trim()?.ToLowerInvariant();
				}

				if (Keywords.IsHelpKeyword(activity.Text))
				{
					waitForNextMessage = await OnHelpReceivedAsync(context, activity, _lastQueryRepetitions);
				}
				else
				{
					waitForNextMessage = await OnMessageReceivedAsync(context, activity, _lastQueryRepetitions);
				}
			}

			// Wait for next message.
			if (waitForNextMessage)
			{
				context.Wait(OnInnerMessageReceivedAsync);
			}
		}

		/// <summary>
		/// Can be overridden to run one time initialization when the dialog is getting constructed.
		/// This does nothing in the base class.
		/// </summary>
		/// <param name="context"></param>
		protected async virtual Task<bool> OnInitializeAsync(IDialogContext context)
		{
			// Does nothing in base.
			return true;
		}

		/// <summary>
		/// Gets called if a message was received. This is a convenience method which removes the IAwaitable and directly delivers the activity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="msgActivity"></param>
		/// <param name="repetitions">indicates how often the same message has been sent before by the user</param>
		/// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
		protected abstract Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity msgActivity, int repetitions);

		/// <summary>
		/// Gets called if the dialog detected that the user is seeking help.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="msgActivity"></param>
		/// <param name="repetitions"></param>
		/// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
		protected abstract Task<bool> OnHelpReceivedAsync(IDialogContext context, Activity msgActivity, int repetitions);

        /// <summary>
        /// Sends a picker with arbitrary choices to the client. Override OnChoiceMadeAsync to react to the result of the picker.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="title"></param>
		/// <param name="pickerId"></param>
		/// <param name="attempts"></param>
        /// <param name="buttons"></param>
        protected void ShowPicker(IDialogContext context, int pickerId, string title, int attempts, params string[] buttons)
		{
			_pendingPickerId = pickerId;
			var promptOptions = new PromptOptions<string>(prompt: title, retry: "", tooManyAttempts: "", options: new List<string>(buttons), attempts: attempts, promptStyler: null);
			PromptDialog.Choice(context, OnInnerPickerSelectedAsync, promptOptions);
		}

		/// <summary>
		/// Sends a yes/no picker to the client. Override <see cref="OnPickerSelectedAsync"/> to react to the result of the picker.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		/// <param name="pickerId">arbitrary ID which will be provided to you when getting the results of the picker</param>
		/// <param name="attempts">number of attemps before giving up if user provides an invalid answer</param>
		protected void ShowPicker(IDialogContext context, int pickerId, string title)
		{
			_pendingPickerId = pickerId;
			var promptOptions = new PromptOptions<string>(prompt: title, retry: "", tooManyAttempts: "", attempts: 1, promptStyler: null);
			PromptDialog.Confirm(context, OnInnerPickerSelectedAsync, promptOptions);
		}

		/// <summary>
		/// Internal handler for picker with arbitrary items.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		async Task OnInnerPickerSelectedAsync(IDialogContext context, IAwaitable<object> result)
		{
			object selectedItem = null;

			try
			{
				selectedItem = await result;
			}
			catch (TooManyAttemptsException)
			{
			}

			if(context.Activity.Type != ActivityTypes.Message)
			{
				//context.Wait(OnInnerMessageReceivedAsync);
				return;
			}

			bool waitForNextMessage = await OnPickerSelectedAsync(context, _pendingPickerId, selectedItem as string);
			if (waitForNextMessage)
			{
				context.Wait(OnInnerMessageReceivedAsync);
			}
		}

		/// <summary>
		/// Internal handler for picker with yes/no items.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		async Task OnInnerPickerSelectedAsync(IDialogContext context, IAwaitable<bool> result)
		{
			bool selectedItem = false;

			try
			{
				selectedItem = await result;
			}
			catch (TooManyAttemptsException)
			{
			}

			bool waitForNextMessage = await OnPickerSelectedAsync(context, _pendingPickerId, selectedItem ? ChoiceYes : ChoiceNo);
			if (waitForNextMessage)
			{
				context.Wait(OnInnerMessageReceivedAsync);
			}
		}

		/// <summary>
		/// Gets called after a choice has been made in a a picker.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="selectedChoice">the title of the selected item. For yes/no picker, user BaseDialog.ConstantYes and BaseDialog.ConstantNo. If this is NULL, the user has entered
		/// and invalid value which can happen if they exceed the number of retry attempts.</param>
		/// <returns>return TRUE if you want the dialog to wait for the next message, otherwise FALSE</returns>
		protected async virtual Task<bool> OnPickerSelectedAsync(IDialogContext context, int pickerId, string selectedChoice)
		{
			if (pickerId == (int)PickerIds.NotUnderstoodMultipleTimes)
			{
				switch(selectedChoice)
				{
					case NotUnderstood_PickerOption_BackToMain:
						PopToRootDialog(context);
						return false;

					case NotUnderstood_PickerOption_GoToQandA:
						PushDialog(context, (int)DialogIds.QandADialog, new QandADialog());
						return false;

					case NotUnderstood_PickerOption_KeepTrying:
						// Just wait for next message.
						await context.PostAsync("Ok, let's try that again!");
						return true;

					default:
						// In doubt: back to start.
						PopToRootDialog(context);
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Navigates to a different dialog. Does not forward the current message to the next dialog but instead expects input form user
		/// or needs the called dialog to proactively output something.
		/// Optional return values can be picked up in OnGetDialogReturnValueAsync.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="dialogId"></param>
		/// <param name="dialog"></param>
		protected void PushDialog(IDialogContext context, int dialogId, IDialog<object> dialog)
		{
			_pendingDialogId = dialogId;
			context.Call(dialog, OnInnerResumeDialogAsync);
		}

		/// <summary>
		/// Navigates to a different dialog. Sends the current message to the next dialog which will then process it immediately.
		/// Optional return values can be picked up in OnGetDialogReturnValueAsync.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="dialogId"></param>
		/// <param name="dialog"></param>
		protected void PushDialog(IDialogContext context, IMessageActivity messageActivity, int dialogId, IDialog<object> dialog)
		{
			_pendingDialogId = dialogId;
			context.Forward(dialog, OnInnerResumeDialogAsync, messageActivity);
		}

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
		/// Helper to handle state of dialogs stack.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		async Task OnInnerResumeDialogAsync(IDialogContext context, IAwaitable<object> result)
		{
			var actualResult = await result;
			string internalReturnValue = (string)actualResult;
			if(internalReturnValue != null && internalReturnValue == InternalMessage_PopToRoot)
			{
				context.Done(internalReturnValue);
				return;
			}

			bool waitForNextMessage = await OnGetDialogReturnValueAsync(context, _pendingDialogId, actualResult);
			if (waitForNextMessage)
			{
				context.Wait(OnInnerMessageReceivedAsync);
			}
		}

		/// <summary>
		/// Override to pick up return value of a dialog. Base implementation does nothing.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="dialogId"></param>
		/// <param name="result"></param>
		/// <returns>return TRUE if you want the dialog to wait for the next message, otherwise FALSE</returns>
		protected virtual Task<bool> OnGetDialogReturnValueAsync(IDialogContext context, int dialogId, object result)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Performs a call to LUIS to get back the best matching intent.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="intentPrefix">use to filter the intents</param>
		/// <param name="minimumScore">require the found itent to have a certain minimum score. If below threshold, NULL will be returned for the top scoring intent</param>
		/// <returns></returns>
		protected async Task<LuisResult> PredictLuisAsync(string message, string intentPrefix = null, float minimumScore = 0.6f)
		{
			if(string.IsNullOrWhiteSpace(message))
			{
				return null;
			}

			// Reference:
			// https://github.com/Microsoft/Cognitive-LUIS-Windows

			// TODO: Move config settings to web.config
			var lc = new LuisClient(
				appId: "e9412ee5-9529-42fa-bc5f-ae25069e3b40",
				// Bootstrap key: 4f7be0062bdc4ccc91240323a99992dc
				appKey: "4f7be0062bdc4ccc91240323a99992dc") ;

			LuisResult luisResult = null;

			try
			{
				luisResult = await lc.Predict(message);
			}
			catch (Exception ex)
			{
#if DEBUG
				throw;
#endif
				Debug.WriteLine("Unable to contact LUIS: " + ex);
				return null;
			}
			
			if (intentPrefix != null)
			{
				// Remove all intents not matching the prefix.
				luisResult.Intents = luisResult.Intents
					.Where(i => i.Name.StartsWith(intentPrefix, StringComparison.OrdinalIgnoreCase))
					.OrderByDescending(i => i.Score)
					.ToArray();
			}
			else
			{
				luisResult.Intents = luisResult.Intents
					.OrderByDescending(i => i.Score)
					.ToArray();
			}

			// Update the top scoring intent because we have potentially removed the original intent.
			// Also the top intent could be below our required minimum score.
			luisResult.TopScoringIntent = luisResult.Intents.FirstOrDefault(i => i.Score >= minimumScore);

			return luisResult;
		}

		/// <summary>
		/// Helper method to deal with situations where the bot does not understand input.
		/// On every call the method increases an internal counter. If that counter exceeds the threshold,
		/// the optional message is displayed and a picker is shown that allows the user to take action to reover.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="optionalMessage"></param>
		/// <returns></returns>
		protected async Task<bool> HandleInputNotUnderstoodAsync(IDialogContext context, string optionalMessage = null)
		{
			_notUnderstoodCounter++;
			if(_notUnderstoodCounter >= 2)
			{
				_notUnderstoodCounter = 0;
				if(!string.IsNullOrWhiteSpace(optionalMessage))
				{
					await context.PostAsync(optionalMessage);
				}
				ShowDefaultNotUnderstoodPicker(context);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Shows the picker that is used if the bot does not understand input. It is used by <see cref="HandleInputNotUnderstoodAsync(IDialogContext, string)"/>
		/// but you can also use it manually.
		/// </summary>
		/// <param name="context"></param>
		protected void ShowDefaultNotUnderstoodPicker(IDialogContext context)
		{
			ShowPicker(context,
				(int)PickerIds.NotUnderstoodMultipleTimes,
				"Sorry, I did not understand. Are you looking for somehing else?",
				2,
				NotUnderstood_PickerOption_BackToMain, NotUnderstood_PickerOption_GoToQandA, NotUnderstood_PickerOption_KeepTrying);
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