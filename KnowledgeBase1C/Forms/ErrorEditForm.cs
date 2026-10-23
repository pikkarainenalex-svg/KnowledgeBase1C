using KnowledgeBase1C.Data;
using KnowledgeBase1C.Models;

namespace KnowledgeBase1C.Forms;

public class ErrorEditForm : Form
{
    private readonly ErrorEntry? _existing;
    private readonly string _currentUser;

    private TextBox txtCode = null!;
    private TextBox txtTitle = null!;
    private ComboBox cmbConfig = null!;
    private RichTextBox rtbDescription = null!;
    private RichTextBox rtbSteps = null!;

    public ErrorEditForm(ErrorEntry? existing, string currentUser)
    {
        _existing = existing;
        _currentUser = currentUser;

        Text = existing == null ? "Новая ошибка" : $"Редактирование ошибки #{existing.Id}";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(640, 680);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(241, 245, 249);

        int y = 20;
        AddLabel("Код ошибки:", 20, y); y += 22;
        txtCode = new TextBox { Location = new Point(20, y), Width = 590 };
        Controls.Add(txtCode); y += 40;

        AddLabel("Краткое название:", 20, y); y += 22;
        txtTitle = new TextBox { Location = new Point(20, y), Width = 590 };
        Controls.Add(txtTitle); y += 40;

        AddLabel("Конфигурация 1С:", 20, y); y += 22;
        cmbConfig = new ComboBox
        {
            Location = new Point(20, y),
            Width = 590,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbConfig.Items.AddRange(new object[]
        {
            "1С:Бухгалтерия", "1С:ЗУП", "1С:Управление торговлей", "1С:Розница", "Общее"
        });
        cmbConfig.SelectedIndex = 0;
        Controls.Add(cmbConfig); y += 40;

        AddLabel("Описание проблемы:", 20, y); y += 22;
        rtbDescription = new RichTextBox
        {
            Location = new Point(20, y),
            Width = 590,
            Height = 100,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
        Controls.Add(rtbDescription); y += 115;

        AddLabel("Пошаговое решение (1 шаг = 1 строка):", 20, y); y += 22;
        rtbSteps = new RichTextBox
        {
            Location = new Point(20, y),
            Width = 590,
            Height = 150,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
        Controls.Add(rtbSteps); y += 165;

        var btnSave = new Button
        {
            Text = "Сохранить",
            Location = new Point(420, y),
            Width = 190,
            Height = 38,
            BackColor = Color.FromArgb(22, 163, 74),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSave.Click += BtnSave_Click;

        var btnCancel = new Button
        {
            Text = "Отмена",
            Location = new Point(220, y),
            Width = 190,
            Height = 38,
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange(new Control[] { btnSave, btnCancel });
        AcceptButton = btnSave;
        CancelButton = btnCancel;

        if (existing != null)
        {
            txtCode.Text = existing.ErrorCode;
            txtTitle.Text = existing.Title;
            cmbConfig.SelectedItem = existing.Config1C;
            if (cmbConfig.SelectedIndex < 0) cmbConfig.SelectedIndex = 4;
            rtbDescription.Text = existing.Description;
            rtbSteps.Text = existing.Steps;
        }
    }

    private void AddLabel(string text, int x, int y)
    {
        Controls.Add(new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        });
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text))
        {
            MessageBox.Show("Введите код ошибки.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            MessageBox.Show("Введите название ошибки.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(rtbDescription.Text))
        {
            MessageBox.Show("Введите описание проблемы.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(rtbSteps.Text))
        {
            MessageBox.Show("Введите шаги решения.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var entry = new ErrorEntry
        {
            Id = _existing?.Id ?? 0,
            ErrorCode = txtCode.Text.Trim(),
            Title = txtTitle.Text.Trim(),
            Config1C = cmbConfig.SelectedItem?.ToString() ?? "Общее",
            Description = rtbDescription.Text.Trim(),
            Steps = rtbSteps.Text.Trim(),
            CreatedBy = _existing?.CreatedBy ?? _currentUser,
            CreatedAt = _existing?.CreatedAt ?? DateTime.Now.ToString("dd.MM.yyyy HH:mm")
        };

        try
        {
            if (_existing == null)
                Database.AddError(entry);
            else
                Database.UpdateError(entry);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}