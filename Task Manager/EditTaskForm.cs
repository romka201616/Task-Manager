using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskManager;

namespace Task_Manager
{
    public partial class EditTaskForm : Form
    {
        private int taskId;
        private ComboBox comboBoxCategory; // Добавлен ComboBox для выбора категории

        // Создание элементов управления
        private TextBox textBoxTaskName;
        private DateTimePicker dateTimePickerDueDate;
        private TextBox textBoxDescription;
        private Button buttonSave;
        private Button buttonCancel;

        public EditTaskForm(int taskId)
        {
            InitializeComponent();
            this.taskId = taskId;
            CreateControls();
            LoadTaskData();
            LoadCategories(); // Загрузка списка категорий в ComboBox
        }

        private void CreateControls()
        {
            // Создание TextBox для названия задачи
            textBoxTaskName = new TextBox();
            textBoxTaskName.Location = new Point(10, 10);
            textBoxTaskName.Size = new Size(200, 20);
            this.Controls.Add(textBoxTaskName);

            // Создание DateTimePicker для выбора новой даты выполнения задачи
            dateTimePickerDueDate = new DateTimePicker();
            dateTimePickerDueDate.Location = new Point(10, 40);
            dateTimePickerDueDate.Size = new Size(200, 20);
            this.Controls.Add(dateTimePickerDueDate);

            // Создание TextBox для описания задачи
            textBoxDescription = new TextBox();
            textBoxDescription.Location = new Point(10, 70);
            textBoxDescription.Size = new Size(200, 100);
            textBoxDescription.Multiline = true;
            this.Controls.Add(textBoxDescription);

            // Создание ComboBox для выбора категории задачи
            comboBoxCategory = new ComboBox();
            comboBoxCategory.Location = new Point(10, 180);
            comboBoxCategory.Size = new Size(200, 20);
            this.Controls.Add(comboBoxCategory);

            // Создание кнопки "Сохранить"
            buttonSave = new Button();
            buttonSave.Text = "Сохранить";
            buttonSave.Location = new Point(10, 210);
            buttonSave.Click += buttonSave_Click;
            this.Controls.Add(buttonSave);

            // Создание кнопки "Отмена"
            buttonCancel = new Button();
            buttonCancel.Text = "Отмена";
            buttonCancel.Location = new Point(100, 210);
            buttonCancel.Click += buttonCancel_Click;
            this.Controls.Add(buttonCancel);
        }

        private void LoadTaskData()
        {
            // Получите данные задачи с использованием идентификатора
            DataRow taskRow = PostgreManager.GetTaskById(taskId);

            // Загрузите данные задачи в элементы управления формы
            if (taskRow != null)
            {
                string taskName = taskRow["taskName"].ToString();
                DateTime endTime = Convert.ToDateTime(taskRow["TaskDueDate"]);
                string description = taskRow["TaskDescription"].ToString();
                int categoryId = Convert.ToInt32(taskRow["CategoryId"]);

                textBoxTaskName.Text = taskName;
                dateTimePickerDueDate.Value = endTime;
                textBoxDescription.Text = description;
                comboBoxCategory.SelectedValue = categoryId; // Установка выбранной категории в ComboBox
            }
        }

        private void LoadCategories()
        {
            // Обновление ComboBox с категориями
            PostgreManager.UpdateComboBox(comboBoxCategory);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // Получите данные задачи из элементов управления формы
            string taskName = textBoxTaskName.Text;
            DateTime endTime = dateTimePickerDueDate.Value;
            string description = textBoxDescription.Text;

            object selectedValue = comboBoxCategory.Text;

            if (selectedValue != null)
            {
                int categoryId = PostgreManager.GetCategoryId(selectedValue.ToString());
                // Обновите задачу в базе данных
                PostgreManager.UpdateTask(taskId, taskName, description, endTime, categoryId);

                MessageBox.Show("Задача успешно обновлена!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            else
            {
                MessageBox.Show("Вы не выбрали задачу");
            }
            
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}