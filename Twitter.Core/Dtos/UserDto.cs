using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Twitter.Core.Dtos
{
	public class UserDto
	{
		///	<summary>
		///	Kullanıcı Id
		/// </summary>
		
		public int UserId { get; set; }

		///	<summary>
		///	Kullanıcı adı
		/// </summary>
		public string Username { get; set; }

		///	<summary>
		///	Kullanıcının İsmi
		/// </summary>
		public string Name { get; set; }

		///	<summary>
		///	Kullanıcının mail adresi
		/// </summary>

		public string Email { get; set; }

		///	<summary>
		///	Parola
		/// </summary>
		public string Password { get; set; }

		///	<summary>
		///	Kullanıcı rolü
		/// </summary>
		public string UserRole { get; set; }

        /// <summary>
        /// kullanıcı ne zaman kaydolmuş
        /// </summary>
        public DateTime RegisterDate { get; set; }
    }
}
