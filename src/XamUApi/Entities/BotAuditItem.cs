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
		/// Timestemp
		/// </summary>
		public DateTime TimestampUtc { get; set; }

		/// <summary>
		/// Context the question was asked in, e.g. "Teams", "Classes", ...
		/// </summary>
		public string Context { get; set; }

		/// <summary>
		/// The question that was asked
		/// </summary>
		public string Question { get; set; }

		/// <summary>
		/// The answer returned by the bot
		/// </summary>
		public string Answer { get; set; }

		/// <summary>
		/// If available, the ID of the XamU user
		/// </summary>
		public int? UserId { get; set; }

	}
}
