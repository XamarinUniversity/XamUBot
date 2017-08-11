using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;

namespace XamUBot
{
	/// <summary>
	/// This controller handles all incoming messages the bot can process.
	/// </summary>
	[BotAuthentication]
    [RoutePrefix("api/messages")]
    public class MessagesController : ApiController
    {
	    /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> HandleIncomingActivity([FromBody]Activity activity)
        {
            if(activity == null)
            {
                return BadRequest("No activity provided.");
            }


			if (activity.Type == ActivityTypes.ConversationUpdate)
			{
				IConversationUpdateActivity update = activity;

				// Remove bot from the members added
				update.MembersAdded = update.MembersAdded.Where(member => member.Id != update.Recipient.Id).ToList();

				if (update.MembersAdded.Count == 0)
				{
					return Ok();
				}
			}

			await Conversation.SendAsync(activity, () => new Dialogs.SourceDialog());

			return Ok();
        }
    }
}