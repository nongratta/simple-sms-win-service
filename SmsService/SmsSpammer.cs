using System;
using System.Linq;
using System.Data.Entity;
using Serilog;

namespace SmsService
{
    /// <summary>
    /// Спаммер СМС по задачам
    /// </summary>
    class SmsSpammer
    {
        /// <summary>
        /// главный метод рассыльщика. Рассылает начиная с указанного ID трафика
        /// </summary>
        /// <param name="lastCheckId"></param>
        /// <returns></returns>
        public int SpamSms(int? lastCheckId)
        {
            if (lastCheckId == null)
            {
                // первый запус, нужно просто найти последний ID и дальше отслеживать
                using (SecuritySystemContext ctx = new SecuritySystemContext())
                {
                    return ctx.vTrafficByUsers.Max(a => a.id);
                }
            }

            // стандартная процедура проверки
            using (SecuritySystemContext ctx = new SecuritySystemContext())
            {
                var checkList = ctx.vTrafficByUsers.Where(a => a.id > lastCheckId).ToList();
                if (checkList.Any())
                {
                    var smsSpamTasks = ctx.SmsTasks.Include(a => a.SmsTasksByPassPoints).ToList();

                    foreach (var item in checkList
                    ) // список событий должен быть короче обычно списка заданий... 1-2 записи против 3-4. Так быстрее
                    {
                        var smsTasks = smsSpamTasks.Where(a =>
                                a.LastName.Clean() == item.LastName.Clean() && // ищем по ФИО
                                a.FirstName.Clean() == item.FirstName.Clean() &&
                                a.MiddleName.Clean() == item.MiddleName.Clean() &&
                                (a.SmsTasksByPassPoints.Any(aa =>
                                     aa.PassPointId == item.Door) || // дальше или в списке дверей
                                 a.OnEnter &&
                                 item.DoorIsEntry == true)) // или проход через входную дверь и мы ждем такие события
                            .ToList();

                        // для всех найденных задач выполняем рассылку
                        foreach (var task in smsTasks)
                        {
                            var message = Ext1.GetMessage(item.datetime.Value, item.IsEntry ?? false,
                                Ext1.GetFio(item.LastName, item.MiddleName, item.FirstName),
                                item.Alias ?? item.DoorDesc);
                            SmsWrapper.SendTo(task.Phone, message);

                            Log.Information("Отправлено сообщение по задаче {Name} на телефон {Phone}. {Message}",
                                task.Name, task.Phone, message);
                        }
                    }

                    lastCheckId = checkList.Max(a => a.id);
                }
            }

            return lastCheckId.Value;
        }


    }

    /// <summary>
    /// Вспомогательные функции
    /// </summary>
    public static class Ext1
    {
        public static string Clean(this string text)
        {
            if (text == null)
                return string.Empty;

            return text.Trim().ToUpper();
        }

        public static string GetFio(string lastName, string middleName, string firstName)
        {
            string str = string.Empty;

            str += lastName;
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                str += $" {firstName[0]}.".ToUpper();
                if (!string.IsNullOrWhiteSpace(middleName))
                    str += $"{middleName[0]}.".ToUpper();
            }

            return str;
        }

        public static string GetMessage(DateTime date, bool isEntry, string fio, string doorDescription)
        {
            return $"{date:t} - {(isEntry ? "Вход" : "Выход")} {fio} - {doorDescription}";
        }
    }

}
