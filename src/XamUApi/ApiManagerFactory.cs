using System;

namespace XamUApi
{
	/// <summary>
	/// Creates an instance of IApiManager.
	/// </summary>
	public static class ApiManagerFactory
	{
		static Lazy<IApiManager> _instance = new Lazy<IApiManager>(() => new ApiManager(), false);

		/// <summary>
		/// Returns a singleton of the manager.
		/// </summary>
		public static IApiManager Instance => _instance.Value;
	}
}
