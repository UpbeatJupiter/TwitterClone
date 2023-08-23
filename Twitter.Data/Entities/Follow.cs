using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter.Data.Entities
{
	public class Follow
	{
		[Key]
		public int Id { get; set; }	

		/// <summary>
		/// takip edilen id
		/// </summary>
		public int FollowedUserId { get; set; }
		[ForeignKey("FollowedUserId")]
		public User User { get; set; }

		/// <summary>
		/// takip eden id
		/// </summary>
		public int FollowingUserId { get; set; }

		/// <summary>
		/// kullanıcının takip tarihi
		/// </summary>
		public DateTime FollowDate { get; set; }
	}
}
