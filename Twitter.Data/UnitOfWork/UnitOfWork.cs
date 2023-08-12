using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Twitter.Data.Contexts;
using Twitter.Data.Repositories;

namespace Twitter.Data.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork, IDisposable
	{
		private TwitterContext _context;
		private IUserRepository _userRepo;
		private ITweetRepository _tweetRepo;
		private IFollowRepository _followRepo;
		public IUserRepository UserRepository
		{
			get { return _userRepo = _userRepo ?? new UserRepository(_context); }
		}
		public ITweetRepository TweetRepository
		{
			get { return _tweetRepo = _tweetRepo ?? new TweetRepository(_context); }
		}
		public IFollowRepository FollowRepository
		{
			get { return _followRepo = _followRepo ?? new FollowRepository(_context); }
		}

		
		public UnitOfWork()
		{
			_context = new TwitterContext();
		}

		public void Commit()
		{
			_context.SaveChanges();
		}
		public void Dispose()
		{
			_context.Dispose();
		}

		public void Rollback()
		{
			_context.Dispose();
		}
	}

}
