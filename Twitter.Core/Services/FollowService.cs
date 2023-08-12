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
				FollowingUserId = myUserId
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
					FollowingUserId = x.FollowingUserId
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
					FollowingUserId = x.FollowingUserId
				}).ToList();
			}
			return list;
		}
	}
}
