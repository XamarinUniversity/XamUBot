using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;
using System.Collections.Generic;
using System.Linq;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class TestCardsDialog : IDialog<object>
	{
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync("Testing rich cards");
			context.Wait(MessageReceivedAsync);
		}

		async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var activity = await result as Activity;

			var name = activity.Text;

			if(name.Contains("exit"))
			{
				context.Done("User exited");
				return;
			}


			// Create a sign-in card as an example.
			// More: https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-add-rich-card-attachments

			Activity replyToConversation = activity.CreateReply("Should go to conversation");
			replyToConversation.Attachments = new List<Attachment>();

			List<CardAction> cardButtons = new List<CardAction>();

			CardAction plButton = new CardAction()
			{
				Value = $"https://xamarin.com",
				Type = "signin",
				Title = "Connect"
			};

			cardButtons.Add(plButton);

			SigninCard plCard = new SigninCard("You need to authorize me", cardButtons);

			Attachment plAttachment = plCard.ToAttachment();
			replyToConversation.Attachments.Add(plAttachment);

			await context.PostAsync(replyToConversation);

			context.Wait(MessageReceivedAsync);
		}
	}
}