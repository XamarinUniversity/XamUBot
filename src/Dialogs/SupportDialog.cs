using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;
using System.Collections.Generic;
using System.Linq;

namespace XamUBot.Dialogs
{
	/// <summary>
	/// Provides support.
	/// </summary>
	[Serializable]
	public class SupportDialog : BaseDialog
	{
		protected async override Task<bool> OnInitializeAsync(IDialogContext context)
		{
			await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.SupportWelcome));
			return true;
		}

		protected async override Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity activity, int repetitions)
		{
			return true;
		}
	}
}