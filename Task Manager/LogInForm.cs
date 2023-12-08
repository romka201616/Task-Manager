using Npgsql.PostgresTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskManager
{
    public partial class LogInForm : Form
    {
        private static LogInForm instance;

        public LogInForm()
        {
            InitializeComponent();
            InitializeComponents();
        }

        public static LogInForm GetInstance()
        {
            if (instance == null)
            {
                instance = new LogInForm();
            }
            return instance;
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

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ControlBox = false;
            this.Text = "";
        }

        private void LogInForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if(siticoneTextBox2.Text.Length<8)
            //{
            //    siticoneMessageDialog1.Show("Пароль должен содержать как минимум 8 символов.", "Неверный формат пароля!");
            //    return;
            //}
            //string username = siticoneTextBox1.Text;  // Получаем имя пользователя из текстового поля
            //string password = siticoneTextBox2.Text;  // Получаем пароль из текстового поля
            
            //// Проверяем, существует ли пользователь с таким именем в базе данных
            //if (!PostgreManager.IsUserExists(username))
            //{
            //    // Выполняем операцию регистрации нового пользователя
            //    PostgreManager.RegisterUser(username, password);
            //    siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Information;
            //    siticoneMessageDialog1.Show("Вы молодец. Теперь можете войти в этот аккаунт", "Пользователь успешно зарегистрирован!");
            //    siticoneTextBox2.Clear();
            //}
            //else
            //{
            //    siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
            //    siticoneMessageDialog1.Show("Измените имя пользователя.", "Пользователь уже существует!");
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void siticoneTextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void siticoneButton1_Click(object sender, EventArgs e)
        {
            if (siticoneTextBox2.Text != siticoneTextBox3.Text)
            {
                siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
                siticoneMessageDialog1.Show("Пароль и повтор пароля не совпадают.", "Проверьте пароль!");
                return;
            }

            if (siticoneTextBox1.Text.Length < 4)
            {
                siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
                siticoneMessageDialog1.Show("Имя пользователя должно содержать как минимум 4 символа.", "Неверный формат имени!");
                return;
            }

            if (siticoneTextBox2.Text.Length < 8)
            {
                siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
                siticoneMessageDialog1.Show("Пароль должен содержать как минимум 8 символов.", "Неверный формат пароля!");
                return;
            }

            string username = siticoneTextBox1.Text;  // Получаем имя пользователя из текстового поля
            string password = siticoneTextBox2.Text;  // Получаем пароль из текстового поля

            // Проверяем, существует ли пользователь с таким именем в базе данных
            if (!PostgreManager.IsUserExists(username))
            {
                // Выполняем операцию регистрации нового пользователя
                PostgreManager.RegisterUser(username, password);
                siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Information;
                siticoneMessageDialog1.Show("Вы молодец. Теперь можете войти в этот аккаунт", "Пользователь успешно зарегистрирован!");
                siticoneTextBox2.Clear();
                siticoneTextBox3.Clear();
            }
            else
            {
                siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
                siticoneMessageDialog1.Show("Измените имя пользователя.", "Пользователь уже существует!");
            }
        }

        private void siticoneButton2_Click(object sender, EventArgs e)
        {
            string username = siticoneTextBox1.Text;
            string password = siticoneTextBox2.Text;

            if (PostgreManager.IsUserExists(username))
            {

                if (PostgreManager.CheckPassword(username, password))
                {
                    int userId = PostgreManager.GetUserId(username);
                    CurrentUser.UserId = userId;
                    CurrentUser.Username = username;

                    MainForm mainForm = MainForm.GetInstance();
                    mainForm.Show();
                    Program.SetActiveForm(mainForm);
                    Program.timer.Start();
                    this.Hide();
                    instance = null;
                }
                else
                {
                    siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
                    siticoneMessageDialog1.Show("Проверьте пароль и попробуйте ввести заново.", "Неверный пароль!");
                    //MessageBox.Show("Неверный пароль!");
                }
            }
            else
            {
                siticoneMessageDialog1.Icon = Siticone.Desktop.UI.WinForms.MessageDialogIcon.Error;
                siticoneMessageDialog1.Show("Проверьте имя пользователя и попробуйте ввести заново.", "Пользователь не найден!");
            }
        }

        private void siticoneButton3_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void siticoneTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void siticoneTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar >= 'A' && e.KeyChar <= 'z') && !(char.IsNumber(e.KeyChar)) && !(e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }

        private void siticoneTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar >= 'A' && e.KeyChar <= 'z') && !(char.IsNumber(e.KeyChar)) && !(e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void siticoneTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar >= 'A' && e.KeyChar <= 'z') && !(char.IsNumber(e.KeyChar)) && !(e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }
    }
}
