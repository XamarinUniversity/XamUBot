using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using XamUBot.Utterances;
using System.Collections.Generic;

namespace XamUBot.Dialogs
{
	// Generic types: IActivity = the item processed by the scorable
	//                string    = the state it is holding on to
	//                double    = the resulting score
	// (and I have no idea why this is generic...seems like this is the only used 
	//  combination of types!?)
	// Good reference: http://www.garypretty.co.uk/2017/04/13/using-scorables-for-global-message-handling-and-interrupt-dialogs-in-bot-framework


	/// <summary>
	/// Global handler that will kick in if the user wants to get help.
	/// Pushes a temporary dialog onto the stack and then passes message handling back to the previously active dialog.
	/// </summary>
	public class GlobalHandlerDialog : ScorableBase<IActivity, string, double>
	{
		readonly IDialogTask _task;
        static List<string> trio = new List<string>(3);

		// Apparently when this gets constructed, an IDialogTask instance is given to us.
		// I do not understand yet, where this is coming from and what exactly it is...
		public GlobalHandlerDialog(IDialogTask task)
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
			if (message?.Text == null)
			{
				return Task.FromResult<string>(null);
			}

			var keyword = message.Text.ToLowerInvariant().Trim();

            if (Keywords.IsExitKeyword(keyword))
            {
                // We set the message as the current state. Can really be any string here
                // but this way we can pick up the entered message later when handling it.
                return Task.FromResult(Keywords.Exit);
            }
            //else if (Keywords.IsHelpKeyword(keyword))
            //{
            //    return Task.FromResult(Keywords.Help);
            //}
            else if (Keywords.IsSwearWord(keyword))
            {
                return Task.FromResult(Keywords.Swear);
            }

            // Look for repeats.
            trio.Add(keyword);

            if (trio.Count > 3)
            {
                trio.RemoveAt(0);
            }

            if (trio.Count == 3 && trio[0] == trio[1] && trio[1] == trio[2])
            {
                return Task.FromResult(Keywords.Repeat);
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
            switch (state)
            {
                case Keywords.Exit:
					// React to exit request. Brings us back to the root dialog.
					// Note: Reset() clears the stack and will make the message controller create a new instance of the RootDialog().
					//       This means all state of the dialog will be lost.
					_task.Reset();
                    break;

                case Keywords.Repeat:
                    {
                        var response = ResponseUtterances.GetResponse("Repeat");
                        var reply = new CommonResponsesDialog(response);
                        var interup = reply.Void<object, IMessageActivity>();
                        _task.Call(interup, null);
                        await _task.PollAsync(token);
                        trio.Clear();
                    }
                    break;
                    
                //case Keywords.Help:
                //    var replyDialog = new CommonResponsesDialog($"Sometimes I also feel **{state}**...");

                //    // See: https://stackoverflow.com/questions/45282506/what-are-the-void-and-pollasync-methods-of-idialogtask-for/45283394#45283394
                //    var interruption = replyDialog.Void<object, IMessageActivity>();
                //    _task.Call(interruption, null);
                //    await _task.PollAsync(token);
                //    break;
                case Keywords.Swear:
                    {
                        var response = ResponseUtterances.GetResponse("Swear");
                        var reply = new CommonResponsesDialog(response);
                        var interup = reply.Void<object, IMessageActivity>();
                        _task.Call(interup, null);
                        await _task.PollAsync(token);
                    }
                    break;
                    
            }
        }

		protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
		{
			// Can be used for cleanup.
			return Task.CompletedTask;
		}
    }

	[Serializable]
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
			context.Done<object>((IMessageActivity)null);
		}
	}
}