using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using static Microsoft.Bot.Builder.Dialogs.PromptDialog;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;

namespace XamUBot.Dialogs
{

	/// <summary>
	/// Customized version of the prompt dilog to fix an issue of the bot framework.
	/// The problem with the built in ones (Promptdialog.Choice()) is that it reacts to all messages,
	/// regardless of the type. This means the prompt cannot be used proactively to start a conversation.
	/// Reason: multiple messages arrive, mostly "conversation updates" and the buit in dialog intperets those
	/// as answers to to the prompt and will keep on prompting over and over again.
	/// This customized version ignores all non-message type messages.
	/// </summary>
	[Serializable]
	public class CustomPromptDialog : PromptChoice<string>
	{
		public CustomPromptDialog(string title, params string [] buttons) : base(
			new PromptOptions<string>(prompt: title, retry: "", tooManyAttempts: "", options: new List<string>(buttons), attempts: 2, promptStyler: null))
		{
		}

		protected async override Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
		{
			var activity = await message;
			if(activity.Type != ActivityTypes.Message)
			{
				return;
			}

			await base.MessageReceivedAsync(context, message);
		}
	}
}