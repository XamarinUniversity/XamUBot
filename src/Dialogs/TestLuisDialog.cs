using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace XamUBot.Dialogs
{
	[Serializable]
	[LuisModel("e9412ee5-9529-42fa-bc5f-ae25069e3b40", "4f7be0062bdc4ccc91240323a99992dc", domain: "westus.api.cognitive.microsoft.com", Staging = true, Verbose = true)]

	public class TestLuisDialog : LuisDialog<object>
	{
		[LuisIntent("")]
		public async Task None(IDialogContext context, LuisResult result)
		{
			string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
			await context.PostAsync(message);
			context.Wait(MessageReceived);
		}


		[LuisIntent("IntroduceUser")]
		public async Task IntroduceUser(IDialogContext context, LuisResult result)
		{
			string name = "unknown code monkey";
			EntityRecommendation userNameEntity;
			if (result.TryFindEntity("UserName", out userNameEntity))
			{
				name = userNameEntity.Entity;
			}
			
			await context.PostAsync($"Nice to meet you, **{name}**!");
			context.Wait(MessageReceived);
		}

		[LuisIntent("Help")]
		public async Task Help(IDialogContext context, LuisResult result)
		{
			await context.PostAsync("We all are looking for someone to help us.");
			context.Wait(MessageReceived);
		}
	}
}