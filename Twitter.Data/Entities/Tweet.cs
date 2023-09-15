using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twitter.Data.Entities
{
	public class Tweet
	{
		[Key]
		public int Id { get; set; }

		/// <summary>
		/// tweet atanın id'si
		/// </summary>
		public int UserId { get; set; }
		public User User { get; set; }

		/// <summary>
		/// tweet içeriği
		/// </summary>
		[Column("Tweets")]
		public string TweetContent { get; set; }

		/// <summary>
		/// tweetin atılma tarihi
		/// </summary>
		public DateTime TweetDate { get; set; }

		/// <summary>
		/// tweetin like sayısı
		/// </summary>
		public int LikeCount { get; set; }

		/// <summary>
		/// tweetin Retweet sayısı
		/// </summary>
		public int RetweetCount { get; set; }

		public ICollection<Interaction> Interactions { get; set; }
	}
}
