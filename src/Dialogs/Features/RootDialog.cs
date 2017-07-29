using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace XamUBot.Dialogs
{
    [Serializable]
    public class RootDialog : BaseDialog
    {
        Dictionary<string, Type> MainMenuChoices = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "Team", typeof(TeamDialog) },
            { "Q & A", typeof(QandADialog) },
            { "Support", typeof(SupportDialog) }
        };

        Dictionary<string, Type> AlternativeChoices = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "XamU", typeof(TeamDialog) },
            { "QnA", typeof(QandADialog) },
            { "Q&A", typeof(QandADialog) },
            { "FAQ", typeof(QandADialog) },
            { "Questions", typeof(QandADialog) },
            { "Answers", typeof(QandADialog) }
        };

        const string DefaultHelpPrompt = "Please select one of the following categories:";

        protected override async Task OnInitializeAsync(IDialogContext context)
        {
            await context.PostAsync(ResponseUtterances.GetResponse(
                ResponseUtterances.ReplyTypes.Welcome));

            await base.OnInitializeAsync(context);
        }


        protected async override Task OnHelpReceivedAsync(IDialogContext context, Activity activity)
        {
            await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootHelp));

            WaitForNextMessage(context);
        }

        protected async override Task OnMessageReceivedAsync(IDialogContext context, Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    await ShowTopicsAsync(context);
                    break;

                case ActivityTypes.Message:
                    await CheckForKnownVerbAsync(context, activity);
                    break;

                default:
                    WaitForNextMessage(context);
                    break;
            }
        }

        private async Task CheckForKnownVerbAsync(IDialogContext context, Activity activity)
        {
            string message = activity.Text;
            await ProcessResultAsync(context, message);
        }

        private Task ShowTopicsAsync(IDialogContext context)
        {
            // Display a dialog box with choices.
            FuzzyPromptDialog<string>.Choice(
                context: context,
                resume: OnMainMenuItemSelected,
                promptOptions: FuzzyPromptOptions<string>.Create(
                    prompt: ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt),
                    attempts: 0, 
                    options: MainMenuChoices.Keys.ToArray(),
                    validOptions: MainMenuChoices.Keys.Union(AlternativeChoices.Keys).ToArray()));

            return Task.CompletedTask;
        }

        private async Task OnMainMenuItemSelected(IDialogContext context, IAwaitable<object> result)
        {
            string selectedChoice = await result.GetValueAsync<string>();
            await ProcessResultAsync(context, selectedChoice);
        }

        private async Task ProcessResultAsync(IDialogContext context, string selectedChoice)
        {
            if (selectedChoice != null)
            {
                // Forward to the proper dialog based on our mapping.
                Type dialogType;
                if (MainMenuChoices.TryGetValue(selectedChoice, out dialogType)
                    || AlternativeChoices.TryGetValue(selectedChoice.Replace(" ", ""), out dialogType))
                {
                    var dialog = Activator.CreateInstance(dialogType) as IDialog<object>;
                    if (dialog != null)
                    {
                        // Move to the next dialog in the chain.
                        await context.Forward(dialog, OnResumeDialog, null, CancellationToken.None);
                        return;
                    }
                }
            }

            // User exceeded maximum retries and always provided a value 
            // that's not part of the picker. Or used a cancel term.
            // Show main menu picker again.
            await context.PostAsync(DefaultHelpPrompt);
            await ShowTopicsAsync(context);
            //WaitForNextMessage(context);
        }

        async Task OnResumeDialog(IDialogContext context, IAwaitable<object> result)
        {
            await ShowTopicsAsync(context);
        }
    }
}