using KnowledgeBase1C.Data;
using KnowledgeBase1C.Forms;

namespace KnowledgeBase1C;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            Database.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка инициализации базы данных:\n{ex.Message}",
                "Критическая ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        using var login = new LoginForm();
        if (login.ShowDialog() == DialogResult.OK && login.AuthenticatedUser != null)
        {
            Application.Run(new MainForm(login.AuthenticatedUser));
        }
    }
}