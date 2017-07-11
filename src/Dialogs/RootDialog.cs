using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Diagnostics;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class RootDialog : IDialog<object>
	{
		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);
			return Task.CompletedTask;
		}

		async Task OnAfterPickTopic(IDialogContext context, IAwaitable<string> result)
		{
			var topic = await result;

			await context.PostAsync("You selected " + topic + "...");

			if (topic.ToLowerInvariant().Contains("tracks"))
			{
				context.Call(new TracksDialog(), OnAfterTracksDialog);
			}
			else if (topic.ToLowerInvariant().Contains("team"))
			{
				context.Call(new TeamDialog(), OnAfterTeamDialog);
			}
			else if (topic.ToLowerInvariant().Contains("test luis"))
			{
				context.Call(new TestLuisDialog(), OnAfterLuisDialog);
			}
			else
			{
				await context.PostAsync($"Unfortunately I cannot help you with that.");
				ShowTopics(context);
				context.Wait(MessageReceivedAsync);
			}
		}

		void ShowTopics(IDialogContext context)
		{
			PromptDialog.Choice(context, OnAfterPickTopic, new[] { "Team", "Tracks", "Test LUIS" }, "What would you like to know?");
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var activity = await result;

			switch (activity.Type)
			{
				case ActivityTypes.ConversationUpdate:
					ShowTopics(context);
					break;

				case ActivityTypes.Message:
					int length = (activity.Text ?? string.Empty).Length;

					await context.PostAsync($"You sent {activity.Text} which was {length} characters");
					context.Wait(MessageReceivedAsync);
					break;

				default:
					context.Wait(MessageReceivedAsync);
					break;
			}

			
		}

		private Task OnAfterTracksDialog(IDialogContext context, IAwaitable<object> result)
		{
			context.Wait(MessageReceivedAsync);

			return Task.CompletedTask;
		}

		async Task OnAfterTeamDialog(IDialogContext context, IAwaitable<object> result)
		{
			await context.PostAsync("Data returned: " + (await result));
			context.Wait(MessageReceivedAsync);
		}

		async Task OnAfterLuisDialog(IDialogContext context, IAwaitable<object> result)
		{
			await context.PostAsync("Data returned: " + (await result));
			context.Wait(MessageReceivedAsync);
		}
	}
}