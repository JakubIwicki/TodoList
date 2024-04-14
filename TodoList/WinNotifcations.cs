using Windows.UI.Notifications;

namespace TodoList
{
    public class WindowsNotifications
    {
        public static void SendNotification(string header, string content)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(header));
            stringElements[1].AppendChild(toastXml.CreateTextNode(content));

            var toast = new ToastNotification(toastXml);

            ToastNotificationManager.CreateToastNotifier("TodoList").Show(toast);
        }
    }
}
