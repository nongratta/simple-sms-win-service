using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Mercury.Services.FrameWork;
using SmsService.Properties;

namespace SmsService
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return; // режим только для тестирования

            if (args[0] != "test")
                return;// режим только для тестирования

            Workspace.Initilize("SecuritySystemSmsService",
                Settings.Default.EmailHost,
                Settings.Default.SmtpPort,
                Settings.Default.EmailLogin,
                Settings.Default.EmailPassword,
                Settings.Default.EmailLogin,
                Settings.Default.NotificatedEmail);

            //ServiceBase[] servicesToRun;
            //servicesToRun = new ServiceBase[]
            //{
            //    new SecuritySystemSmsService()
            //};
            //ServiceBase.Run(servicesToRun);

            SmsSpammer spammer = new SmsSpammer();
            spammer.SpamSms(4219986);
        }
    }
}
