/* DB Structure
CREATE TABLE Users(
	ID SERIAL PRIMARY KEY,
	Username VARCHAR(50),
	Password VARCHAR(50)
);

CREATE TABLE Categories(
	ID SERIAL PRIMARY KEY,
	UserID INTEGER REFERENCES Users (ID),
	CategoryName VARCHAR(100)
);

CREATE TABLE Tasks(
	ID SERIAL PRIMARY KEY,
	UserID INTEGER REFERENCES Users (ID),
	CategoryID INTEGER REFERENCES Categories,
	TaskName VARCHAR(100),
	TaskDescription VARCHAR(500),
	TaskDueDate TIMESTAMP,
    Expired BOOLEAN
);
*/
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;
using TaskManager;
using Npgsql;

namespace TaskManager
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
    }

    public class PostgreManager : Form
    {
        private const string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=admin";

        public static bool IsUserExists(string username)
        {
            // Создаем подключение к базе данных
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем наличие пользователя с заданным именем
                string query = "SELECT COUNT(*) FROM Users WHERE username = @Username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static void RegisterUser(string username, string password)
        {
            // Создаем подключение к базе данных
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Выполняем операцию вставки нового пользователя в таблицу
                string query = "INSERT INTO Users (username, password) VALUES (@Username, @Password)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static bool CheckPassword(string username, string password)
        {
            // Создаем подключение к базе данных
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем соответствие введенного пароля для заданного пользователя
                string query = "SELECT COUNT(*) FROM Users WHERE username = @Username AND password = @Password";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static int GetUserId(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT ID FROM Users WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("Username", username);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return -1; // Возврат значения -1, если пользователь не найден
        }

        public static string GetUsername(int userId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT Username FROM Users WHERE ID = @ID", connection))
                {
                    command.Parameters.AddWithValue("ID", userId);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                }
            }

            return null; // Возврат значения null, если пользователь не найден
        }

        public static bool IsCategoryExists(string category)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем наличие категории для заданного пользователя
                string query = "SELECT COUNT(*) FROM Categories WHERE CategoryName = @Category AND UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static void AddCategory(string category)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Добавляем новую категорию в базу данных
                string query = "INSERT INTO Categories (UserId, CategoryName) VALUES (@UserId, @Category)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    command.Parameters.AddWithValue("@Category", category);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void AddTask(string taskName, string taskDescription, DateTime dueDate, int categoryId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Добавляем новую задачу в базу данных
                string query = "INSERT INTO Tasks (UserId, CategoryId, TaskName, TaskDescription, TaskDueDate) " +
                    "VALUES (@UserId, @CategoryId, @TaskName, @TaskDescription, @TaskDueDate)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    command.Parameters.AddWithValue("@TaskName", taskName);
                    command.Parameters.AddWithValue("@TaskDescription", taskDescription);
                    command.Parameters.AddWithValue("@TaskDueDate", dueDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateComboBox(System.Windows.Forms.ComboBox comboBox)
        {
            comboBox.Items.Clear();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Получаем список категорий для указанного пользователя
                string query = "SELECT CategoryName FROM Categories WHERE UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string categoryName = reader.GetString(0);
                            comboBox.Items.Add(categoryName);
                        }
                    }
                }
            }
        }



        // Метод для получения идентификатора категории по названию
        public static int GetCategoryId(string categoryName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT id FROM Categories WHERE CategoryName = @CategoryName AND UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", categoryName);
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return -1; // Если категория не найдена, возвращаем -1
        }

        public static string GetCategoryName(int categoryId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT CategoryName FROM Categories WHERE Id = @CategoryId AND UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                }
            }

            return null; // Если категория не найдена, возвращаем null
        }

        // Метод для удаления категории из базы данных
        public static void DeleteCategory(int categoryId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Categories WHERE id = @CategoryId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetTasks()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM tasks WHERE UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public static int GetCategoryAmount()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Categories WHERE UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public static void DeleteTask(int taskId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM tasks WHERE ID = @taskId";
                    command.Parameters.AddWithValue("taskId", taskId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateTaskStatus(int taskId, int status)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Tasks SET Status = @Status WHERE ID = @TaskId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateTask(int taskId, string taskName, string taskDescription, DateTime dueDate, int categoryId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Tasks SET TaskName = @TaskName, TaskDescription = @TaskDescription, TaskDueDate = @TaskDueDate, CategoryId = @CategoryId WHERE ID = @TaskId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskName", taskName);
                    command.Parameters.AddWithValue("@TaskDescription", taskDescription);
                    command.Parameters.AddWithValue("@TaskDueDate", dueDate);
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataRow GetTaskById(int taskId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Tasks WHERE ID = @TaskId AND UserId = @UserId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            return dataTable.Rows[0];
                        }
                    }
                }
            }

            return null; // Если задача не найдена, возвращаем null
        }

        public static DataTable GetExpiredTasks(int userId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT * FROM Tasks WHERE UserID = @UserID AND TaskDueDate <= NOW() AND status = 0", connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);

                    DataTable tasks = new DataTable();
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                    adapter.Fill(tasks);
                    return tasks;

                }
            }
        }

        public static void UpdateTaskExpiredStatus(int taskId, bool expired)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(@"UPDATE Tasks SET expired = @Expired WHERE ID = @TaskID", connection))
                {
                    command.Parameters.AddWithValue("@Expired", expired);
                    command.Parameters.AddWithValue("@TaskID", taskId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void ShowTaskNotification(string taskName, string taskDescription)
        {
            Program.notifyIcon.BalloonTipText = $"Задача \"{taskName}\" истекла: {taskDescription}";
            Program.notifyIcon.ShowBalloonTip(5000, $"{taskName} истекла", taskDescription, ToolTipIcon.Info); // Показываем уведомление на 5 секунд
        }

    }
}
