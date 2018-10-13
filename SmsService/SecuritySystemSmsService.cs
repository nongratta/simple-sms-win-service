using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mercury.Services.FrameWork;
using Serilog;
using SmsService.Properties;

namespace SmsService
{
    partial class SecuritySystemSmsService : ServiceBase
    {
        private static Timer _timer;
        private bool _lastSyncDone;
        private int? _lastCheckId = null;

        public SecuritySystemSmsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Workspace.Initilize("SecuritySystemSmsService",
                Settings.Default.EmailHost, 
                Settings.Default.SmtpPort,
                Settings.Default.EmailLogin,
                Settings.Default.EmailPassword,
                Settings.Default.EmailLogin,
                Settings.Default.NotificatedEmail);

            Log.Information("Служба оповещения SMS по проходной запущена.");

            StartTimer();
        }

        protected override void OnStop()
        {
            StopTimer();
            Log.Information($"Служба оповещения SMS по проходной остановлена");
        }

        private void StartTimer()
        {
            try
            {
                TimerCallback callBack = ScheduledCallBack;

                _timer = new Timer(callBack, null, 0, Settings.Default.UpdateTimeInterval);
                Log.Information("Таймер запущен.");
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(StartTimer)}: {ex.Message}");
            }
        }


        public void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                Log.Information("Таймер остановлен.");
            }
        }

        private void ScheduledCallBack(object state)
        {
            try
            {
                if (_lastSyncDone)
                {
                    Log.Information("Проверка прервана т.к. не завершилась предыдущая проверка");
                    return;
                }

                _lastSyncDone = true;

                Log.Information("Проверка запущена.");
                try
                {
                    SmsSpammer spammer = new SmsSpammer();
                    _lastCheckId = spammer.SpamSms(_lastCheckId);
                }
                catch (Exception e)
                {
                    Log.Error($"При синхронизации логов произошла ошибка {@e.Message}");
                    Log.Warning("Синхронизация прервана");
                }

                Log.Information("Проверка завершена");

            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(StartTimer)}: {ex.Message}");
                _lastCheckId = null;    // сбрасываем, чтобы дальше не сыпались ошибки на этой записи
            }
            finally
            {
                _lastSyncDone = false; // в любом случае, завершаем процесс, разблокируя семафор
            }
        }
    }
}
