using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using XamUApi;
using XamUBot.Audit;

namespace XamUBot
{
    public class DebugActivityLogger : IActivityLogger
    {
        public async Task LogAsync(IActivity activity)
        {

            var auditItem = new BotAuditItem();
            auditItem.TimestampUtc = activity.Timestamp;
            auditItem.SenderId = activity.From.Id;
            auditItem.SenderName = activity.From.Name;
            auditItem.RecipientId = activity.Recipient.Id;
            auditItem.RecipientName = activity.Recipient.Name;
            //auditItem.UserId = ??? -- no idea how to get user id
            auditItem.ConversationId = activity.Conversation.Id;
            auditItem.ReplyToId = activity.ReplyToId;
            auditItem.MessageId = activity.Id;

            StringBuilder message = new StringBuilder();

            // now try and fill in the message
            var msg = activity.AsMessageActivity();
            if (msg is Activity)
            {
                Activity a = msg as Activity;
                foreach (var attachment in a.Attachments)
                {
                    if (attachment.Content is HeroCard)
                    {
                        var card = attachment.Content as HeroCard;
                        message.AppendLine($"Sent HeroCard: {card.Text}");
                    }
                    else
                    {
                        message.AppendLine ($"Sent Attachment: {attachment.ContentType}");
                    }
                }

                if (!String.IsNullOrEmpty(a.Text))
                    message.AppendLine($"{a.Text}");

            }
            else
            {
                message.AppendLine($"*** {activity.AsMessageActivity()?.Text}");
            }

            auditItem.Message = message.ToString();

            Debug.WriteLine(auditItem);
            // maybe in a real one we would
            //await ApiManagerFactory.Instance.SaveAuditAsync(auditItem);
        }


    }
}