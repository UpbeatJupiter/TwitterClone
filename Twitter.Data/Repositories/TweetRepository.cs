using System;
using Twitter.Data.Contexts;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public class TweetRepository : GenericRepository<Tweet>, ITweetRepository
	{
		protected readonly TwitterContext _context;
		public TweetRepository(TwitterContext context) : base(context)
		{
			_context = context;
		}

		public void AddTweet(Tweet tweet)
		{
			_context.Tweets.Add(tweet);
		}

		public Tweet GetTweetById(int id)
		{
			return _context.Tweets.Where(x => x.Id == id).FirstOrDefault();
		}

		public IEnumerable<Tweet> GetTweets()
		{
			return _context.Tweets;
		}

		public Tweet GetLastTweet()
		{
			return _context.Tweets.OrderByDescending(x => x.Id).First();
		}

		public string GetTimeAgo(DateTime dateTime)
		{
			TimeSpan timeDifference = DateTime.Now - dateTime;

			if (timeDifference.TotalSeconds >= 31536000)
			{
				int yearsAgo = (int)(timeDifference.TotalSeconds / 31536000);
				return $"{yearsAgo} {(yearsAgo == 1 ? "year" : "years")} ago";
			}

			if (timeDifference.TotalSeconds >= 2592000)
			{
				int monthsAgo = (int)(timeDifference.TotalSeconds / 2592000);
				return $"{monthsAgo} {(monthsAgo == 1 ? "month" : "months")} ago";
			}

			if (timeDifference.TotalSeconds >= 604800) 
			{
				int weeksAgo = (int)(timeDifference.TotalSeconds / 604800);
				return $"{weeksAgo} {(weeksAgo == 1 ? "week" : "weeks")} ago";
			}

			if (timeDifference.TotalSeconds >= 86400)
			{
				int daysAgo = (int)(timeDifference.TotalSeconds / 86400);
				return $"{daysAgo} {(daysAgo == 1 ? "day" : "days")} ago";
			}

			if (timeDifference.TotalSeconds >= 3600)
			{
				int hoursAgo = (int)(timeDifference.TotalSeconds / 3600);
				return $"{hoursAgo} {(hoursAgo == 1 ? "hour" : "hours")} ago";
			}

			if (timeDifference.TotalSeconds >= 60)
			{
				int minutesAgo = (int)(timeDifference.TotalSeconds / 60);
				return $"{minutesAgo} {(minutesAgo == 1 ? "minute" : "minutes")} ago";
			}
			if(timeDifference.TotalSeconds < 1)
			{
				return $"now";
			}
			return $"{(int)timeDifference.TotalSeconds} seconds ago";
		}

		
	}
}
