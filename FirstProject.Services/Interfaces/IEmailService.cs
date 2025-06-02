using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstProject.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendResetLink(string to, string subject, string body);
    }
}
