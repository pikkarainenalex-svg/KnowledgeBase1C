using KnowledgeBase1C.Data;
using KnowledgeBase1C.Models;

namespace KnowledgeBase1C.Forms;

public class LoginForm : Form
{
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;

    public User? AuthenticatedUser { get; private set; }

    public LoginForm()
    {
        Text = "Авторизация — БазаЗнаний 1С";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(420, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(241, 245, 249);
        Font = new Font("Segoe UI", 10);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.FromArgb(30, 58, 138)
        };
        header.Controls.Add(new Label
        {
            Text = "БазаЗнаний 1С\nООО «СтавАналит»",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold)
        });
        Controls.Add(header);

        var lblUser = new Label { Text = "Логин:", Location = new Point(30, 110), AutoSize = true };
        txtUsername = new TextBox { Location = new Point(30, 135), Width = 340, Text = "operator" };

        var lblPass = new Label { Text = "Пароль:", Location = new Point(30, 175), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(30, 200), Width = 340, UseSystemPasswordChar = true, Text = "123" };

        btnLogin = new Button
        {
            Text = "Войти в систему",
            Location = new Point(30, 245),
            Width = 340,
            Height = 40,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnLogin.Click += BtnLogin_Click;

        var hint = new Label
        {
            Text = "",
            Location = new Point(30, 295),
            Width = 340,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8)
        };

        Controls.AddRange(new Control[] { lblUser, txtUsername, lblPass, txtPassword, btnLogin, hint });
        AcceptButton = btnLogin;
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Введите логин и пароль.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var user = Database.Authenticate(username, password);
        if (user == null)
        {
            MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        AuthenticatedUser = user;
        DialogResult = DialogResult.OK;
        Close();
    }
}