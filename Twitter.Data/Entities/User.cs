using Azure;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter.Data.Entities
{
	public class User
	{
		///	<summary>
		///	Kullanıcı Id
		/// </summary>
		[Key]
		[Column("Id")]
		public int UserId { get; set; }

		///	<summary>
		///	Kullanıcı adı
		/// </summary>
		[Column("Username")]
		public string Username { get; set; }

		///	<summary>
		///	Kullanıcının İsmi
		/// </summary>
		[Column("Name")]
		public string Name { get; set; }


		///	<summary>
		///	Kullanıcının mail adresi
		/// </summary>
		[Column("E-mail")]
		public string Email { get; set; }

		///	<summary>
		///	Parola
		/// </summary>
		[Column("Password")]
		public string Password { get; set; }

		///	<summary>
		///	Kullanıcı Rolü
		/// </summary>
		[Column("Role")]
		public string UserRole { get; set; }

        /// <summary>
        /// kullanıcının kayıt tarihi
        /// </summary>
        public DateTime RegisterDate { get; set; }

        public ICollection<Tweet> Tweets { get; set; }
		public ICollection<Follow> Follows { get; set; }

	}
}
