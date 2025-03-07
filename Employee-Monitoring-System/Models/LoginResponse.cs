using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Monitoring_System.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
