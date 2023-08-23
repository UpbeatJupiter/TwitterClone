using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public interface IUserRepository : IGenericRepository<User>
	{
		void AddUser(User user);
		IEnumerable<User> GetUsers();
		User GetUserById(int userId);
		User GetUserByUsername(string username);
		int UserCount();
	}
}
