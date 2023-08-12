using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public interface ITweetRepository : IGenericRepository<Tweet>
	{
		void AddTweet(Tweet tweet);
		Tweet GetTweetById(int id);
		IEnumerable<Tweet> GetTweets();
		//IEnumerable<Tweet> GetUserTweets(int userId);
		//void AddTweetToUser(int userId, int tweetId);
		string GetTimeAgo(DateTime time);
		Tweet GetLastTweet();
	}
}
