using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Collections.Generic;

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

		public async Task StartAsync(IDialogContext context)
		{
			await OnInitializeAsync(context);
			context.Wait(OnInnerMessageReceivedAsync);
		}

		async Task OnInnerMessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			bool waitForNextMessage = true;

			var activity = await result as Activity;
			if (activity != null)
			{
				waitForNextMessage = await OnMessageReceivedAsync(context, activity);
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
		protected async virtual Task OnInitializeAsync(IDialogContext context)
		{
			// Does nothing in base.
		}

		/// <summary>
		/// Gets called if a message was received. This is a convenience method which removes the IAwaitable and directly delivers the activity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="msgActivity"></param>
		/// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
		protected abstract Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity msgActivity);

		/// <summary>
		/// Sends a picker with arbitrary choices to the client. Override OnChoiceMadeAsync to react to the result of the picker.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		protected void ShowPicker(IDialogContext context, int pickerId, string title, params string[] buttons)
		{
			_pendingPickerId = pickerId;
			PromptDialog.Choice(context, OnInnerPickerSelectedAsync, buttons, title);
		}

		/// <summary>
		/// Sends a yes/no picker to the client. Override OnChoiceMadeAsync to react to the result of the picker.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		protected void SendPicker(IDialogContext context, int pickerId, string title)
		{
			_pendingPickerId = pickerId;
			PromptDialog.Confirm(context, OnInnerPickerSelectedAsync, title);
		}

		/// <summary>
		/// Internal handler for picker with arbitrary items.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		async Task OnInnerPickerSelectedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var selectedItem = await result;
			await OnPickerSelectedAsync(context, _pendingPickerId, selectedItem as string);
		}

		/// <summary>
		/// Internal handler for picker with yes/no items.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		async Task OnInnerPickerSelectedAsync(IDialogContext context, IAwaitable<bool> result)
		{
			var selectedItem = await result;
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
		/// <param name="selectedChoice">the title of the selected item. For yes/no picker, user BaseDialog.ConstantYes and BaseDialog.ConstantNo</param>
		/// <returns>return TRUE if you want the dialog to wait for the next message, otherwise FALSE</returns>
		protected virtual Task<bool> OnPickerSelectedAsync(IDialogContext context, int pickerId, string selectedChoice)
		{
			// Does nothing in base.
			return Task.FromResult(true);
		}

		/// <summary>
		/// Navigates to a different dialog. Optional return values can be picked up in OnGetDialogReturnValueAsync.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="dialogId"></param>
		/// <param name="dialog"></param>
		protected void GoToDialog(IDialogContext context, int dialogId, IDialog<object> dialog)
		{
			_pendingDialogId = dialogId;
			context.Call(dialog, OnInnerResumeDialogAsync);
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
		
	}
}