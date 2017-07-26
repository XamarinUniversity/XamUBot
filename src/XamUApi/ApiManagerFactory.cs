using System;

namespace XamUApi
{
	/// <summary>
	/// Creates an instance of IApiManager.
	/// </summary>
	public sealed class ApiManagerFactory
	{
		static Lazy<IApiManager> _instance = new Lazy<IApiManager>(() => new MockApiManager(), false);

		/// <summary>
		/// Returns a singleton of the manager.
		/// </summary>
		public static IApiManager Instance => _instance.Value;
	}
}
