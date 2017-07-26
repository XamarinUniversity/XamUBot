using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class TracksDialog : IDialog<object>
	{
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync("What would you like to know about our tracks?");
			context.Wait(MessageReceivedAsync);
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;

			var tracks = await ApiManagerFactory.Instance.GetTracksAsync();

			foreach(var track in tracks)
			{
				await context.PostAsync("I found: " + track.Name);
			}

			context.Wait(MessageReceivedAsync);
		}
	}
}