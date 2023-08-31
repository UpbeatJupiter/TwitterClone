using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Humanizer;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Office.Interop.Word;
using NonFactors.Mvc.Grid;
using OfficeOpenXml;
using QRCoder;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Twitter.Core.Dtos;
using Twitter.Core.Services;
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

		#region Get Session

		public const string SessionKeyName = "CurrentUsername";
		public const string SessionKeyId = "CurrentUserid";
		public void GetSession(UserDto user)
		{

			if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
			{
				HttpContext.Session.SetString(SessionKeyName, user.Username);
				HttpContext.Session.SetInt32(SessionKeyId, user.UserId);
			}
			var currentUsername = HttpContext.Session.GetString(SessionKeyName);
			var currentUserid = HttpContext.Session.GetInt32(SessionKeyId).ToString();

			_logger.LogInformation("Session Username: {currentUsername}", currentUsername);
			_logger.LogInformation("Session User id: {currentUserid}", currentUserid);

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

				GetSession(user);

				PdfManipulator(user);
				ConvertToPdf();
				string qrText = "Username: " + user.Username + "\nPassword:" + user.Password;
				GenerateQRCodeImage(qrText, 20);

				if (user.UserRole == "admin")
				{

					WriteToExcelDoc(userService);
				}

				return RedirectToAction("HomePage");
			}
			else //kullanıcı bilgileri yanlışsa
			{
				return RedirectToAction("Login"); // TODO: ajax error 
			}
		}
		#endregion


		#region Pdf Manupilator

		public void PdfManipulator(UserDto userDto)
		{
			string input = "inputWord.docx";
			string result = "result.docx";
			string resultPDF = "resultPDF.pdf";

			string qrImagePath = "output.png";  // Path to save the PNG image

			//QR generator
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			string qrText = $"Username: {userDto.Username}" + Environment.NewLine + $"Password: {userDto.Password}";
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
			QRCode qrCode = new QRCode(qrCodeData);
			Bitmap qrCodeImage = qrCode.GetGraphic(20);

			// Save the BMP image as a PNG image
			qrCodeImage.Save(qrImagePath, ImageFormat.Png);

			using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(input, false)) // Open in read-only mode
			{
				string docText = null;
				using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
				{
					docText = sr.ReadToEnd();
				}

				// Perform replacements
				docText = docText.Replace("@date@", DateTime.Today.ToString("dd-MM-yyyy"));
				docText = docText.Replace("@name@", userDto.Name);


				// Create a new Word document with modified content
				using (WordprocessingDocument newWordDoc = WordprocessingDocument.Create(result, WordprocessingDocumentType.Document))
				{
					MainDocumentPart mainPart = newWordDoc.AddMainDocumentPart();
					using (StreamWriter sw = new StreamWriter(mainPart.GetStream()))
					{
						sw.Write(docText);
						sw.Write('\n');

					}
				}
			}
		}

		public string ReplacePlaceholders(string originalText, string placeholder, string replacement)
		{
			return originalText.Replace(placeholder, replacement);
		}

		

		private readonly string wordDocumentPath = "result.docx"; // Provide the actual path to your Word document.
		private readonly string pdfFilePath = @"C:\Users\Elif Tuncer\source\repos\Twitter\Twitter.UI\PdfFiles"; //PdfFiles adında Twitter.UI a klasör aç

		//To use this instal itext7 7.x.x version
		public void ConvertToPdf()
		{
			string pdfFileName = "ConvertedDocument.pdf"; //oluşan pdf
			string pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "PdfFiles", pdfFileName); //çalışan directorynin path i ve pdffiles dosyasını kombine eder

			using (MemoryStream wordStream = new MemoryStream(System.IO.File.ReadAllBytes(wordDocumentPath)))
			{
				bool conversionResult = ConvertWordToPdf(wordStream, pdfFilePath);

				if (conversionResult)
				{
					byte[] pdfBytes = System.IO.File.ReadAllBytes(pdfFilePath); //pdfbytes
					
				}
				else
				{
					Console.WriteLine("Word to PDF conversion failed.");
				}
			}
		}

		public static bool ConvertWordToPdf(Stream wordStream, string pdfFilePath)
		{
			using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(wordStream, false))
			{
				using (var pdfWriter = new iText.Kernel.Pdf.PdfWriter(pdfFilePath))
				{
					using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfWriter))
					{
						var paragraphs = wordDocument.MainDocumentPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().ToList();

						using (var document = new iText.Layout.Document(pdfDocument))
						{
							foreach (var paragraph in paragraphs)
							{
								if (paragraph.InnerXml.Contains("w:lastRenderedPageBreak"))
								{
									document.Add(new iText.Layout.Element.AreaBreak());
								}
								var text = paragraph.InnerText;
								document.Add(new iText.Layout.Element.Paragraph(text));
								
							}
						}
					}
				}
			}

			return true;

		}
		
		public void GenerateQRCodeImage(string text, int size)
		{
			string input = @"C:\Users\Elif Tuncer\source\repos\Twitter\Twitter.UI\PdfFiles\ConvertedDocument.pdf";
			string result = @"C:\Users\Elif Tuncer\source\repos\Twitter\Twitter.UI\PdfFiles\DocumentWithQR.pdf";
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
			QRCode qrCode = new QRCode(qrCodeData);

			Bitmap qrCodeImage = qrCode.GetGraphic(size);
			using (MemoryStream ms = new MemoryStream())
			{
				qrCodeImage.Save(ms, ImageFormat.Png);
				byte[] qrCodeBytes = ms.ToArray();

				using (var inputPdfStream = new System.IO.FileStream(input, System.IO.FileMode.Open))
				using (var resultPdfStream = new System.IO.FileStream(result, System.IO.FileMode.Create))
				{
					var reader = new PdfReader(inputPdfStream);
					var stamper = new PdfStamper(reader, resultPdfStream);

					for (int i = 1; i <= reader.NumberOfPages; i++)
					{
						//Get nth page
						var page = reader.GetPageN(i);
						var pageSize = reader.GetPageSizeWithRotation(i);

						//Load the Qr code image into iTextSharp's Image class
						var qrCodeImageObj = iTextSharp.text.Image.GetInstance(qrCodeBytes);
						qrCodeImageObj.ScaleToFit(100, 100);

						//Add the Qr code image to the bottom-right corner of the page
						qrCodeImageObj.SetAbsolutePosition(pageSize.Width - qrCodeImageObj.ScaledWidth - 20, 20);

						//Get the content of the current page
						var contentByte = stamper.GetOverContent(i);
						contentByte.AddImage(qrCodeImageObj);
					}
					stamper.Close();
					reader.Close();

				}
			}
		}

		#endregion


		#region WriteToExcel

		/// <summary>
		/// aylık rapor için excel
		/// </summary>
		public void WriteToExcelDoc(UserService userService)
		{
			FollowService followService = new FollowService();
			TweetService twitterService = new TweetService();

			using (ExcelPackage excel = new ExcelPackage())
			{
				excel.Workbook.Worksheets.Add("Monthly Report");
				excel.Workbook.Worksheets.Add("Users Report");

				//to add a header row
				List<string[]> headerRow = new List<string[]>()
				{
					new string[] { "Tarih", "Kaydolan Kullanıcılar", "Toplam Kullanıcı", "En Çok Takipçisi Olan", "Tweet Sayısı" }
				};

				// Determine the header range (e.g. A1:D1)
				string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

				// Target a worksheet
				var worksheet = excel.Workbook.Worksheets["Monthly Report"];

				// Popular header row data
				worksheet.Cells[headerRange].LoadFromArrays(headerRow);

				//Style the row
				worksheet.Cells[headerRange].Style.Font.Bold = true;
				worksheet.Cells[headerRange].Style.Font.Size = 14;
				worksheet.Columns.AutoFit(); //autofit

				//write data to rows
				var date = RegisterDates(userService);
				date.Sort();       // Tarihleri eskiden yeniye sıralama

				int dateRows = 2; //date başlangıç satırı
				
				int userCount = 0;
				int totalUsers = 0;
				int totalTweets = 0;

				foreach (var item in date)
				{
					worksheet.Cells[dateRows, 1].Value = item; //    ay/yıl

					totalUsers = TotalUsers(item, userService);
					worksheet.Cells[dateRows, 2].Value = totalUsers;

					userCount += totalUsers;
					worksheet.Cells[dateRows, 3].Value = userCount; //toplam kullanıcı

					string userHaveMoreFollowers = WhoHaveMoreFollowers(item, followService);
					worksheet.Cells[dateRows, 4].Value = userHaveMoreFollowers; //En çok takipçisi olan kullanıcı

					totalTweets += TotalTweets(item, twitterService);
					worksheet.Cells[dateRows, 5].Value = totalTweets; //Toplam tweet sayısı

					dateRows++;
				}


				//----------------------Diğer Worksheet--------------------------


				//to add a header row
				List<string[]> headerRowUserDetails = new List<string[]>()
				{
					new string[] {"Username", "Takip Edilen", "Takipçi", "Tweet Sayısı", "Bu Ay Atılan Tweetler", "Etkileşim" }
				};

				// Determine the header range (e.g. A1:G1)
				string headerRangeUserDetails = "A1:" + Char.ConvertFromUtf32(headerRowUserDetails[0].Length + 64) + "1";

				// Target a worksheet
				var worksheetUserDetails = excel.Workbook.Worksheets["Users Report"];

				// Popular header row data
				worksheetUserDetails.Cells[headerRangeUserDetails].LoadFromArrays(headerRowUserDetails);

				//Style the row
				worksheetUserDetails.Cells[headerRangeUserDetails].Style.Font.Bold = true;
				worksheetUserDetails.Cells[headerRangeUserDetails].Style.Font.Size = 14;
				worksheetUserDetails.Columns.AutoFit(); //autofit

				//write data to rows
				var usernameList = AllUsersList(userService); //tüm usernameler


				int dateRowsUserDetails = 2; //date başlangıç satırı

				

				foreach (var item in usernameList)
				{
					var userid = userService.GetUserId(item);

					worksheetUserDetails.Cells[dateRowsUserDetails, 1].Value = item;        //username

					worksheetUserDetails.Cells[dateRowsUserDetails, 2].Value = followService.GetUsersFollowing(userid); //takip edilen sayısı

					worksheetUserDetails.Cells[dateRowsUserDetails, 3].Value = followService.GetUsersFollowers(userid); //takipçi sayısı

					worksheetUserDetails.Cells[dateRowsUserDetails, 4].Value = twitterService.GetUserTweets(userid).Count; // tweet sayısı

					worksheetUserDetails.Cells[dateRowsUserDetails, 5].Value = twitterService.GetUsersTweetsThisMonth(userid); // bu ay atılan tweet sayısı

					dateRowsUserDetails++;
				}



				FileInfo excelFile = new FileInfo(@"C:\Users\Elif Tuncer\source\repos\Twitter\Twitter.UI\AdminExcel.xlsx");
				excel.SaveAs(excelFile);
			}
		}

		/// <summary>
		/// aylık kayıt tarihleri
		/// </summary>
		/// <returns>string list</returns>
		public List<string> RegisterDates(UserService userService)
		{
			var list = userService.GetRegisterDates();

			return list;
		}

		/// <summary>
		///  toplam katılımcı sayısı
		/// </summary>
		/// <param name="mounthYear">mm-yyyy</param>
		/// <returns>int</returns>
		public int TotalUsers(string mounthYear, UserService userService)
		{
			var count = userService.UserCount(mounthYear);

			return count;
		}

		/// <summary>
		/// Ayın en çok takipçisi olan kullanıcı
		/// </summary>
		/// <param name="mounthYear"></param>
		/// <returns>username</returns>
		public string WhoHaveMoreFollowers(string mounthYear, FollowService followService)
		{
			string username = followService.WhoHaveMoreFollowers(mounthYear);

			return username;
		}

		/// <summary>
		/// Aylık toplam tweet
		/// </summary>
		/// <param name="mounthYear"></param>
		/// <returns></returns>
		public int TotalTweets(string mounthYear, TweetService tweetService)
		{
			int count = tweetService.TweetCount(mounthYear);

			return count;	
		}

		public List<string> AllUsersList(UserService userService)
		{
			var list = userService.GetAllUsersUsername();

			return list;	
		}

		#endregion


		#region HomePage
		//GET: /Home/HomePage
		public IActionResult HomePage()
		{
			TweetService tweetService = new TweetService();
			UserService userService = new UserService();
			InteractionService interactionService = new InteractionService();

			var username = HttpContext.Session.GetString("CurrentUsername");

			if (username != null)
			{
				UserDto dto = userService.GetUserByUsername(username);

				GetSession(dto);

				//giriş yapan kullanıcının tüm tweetleri 
				List<TweetDto> tweetList = tweetService.GetUserTweets(dto.UserId);

				//giriş yapan kullanıcı dışındaki kullanıcıların listesi
				List<UserDto> userList = userService.GetUsersExceptGivenId(dto.UserId);
				ViewBag.UserList = userList;

				//giriş yapan kullanıcının takip ettiği kullanıcılar listesi
				List<TweetDto> followedUserTweets = tweetService.GetFollowedTweets(dto.UserId);

				ViewBag.Like = interactionService.GetUserLikedTweets(dto.UserId).ToList();

				#region Session

				GetSession(dto);

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
					ViewBag.Like = interactionService.GetUserLikedTweets(dto.UserId).ToList();

				}
				else //giriş yapan kullanıcının takip ettiği kullanıcılar varsa
				{
					ListOfTweetsMethod(tweetList, followedUserTweets, userService, interactionService);

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
			InteractionService interactionService = new InteractionService();	

			UserDto userDto = userService.GetUserById(myUserId);

			//kullanıcının tüm tweetlerini getirir
			List<TweetDto> tweetList = tweetService.GetUserTweets(userDto.UserId);

			//takip ettiği kullanıcıların tweetlerini getirir
			List<TweetDto> followedUserTweets = tweetService.GetFollowedTweets(userDto.UserId);

			ListOfTweetsMethod(tweetList, followedUserTweets, userService, interactionService);

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

		#region Like Tweet

		[HttpPost]
		public JsonResult LikeTweet(int userid, int tweetid)
		{
			if(userid != 0 && tweetid != 0)
			{
				InteractionService interactionService = new InteractionService();

				interactionService.AddLikeInteraction(userid, tweetid);

				return Json(true);
			}
			return Json(false);
		}
		#endregion

		#region UnLike Tweet

		[HttpPost]
		public JsonResult UnlikeTweet(int userid, int tweetid)
		{
			if(userid !=0 && tweetid != 0)
			{
				InteractionService interactionService = new InteractionService();

				interactionService.RemoveLikeInteraction(userid, tweetid);

				return Json(true);
			}
			return Json(false);
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
		public void ListOfTweetsMethod(List<TweetDto> myTweetList, List<TweetDto> myFollowedList, UserService userService, InteractionService interactionService)
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
			int myuserid = myTweetList.Select(x => x.UserId).FirstOrDefault();
			ViewBag.Like = interactionService.GetUserLikedTweets(myuserid).ToList();
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

			var session = (HttpContext.Session.GetString("CurrentUsername") == null ? "Sessionımız var" : "Sessionımız yok");

			return RedirectToAction("HomePage", user);
		}
		#endregion



		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}