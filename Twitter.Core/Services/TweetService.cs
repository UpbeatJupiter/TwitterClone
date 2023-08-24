using System.Globalization;
using Twitter.Core.Dtos;
using Twitter.Data.Entities;
using Twitter.Data.UnitOfWork;

namespace Twitter.Core.Services
{
	public class TweetService
	{
		public IUnitOfWork _unitOfWork;

		public TweetService()
		{
			_unitOfWork = new UnitOfWork();
		}

		/// <summary>
		/// db ye yeni tweet ekler
		/// </summary>
		/// <param name="dto"></param>
		public void AddTweet(TweetDto dto)
		{
			var tweet = new Tweet
			{
				Id = dto.TweetId,
				TweetContent = dto.TweetContent,
				TweetDate = DateTime.Now,
				UserId = dto.UserId
			};

			_unitOfWork.TweetRepository.Insert(tweet);
			_unitOfWork.Commit();
		}

		/// <summary>
		/// db den tweet siler
		/// </summary>
		/// <param name="dto"></param>
		public void DeleteTweet(int tweetid)
		{
			var tweet = _unitOfWork.TweetRepository.Where(x => x.Id == tweetid).FirstOrDefault();
			if (tweet != null)
			{
				_unitOfWork.TweetRepository.Delete(tweet);
				_unitOfWork.Commit();
			}
		}

		/// <summary>
		/// İstenilen kullanıcının tüm tweetlerini getir
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		public List<TweetDto> GetUserTweets(int id)
		{
			var data = _unitOfWork.TweetRepository.GetAll().Where(x => x.UserId == id);
			var list = new List<TweetDto>();
			if (data != null)
			{
				list = data.Select(x => new TweetDto
				{
					TweetContent = x.TweetContent,
					UserId = x.UserId,
					Username = _unitOfWork.UserRepository.GetUserById(x.UserId).Username,
					TweetId = x.Id,
					TweetDate = _unitOfWork.TweetRepository.GetTimeAgo(x.TweetDate)
				}).ToList();
			}
			return list;
		}

		/// <summary>
		/// bu ay kullanıcının attığı tweet sayısı
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int GetUsersTweetsThisMonth(int id)
		{
			var tweetCount = GetAllTweetsWithDateTime().Where(x => x.UserId == id)
				.Where(x => x.TweetDate == DateTime.Now.Date.ToString("MM-yyyy")).Count();

			return tweetCount;
		}

		/// <summary>
		/// takip edilen kullanıcıların tweetleri gelir
		/// </summary>
		/// <param name="userid"></param>
		/// <returns></returns>
		public List<TweetDto> GetFollowedTweets(int userid)
		{
			//takip edilen kullanıcı id leri listesi
			var data = _unitOfWork.FollowRepository.GetAll().Where(x => x.FollowingUserId == userid).Select(y => y.FollowedUserId).ToList();
			
			//tweetdto tipinde tweet listesi
			var list = new List<TweetDto>();

			if (data != null)
			{
				//her bir id için tweetdto oluştur listeye at
				foreach (var x in data)
				{
					list.AddRange(GetUserTweets(x));
				}
			}
			return list;
		}

		public List<TweetDto> GetAll()
		{
			var data = _unitOfWork.TweetRepository.GetAll();
			var list = new List<TweetDto>();
			if (data != null)
			{
				list = data.Select(x => new TweetDto
				{
					TweetContent = x.TweetContent,
					UserId = x.UserId,
					Username = _unitOfWork.UserRepository.GetUserById(x.UserId).Username,
					TweetId = x.Id,
					TweetDate = _unitOfWork.TweetRepository.GetTimeAgo(x.TweetDate)
				}).ToList();
			}
			return list;
		}

		public List<TweetDto> GetAllTweetsWithDateTime()
		{
			var data = _unitOfWork.TweetRepository.GetAll();
			var list = new List<TweetDto>();
			if (data != null)
			{
				list = data.Select(x => new TweetDto
				{
					TweetContent = x.TweetContent,
					UserId = x.UserId,
					Username = _unitOfWork.UserRepository.GetUserById(x.UserId).Username,
					TweetId = x.Id,
					TweetDate = x.TweetDate.ToString("MM-yyyy")
				}).ToList();
			}
			return list;
		}
		/// <summary>
		/// En son atılan tweet i getirir
		/// </summary>
		/// <returns></returns>
		public TweetDto GetLastTweet()
		{
			var tweetentity = _unitOfWork.TweetRepository.GetLastTweet();
			return MapTweetEntityToDTO(tweetentity);
		}


		public TweetDto GetTweetById(int Id)
		{
			var tweetentity = _unitOfWork.TweetRepository.GetTweetById(Id);

			return MapTweetEntityToDTO(tweetentity);
		}

		/// <summary>
		/// tüm tweet sayısını getirir
		/// </summary>
		/// <returns>count</returns>
		public int TweetCount(string monthYear)
		{
			DateTime targetDate;

			if (DateTime.TryParseExact(monthYear, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
			{
				var tweets = _unitOfWork.TweetRepository.GetAll();
				var count = tweets.Count(tweet => tweet.TweetDate.Year == targetDate.Year && tweet.TweetDate.Month == targetDate.Month);
				return count;
			}
			else
			{
				throw new ArgumentException("Invalid monthYear format. Please use MM/yyyy format.");
			}
		}

		/// <summary>
		/// Entity den Dto tipine çevirir
		/// </summary>
		/// <param name="tweetEntity"></param>
		/// <returns></returns>
		private TweetDto MapTweetEntityToDTO(Tweet tweetEntity)
		{
			return new TweetDto
			{
				UserId = tweetEntity.UserId,
				TweetId = tweetEntity.Id,
				TweetContent = tweetEntity.TweetContent,
				TweetDate = tweetEntity.TweetDate.ToString()
			};
		}

		
	}
}
