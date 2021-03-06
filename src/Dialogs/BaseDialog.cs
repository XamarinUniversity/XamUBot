﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using XamUBot.Utterances;
using System.Threading;

namespace XamUBot.Dialogs
{
    /// <summary>
    /// Contains base functionality to simplify common dialog tasks.
    /// </summary>
    [Serializable]
    public abstract class BaseDialog : IDialog<object>
    {
        // TODO: move to string table.
        const string NotUnderstood_PickerTitle = "Sorry, I did not understand. Are you looking for somehing else?";
        const string NotUnderstood_PickerOption_BackToMain = "Show all options";
        const string NotUnderstood_PickerOption_KeepTrying = "Stay here";

        /// <summary>
        /// "Yes" constant for a yes/no style dialog.
        /// </summary>
        public const string ChoiceYes = "yes";

        /// <summary>
        /// "No" constant for a yes/no style dialog.
        /// </summary>
        public const string ChoiceNo = "no";

        public async Task StartAsync(IDialogContext context)
        {
            await OnInitializeAsync(context);
        }

        /// <summary>
        /// Can be overridden to run one time initialization when the dialog is getting constructed.
        /// This does nothing in the base class.
        /// </summary>
        /// <param name="context"></param>
        protected virtual Task OnInitializeAsync(IDialogContext context)
        {
            WaitForNextMessage(context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates options for displaying a selection of options.
        /// </summary>
        /// <param name="title">title of the prompt</param>
        /// <param name="numRetries">number of times to ask if user provides an invalid answer (0 = don't ask again)</param>
        /// <param name="buttons">options to display</param>
        /// <returns></returns>
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
        /// Makes the dialog wait for the next incoming message.
        /// </summary>
        /// <param name="context"></param>
        protected void WaitForNextMessage(IDialogContext context)
        {
            context.Wait(OnInnerMessageReceivedAsync);
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
        /// Gets called if a message was received. This is a convenience method which removes the IAwaitable and directly delivers the activity.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
        protected abstract Task OnMessageReceivedAsync(IDialogContext context, Activity activity);

        /// <summary>
        /// Gets called if the dialog detected that the user is seeking help.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
        protected abstract Task OnHelpReceivedAsync(IDialogContext context, Activity activity);

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
            if (!string.IsNullOrWhiteSpace(optionalMessage))
            {
                await context.PostAsync(optionalMessage);
            }

            PromptDialog.Choice(
                context,
                OnNotUnderstoodSelected,
                CreateDefaultPromptOptions(NotUnderstood_PickerTitle, 
                        1, NotUnderstood_PickerOption_BackToMain, 
                        NotUnderstood_PickerOption_KeepTrying));
        }

		/// <summary>
		/// Helper to show a simple picker with two choices.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="resume"></param>
		/// <param name="message"></param>
		/// <param name="yes"></param>
		/// <param name="no"></param>
		protected void ShowYesNoPicker(IDialogContext context, ResumeAfter<string> resume, string message, string yes = ChoiceYes, string no = ChoiceNo)
		{
			PromptDialog.Choice(context, resume, CreateDefaultPromptOptions(message, 2, yes, no));
		}

        async Task OnNotUnderstoodSelected(IDialogContext context, IAwaitable<object> result)
        {
            var selectedChoice = await result.GetValueAsync<string>();
            switch (selectedChoice)
            {
                case NotUnderstood_PickerOption_BackToMain:
                    PopToRootDialog(context);
                    break;
                
                case NotUnderstood_PickerOption_KeepTrying:
                default:
                    // Just wait for next message.
                    await context.PostAsync("Ok!");
                    WaitForNextMessage(context);
                    break;
            }
        }

        /// <summary>
        /// Clears the stack and restarts with the root dialog.
		/// Note: state will be lost! A new instance of the root dialog will be created.
        /// </summary>
        /// <param name="context"></param>
        protected void PopToRootDialog(IDialogContext context)
        {
			context.Reset();
        }

		/// <summary>
		/// Helper to forward an activity to the Q&A dialog and make the dialog return immediately.
		/// The answer found in the FAQs will be sent to the client or, if no answer was found, a
		/// picker will be shown which lets the user decide whether they want to stay in the current dialog
		/// or go back to theroot dialog.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="activity"></param>
		/// <returns></returns>
		protected async Task CheckFaqAsync(IDialogContext context, Activity activity)
		{
			await context.PostAsync("I don't know a concrete answer to this but let me check our Q&A for you...");
			await context.Forward(new QandADialog(interactiveMode: false), OnResumeAfterQandAChecked, activity, CancellationToken.None);
		}

		/// <summary>
		/// Gets called if we checked the Q&A because the current dialog did not know an answer.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result">contains the answer found by the Q&A dialog or NULL if nothing found</param>
		/// <returns></returns>
		async Task OnResumeAfterQandAChecked(IDialogContext context, IAwaitable<object> result)
		{
			string foundAnswer = await result.GetValueAsync<string>();

			if (string.IsNullOrWhiteSpace(foundAnswer))
			{
				await ShowDefaultNotUnderstoodPicker(context, "Our FAQs don't seem to contain anything about your inquiry");
				return;
			}

			await context.PostAsync($"This is what I found in the FAQs: {foundAnswer}.");
			WaitForNextMessage(context);
		}
	}
}