using Twitter.Core.Dtos;
using Twitter.Data.Entities;
using Twitter.Data.UnitOfWork;

namespace Twitter.Core.Services
{
	public class InteractionService
	{
		IUnitOfWork _unitOfWork;

		public InteractionService()
		{
			_unitOfWork = new UnitOfWork();
		}

		/// <summary>
		/// Tweet likelandığında user id ve tweet id db ye eklenir
		/// </summary>
		/// <param name="userId">tweeti beğenen kullanıcı id</param>
		/// <param name="tweetId">beğenilen tweet id</param>
		public void AddLikeInteraction(int userId, int tweetId)
		{
			var interaction = new Interaction
			{
				UserId = userId,
				TweetId = tweetId
			};

			_unitOfWork.InteractionRepository.Insert(interaction);
			_unitOfWork.Commit();

		}

		/// <summary>
		/// Like geri çekildiğinde db den silinir
		/// </summary>
		/// <param name="userId">tweet beğenisini silen kullanıcı id</param>
		/// <param name="tweetId">beğeni çekilen tweet id</param>
		public void RemoveLikeInteraction(int userId, int tweetId)
		{
			var interaction = _unitOfWork.InteractionRepository.GetAll()
				.Where(x => x.UserId == userId)
				.Where(x => x.TweetId == tweetId)
				.FirstOrDefault();

			_unitOfWork.InteractionRepository.Delete(interaction);
			_unitOfWork.Commit();
		}

		public List<InteractionDto> GetUserLikedTweets(int userid)
		{
			var data = _unitOfWork.InteractionRepository.GetAll().Where(x => x.UserId == userid);
			var list = new List<InteractionDto>();

			if (data != null)
			{
				list = data.Select(x => new InteractionDto
				{
					Id = x.InteractionId,
					UserId = x.UserId,
					TweetId = x.TweetId
				}).ToList();
			}
			return list;
		}
	}
}
