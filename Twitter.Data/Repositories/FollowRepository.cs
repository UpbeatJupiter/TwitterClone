using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twitter.Data.Contexts;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public class FollowRepository : GenericRepository<Follow>, IFollowRepository
	{
		protected readonly TwitterContext _context;
		public FollowRepository(TwitterContext context) : base(context)
		{
			_context = context;
		}

		//takip et butonuna basınca db ye ekle
		public void AddFollowedUser(Follow followed)
		{
			_context.Follows.Add(followed);
		}

		//unfollow butonuna basınca db den çıkart
		public void DeleteFollowedUser(Follow followed)
		{
			_context.Follows.Remove(followed);
		}
	}
}
