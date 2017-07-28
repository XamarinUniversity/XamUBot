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

        Dictionary<string, Type> AlternativeText = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "XamU", typeof(TeamDialog) },
            { "QnA", typeof(QandADialog) },
            { "Q&A", typeof(QandADialog) },
            { "FAQ", typeof(QandADialog) },
            { "Questions", typeof(QandADialog) },
            { "Answers", typeof(QandADialog) }
        };

        bool _firstVisit = true;

        const string WelcomeBackToMainMenu = "You're back to the main menu!";
        const string DefaultHelpPrompt = "Looks like we have a small communication problem. If you need support, please say 'help' or pick one of the available options.";

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
                CreateDefaultPromptOptions(
                    ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt),
                    0, MainMenuChoices.Keys.ToArray()));
        }

        async Task OnMainMenuItemSelected(IDialogContext context, IAwaitable<object> result)
        {
            string selectedChoice = await result.GetValueAsync<string>();

            if (selectedChoice == null)
            {
                // User exceeded maximum retries and always provided a value that's not part of the picker.
                // Show main menu picker again.
                await context.PostAsync(DefaultHelpPrompt);
                await ShowTopics(context);
                return;
            }

            // Forward to the proper dialog based on our mapping.
            Type dialogType;
            if (MainMenuChoices.TryGetValue(selectedChoice, out dialogType)
                || AlternativeText.TryGetValue(selectedChoice, out dialogType))
            {
                var dialog = Activator.CreateInstance(dialogType) as IDialog<object>;
                if (dialog != null)
                {
                    await context.Forward(dialog, OnResumeDialog, null, CancellationToken.None);
                }
                else
                {
                    WaitForNextMessage(context);
                }
            }
            else
            {
                WaitForNextMessage(context);
            }
        }

        async Task OnResumeDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync(WelcomeBackToMainMenu);
            await ShowTopics(context);
        }
    }
}