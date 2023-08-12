using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter.Core.Dtos
{
	public class FollowDto
	{
		public int Id { get; set; }

		/// <summary>
		/// takip edilen kullanıcı
		/// </summary>
		public int FollowedUserId { get; set; }

		/// <summary>
		/// takip eden kullanıcı
		/// </summary>
		public int FollowingUserId { get; set; }

	}
}
