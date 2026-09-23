using System;
using System.Collections.Generic;
using System.Text;

namespace Exam.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
