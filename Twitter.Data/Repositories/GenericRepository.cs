using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Twitter.Data.Contexts;
using Twitter.Data.Entities;

namespace Twitter.Data.Repositories
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		protected readonly TwitterContext _context;
		public GenericRepository(TwitterContext context)
		{
			_context = context;
		}

		public IList<T> GetAll()
		{
			return _context.Set<T>().ToList();
		}

		public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
		{
			return _context.Set<T>().Where(expression);
		}
		public T GetById(int id)
		{
			return _context.Set<T>().Find(id);
		}

		public bool AuthenticateUser(string username, string password)
		{
			var exist = _context.Users.Where(x => username.Contains(x.Username) && password.Contains(x.Password)).FirstOrDefault();

			if (exist != null)
			{
				return true;
			}
			return false;
		}

		public IList<T> Where(Expression<Func<T, bool>> predicate)
		{
			var result = _context.Set<T>().Where(predicate);
			return result.ToList();
		}

		public void Insert(T entity)
		{
			_context.Set<T>().Add(entity);
		}

		public void Delete(T entities)
		{
			_context.Set<T>().Remove(entities);
		}

	}
}
