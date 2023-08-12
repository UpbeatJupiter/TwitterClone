using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter.Core.Dtos
{
	public class TweetDto
	{
		public int TweetId { get; set; }

		/// <summary>
		/// tweet atanın kullanıcı id
		/// </summary>
		public int UserId { get; set; }	

		/// <summary>
		/// tweet atanın kullanıcı adı
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// tweet içeriği
		/// </summary>
		public string TweetContent { get; set; }

		/// <summary>
		/// tweet ne kadar zaman önce atılmış
		/// </summary>
		public string TweetDate { get; set; }
	}
}
