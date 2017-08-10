using System;

namespace XamUApi
{
	/// <summary>
	/// The C# representation of a scheduled class.
	/// </summary>
	public class ClassTime
	{
		public string Slug { get; set; }
		public DateTime StartTime { get; set; }
		public string Timezone { get; set; }
		public int MaxCapacity { get; set; }
		public int NumberOfRegistrants { get; set; }

		public string InstructorName { get; set; }
		public string InstructorIcon { get; set; }

		public int SpotsRemaining { get; set; }
	}
}
