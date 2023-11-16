using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Task_Manager;
using Siticone.Desktop.UI.WinForms;

namespace TaskManager
{
    public partial class MainForm : Form
    {
        // Единственный экземпляр MainForm
        private static MainForm instance;
        SiticoneTextBox searchTextBox = new SiticoneTextBox();


        public MainForm() // Обновленный конструктор
        {
            InitializeComponent();
            InitializeComponents();
            InitializeTasks(searchTextBox.Text);
            
        }

        private void InitializeComponents()
        {
            // Приложение не закрывается при закрытии формы
            this.FormClosing += (sender, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true; // Отменяем закрытие формы
                    this.Hide(); // Скрываем форму

                }
            };

            

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            //this.TopMost = true;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            int formWidth = this.Width;
            int formHeight = this.Height;

            button1.Location = new Point(235, formHeight - 100 - button1.Height);
            button2.Location = new Point(formWidth - formWidth / 2 - button2.Width / 2 - 200, formHeight - 100 - button2.Height);
            button3.Location = new Point(formWidth - 100 - button3.Width, formHeight - 100 - button3.Height);
            button4.Location = new Point(formWidth - formWidth / 2 - button2.Width / 2 + 200, formHeight - 100 - button2.Height);


            // Привязка обработчика события Shown
            this.Activated += MainForm_Activated;

            // Создание TextBox для поля поиска
            searchTextBox.Size = new Size(230*6+80, 40); // Установите нужный размер
            searchTextBox.Location = new Point(233, 5); // Установите нужные координаты
            searchTextBox.TextChanged += SearchTextBox_TextChanged; // Привязка обработчика события изменения текста
            this.Controls.Add(searchTextBox); // Добавление TextBox на форму

        }

        private void InitializeTasks(string searchText)
        {
            // Создание и настройка TabControl
            var tabControl = new SiticoneTabControl();
            tabControl.Dock = DockStyle.Fill;
            

            DataTable tasksTable = PostgreManager.GetTasks();

            // Группировка задач по категориям
            var tasksByCategory = tasksTable.AsEnumerable()
                .GroupBy(row => row.Field<int>("CategoryId"))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Создание прокручиваемой панели для вкладки "Все"
            Panel scrollableAllPagePanel = new Panel { AutoScroll = true, Width = 1920, Height = 1080 - 200, Dock=DockStyle.Top };
            TabPage allPage = new TabPage("Все") { };
            allPage.Controls.Add(scrollableAllPagePanel);

            int taskCount = 0;
            int rowCount = 0;
            foreach (var categoryId in tasksByCategory.Keys)
            {
                foreach (DataRow row in tasksByCategory[categoryId])
                {
                    string taskName = row["taskName"].ToString();
                    DateTime endTime = Convert.ToDateTime(row["TaskDueDate"]);
                    string description = row["TaskDescription"].ToString();
                    int status = Convert.ToInt16(row["status"]);

                    // Проверка, соответствует ли задача поисковому запросу
                    if (!(taskName.Contains(searchText) || description.Contains(searchText)))
                    {
                        continue; // Пропускаем задачу, если она не соответствует поисковому запросу
                    }
                    

                    string truncatedDescription = description.Length > 68 ? description.Substring(0, 68) + "..." : description;

                    // Создание контейнерного элемента (Panel)
                    Panel compositeElement = new Panel();
                    compositeElement.Size = new Size(200, 200);
                    compositeElement.Location = new Point(50 + 230 * taskCount, 50 + 210 * rowCount);
                    compositeElement.BackColor = Color.AliceBlue;
                    compositeElement.BorderStyle = BorderStyle.FixedSingle;

                    // Создание label1
                    var label1 = new Label();
                    label1.AutoSize = true;
                    label1.Size = new Size(150, 20);
                    label1.Location = new Point(20, 20);
                    label1.Text = taskName;
                    

                    // Создание label2
                    var label2 = new Label();
                    label2.AutoSize = true;
                    label2.Size = new Size(150, 30);
                    label2.Location = new Point(20, 50);
                    label2.Text = truncatedDescription;


                    // Создание label3
                    var label3 = new Label();
                    label3.AutoSize = true;
                    label3.Location = new Point(20, 90);
                    label3.Text = endTime.ToString();
                    label3.ForeColor = label1.ForeColor;

                    // Создание кнопки редактирования
                    var editButton = new SiticoneButton();
                    editButton.Text = "Редактировать";
                    editButton.Location = new Point(20, 130);
                    editButton.Size = new Size(150,20);
                    editButton.BorderRadius = 10;
                    editButton.Tag = row["ID"];
                    editButton.Click += EditButton_Click;
                    
                    // Создание кнопки удаления
                    var deleteButton = new SiticoneButton();
                    deleteButton.Text = "Удалить";
                    deleteButton.Location = new Point(20, 155);
                    deleteButton.Size = new Size(150,20);
                    deleteButton.BorderRadius = 10;
                    deleteButton.Tag = row["ID"];
                    deleteButton.Click += DeleteButton_Click;

                    //Создание markButton
                    var markButton = new SiticoneButton();
                    markButton.Size = new Size(150, 20);
                    markButton.BorderRadius = 10;
                    markButton.Location = new Point(20, 176);
                    markButton.Text = status == -1 ? "Настало время" : status == 0 ? "Ждём" : "Выполнено";
                    markButton.Tag = row["ID"];
                    markButton.Click += MarkButton_Click;


                    // Добавление элементов на контейнерный элемент
                    compositeElement.Controls.Add(label1);
                    compositeElement.Controls.Add(label2);
                    compositeElement.Controls.Add(label3);
                    compositeElement.Controls.Add(markButton);
                    compositeElement.Controls.Add(deleteButton);
                    compositeElement.Controls.Add(editButton);

                    scrollableAllPagePanel.Controls.Add(compositeElement);

                    taskCount++;
                    if (taskCount % 7 == 0)
                    {
                        taskCount = 0;
                        rowCount++;
                    }
                }
            }

            tabControl.TabPages.Add(allPage);

            Panel scrollableCompletedPagePanel = new Panel { AutoScroll = true, Width = 1920, Height = 1080 - 200, Dock=DockStyle.Top };
            TabPage completedPage = new TabPage("Выполнено") { };
            completedPage.Controls.Add(scrollableCompletedPagePanel);
            taskCount = 0;
            rowCount = 0;
            foreach (var categoryId in tasksByCategory.Keys)
            {
                foreach (DataRow row in tasksByCategory[categoryId])
                {
                    string taskName = row["taskName"].ToString();
                    DateTime endTime = Convert.ToDateTime(row["TaskDueDate"]);
                    string description = row["TaskDescription"].ToString();
                    int status = Convert.ToInt16(row["status"]);

                    if(status != 1)
                    {
                        continue;
                    }

                    // Проверка, соответствует ли задача поисковому запросу
                    if (!(taskName.Contains(searchText) || description.Contains(searchText)))
                    {
                        continue; // Пропускаем задачу, если она не соответствует поисковому запросу
                    }


                    string truncatedDescription = description.Length > 68 ? description.Substring(0, 68) + "..." : description;

                    // Создание контейнерного элемента (Panel)
                    Panel compositeElement = new Panel();
                    compositeElement.Size = new Size(200, 200);
                    compositeElement.Location = new Point(50 + 230 * taskCount, 50 + 210 * rowCount);
                    compositeElement.BackColor = Color.AliceBlue;
                    compositeElement.BorderStyle = BorderStyle.FixedSingle;

                    // Создание label1
                    var label1 = new Label();
                    label1.AutoSize = true;
                    label1.Size = new Size(150, 20);
                    label1.Location = new Point(20, 20);
                    label1.Text = taskName;


                    // Создание label2
                    var label2 = new Label();
                    label2.AutoSize = true;
                    label2.Size = new Size(150, 30);
                    label2.Location = new Point(20, 50);
                    label2.Text = truncatedDescription;


                    // Создание label3
                    var label3 = new Label();
                    label3.AutoSize = true;
                    label3.Location = new Point(20, 90);
                    label3.Text = endTime.ToString();
                    label3.ForeColor = label1.ForeColor;

                    // Создание кнопки редактирования
                    var editButton = new SiticoneButton();
                    editButton.Text = "Редактировать";
                    editButton.Location = new Point(20, 130);
                    editButton.Size = new Size(150, 20);
                    editButton.BorderRadius = 10;
                    editButton.Tag = row["ID"];
                    editButton.Click += EditButton_Click;

                    // Создание кнопки удаления
                    var deleteButton = new SiticoneButton();
                    deleteButton.Text = "Удалить";
                    deleteButton.Location = new Point(20, 155);
                    deleteButton.Size = new Size(150, 20);
                    deleteButton.BorderRadius = 10;
                    deleteButton.Tag = row["ID"];
                    deleteButton.Click += DeleteButton_Click;

                    //Создание markButton
                    var markButton = new SiticoneButton();
                    markButton.Size = new Size(150, 20);
                    markButton.BorderRadius = 10;
                    markButton.Location = new Point(20, 176);
                    markButton.Text = status == -1 ? "Настало время" : status == 0 ? "Ждём" : "Выполнено";
                    markButton.Tag = row["ID"];
                    markButton.Click += MarkButton_Click;


                    // Добавление элементов на контейнерный элемент
                    compositeElement.Controls.Add(label1);
                    compositeElement.Controls.Add(label2);
                    compositeElement.Controls.Add(label3);
                    compositeElement.Controls.Add(markButton);
                    compositeElement.Controls.Add(deleteButton);
                    compositeElement.Controls.Add(editButton);

                    scrollableCompletedPagePanel.Controls.Add(compositeElement);

                    taskCount++;
                    if (taskCount % 7 == 0)
                    {
                        taskCount = 0;
                        rowCount++;
                    }
                }
            }

            tabControl.TabPages.Add(completedPage);

            foreach (var categoryId in tasksByCategory.Keys)
            {
                // Создание вкладки и добавление DataGridView на вкладку
                TabPage tabPage = new TabPage(PostgreManager.GetCategoryName(categoryId)) { UseVisualStyleBackColor = true };

                // Создание прокручиваемой панели для каждой вкладки
                Panel scrollableTabPagePanel = new Panel { AutoScroll = true, Width = 1920, Height = 1080 - 200, Dock = DockStyle.Top };
                tabPage.Controls.Add(scrollableTabPagePanel);

                taskCount = 0;
                rowCount = 0;
                foreach (DataRow row in tasksByCategory[categoryId])
{
                    string taskName = row["taskName"].ToString();
                    DateTime endTime = Convert.ToDateTime(row["TaskDueDate"]);
                    string description = row["TaskDescription"].ToString();
                    int status = Convert.ToInt16(row["status"]);

                    // Проверка, соответствует ли задача поисковому запросу
                    if (!(taskName.Contains(searchText) || description.Contains(searchText)))
                    {
                        continue; // Пропускаем задачу, если она не соответствует поисковому запросу
                    }

                    string truncatedDescription = description.Length > 68 ? description.Substring(0, 68) + "..." : description;

                    // Создание контейнерного элемента (Panel)
                    Panel compositeElement = new Panel();
                    compositeElement.Size = new Size(200, 200);
                    compositeElement.Location = new Point(50 + 230 * taskCount, 50 + 210 * rowCount);
                    compositeElement.BackColor = Color.AliceBlue;
                    compositeElement.BorderStyle = BorderStyle.FixedSingle;

                    // Создание label1
                    var label1 = new Label();
                    label1.AutoSize = true;
                    label1.Size = new Size(150, 20);
                    label1.Location = new Point(20, 20);
                    label1.Text = taskName;


                    // Создание label2
                    var label2 = new Label();
                    label2.AutoSize = true;
                    label2.Size = new Size(150, 30);
                    label2.Location = new Point(20, 50);
                    label2.Text = truncatedDescription;


                    // Создание label3
                    var label3 = new Label();
                    label3.AutoSize = true;
                    label3.Location = new Point(20, 90);
                    label3.Text = endTime.ToString();
                    label3.ForeColor = label1.ForeColor;

                    // Создание кнопки редактирования
                    var editButton = new SiticoneButton();
                    editButton.Text = "Редактировать";
                    editButton.Location = new Point(20, 130);
                    editButton.Size = new Size(150, 20);
                    editButton.BorderRadius = 10;
                    editButton.Tag = row["ID"];
                    editButton.Click += EditButton_Click;

                    // Создание кнопки удаления
                    var deleteButton = new SiticoneButton();
                    deleteButton.Text = "Удалить";
                    deleteButton.Location = new Point(20, 155);
                    deleteButton.Size = new Size(150, 20);
                    deleteButton.BorderRadius = 10;
                    deleteButton.Tag = row["ID"];
                    deleteButton.Click += DeleteButton_Click;

                    //Создание markButton
                    var markButton = new SiticoneButton();
                    markButton.Size = new Size(150, 20);
                    markButton.BorderRadius = 10;
                    markButton.Location = new Point(20, 176);
                    markButton.Text = status == -1 ? "Настало время" : status == 0 ? "Ждём" : "Выполнено";
                    markButton.Tag = row["ID"];
                    markButton.Click += MarkButton_Click;


                    // Добавление элементов на контейнерный элемент
                    compositeElement.Controls.Add(label1);
                    compositeElement.Controls.Add(label2);
                    compositeElement.Controls.Add(label3);
                    compositeElement.Controls.Add(markButton);
                    compositeElement.Controls.Add(deleteButton);
                    compositeElement.Controls.Add(editButton);

                    scrollableTabPagePanel.Controls.Add(compositeElement);

                    taskCount++;
                    if (taskCount % 7 == 0)
                    {
                        taskCount = 0;
                        rowCount++;
                    }
                }

                tabControl.TabPages.Add(tabPage);
            }

            // Добавление TabControl на форму
            this.Controls.Add(tabControl);
        }



        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchTextBox.Text; // Получение текста из поля поиска

            // Удаление текущих задач
            foreach (Control control in this.Controls)
            {
                if (control is TabControl tabControl)
                {
                    Control parent = tabControl.Parent;
                    parent.Controls.Remove(tabControl);
                }
            }

            // Обновление задач с учетом поискового запроса
            InitializeTasks(searchText);
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            SiticoneButton editButton = (SiticoneButton)sender;
            int taskId = (int)editButton.Tag; // Получаем идентификатор задачи из Tag кнопки

            EditTaskForm editTaskForm = new EditTaskForm(taskId);
            editTaskForm.ShowDialog();

            // После закрытия формы редактирования, обновляем интерфейс пользователя
            RefreshTasks();
        }

        private void RefreshTasks()
        {
            // Удаление текущих задач
            foreach (Control control in this.Controls)
            {
                if (control is TabControl tabControl)
                {
                    Control parent = tabControl.Parent;
                    parent.Controls.Remove(tabControl);
                }
            }

            // Добавление обновленных задач
            InitializeTasks(searchTextBox.Text);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            SiticoneButton deleteButton = (SiticoneButton)sender;
            int taskId = (int)deleteButton.Tag; // Получаем идентификатор задачи из Tag кнопки

            // Проверяем, хотите ли вы действительно удалить задачу
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить эту задачу?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // Удаляем задачу из базы данных
                PostgreManager.DeleteTask(taskId);

                // Удаляем контейнерный элемент, содержащий задачу, из родительского элемента
                Control compositeElement = deleteButton.Parent;
                compositeElement.Parent.Controls.Remove(compositeElement);

                // Обновляем интерфейс пользователя
                RefreshTasks();
            }
        }

        private void MarkButton_Click(object sender, EventArgs e)
        {
            SiticoneButton markButton = (SiticoneButton)sender;
            int taskId = (int)markButton.Tag; // Получаем идентификатор задачи из Tag кнопки

            
            if(markButton.Text != "Выполнено")
            {
                PostgreManager.UpdateTaskStatus(taskId, 1);
                markButton.Text = "Выполнено";
            }
            else
            {
                PostgreManager.UpdateTaskStatus(taskId, 0);
                markButton.Text = "Ждём";
            }
                
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            label1.Text = CurrentUser.Username;
        }



        // Метод для получения экземпляра MainForm
        public static MainForm GetInstance()
        {
            if (instance == null)
            {
                instance = new MainForm();
            }
            return instance;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LogInForm logInForm = LogInForm.GetInstance();
            logInForm.Show();
            Program.SetActiveForm(logInForm);
            Program.timer.Stop();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CreateTaskForm createTaskForm = CreateTaskForm.GetInstance();
            createTaskForm.Show();
            Program.SetActiveForm(createTaskForm);
            this.Hide();
            instance = null;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Activated_1(object sender, EventArgs e)
        {
            RefreshTasks();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string searchText = searchTextBox.Text; // Получение текста из поля поиска

            // Удаление текущих задач
            foreach (Control control in this.Controls)
            {
                if (control is TabControl tabControl)
                {
                    Control parent = tabControl.Parent;
                    parent.Controls.Remove(tabControl);
                }
            }

            // Обновление задач с учетом поискового запроса
            InitializeTasks(searchText);
        }
    }
}
