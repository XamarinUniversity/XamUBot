using System.Collections.Generic;
using System.Threading.Tasks;

namespace XamUApi
{
	/// <summary>
	/// Specifies communication between bot and XamU.
	/// </summary>
	public interface IApiManager
	{
		/// <summary>
		/// Gets the XamU team
		/// </summary>
		Task<IList<TeamResponse>> GetTeamAsync();

		/// <summary>
		/// Gets the available tracks of XamU.
		/// </summary>
		/// <param name="filter">filter by a specific keyword. Can be NULL.</param>
		Task<IList<Track>> GetTracksAsync(string filter = null);

		/// <summary>
		/// Save audit to keep track of questions and answers.
		/// </summary>
		/// <param name="auditItem">item to store. Does nothing if item is null.</param>
		/// <returns>the item with the timestamp set</returns>
		Task<BotAuditItem> SaveAuditAsync(BotAuditItem auditItem);
	}
}