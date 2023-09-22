using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
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
				Username = _unitOfWork.UserRepository.GetUserById(userId).Username,
				TweetId = tweetId,
				InteractionType = "Like"
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
				.Where(x => x.InteractionType == "Like")
				.Where(x => x.UserId == userId)
				.Where(x => x.TweetId == tweetId)
				.FirstOrDefault();

			_unitOfWork.InteractionRepository.Delete(interaction);
			_unitOfWork.Commit();
		}

		public List<InteractionDto> GetUserLikedTweets(int userid)
		{
			var data = _unitOfWork.InteractionRepository.GetAll().Where(x => x.InteractionType == "Like").Where(x => x.UserId == userid);
			var list = new List<InteractionDto>();

			if (data != null)
			{
				list = data.Select(x => new InteractionDto
				{
					Id = x.InteractionId,
					UserId = x.UserId,
					Username = _unitOfWork.UserRepository.GetUserById(x.UserId).Username,
					TweetId = x.TweetId,
					InteractionType = x.InteractionType
				}).ToList();
			}
			return list;
		}

		public void AddLikeToTweet(int tweetId)
		{
			var tweet = _unitOfWork.TweetRepository.GetTweetById(tweetId);
			tweet.LikeCount++;
			_unitOfWork.Commit();
		}

		public void RemoveLikeToTweet(int tweetId)
		{
			var tweet = _unitOfWork.TweetRepository.GetTweetById(tweetId);
			if (tweet != null && tweet.LikeCount > 0)
			{
				tweet.LikeCount--;
				_unitOfWork.Commit();
			}
		}

		public int GetLikeCountofTweet(int tweetId)
		{
			int count = _unitOfWork.TweetRepository.GetTweetById(tweetId).LikeCount;

			return count;
		}


		/// <summary>
		/// Tweet retweetlendiğinde user id ve tweet id db ye eklenir
		/// </summary>
		/// <param name="userId">tweeti retweetleyen kullanıcı id</param>
		/// <param name="tweetId">retweetlenen tweet id</param>
		public void AddRetweetInteraction(int userId, int tweetId)
		{
			var interaction = new Interaction
			{
				UserId = userId,
				Username = _unitOfWork.UserRepository.GetUserById(userId).Username,
				TweetId = tweetId,
				InteractionType = "Retweet"
			};

			_unitOfWork.InteractionRepository.Insert(interaction);
			_unitOfWork.Commit();

		}

		/// <summary>
		/// retweet geri çekildiğinde db den silinir
		/// </summary>
		/// <param name="userId">tweet retweeti silen kullanıcı id</param>
		/// <param name="tweetId">retweeti çekilen tweet id</param>
		public void RemoveRetweetInteraction(int userId, int tweetId)
		{
			var interaction = _unitOfWork.InteractionRepository.GetAll()
				.Where(x => x.InteractionType == "Retweet")
				.Where(x => x.UserId == userId)
				.Where(x => x.TweetId == tweetId)
				.FirstOrDefault();

			_unitOfWork.InteractionRepository.Delete(interaction);
			_unitOfWork.Commit();
		}

		public void AddRetweetToTweet(int tweetId)
		{
			var tweet = _unitOfWork.TweetRepository.GetTweetById(tweetId);
			tweet.RetweetCount++;
			_unitOfWork.Commit();
		}

		public void RemoveRetweetToTweet(int tweetId)
		{
			var tweet = _unitOfWork.TweetRepository.GetTweetById(tweetId);
			if (tweet != null && tweet.LikeCount > 0)
			{
				tweet.RetweetCount--;
				_unitOfWork.Commit();
			}
		}

		public int GetRetweetCountofTweet(int tweetId)
		{
			int count = _unitOfWork.TweetRepository.GetTweetById(tweetId).LikeCount;

			return count;
		}

		public List<InteractionDto> GetUserRetweetedTweets(int userid)
		{
			var data = _unitOfWork.InteractionRepository.GetAll().Where(x => x.InteractionType == "Retweet").Where(x => x.UserId == userid);
			var list = new List<InteractionDto>();

			if (data != null)
			{
				list = data.Select(x => new InteractionDto
				{
					Id = x.InteractionId,
					UserId = x.UserId,
					Username = _unitOfWork.UserRepository.GetUserById(x.UserId).Username,
					TweetId = x.TweetId,
					InteractionType = x.InteractionType
				}).ToList();
			}
			return list;
		}

		/// <summary>
		/// Kullanıcının retweetlediği tweetleri getirir
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public List<TweetDto> GetFollowedUsersRetweets(int userId)
		{
			// tweet id
			var data = _unitOfWork.InteractionRepository.GetAll().Where(x => x.InteractionType == "Retweet")
				.Where(x => x.UserId == userId).Select(x => x.TweetId);

			TweetService tweetService = new TweetService();

			var list = new List<TweetDto>();

			foreach (var item in data)
			{
				list.Add(tweetService.GetTweetById(item));
			}

			return list;
		}

		public InteractionDto GetInteractionRetweet(int userId)
		{
			var data = _unitOfWork.InteractionRepository.GetAll().Where(x => x.InteractionType == "Retweet")
				.Where(x => x.UserId == userId).FirstOrDefault();

			var dataDto = new InteractionDto();

			if (data != null)
			{
				dataDto = new InteractionDto
				{
					Id = data.InteractionId,
					UserId = data.UserId,
					Username = _unitOfWork.UserRepository.GetUserById(data.UserId).Username,
					TweetId = data.TweetId,
					InteractionType = data.InteractionType
				};
				
			}
			return dataDto;
		}

	}
}
