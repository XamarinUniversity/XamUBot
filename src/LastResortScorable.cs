using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;

namespace XamUBot
{
	// Generic types: IActivity = the item processed by the scorable
	//                string    = the state it is holding on to
	//                double    = the resulting score
	// (and I have no idea why this is generic...seems like this is the only used 
	//  combination of types!?)
	// Good reference: http://www.garypretty.co.uk/2017/04/13/using-scorables-for-global-message-handling-and-interrupt-dialogs-in-bot-framework
	public class LastResortScorable : ScorableBase<IActivity, string, double>
	{
		readonly IDialogTask _task;

		// Apparently when this gets constructed, an IDialogTask instance is given to us.
		// I do not understand yet, where this is coming from and what exactly it is...
		public LastResortScorable(IDialogTask task)
		{
			_task = task;
		}

		protected override Task<string> PrepareAsync(IActivity item, CancellationToken token)
		{
			// This method will be called to allow us to set a state.
			// The type of the state handled by this scorable is string (see second generic
			// type param).
			// Setting the state is done by returning a value here.

			var message = item as IMessageActivity;
			if(message?.Text == null)
			{
				return Task.FromResult<string>(null);
			}

			if(message.Text.ToLowerInvariant().Contains("panic"))
			{
				// We set the message as the current state. Can really be any string here
				// but this way we can pick up the entered message later when handling it.
				return Task.FromResult(message.Text);
			}

			return Task.FromResult<string>(null);
		}

		protected override bool HasScore(IActivity item, string state)
		{
			// This method will be called to allow the bot framework to find out
			// if this scorable would like to contribute a score.
			// Simple in our case: if we found a keyword in PrepareAsync(), we contribute.
			// (The "state" parameter is set to the return value of PrepareAsync()).
			return state != null;
		}

		protected override double GetScore(IActivity item, string state)
		{
			// If we have a score to offer, it will be returned here.
			// All contributors are sorted and the highest ranked scorable wins.
			// For sake of simplicity, this returns 1 and thus always wins.
			return 1f;
		}

		protected async override Task PostAsync(IActivity item, string state, CancellationToken token)
		{
			// Gets called if evaluation of our score resulted in us handling the incoming message.
			// We can redirect to another dialog in here or output a message - up to us.
			var message = item as Activity;
			if (message == null)
			{
				return;
			}

			var replyDialog = new CommonResponsesDialog($"Sometimes I also feel **{state}**...");

			// Wat??? Docs say this calls "the voided dialog" - very helpful!
			replyDialog.Void<object, IMessageActivity>();

			_task.Call(replyDialog, null);
			await _task.PollAsync(token);
		}

		protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
		{
			// Can be used for cleanup.
			return Task.CompletedTask;
		}
	}

	public class CommonResponsesDialog : IDialog<object>
	{
		private readonly string _messageToSend;

		public CommonResponsesDialog(string message)
		{
			_messageToSend = message;
		}

		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync(_messageToSend);
			context.Done<object>(null);
		}
	}
}