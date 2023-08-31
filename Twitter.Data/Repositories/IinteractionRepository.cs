using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public interface IinteractionRepository : IGenericRepository<Interaction>
	{
		void AddLikeInteraction(Interaction interaction);

		void RemoveLikeInteraction(Interaction interaction);
	}
}
