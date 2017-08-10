using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using QnAMakerDialog;

namespace XamUBot.Dialogs
{
    /// <summary>
    /// Queries Q&A. Returns a string containing the found answer (if any).
    /// </summary>
    [Serializable]
    [QnAMakerService("0646d377dab54c62b894ba9427ab0e7f", "732de060-547d-430b-ad75-a781ede77355")]
    public class QandADialog : QnAMakerDialog<object>
    {
        /// <summary>
        /// Default constructor (required). Starts the dialog in interactive mode.
        /// </summary>
        public QandADialog() : this(true)
        {
        }

        /// <summary>
        /// Creates a new instance of the dialog
        /// </summary>
        /// <param name="interactiveMode">use TRUE to run the dialog and make it wait for incoming messages. Use FALSE if
        /// you are calling the dialog just in order to get a result back for further processing.</param>
        public QandADialog(bool interactiveMode)
        {
            _interactivMode = interactiveMode;
        }

        bool _interactivMode;

        // Contains the result found in the FAQs.
        string _foundResult;

        public async override Task StartAsync(IDialogContext context)
        {
            if (_interactivMode)
            {
                await context.PostAsync(ResponseUtterances.GetResponse(
                    ResponseUtterances.ReplyTypes.FAQWelcome));
            }

            await base.StartAsync(context);
        }


        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            await base.MessageReceived(context, item);
        }

        protected override void WaitOrExit(IDialogContext context)
        {
            if (_interactivMode)
            {
                base.WaitOrExit(context);
            }
            else
            {
                // Exit and return found result.
                context.Done(_foundResult);
            }
        }

        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            _foundResult = null;

			if (_interactivMode)
			{
				await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'." +
					"\r\n\r\nYou can try another question, or type 'exit' to return to the main menu.");
			}
        }

        [QnAMakerResponseHandler(90)]
        public async Task HiScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            _foundResult = result.Answer;

			if (_interactivMode)
			{
				await context.PostAsync($"{result.Answer}.");
			}
        }

        [QnAMakerResponseHandler(70)]
        public async Task PrettyHighScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            _foundResult = result.Answer;

			if (_interactivMode)
			{
				await context.PostAsync($"This seems to be what you are after: {result.Answer}.");
			}
        }

        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            _foundResult = result.Answer;

			if (_interactivMode)
			{
				await context.PostAsync($"I'm not exactly sure, but this might help: {result.Answer}.");
			}
        }
    }
}