using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NonFactors.Mvc.Grid;
using OfficeOpenXml;
using QRCoder;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Twitter.Core.Dtos;
using Twitter.Core.Services;
using Twitter.UI.Helper;
using Twitter.UI.Models;

namespace Twitter.UI.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}


		#region Link to Index-Login-Logout-Signup-Review
		public IActionResult Index()
		{
			return View("Login");
		}

		// GET: Home/Login
		public IActionResult Login()
		{
			return View();
		}

		// GET: Home/Logout
		public IActionResult Logout()
		{
			//HttpContext.Session.Remove("user");
			HttpContext.Session.Clear();
			//HttpContext.SignOutAsync();
			return RedirectToAction("Login");
		}

		// GET: Home/Register
		public IActionResult Register()
		{
			return View();
		}

		// GET: Home/HomePage
		//public ActionResult HomePage() 
		//{
		//	return View();
		//}

		// GET: Home/Review
		public IActionResult Review(int id)
		{
			ViewBag.id = id;
			return View("Review");
		}

		#endregion

		#region Export
		// GET: Home/AdminExportFile
		public IActionResult AdminExportFile()
		{
			UserService userService = new UserService();
			List<UserDto> todayRegisteredUsers = new List<UserDto>();

			todayRegisteredUsers = userService.GetTodayRegisteredUsers();

			return View(todayRegisteredUsers);
		}


		[HttpGet]
		public FileContentResult ExportIndex()
		{
			return Export(CreateExportableGrid(), "UsersRegisteredToday");
		}

		// Using EPPlus from nuget.
		// Export grid method can be reused for all grids.
		private FileContentResult Export(IGrid grid, string fileName)
		{
			int col = 1;
			using ExcelPackage package = new ExcelPackage();
			ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Data");

			foreach (IGridColumn column in grid.Columns)
			{
				sheet.Cells[1, col].Value = column.Title;
				sheet.Column(col++).Width = 18;

				column.IsEncoded = false;
			}

			foreach (IGridRow<Object> row in grid.Rows)
			{
				col = 1;

				foreach (IGridColumn column in grid.Columns)
					sheet.Cells[row.Index + 2, col++].Value = column.ValueFor(row);
			}

			return File(package.GetAsByteArray(), "application/unknown", $"{fileName}.xlsx");
		}
		private IGrid<UserDto> CreateExportableGrid()
		{
			UserService userService = new UserService();
			IGrid<UserDto> grid = new Grid<UserDto>(userService.GetTodayRegisteredUsers());
			grid.ViewContext = new ViewContext { HttpContext = HttpContext };
			grid.Query = Request.Query;

			grid.Columns.Add(model => model.Username).Titled("Username");
			grid.Columns.Add(model => model.Name).Titled("Name");
			grid.Columns.Add(model => model.Email).Titled("Email");
			grid.Columns.Add(model => model.RegisterDate).Titled("Registeration Date").Formatted("{0:d}");

			// Pager should be excluded on export if all data is needed.
			grid.Pager = new GridPager<UserDto>(grid);
			grid.Processors.Add(grid.Pager);
			grid.Processors.Add(grid.Sort);
			grid.Pager.RowsPerPage = 6;

			foreach (IGridColumn column in grid.Columns)
			{
				column.Filter.IsEnabled = true;
				column.Sort.IsEnabled = true;
			}

			return grid;
		}

		#endregion

		#region Register Page
		/// <summary>
		/// Yeni bir kullanıcı oluşturur
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult Register(UserDto dto)
		{
			UserService userService = new UserService();
			userService.AddUser(dto);

			return Json(new { success = true });
		}
		#endregion

		#region Login Page
		/// <summary>
		/// Kullanıcı kayıtlıysa anasayfayı getirir, 
		/// kullanıcı kayıtlı değilse tekrar login sayfasını getirir.
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult Login(UserDto dto)
		{
			UserService userService = new UserService();

			//giriş yapılmak istenilen bilgilerde bir user var mı yok mu bakılır
			bool isUserExists = userService.AuthenticateUserLogin(dto.Username, dto.Password);


			//giriş yapılmak istenilen kullanıcı mevcutsa
			if (isUserExists)
			{
				//username ile userdto oluşturulur
				UserDto user = userService.GetUserByUsername(dto.Username);

				// Set data
				SessionHelper.SetObjectAsJson(HttpContext.Session, "userid", user.UserId);

				PdfManipulator(user);

				return RedirectToAction("HomePage", "Home");
			}
			else //kullanıcı bilgileri yanlışsa
			{
				return RedirectToAction("Login", "Home");
			}
		}
		#endregion

		//#region Create QR
		//public void CreateQR(UserDto userDto)
		//{
		//	//QR generator
		//	QRCodeGenerator qrGenerator = new QRCodeGenerator();
		//	string qrText = $"Username: {userDto.Username}" + Environment.NewLine + $"Password: {userDto.Password}";
		//	QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
		//	QRCode qrCode = new QRCode(qrCodeData);
		//	Bitmap qrCodeImage = qrCode.GetGraphic(20);
		//}
		//#endregion

		#region Pdf Manupilator

		public void PdfManipulator(UserDto userDto)
		{
			string input = "input.pdf";
			string result = "result.pdf";

			//QR generator
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			string qrText = $"Username: {userDto.Username}" + Environment.NewLine + $"Password: {userDto.Password}";
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
			QRCode qrCode = new QRCode(qrCodeData);
			Bitmap qrCodeImage = qrCode.GetGraphic(20);

			// Convert QR code image to a byte array
			using (MemoryStream stream = new MemoryStream())
			{
				qrCodeImage.Save(stream, ImageFormat.Png);
				byte[] qrCodeBytes = stream.ToArray();

				using (var inputPdfStream = new System.IO.FileStream(input, System.IO.FileMode.Open))
				using (var resultPdfStream = new System.IO.FileStream(result, System.IO.FileMode.Create))
				{
					var reader = new PdfReader(inputPdfStream);
					var stamper = new PdfStamper(reader, resultPdfStream);

					for (int i = 1; i <= reader.NumberOfPages; i++)
					{
						// Get the nth page
						var page = reader.GetPageN(i);
						var pageSize = reader.GetPageSizeWithRotation(i);

						//existing text
						var existingText = PdfTextExtractor.GetTextFromPage(reader, i, new SimpleTextExtractionStrategy());

						// Replace placeholders in existing text
						var newText = existingText.Replace("@name@", userDto.Name);
						newText = newText.Replace("@date@", DateTime.Today.ToString("dd-MM-yyyy"));

						// Load the QR code image into iTextSharp's Image class
						var qrCodeImageObj = iTextSharp.text.Image.GetInstance(qrCodeBytes);
						qrCodeImageObj.ScaleToFit(100, 100); // Adjust size as needed

						// Add the QR code image to the bottom-right corner of the page
						qrCodeImageObj.SetAbsolutePosition(pageSize.Width - qrCodeImageObj.ScaledWidth - 20, 20);

						// Get the content of the current page
						var contentByte = stamper.GetOverContent(i);
						contentByte.AddImage(qrCodeImageObj);

						var contentBytes = stamper.GetUnderContent(i);

						// Create a ColumnText object to add the modified text
						var columnText = new ColumnText(contentBytes);
						var font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
						columnText.SetSimpleColumn(50, 50, pageSize.Width - 50, pageSize.Height - 50);
						
						// Set font and size
						contentByte.SetFontAndSize(font, 12);

						// Calculate position based on your requirements
						float x = 50;
						float y = pageSize.Height - 50;

						contentByte.BeginText();
						contentByte.SetTextMatrix(x, y);
						contentByte.ShowText(newText);
						contentByte.EndText();

						// Add the modified text to the page
						columnText.Go();
					}
					stamper.Close();
					reader.Close();
				}
			}


		}
		#endregion

		#region HomePage
		public IActionResult HomePage()
		{
			TweetService tweetService = new TweetService();
			UserService userService = new UserService();

			int id = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "userid");

			if (id != 0)
			{
				UserDto dto = userService.GetUserById(id);

				//giriş yapan kullanıcının tüm tweetleri 
				List<TweetDto> tweetList = tweetService.GetUserTweets(dto.UserId);

				//giriş yapan kullanıcı dışındaki kullanıcıların listesi
				List<UserDto> userList = userService.GetUsersExceptGivenId(dto.UserId);
				ViewBag.UserList = userList;

				//giriş yapan kullanıcının takip ettiği kullanıcılar listesi
				List<TweetDto> followedUserTweets = tweetService.GetFollowedTweets(dto.UserId);

				#region Session
				//session ekle
				HttpContext.Session.SetInt32("userId", dto.UserId);

				HttpContext.Session.SetString("username", dto.Username);

				// Get Data
				//int userId = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "userid");

				#endregion

				//giriş yapan kullanıcının hiçbir takip ettiği kullanıcı yoksa
				if (followedUserTweets == null)
				{
					//kullanıcının tüm tweetlerini yeniden eskiye doğru sıralar
					ViewBag.Tweets = tweetList.OrderByDescending(x => x.TweetId).ToList();
					ViewBag.TweetsTotal = tweetList.Count();

					//Username Listesi
					List<string> usernameList = new List<string>
				{
					dto.Username
				};

					ViewBag.Username = usernameList;

					//takip edilen kullanıcı varsa onların userid listesi = null
					ViewBag.FollowedUserList = followedUserTweets.Select(x => x.UserId).ToList();

				}
				else //giriş yapan kullanıcının takip ettiği kullanıcılar varsa
				{
					ListOfTweetsMethod(tweetList, followedUserTweets, userService);

					//takip edilen kullanıcıların userid listesi
					ViewBag.FollowedUserList = followedUserTweets.Select(x => x.UserId).ToList();
				}
				return View("HomePage", dto);
			}
			else
			{
				return RedirectToAction("Login");
			}



		}
		#endregion

		#region AllTweets
		public IActionResult AllTweets(int myUserId)
		{
			UserService userService = new UserService();
			TweetService tweetService = new TweetService();

			UserDto userDto = userService.GetUserById(myUserId);

			//kullanıcının tüm tweetlerini getirir
			List<TweetDto> tweetList = tweetService.GetUserTweets(userDto.UserId);

			//takip ettiği kullanıcıların tweetlerini getirir
			List<TweetDto> followedUserTweets = tweetService.GetFollowedTweets(userDto.UserId);

			ListOfTweetsMethod(tweetList, followedUserTweets, userService);

			return View("AllTweetPV", userDto);
		}
		#endregion

		#region Post Tweet
		/// <summary>
		/// Yeni tweet atıldığında o tweeti kaydedip ekrana yazdırır.
		/// </summary>
		/// <param name="tweetdto"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult PostTweets(TweetDto tweetdto)
		{
			TweetService tweetService = new TweetService();

			//tweeti servis aracılığıyla db ye ekler
			tweetService.AddTweet(tweetdto);

			return Json(true);

			////tweetin idsini dbye kaydettikten sonra alır
			//tweetdto.TweetId = tweetService.GetLastTweet().TweetId;

		}
		#endregion

		#region Delete Tweet
		/// <summary>
		/// Tweeti siler ve ekrandan kaldırır.
		/// </summary>
		/// <param name="tweetid"> silinmek istenen tweet id </param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult DeleteTweet(int tweetid)
		{
			if (tweetid != 0)
			{
				TweetService tweetService = new TweetService();

				//istenilen tweeti siler
				tweetService.DeleteTweet(tweetid);

				return Json(true);

			}

			return Json(false);
		}
		#endregion

		#region Follow User
		/// <summary>
		/// Takip etmek istenilen kullanıcı takip edilir.
		/// </summary>
		/// <param name="userid"> takip edilen </param>
		/// <param name="myUserId"> takip eden </param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult FollowUser(int userid, int myUserId)
		{
			if (userid != 0 && myUserId != 0)
			{
				FollowService followService = new FollowService();

				//takip etmek istenilen kulanıcı db ye eklenir
				followService.AddFollowed(userid, myUserId);

				return Json(true);

			}

			return Json(false);
		}
		#endregion

		#region Unfollow User
		/// <summary>
		/// Takipten çıkılmak istenilen kullanıcı takipten çıkılır.
		/// </summary>
		/// <param name="userid">takip edilen</param>
		/// <param name="myUserId">takip eden</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult UnfollowUser(int userid, int myUserId)
		{
			FollowService followService = new FollowService();

			//takipten çılan kullanıcıyı db den siler
			followService.DeleteFollowed(userid, myUserId);

			return Json(true);

		}
		#endregion

		#region Followed and My Tweets
		/// <summary>
		/// Takip edilen kullanıcıların ve kendi tweetlerimizi getirir.
		/// </summary>
		/// <param name="tweetList"></param>
		/// <param name="followedList"></param>
		/// <param name="userService"></param>
		/// <param name="dto"></param>
		public void ListOfTweetsMethod(List<TweetDto> myTweetList, List<TweetDto> myFollowedList, UserService userService)
		{
			List<TweetDto> tweets = new List<TweetDto>();

			//benim takip ettiklerimin listesi
			if (myFollowedList != null)
			{
				tweets.AddRange(myFollowedList);
			}

			//benim tweetlerim
			if (myTweetList != null)
			{
				tweets.AddRange(myTweetList);
			}

			ViewBag.Tweets = tweets.OrderByDescending(x => x.TweetId).ToList();
			ViewBag.TweetsTotal = tweets.Count();
			ViewBag.Followed = myFollowedList;

			//giriş yapan ve takip edilen kullanıcıların userid listesi
			List<int> userIdList = tweets.Select(x => x.UserId).ToList();

			//giriş yapan ve takip edilen kullanıcıların username listesi
			List<string> usernameList = new List<string>();

			foreach (var item in userIdList)
			{
				usernameList.Add(userService.GetUserById(item).Username);
			}

			ViewBag.Username = usernameList;
		}
		#endregion

		#region Write To Google Sheets
		/// <summary>
		/// kullanıcı yorumlarını google sheets e aktarma
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public IActionResult SendToSheets(int userid, string review)
		{
			string[] Scopes = { SheetsService.Scope.Spreadsheets };
			string ApplicationName = ".Net Core To Sheets";
            string SpreadsheetId = "1Mt6ZoK_ysLzeLPIaTk1rJ8K_BtYIX5jLoWioToOnPcs";
            string sheet = "Sheet1";

			SheetsService service;

			GoogleCredential credential;
			using (var stream = new FileStream("google-credentials.json", FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream)
					.CreateScoped(Scopes);
			}

			service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			var range = $"{sheet}!A:F";
			var valueRange = new ValueRange();

			var objectList = new List<object>() { $"{userid}", $"{review}" };
			valueRange.Values = new List<IList<object>> { objectList };

			var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var appendResponse = appendRequest.Execute();

			UserService userService = new UserService();
			var user = userService.GetUserById(userid);

			return View("HomePage", user);
		}
		#endregion



		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}