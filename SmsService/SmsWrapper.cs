using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Mercury.Services.FrameWork;
using Serilog;

namespace SmsService
{
    internal class SmsWrapper
    {
        public static void SendTo(string phone, string message)
        {
            string login = Properties.Settings.Default.SmsLogin;
            string password = Properties.Settings.Default.SmsPassword;

            WebResponse result = null;
            WebRequest req = null;
            Stream newStream = null;
            Stream ReceiveStream = null;
            StreamReader sr = null;

            try
            {
                phone = phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
                // Url запрашиваемого методом POST скрипта
                //req = WebRequest.Create("http://www.websms.ru/http_in5.asp");
                req = WebRequest.Create("http://cab.websms.ru/http_in5.asp");
                req.Method = "POST";
                req.Timeout = 15000;

                // эта строка необходима только при защите скрипта на сервере Basic авторизацией
                //req.Credentials = new NetworkCredential("login", "password");
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] SomeBytes = null;

                // передаем список пар параметров / значений для запрашиваемого скрипта методом POST
                // в случае нескольких параметров необходимо использовать символ & для разделения параметров
                // в данном случае используется кодировка windows-1251 для Url кодирования спец. символов значения параметров
                SomeBytes = Encoding.GetEncoding(1251).GetBytes("http_username=" + login + "&http_password=" + password + "&phone_list=" + phone + "&message=" + HttpUtility.UrlEncode(message, Encoding.GetEncoding(1251)) + "&format=xml&fromPhone=TBSS");
                req.ContentLength = SomeBytes.Length;
                newStream = req.GetRequestStream();
                newStream.Write(SomeBytes, 0, SomeBytes.Length);
                newStream.Close();


                // считываем результат работы
                result = req.GetResponse();
                ReceiveStream = result.GetResponseStream();
                Encoding encode = Encoding.GetEncoding(1251);
                sr = new StreamReader(ReceiveStream, encode);

                XmlDocument XD = new XmlDocument();
                XD.Load(sr);
                Log.Debug("Получен результат работы запрошенного методом POST скрипта");
                XmlNode x1 = XD.ChildNodes[1];
                XmlNode xnhttpIn = x1.ChildNodes[0];
                XmlNode xnsms = xnhttpIn.LastChild;

                string error_num = xnhttpIn.Attributes["error_num"].Value;
                string message_id = xnsms.Attributes["message_id"].Value;

                if (error_num != "0")
                {
                    Log.Error("Send sms.. Error request");
                }
                else if (error_num == "0")
                { Log.Debug("Send sms..Request complete"); }

                #region отправка сообщения при недостатке баланса
                if ((error_num.IndexOf("Не хватает средств для рассылки") >= 0) || (Convert.ToInt32(error_num) == 3))
                {
                    Workspace.Email.SendAdminEmail("Не хватает средств для рассылки СМC.", "Не хватает средств для рассылки СМC.");
                }
                #endregion
            }

            catch (Exception ex)
            {
                Log.Information("Ошибка: " + ex.Message, ex);
            }
            finally
            {
                if (newStream != null)
                    newStream.Close();
                if (ReceiveStream != null)
                    ReceiveStream.Close();
                if (sr != null)
                    sr.Close();
                if (result != null)
                    result.Close();
            }
        }
    }
}