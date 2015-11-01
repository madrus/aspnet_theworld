using System;
using System.Diagnostics;

namespace TheWorld.Services
{
    public class MailService : IMailService
    {
        public bool SendMail(string to, string from, string subject, string body)
        {
            Console.WriteLine("Not yet implemented, so we return true");
            return true;
        }
    }
}