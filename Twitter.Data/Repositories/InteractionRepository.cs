using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twitter.Data.Contexts;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public class InteractionRepository : GenericRepository<Interaction>, IinteractionRepository
	{
		protected readonly TwitterContext _context;
		public InteractionRepository(TwitterContext context) : base(context)
		{
			_context = context;
		}

		/// <summary>
		/// Like butonuna basınca db ye ekle
		/// </summary>
		/// <param name="interaction"></param>
		public void AddLikeInteraction(Interaction interaction)
		{
			_context.Interactions.Add(interaction);
		}

		/// <summary>
		/// unlike yapınca db den sil
		/// </summary>
		/// <param name="interaction"></param>
		public void RemoveLikeInteraction(Interaction interaction)
		{
			_context.Interactions.Remove(interaction);
		}
	}
}
