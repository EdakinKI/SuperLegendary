using System;
using System.Windows.Forms;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Проверка подключения к БД
            var dbService = new DatabaseService();

            if (!dbService.TestConnection())
            {
                var result = MessageBox.Show(
                    "Не удалось подключиться к базе данных PostgreSQL.\n" +
                    "Хотите продолжить без базы данных?\n\n" +
                    "Причина: PostgreSQL может быть не запущен или неправильно настроен.",
                    "Предупреждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    MessageBox.Show(
                        "Пожалуйста, убедитесь, что:\n" +
                        "1. PostgreSQL запущен\n" +
                        "2. База данных 'Forecast16' существует\n" +
                        "3. Пароль в App.config правильный",
                        "Информация",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
            }

            Application.Run(new MainMenuForm());
        }
    }
}