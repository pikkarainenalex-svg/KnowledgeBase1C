using System.Text;
using KnowledgeBase1C.Data;
using KnowledgeBase1C.Models;

namespace KnowledgeBase1C.Forms;

public class MainForm : Form
{
    private User _currentUser;
    private DataGridView dgvErrors = null!;
    private TextBox txtSearch = null!;
    private ComboBox cmbConfig = null!;
    private Label lblStatus = null!;
    private Label lblDetailHeader = null!;
    private RichTextBox rtbDetail = null!;
    private int _selectedErrorId = 0;

    private Button _btnAll = null!;
    private Button _btnBP = null!;
    private Button _btnZUP = null!;
    private Button _btnUT = null!;
    private Button _btnRoz = null!;
    private List<Button> _sidebarButtons = new();

    private readonly Color ColorHeader = Color.FromArgb(30, 58, 138);
    private readonly Color ColorSidebar = Color.FromArgb(15, 23, 42);
    private readonly Color ColorSidebarHover = Color.FromArgb(51, 65, 85);
    private readonly Color ColorMainBg = Color.FromArgb(241, 245, 249);
    private readonly Color ColorDanger = Color.FromArgb(220, 38, 38);
    private readonly Color ColorSuccess = Color.FromArgb(22, 163, 74);
    private readonly Color ColorPrimary = Color.FromArgb(37, 99, 235);

    public MainForm(User user)
    {
        _currentUser = user;
        Text = $"БазаЗнаний 1С — {user.FullName}";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        Font = new Font("Segoe UI", 10);
        BackColor = ColorMainBg;
        MinimumSize = new Size(1000, 600);

        BuildUi();
        RefreshGrid();
    }

    private void BuildUi()
    {

        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 250,
            BackColor = ColorSidebar,
            Padding = new Padding(0, 20, 0, 20)
        };

        var lblLogo = new Label
        {
            Text = "БАЗА ЗНАНИЙ 1С",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 60
        };

        var menuPanel = new Panel { Dock = DockStyle.Top, Height = 320 };

        _btnAll = CreateSidebarButton("Все ошибки", true);
        _btnAll.Click += (s, e) => SetFilter("Все");

        var lblConfigs = new Label
        {
            Text = "ФИЛЬТР ПО 1С",
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(15, 0, 0, 5)
        };

        _btnBP = CreateSidebarButton("1С:Бухгалтерия", false);
        _btnBP.Click += (s, e) => SetFilter("1С:Бухгалтерия");

        _btnZUP = CreateSidebarButton("1С:ЗУП", false);
        _btnZUP.Click += (s, e) => SetFilter("1С:ЗУП");

        _btnUT = CreateSidebarButton("1С:Управление торговлей", false);
        _btnUT.Click += (s, e) => SetFilter("1С:Управление торговлей");

        _btnRoz = CreateSidebarButton("1С:Розница", false);
        _btnRoz.Click += (s, e) => SetFilter("1С:Розница");

        _sidebarButtons.AddRange(new[] { _btnAll, _btnBP, _btnZUP, _btnUT, _btnRoz });

        menuPanel.Controls.Add(_btnRoz);
        menuPanel.Controls.Add(_btnUT);
        menuPanel.Controls.Add(_btnZUP);
        menuPanel.Controls.Add(_btnBP);
        menuPanel.Controls.Add(lblConfigs);
        menuPanel.Controls.Add(_btnAll);

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 100 };

        var btnAbout = new Button
        {
            Text = "ℹ️ О программе",
            Dock = DockStyle.Top,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorSidebar,
            ForeColor = Color.FromArgb(203, 213, 225),
            Font = new Font("Segoe UI", 10),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        btnAbout.FlatAppearance.BorderSize = 0;
        btnAbout.FlatAppearance.MouseOverBackColor = ColorSidebarHover;
        btnAbout.Click += (s, e) => ShowAbout();

        var btnLogout = new Button
        {
            Text = "🚪 Выйти из системы",
            Dock = DockStyle.Bottom,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorDanger,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (s, e) => Logout();

        bottomPanel.Controls.Add(btnAbout);
        bottomPanel.Controls.Add(btnLogout);

        sidebar.Controls.Add(menuPanel);
        sidebar.Controls.Add(bottomPanel);
        sidebar.Controls.Add(lblLogo);

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = ColorHeader,
            Padding = new Padding(15)
        };

        txtSearch = new TextBox
        {
            Location = new Point(15, 18),
            Width = 300,
            Height = 34,
            Font = new Font("Segoe UI", 11),
            PlaceholderText = "Поиск...",
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        txtSearch.TextChanged += (s, e) => RefreshGrid();

        cmbConfig = new ComboBox { Visible = false };
        cmbConfig.Items.AddRange(new object[] { "Все", "1С:Бухгалтерия", "1С:ЗУП", "1С:Управление торговлей", "1С:Розница", "Общее" });
        cmbConfig.SelectedIndex = 0;
        cmbConfig.SelectedIndexChanged += (s, e) => RefreshGrid();

        var buttonsPanel = new FlowLayoutPanel
        {
            Location = new Point(330, 15), 
            Height = 40,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var btnAdd = new Button
        {
            Text = "+ Добавить",
            Width = 120,
            Height = 36,
            BackColor = ColorSuccess,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Visible = _currentUser.Role == "specialist",
            Margin = new Padding(0, 0, 10, 0)
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Click += (s, e) => AddError();

        var btnEdit = new Button
        {
            Text = "✏️ Изменить",
            Width = 120,
            Height = 36,
            BackColor = ColorPrimary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Visible = _currentUser.Role == "specialist",
            Margin = new Padding(0, 0, 10, 0)
        };
        btnEdit.FlatAppearance.BorderSize = 0;
        btnEdit.Click += (s, e) => EditError();

        var btnDelete = new Button
        {
            Text = "🗑️ Удалить",
            Width = 120,
            Height = 36,
            BackColor = ColorDanger,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Visible = _currentUser.Role == "specialist",
            Margin = new Padding(0, 0, 10, 0)
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += (s, e) => DeleteError();

        var btnExport = new Button
        {
            Text = "📤 Экспорт",
            Width = 110,
            Height = 36,
            BackColor = Color.FromArgb(71, 85, 105),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.Click += (s, e) => ExportCsv();

        var btnImport = new Button
        {
            Text = "📥 Импорт",
            Width = 110,
            Height = 36,
            BackColor = Color.FromArgb(71, 85, 105),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Visible = _currentUser.Role == "specialist",
            Margin = new Padding(0, 0, 0, 0)
        };
        btnImport.FlatAppearance.BorderSize = 0;
        btnImport.Click += (s, e) => ImportCsv();

        buttonsPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnExport, btnImport });

        topPanel.Controls.Add(txtSearch);
        topPanel.Controls.Add(cmbConfig);
        topPanel.Controls.Add(buttonsPanel);

        var statusPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.Gray
        };
        statusPanel.Controls.Add(lblStatus);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 750,
            BackColor = ColorMainBg,
            Padding = new Padding(10)
        };

        dgvErrors = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            GridColor = Color.FromArgb(226, 232, 240),
            AllowUserToResizeRows = false,
            RowTemplate = { Height = 35 }
        };

        dgvErrors.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgvErrors.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvErrors.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
        dgvErrors.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
        dgvErrors.ColumnHeadersHeight = 45;
        dgvErrors.EnableHeadersVisualStyles = false;
        dgvErrors.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        dgvErrors.DefaultCellStyle.SelectionForeColor = Color.Black;

        dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Код", Name = "error_code", FillWeight = 12 });
        dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", Name = "title", FillWeight = 45 });
        dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "1С", Name = "config_1c", FillWeight = 15 });
        dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "👁", Name = "views", FillWeight = 8 });
        dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата", Name = "created_at", FillWeight = 20 });
        dgvErrors.SelectionChanged += DgvErrors_SelectionChanged;

        split.Panel1.Controls.Add(dgvErrors);

        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            BorderStyle = BorderStyle.FixedSingle
        };

        lblDetailHeader = new Label
        {
            Dock = DockStyle.Top,
            Height = 50,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = ColorHeader,
            Text = "Выберите ошибку",
            TextAlign = ContentAlignment.MiddleLeft
        };

        rtbDetail = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 11)
        };

        rightPanel.Controls.Add(rtbDetail);
        rightPanel.Controls.Add(lblDetailHeader);
        split.Panel2.Controls.Add(rightPanel);

        Controls.Add(split);
        Controls.Add(topPanel);
        Controls.Add(statusPanel);
        Controls.Add(sidebar);
    }

    private Button CreateSidebarButton(string text, bool isActive)
    {
        var btn = new Button
        {
            Text = "  " + text,
            Dock = DockStyle.Top,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            BackColor = isActive ? ColorHeader : ColorSidebar,
            ForeColor = isActive ? Color.White : Color.FromArgb(203, 213, 225),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, isActive ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ColorSidebarHover;
        return btn;
    }

    private void SetFilter(string config)
    {
        cmbConfig.SelectedItem = config;

        foreach (var btn in _sidebarButtons)
        {
            btn.BackColor = ColorSidebar;
            btn.ForeColor = Color.FromArgb(203, 213, 225);
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        }

        Button? activeBtn = config switch
        {
            "Все" => _btnAll,
            "1С:Бухгалтерия" => _btnBP,
            "1С:ЗУП" => _btnZUP,
            "1С:Управление торговлей" => _btnUT,
            "1С:Розница" => _btnRoz,
            _ => null
        };

        if (activeBtn != null)
        {
            activeBtn.BackColor = ColorHeader;
            activeBtn.ForeColor = Color.White;
            activeBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }
        RefreshGrid();
    }

    private void Logout()
    {
        if (MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            Hide();
            using var login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK && login.AuthenticatedUser != null)
            {
                _currentUser = login.AuthenticatedUser;
                Text = $"БазаЗнаний 1С — {_currentUser.FullName}";
                Controls.Clear();
                BuildUi();
                RefreshGrid();
                Show();
            }
            else
            {
                Application.Exit();
            }
        }
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "БазаЗнаний 1С\nВерсия 1.0\n\n" +
            "Информационная система для учёта типовых ошибок программ 1С.\n" +
            "ООО «СтавАналит», г. Ставрополь, 2026 г.",
            "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RefreshGrid()
    {
        try
        {
            var errors = Database.GetErrors(txtSearch.Text, cmbConfig.SelectedItem?.ToString() ?? "Все");
            dgvErrors.Rows.Clear();

            foreach (var e in errors)
            {
                int rowIndex = dgvErrors.Rows.Add(e.ErrorCode, e.Title, e.Config1C, e.Views, e.CreatedAt);
                dgvErrors.Rows[rowIndex].Tag = e;
                dgvErrors.Rows[rowIndex].Cells["views"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            lblStatus.Text = $"Всего записей: {errors.Count} | Пользователь: {_currentUser.FullName}";
            ClearDetail();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearDetail()
    {
        lblDetailHeader.Text = "Выберите ошибку";
        rtbDetail.Clear();
        _selectedErrorId = 0;
    }

    private void DgvErrors_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvErrors.SelectedRows.Count == 0) return;
        if (dgvErrors.SelectedRows[0].Tag is not ErrorEntry entry) return;

        _selectedErrorId = entry.Id;
        Database.IncrementViews(entry.Id);
        entry.Views++;

        lblDetailHeader.Text = $"{entry.ErrorCode}";
        rtbDetail.Clear();

        rtbDetail.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
        rtbDetail.SelectionColor = Color.Gray;
        rtbDetail.AppendText($"Конфигурация: {entry.Config1C}   |   👁 {entry.Views}   |   {entry.CreatedBy} ({entry.CreatedAt})\n");

        rtbDetail.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
        rtbDetail.SelectionColor = Color.Black;
        rtbDetail.AppendText($"\n{entry.Title}\n\n");

        rtbDetail.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
        rtbDetail.SelectionColor = ColorHeader;
        rtbDetail.AppendText("ОПИСАНИЕ ПРОБЛЕМЫ\n");
        rtbDetail.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
        rtbDetail.SelectionColor = Color.Black;
        rtbDetail.AppendText(entry.Description + "\n\n");

        rtbDetail.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
        rtbDetail.SelectionColor = ColorSuccess;
        rtbDetail.AppendText("ПОШАГОВОЕ РЕШЕНИЕ\n");
        rtbDetail.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
        rtbDetail.SelectionColor = Color.Black;

        var steps = entry.Steps.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int n = 1;
        foreach (var step in steps)
        {
            rtbDetail.AppendText($"{n}. {step.Trim()}\n");
            n++;
        }
    }

    private void AddError()
    {
        using var form = new ErrorEditForm(null, _currentUser.FullName);
        if (form.ShowDialog() == DialogResult.OK)
            RefreshGrid();
    }

    private void EditError()
    {
        if (_selectedErrorId == 0) { MessageBox.Show("Выберите ошибку.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        var entry = Database.GetErrors().FirstOrDefault(x => x.Id == _selectedErrorId);
        if (entry == null) return;

        using var form = new ErrorEditForm(entry, _currentUser.FullName);
        if (form.ShowDialog() == DialogResult.OK)
            RefreshGrid();
    }

    private void DeleteError()
    {
        if (_selectedErrorId == 0) { MessageBox.Show("Выберите ошибку.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        var entry = Database.GetErrors().FirstOrDefault(x => x.Id == _selectedErrorId);
        if (entry == null) return;

        if (MessageBox.Show($"Удалить ошибку {entry.ErrorCode}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            Database.DeleteError(entry.Id);
            RefreshGrid();
        }
    }

    private void ExportCsv()
    {
        using var sfd = new SaveFileDialog { Filter = "CSV-файл (*.csv)|*.csv", FileName = $"knowledge_base_{DateTime.Now:yyyyMMdd}.csv" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Код;Название;Конфигурация;Просмотры;Дата");
            foreach (DataGridViewRow row in dgvErrors.Rows)
                sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Экспорт завершён.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ImportCsv()
    {
        using var ofd = new OpenFileDialog { Filter = "CSV-файл (*.csv)|*.csv" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
            if (lines.Length <= 1) { MessageBox.Show("Файл пуст."); return; }

            int added = 0;
            int skipped = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 3)
                {
                    var entry = new ErrorEntry
                    {
                        ErrorCode = parts[0].Trim(),
                        Title = parts[1].Trim(),
                        Config1C = parts[2].Trim(),
                        Views = parts.Length > 3 && int.TryParse(parts[3], out int v) ? v : 0,
                        CreatedAt = parts.Length > 4 ? parts[4].Trim() : DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                        Description = "Описание добавлено при импорте.",
                        Steps = "1. Изучить проблему.",
                        CreatedBy = _currentUser.FullName
                    };

                    if (Database.ImportError(entry))
                        added++;
                    else
                        skipped++;
                }
            }
            MessageBox.Show($"Импортировано новых: {added}\nПропущено дубликатов: {skipped}", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshGrid();
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}