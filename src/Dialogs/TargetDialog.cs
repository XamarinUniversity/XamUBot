using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class TargetDialog : IDialog<object>
	{
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync("In the target dialog.");
			WaitForNextMessage(context);
		}

		protected void WaitForNextMessage(IDialogContext context)
		{
			context.Wait(OnInnerMessageReceivedAsync);
		}

		Task OnInnerMessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			//Nothing
			return Task.CompletedTask;
		}
	}
}