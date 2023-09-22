using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter.Data.Entities
{
	public class Interaction
	{
		[Key]
		public int InteractionId { get; set; }

		/// <summary>
		/// Kim tweeti beğendi
		/// </summary>
		public int UserId { get; set; }
		public User User { get; set; }

		public string Username { get; set; }

		/// <summary>
		/// hangi tweet beğenildi
		/// </summary>
		public int TweetId { get; set; }
		public Tweet Tweet { get; set; }	

		public string InteractionType { get; set; }	
	}
}
