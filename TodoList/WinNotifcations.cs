using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static TodoList.ImageManager;

namespace TodoList
{
    public class WindowsNotifications
    {
        public static void SendNotification(string header, string content)
        {
            /*var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode("Title"));
            stringElements[1].AppendChild(toastXml.CreateTextNode("Content"));*/
        }
    }
}
