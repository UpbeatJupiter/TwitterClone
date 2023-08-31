using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public interface IFollowRepository : IGenericRepository<Follow>
	{
		void AddFollowedUser(Follow follow);
		void DeleteFollowedUser(Follow follow);
	}
}
