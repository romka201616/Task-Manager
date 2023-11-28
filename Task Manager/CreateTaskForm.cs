using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskManager;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Task_Manager
{
    public partial class CreateTaskForm : Form
    {
        private static CreateTaskForm instance;

        public CreateTaskForm()
        {
            InitializeComponent();
            InitializeComponents();
            
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

            this.TopMost = true;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            int formWidth = this.Width;
            int formHeight = this.Height;

            button1.Location = new Point(100, formHeight - 100 - button1.Height);
            button2.Location = new Point(formWidth - 100 - button2.Width, formHeight - 100 - button2.Height);

            textBox3.KeyPress += textBox3_KeyPress;
            comboBox2.SelectedIndexChanged += comboBox3_SelectedIndexChanged;

            // Настройка свойств элемента управления
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd.MM.yyyy HH:mm";

            PostgreManager.UpdateComboBox(comboBox1);
            PostgreManager.UpdateComboBox(comboBox2);
        }

        // Обработчик события SelectedIndexChanged для comboBox3
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCategory = comboBox2.SelectedItem.ToString();

            // Получаем идентификатор выбранной категории
            int categoryId = PostgreManager.GetCategoryId(selectedCategory);

            // Удаляем выбранную категорию из базы данных
            PostgreManager.DeleteCategory(categoryId);

            // Обновляем ComboBox с категориями
            PostgreManager.UpdateComboBox(comboBox2);
            PostgreManager.UpdateComboBox(comboBox1);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string category = textBox3.Text.Trim();
                // Проверяем наличие категории в базе данных
                bool categoryExists = PostgreManager.IsCategoryExists(category);

                if (categoryExists)
                {
                    MessageBox.Show("Категория уже существует");
                }
                else
                {
                    // Добавляем категорию в базу данных
                    PostgreManager.AddCategory(category);
                    MessageBox.Show("Категория добавлена");
                    PostgreManager.UpdateComboBox(comboBox1);
                    PostgreManager.UpdateComboBox(comboBox2);
                }

                // Очищаем поле ввода
                textBox3.Clear();
            }
        }

        // Метод для получения экземпляра MainForm
        public static CreateTaskForm GetInstance()
        {
            if (instance == null)
            {
                instance = new CreateTaskForm();
            }
            return instance;
        }

        private void CreateTaskForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm mainForm = MainForm.GetInstance();
            mainForm.Show();
            Program.SetActiveForm(mainForm);
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PostgreManager.AddTask(textBox1.Text, textBox2.Text, dateTimePicker1.Value, PostgreManager.GetCategoryId(comboBox1.Text));
            MessageBox.Show("Задача создана");
            MainForm mainForm = MainForm.GetInstance();
            mainForm.Show();
            this.Hide();
            instance = null;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
