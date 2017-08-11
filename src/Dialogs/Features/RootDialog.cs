using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class RootDialog : BaseDialog
	{
		[NonSerialized]
		static Dictionary<string, Type> MainMenuChoices = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Team", typeof(TeamDialog) },
			{ "Q & A", typeof(QandADialog) },
			{ "Support", typeof(SupportDialog) }
		};

		[NonSerialized]
		static Dictionary<string, Type> AlternativeChoices = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
		{
			{ "XamU", typeof(TeamDialog) },
			{ "QnA", typeof(QandADialog) },
			{ "Q&A", typeof(QandADialog) },
			{ "FAQ", typeof(QandADialog) },
			{ "Questions", typeof(QandADialog) },
			{ "Answers", typeof(QandADialog) }
		};

		const string DefaultHelpPrompt = "Please select one of the following categories:";

		protected async override Task OnHelpReceivedAsync(IDialogContext context, Activity activity)
		{
			await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootHelp));

			WaitForNextMessage(context);
		}

		protected async override Task OnMessageReceivedAsync(IDialogContext context, Activity activity)
		{
			switch (activity.Type)
			{
				case ActivityTypes.Event:
					if ((activity.Value as string) == "INIT_DIALOG")
					{
						await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt));
						ShowMainTopics(context);
					}
					else
					{
						WaitForNextMessage(context);
					}
					break;

				default:
					WaitForNextMessage(context);
					break;
			}
		}

		void ShowMainTopics(IDialogContext context)
		{
			// Display a dialog box with choices.
			FuzzyPromptDialog<string>.Choice(
				context: context,
				resume: OnMainTopicSelectedAsync,
				promptOptions: FuzzyPromptOptions<string>.Create(
					prompt: ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.RootPrompt),
					attempts: 0,
					// The "too many attempts" string is always shown by the choice dialog. 
					// See the "PostAsync() call in https://github.com/Microsoft/BotBuilder/blob/4621a4c611889a6ebade4717329f8e6ea62f2f7f/CSharp/Library/Microsoft.Bot.Builder/Dialogs/PromptDialog.cs#L1178
					// This means: using NULL will lead to the defautl string and using empty string will output an empty message. Instead of subclassing
					// the prompt dialog and trying to fix it, let's just use it instead of our own PostAsync() call. It wil always show up because the fuzzy
					// dialog has a retry count of 0.
					tooManyAttempts: ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.PickOneOfTheseTopics),
					retry: string.Empty,
					options: MainMenuChoices.Keys.ToArray(),
					validOptions: MainMenuChoices.Keys.Union(AlternativeChoices.Keys).ToArray()));
		}

		async Task OnMainTopicSelectedAsync(IDialogContext context, IAwaitable<object> result)
		{
			string selectedChoice = await result.GetValueAsync<string>();
			await ProcessSelectedMainMenuItemAsync(context, selectedChoice);
		}

		async Task ProcessSelectedMainMenuItemAsync(IDialogContext context, string selectedChoice)
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
						await context.Forward(dialog, OnResumeDialog, null, CancellationToken.None);
						return;
					}
				}
			}

			// User exceeded maximum retries and always provided a value 
			// that's not part of the picker. Or used a cancel term.
			// Show main menu picker again.
			ShowMainTopics(context);
			return;
		}

		async Task OnResumeDialog(IDialogContext context, IAwaitable<object> result)
		{
			await context.PostAsync(DefaultHelpPrompt);
			ShowMainTopics(context);
		}

		/// <summary>
		/// Helper to quickly go to root dialog and optionally reset the dialog stack.
		/// </summary>
		/// <param name="dialogTask">context/stack</param>
		/// <param name="clearStack">TRUE to clear stack, FALSE to leave it unchanged</param>
		internal async static Task ForwardToRootDialogAsync(IDialogTask dialogTask, bool clearStack)
		{
			if (clearStack)
			{
				dialogTask.Reset();
			}
			var newActivity = Activity.CreateEventActivity();
			newActivity.Value = "INIT_DIALOG";
			await dialogTask.Forward(new RootDialog(), null, newActivity, CancellationToken.None);
		}
	}
}