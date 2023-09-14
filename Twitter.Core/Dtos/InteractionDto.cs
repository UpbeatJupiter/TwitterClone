namespace Twitter.Core.Dtos
{
	public class InteractionDto
	{
		public int Id { get; set; }	

		/// <summary>
		/// Kim tweeti beğendi
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// hangi tweet beğenildi
		/// </summary>
		public int TweetId { get; set; }	

		/// <summary>
		/// etkileşim türü
		/// </summary>
		public string InteractionType { get; set; }
	}
}
