
using Twitter.Data.Repositories;

namespace Twitter.Data.UnitOfWork
{
	public interface IUnitOfWork : IDisposable
	{
		IUserRepository UserRepository { get; }
		ITweetRepository TweetRepository { get; }
		IFollowRepository FollowRepository { get; }

		/// <summary>
		/// Commits all changes
		/// </summary>
		void Commit();

		/// <summary>
		/// Discards all changes that has not been commited
		/// </summary>
		void Rollback();

		void Dispose();

	}
}
