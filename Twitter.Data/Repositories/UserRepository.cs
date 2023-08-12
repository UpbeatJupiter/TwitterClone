using Twitter.Data.Contexts;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public class UserRepository : GenericRepository<User>, IUserRepository
	{
		protected readonly TwitterContext _context;

		public UserRepository(TwitterContext context) : base(context)
		{
			_context = context;
		}

		public void AddUser(User user)
		{
			_context.Users.Add(user);
		}

		public User GetUserById(int id)
		{
			return _context.Users.Where(x => x.UserId == id).FirstOrDefault();
		}
		public User GetUserByUsername(string username)
		{
			 return _context.Users.Where(x => x.Username == username).FirstOrDefault();
		}
		public IEnumerable<User> GetUsers()
		{
			return _context.Users;
		}
	}
}
