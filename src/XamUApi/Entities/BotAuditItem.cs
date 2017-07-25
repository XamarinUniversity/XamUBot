using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamUApi
{
	/// <summary>
	/// Keeps track of what questions have been asked.
	/// </summary>
	[Serializable]
	public class BotAuditItem
	{
		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime? TimestampUtc { get; set; }

        /// <summary>
        /// Id of the conversation
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Id of the message
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Id of the message this is replying to
        /// </summary>
        public string ReplyToId { get; set; }

        /// <summary>
        /// Id of the sender
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Friendly Name of the sender
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Id of the message recipient
        /// </summary>
        public string RecipientId { get; set; }

        // Name of the recipient
        public string RecipientName { get; set; }

        // message text
        public string Message { get; set; }

		/// <summary>
		/// If available, the ID of the XamU user
		/// </summary>
		public int? UserId { get; set; }

        public override string ToString()
        {
            return $"{TimestampUtc} - From: {SenderName} To: {RecipientName} - {Message}";
        }
    }

    
}
