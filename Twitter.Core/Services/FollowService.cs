using Twitter.Core.Dtos;
using Twitter.Data.Entities;
using Twitter.Data.UnitOfWork;

namespace Twitter.Core.Services
{
	public class FollowService
	{
		public IUnitOfWork _unitOfWork;

		public FollowService()
		{
			_unitOfWork = new UnitOfWork();
		}

		/// <summary>
		/// takip edilen ve takip eden user id'ler ile db ye ilişkilerle kaydet
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="myUserId"></param>
		public void AddFollowed(int userid, int myUserId)
		{
			var followedUser = new Follow
			{
				FollowedUserId = userid,
				FollowingUserId = myUserId,
				FollowDate = DateTime.Now.Date
			};

			_unitOfWork.FollowRepository.Insert(followedUser);
			_unitOfWork.Commit();
		}

		/// <summary>
		/// takipten çıkıldığında db den ilişkiyi silme servisi
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="myUserId"></param>
		public void DeleteFollowed(int userid, int myUserId)
		{
			var followedUser = _unitOfWork.FollowRepository.Where(x => x.FollowedUserId == userid)
				.Where(y => y.FollowingUserId == myUserId)
				.FirstOrDefault();

			_unitOfWork.FollowRepository.Delete(followedUser);
			_unitOfWork.Commit();
		}

		/// <summary>
		/// kullanıcının takip ettiği kullanıcıları getirir
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		public List<FollowDto> GetMyFollowed(UserDto dto)
		{
			var data = _unitOfWork.FollowRepository.GetAll().Where(x => x.FollowingUserId == dto.UserId);
			var list = new List<FollowDto>();
			if (data != null)
			{
				list = data.Select(x => new FollowDto
				{
					Id = x.Id,
					FollowedUserId = x.FollowedUserId,
					FollowingUserId = x.FollowingUserId,
					FollowDate = x.FollowDate
				}).ToList();
			}
			return list;
		}


		public List<FollowDto> GetAll()
		{
			var data = _unitOfWork.FollowRepository.GetAll();
			var list = new List<FollowDto>();
			if (data != null)
			{
				list = data.Select(x => new FollowDto
				{
					Id = x.Id,
					FollowedUserId = x.FollowedUserId,
					FollowingUserId = x.FollowingUserId,
					FollowDate = x.FollowDate
				}).ToList();
			}
			return list;
		}

		/// <summary>
		/// aylık en çok takipçisi olan kullanıcı
		/// </summary>
		/// <param name="monthYear"></param>
		/// <returns>username</returns>
		public string WhoHaveMoreFollowers(string monthYear)
		{
			var userid = _unitOfWork.FollowRepository.GetAll()
				.Where(record => record.FollowDate.ToString("MM/yyyy") == monthYear)
				.GroupBy(record => record.FollowedUserId)
				.OrderByDescending(group => group.Count())
				.Select(group => group.Key)
				.FirstOrDefault();

			UserService userService = new UserService();
			var username = userService.GetUserById(userid).Username;

			return username;
		}

		/// <summary>
		/// takip edilen sayısını getirir
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public int GetUsersFollowing(int userid)
		{
			UserService userService = new UserService();
			int count = _unitOfWork.FollowRepository.GetAll()
				.Where(record => record.FollowingUserId == userid)
				.Count();

			return count;
		}

		/// <summary>
		/// takipçi sayısını getirir 
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public int GetUsersFollowers(int userid) 
		{
			UserService userService = new UserService();
			int count = _unitOfWork.FollowRepository.GetAll()
				.Where(record => record.FollowedUserId == userid)
				.Count();

			return count;
		}

		/// <summary>
		/// kullanıcının takip ettiği kullanıcı id listesini getirr
		/// </summary>
		/// <param name="id">giriş yapmış olan kullanıcı</param>
		/// <returns>takip ettiği kullanıcı listesi</returns>
		public List<int> GetFollowedUsers(int id)
		{
			var list = _unitOfWork.FollowRepository.GetAll()
				.Where(x => x.FollowingUserId == id)
				.Select(x => x.FollowedUserId)
				.ToList();

			return list;
		}

	}
}
