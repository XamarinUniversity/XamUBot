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
using System.Threading;

namespace XamUBot.Dialogs
{
    /// <summary>
    /// Containsbase functionality to simplify common dialog tasks.
    /// </summary>
    [Serializable]
    public abstract class BaseDialog : IDialog<object>
    {
        // TODO: move to string table.
        const string NotUnderstood_WhatAreYouLookingFor = "Sorry, I did not understand. Are you looking for something else?";
        const string NotUnderstood_PickerOption_BackToMain = "Show all options";
        const string NotUnderstood_PickerOption_GoToQandA = "Check Q&A";
        const string NotUnderstood_PickerOption_KeepTrying = "Stay here";

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
                if (Keywords.IsHelpKeyword(activity.Text))
                {
                    waitForNextMessage = await OnHelpReceivedAsync(context, activity);
                }
                else
                {
                    waitForNextMessage = await OnMessageReceivedAsync(context, activity);
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
        protected virtual Task<bool> OnInitializeAsync(IDialogContext context)
        {
            // Does nothing in base.
            return Task.FromResult(true);
        }

        /// <summary>
        /// Gets called if a message was received. This is a convenience method which removes the IAwaitable and directly delivers the activity.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msgActivity"></param>
        /// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
        protected abstract Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity msgActivity);

        /// <summary>
        /// Gets called if the dialog detected that the user is seeking help.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msgActivity"></param>
        /// <returns>return TRUE if you want to wait for the next message, return FALSE if you are redirecting to another dialog or create a poll</returns>
        protected abstract Task<bool> OnHelpReceivedAsync(IDialogContext context, Activity msgActivity);

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
                    default:
                        // Just wait for next message.
                        await context.PostAsync("Ok, let's try that again!");
                        await OnMessageReceivedAsync(context, (Activity)context.Activity);
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
            context.Forward(dialog, OnInnerResumeDialogAsync, messageActivity, CancellationToken.None);
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
            string internalReturnValue = actualResult as string;
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

            string appId = ConfigurationManager.AppSettings["LuisClientAppId"];
            string appKey = ConfigurationManager.AppSettings["LuisClientAppKey"];
            var luisClient = new LuisClient(appId: appId, appKey: appKey) ;

            LuisResult luisResult = null;

            try
            {
                luisResult = await luisClient.Predict(message);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("Unable to contact LUIS: " + ex);
                throw;
#else
                return null;
#endif
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
        protected async Task ShowDefaultNotUnderstoodPicker(IDialogContext context, string optionalMessage = null)
        {
            if(!string.IsNullOrWhiteSpace(optionalMessage))
            {
                await context.PostAsync(optionalMessage);
            }

            ShowPicker(context,
                (int)PickerIds.NotUnderstoodMultipleTimes,
                NotUnderstood_WhatAreYouLookingFor,
                0,
                NotUnderstood_PickerOption_BackToMain, NotUnderstood_PickerOption_GoToQandA, NotUnderstood_PickerOption_KeepTrying);
        }

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