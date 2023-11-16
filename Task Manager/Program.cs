using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskManager;

namespace TaskManager
{
    static class Program
    {
        private static Form activeForm; // Глобальная переменная для хранения ссылки на активную форму
        public static NotifyIcon notifyIcon = new NotifyIcon();
        public static Timer timer = new Timer();


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Создаем экземпляр формы
            var logInForm = new LogInForm();
            activeForm = logInForm; // Сохраняем ссылку на активную форму

            

            notifyIcon.Icon = logInForm.Icon;
            notifyIcon.DoubleClick+= (sender, e) =>
            {
                activeForm.Visible = true;
                activeForm.WindowState = FormWindowState.Normal;
            };

            notifyIcon.BalloonTipClicked += (sender, e) => 
            {
                activeForm.Visible = true;
                activeForm.WindowState = FormWindowState.Normal;
            };

            // Контекстное меню для значка в трее
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Открыть", (sender, e) =>
            {
                if (!activeForm.Visible)
                {
                    activeForm.Visible = true; // Показываем активную форму
                    activeForm.WindowState = FormWindowState.Normal; // Восстанавливаем окно, если оно было свернуто
                }
            });
            contextMenu.MenuItems.Add("Выход", (sender, e) =>
            {
                notifyIcon.Visible = false; // Убираем значок из трея
                Application.Exit(); // Закрываем приложение
            });
            notifyIcon.ContextMenu = contextMenu;

            notifyIcon.Visible = true; // Отображаем значок в трее

            timer.Interval = 60000; // Интервал в миллисекундах (1 минута)
            timer.Tick += (object sender, EventArgs e) =>
            {
                // Получение текущего пользователя
                int currentUserId = CurrentUser.UserId;

              
                DataTable tasks = PostgreManager.GetExpiredTasks(currentUserId);

                foreach (DataRow task in tasks.Rows)
                {
                    int taskId = Convert.ToInt32(task["ID"]);

                    PostgreManager.UpdateTaskStatus(taskId, -1);

                    // Отображение уведомления
                    string taskName = task["TaskName"].ToString();
                    string taskDescription = task["TaskDescription"].ToString();
                    PostgreManager.ShowTaskNotification(taskName, taskDescription);
                }
            };
            Application.Run(logInForm); // Запускаем обработку сообщений приложения
        }

        public static void SetActiveForm(Form form)
        {
            activeForm = form; // Обновляем ссылку на активную форму
        }
    }
}
