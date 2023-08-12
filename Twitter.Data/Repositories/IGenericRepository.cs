using System.Linq.Expressions;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public interface IGenericRepository<T> where T : class
	{
		IList<T> GetAll(); //get all 
		bool AuthenticateUser(string username, string password);
		IList<T> Where(Expression<Func<T, bool>> predicate);
		void Insert(T entity);
		void Delete(T entities);
		T GetById(int id);
		IEnumerable<T> Find(Expression<Func<T, bool>> expression);
	}
}
