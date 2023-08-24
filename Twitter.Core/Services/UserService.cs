using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Globalization;
using System.Security.Claims;
using Twitter.Core.Dtos;
using Twitter.Data.Entities;
using Twitter.Data.UnitOfWork;

namespace Twitter.Core.Services
{
	public class UserService
	{
		public IUnitOfWork _unitOfWork;
		public UserService()
		{
			_unitOfWork = new UnitOfWork();
		}

		/// <summary>
		/// yeni kullanıcı ekler
		/// </summary>
		/// <param name="userDto"></param>
		public void AddUser(UserDto userDto)
		{
			var user = new User
			{
				Email = userDto.Email,
				Username = userDto.Username,
				Password = userDto.Password,
				Name = userDto.Name,
				UserRole = "user",
                RegisterDate = DateTime.Now.Date
            };

			_unitOfWork.UserRepository.Insert(user);
			_unitOfWork.Commit();
		}

		/// <summary>
		/// id ile user dto getirir
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public UserDto GetUserById(int Id)
		{
			var userEntity = _unitOfWork.UserRepository.GetUserById(Id);
			
			return MapUserEntityToDTO(userEntity);
		}

		public UserDto GetUserByUsername(string username)
		{
			var userEntity = _unitOfWork.UserRepository.GetUserByUsername(username);
			
			return MapUserEntityToDTO(userEntity);
		}

		/// <summary>
		/// tweet id ile user dto getirir
		/// </summary>
		/// <param name="tweetId"></param>
		/// <returns></returns>
		public UserDto GetUserByTweetId(int tweetId)
		{
			TweetService tweetService = new TweetService();
			TweetDto tweetDto = tweetService.GetTweetById(tweetId);

			UserDto userDto = GetUserById(tweetDto.UserId);

			return userDto;
		}

		/// <summary>
		/// giriş yapılan bilgilerle eşleşen kullanıcı var mı yok mu
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public bool AuthenticateUserLogin(string username, string password)
		{
			var user = GetUserByUsername(username);
			
			if (user != null)
			{
				return _unitOfWork.UserRepository.AuthenticateUser(username, password);
				
			}
			return false;
		}

		/// <summary>
		/// kullanıcı admin mi değil mi diye bakar
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool IsAdmin(int userId)
		{
			var user = GetUserById(userId);
			if (user.UserRole == "admin")
			{
				return true;
			}
			return false;
		}

		public List<UserDto> GetTodayRegisteredUsers()
		{
            var data = _unitOfWork.UserRepository.GetAll().Where(x => x.RegisterDate.Date == DateTime.Now.Date);
            var list = new List<UserDto>();
            if (data != null)
            {
                list = data.Select(x => new UserDto
                {
                    UserId = x.UserId,
                    Name = x.Name,
                    Email = x.Email,
                    Username = x.Username,
                    Password = x.Password,
                    UserRole = x.UserRole,
                    RegisterDate = x.RegisterDate

                }).ToList();
            }
            return list;
        }

		/// <summary>
		/// kayıt tarihlerini ay/yıl şeklinde getirir
		/// </summary>
		/// <returns></returns>
		public List<string> GetRegisterDates()
		{
			var data = _unitOfWork.UserRepository.GetAll().Select(x => x.RegisterDate);

			var list = new List<string>();
			if (data != null)
			{
				foreach (var x in data)
				{
					if (!list.Contains(x.ToString("MM/yyyy")))
					{
						list.Add(x.ToString("MM/yyyy"));
					}
					
				}
			}
			return list;
		}

		/// <summary>
		/// tüm kullanıcı sayısını getirir
		/// </summary>
		/// <returns></returns>
		public int UserCount(string monthYear)
		{
			DateTime targetDate;

			if (DateTime.TryParseExact(monthYear, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
			{
				var users = _unitOfWork.UserRepository.GetAll();
				var count = users.Count(user => user.RegisterDate.Year == targetDate.Year && user.RegisterDate.Month == targetDate.Month);
				return count;
			}
			else
			{
				throw new ArgumentException("Invalid monthYear format. Please use MM/yyyy format.");
			}
		}

		/// <summary>
		/// username list getirir
		/// </summary>
		/// <returns></returns>
		public List<string> GetAllUsersUsername()
		{
			var usernameList = GetAll().OrderBy(x => x.RegisterDate).Select(x => x.Username).ToList();

			return usernameList;
		}

		/// <summary>
		/// username ile userid getirir
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public int GetUserId(string username)
		{
			int userId = GetUserByUsername(username).UserId;

			return userId;
		}

		

		/// <summary>
		/// Entity den Dto tipine çevirir
		/// </summary>
		/// <param name="userEntity"></param>
		/// <returns></returns>
		private UserDto MapUserEntityToDTO(User userEntity)
		{
			return new UserDto
			{
				UserId = userEntity.UserId,
				Username = userEntity.Username,
				Password = userEntity.Password,
				Name = userEntity.Name,
				Email = userEntity.Email,
				UserRole = userEntity.UserRole,
				RegisterDate = userEntity.RegisterDate
            };
		}

		/// <summary>
		/// tüm userları getirir
		/// </summary>
		/// <returns></returns>
		public List<UserDto> GetAll()
		{
			var data = _unitOfWork.UserRepository.GetAll();
			var list = new List<UserDto>();
			if (data != null)
			{
				list = data.Select(x => new UserDto
				{
					UserId = x.UserId,
					Name = x.Name,
					Email = x.Email,
					Username = x.Username,
					Password = x.Password,
					UserRole= x.UserRole,
					RegisterDate= x.RegisterDate
				}).ToList();
			}
			return list;
		}

		/// <summary>
		/// giriş yapan kullanıcı dışındaki kullanıcıları listeler
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public List<UserDto> GetUsersExceptGivenId(int id)
		{
			var data = _unitOfWork.UserRepository.GetAll().Where(x => x.UserId != id);
			var list = new List<UserDto>();
			if (data != null)
			{
				list = data.Select(x => new UserDto
				{
					UserId = x.UserId,
					Name = x.Name,
					Email = x.Email,
					Username = x.Username,
					Password = x.Password,
					UserRole = x.UserRole,
					RegisterDate= x.RegisterDate	
				}).ToList();
			}
			return list;
		}

	}
}
