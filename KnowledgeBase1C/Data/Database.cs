using Microsoft.Data.Sqlite;
using BCrypt.Net;
using KnowledgeBase1C.Models;

namespace KnowledgeBase1C.Data;

public static class Database
{
    private static string _connectionString = string.Empty;

    public static void Initialize()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "knowledge_base.db");
        _connectionString = $"Data Source={dbPath}";

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE NOT NULL,
                password_hash TEXT NOT NULL,
                role TEXT NOT NULL CHECK(role IN ('specialist', 'operator')),
                full_name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS errors (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                error_code TEXT NOT NULL,
                title TEXT NOT NULL,
                description TEXT NOT NULL,
                config_1c TEXT NOT NULL,
                steps TEXT NOT NULL,
                created_by TEXT NOT NULL,
                created_at TEXT NOT NULL,
                views INTEGER NOT NULL DEFAULT 0
            );
        ";
        cmd.ExecuteNonQuery();

        SeedUsers(conn);
        SeedErrors(conn);
    }

    private static void SeedUsers(SqliteConnection conn)
    {
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM users";
        if (Convert.ToInt64(check.ExecuteScalar()) > 0) return;

        var users = new[]
        {
            ("operator", "123", "operator", "Иванова Елена Сергеевна"),
            ("specialist", "123", "specialist", "Петров Алексей Владимирович"),
            ("smirnov", "123", "specialist", "Смирнов Дмитрий Игоревич")
        };

        foreach (var (username, password, role, fullName) in users)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO users (username, password_hash, role, full_name) VALUES ($u, $h, $r, $f)";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$h", hash);
            cmd.Parameters.AddWithValue("$r", role);
            cmd.Parameters.AddWithValue("$f", fullName);
            cmd.ExecuteNonQuery();
        }
    }

    private static void SeedErrors(SqliteConnection conn)
    {
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM errors";
        if (Convert.ToInt64(check.ExecuteScalar()) > 0) return;

        var errors = new (string Code, string Title, string Desc, string Config, string Steps, string CreatedBy, string CreatedAt, int Views)[]
        {
            ("НДС-001", "Ошибка выгрузки декларации по НДС в ИФНС",
             "При выгрузке декларации по НДС через 1С-Отчетность возникает ошибка \"Не найден код КЛАДР\".",
             "1С:Бухгалтерия",
             "1. Открыть раздел \"Отчеты\" → \"Регламентированные отчеты\".\n2. Проверить актуальность адреса контрагента (КЛАДР/ФИАС).\n3. Обновить классификатор адресов: \"Главное\" → \"Все функции\" → \"Обновление классификаторов\".\n4. Повторить выгрузку.",
             "Петров Алексей Владимирович", DateTime.Now.AddDays(-10).ToString("dd.MM.yyyy HH:mm"), 47),

            ("ЗУП-002", "Не рассчитывается премия в документе «Начисление зарплаты»",
             "После обновления на ЗУП 3.1.27 перестала рассчитываться премия по сотрудникам, у которых она настроена как постоянное начисление.",
             "1С:ЗУП",
             "1. Проверить настройку начисления: \"Настройка\" → \"Начисления\".\n2. Убедиться, что у начисления стоит флажок \"Входит в фонд оплаты труда\".\n3. Проверить дату начала действия начисления в карточке сотрудника.\n4. Перепровести документ \"Начисление зарплаты и взносов\".",
             "Смирнов Дмитрий Игоревич", DateTime.Now.AddDays(-8).ToString("dd.MM.yyyy HH:mm"), 32),

            ("УТ-003", "Ошибка обмена с интернет-магазином: не синхронизируются остатки",
             "При синхронизации УТ 11.5 с сайтом (Bitrix) остатки товаров не обновляются, в журнале обмена ошибка 403.",
             "1С:Управление торговлей",
             "1. Открыть \"Администрирование\" → \"Настройки обмена данными\" → \"Синхронизация данных\".\n2. Проверить актуальность токена доступа в узле обмена.\n3. На стороне сайта убедиться, что веб-сервис принимает запросы (не блокирует firewall).\n4. Перезапустить сеанс обмена.",
             "Петров Алексей Владимирович", DateTime.Now.AddDays(-7).ToString("dd.MM.yyyy HH:mm"), 28),

            ("РОЗ-004", "Ошибка фискального регистратора АТОЛ при пробитии чека с маркировкой",
             "При пробитии чека с маркированным товаром (Честный ЗНАК) ККТ АТОЛ 30Ф выдает ошибку \"Неверный код маркировки\".",
             "1С:Розница",
             "1. Проверить драйвер ККТ АТОЛ (актуальная версия 10.x).\n2. Убедиться, что товар есть в системе маркировки и код корректен.\n3. В настройках ККТ включить режим \"Разрешительный режим\".\n4. Перезапустить 1С:Розница и повторить пробитие чека.",
             "Смирнов Дмитрий Игоревич", DateTime.Now.AddDays(-6).ToString("dd.MM.yyyy HH:mm"), 51),

            ("НДС-005", "Не заполняется раздел 7 декларации по НДС",
             "В декларации по НДС не заполняется раздел 7 \"Операции, не подлежащие налогообложению\". Проводки есть, а в отчет не попадают.",
             "1С:Бухгалтерия",
             "1. Проверить, что в документе реализации стоит ставка НДС \"Без НДС\".\n2. Убедиться, что операция попадает под ст. 149 НК РФ.\n3. В настройках учетной политики включить флажок \"Ведется раздельный учет НДС\".\n4. Перепровести документы за период и перезаполнить декларацию.",
             "Петров Алексей Владимирович", DateTime.Now.AddDays(-5).ToString("dd.MM.yyyy HH:mm"), 19),

            ("ЗУП-006", "Неверно рассчитывается районный коэффициент",
             "У сотрудника с районным коэффициентом 1.2 зарплата рассчитывается без учета коэффициента.",
             "1С:ЗУП",
             "1. Проверить территориальные условия в карточке сотрудника (\"Главное\" → \"Сотрудники\").\n2. Убедиться, что в настройках начисления указан районный коэффициент.\n3. Проверить график работы — не стоит ли флажок \"Не учитывать районный коэффициент\".\n4. Перепровести начисление зарплаты.",
             "Смирнов Дмитрий Игоревич", DateTime.Now.AddDays(-4).ToString("dd.MM.yyyy HH:mm"), 23),

            ("УТ-007", "Не проводится документ «Реализация товаров и услуг»",
             "При проведении документа реализации возникает ошибка \"Недостаточно товара на складе\", хотя остатки по отчету есть.",
             "1С:Управление торговлей",
             "1. Проверить, на каком складе числится товар (ордерный склад?).\n2. Убедиться, что товар не в резерве под другой заказ.\n3. Проверить, не истек ли срок годности (для партионного учета).\n4. Открыть \"Склад\" → \"Остатки товаров\" и сверить данные.",
             "Петров Алексей Владимирович", DateTime.Now.AddDays(-3).ToString("dd.MM.yyyy HH:mm"), 15),

            ("РОЗ-008", "Не печатается ценник на товар",
             "В 1С:Розница при печати ценника выходит пустой шаблон или ошибка \"Не найден макет\".",
             "1С:Розница",
             "1. Открыть \"Администрирование\" → \"Печатные формы, отчеты и обработки\".\n2. Проверить наличие макета \"Ценник\" в списке.\n3. Если макета нет — восстановить из шаблона (\"Еще\" → \"Восстановить из шаблона\").\n4. Перезапустить программу.",
             "Смирнов Дмитрий Игоревич", DateTime.Now.AddDays(-2).ToString("dd.MM.yyyy HH:mm"), 11),

            ("ОБЩ-009", "Не обновляется конфигурация через интернет",
             "При попытке обновления конфигурации через интернет возникает ошибка \"Не удалось получить список доступных обновлений\".",
             "1С:Бухгалтерия",
             "1. Проверить интернет-соединение и доступ к update.1c.ru.\n2. Проверить настройки прокси в 1С (\"Сервис\" → \"Параметры\").\n3. Убедиться, что лицензия 1С:ИТС активна.\n4. Попробовать обновить вручную через .cfu-файл.",
             "Петров Алексей Владимирович", DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy HH:mm"), 38),

            ("ОБЩ-010", "Не запускается 1С:Предприятие — ошибка лицензии",
             "При запуске 1С выдается ошибка \"Не найдена лицензия на использование программы\".",
             "1С:Бухгалтерия",
             "1. Проверить, подключен ли ключ защиты (USB или программный).\n2. Открыть \"Администрирование лицензий\" (утилита от 1С).\n3. Проверить, что лицензия не истекла и не привязана к другому ПК.\n4. При необходимости — переустановить лицензию через утилиту.",
             "Смирнов Дмитрий Игоревич", DateTime.Now.AddHours(-12).ToString("dd.MM.yyyy HH:mm"), 62)
        };

        foreach (var e in errors)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO errors (error_code, title, description, config_1c, steps, created_by, created_at, views)
                                VALUES ($c, $t, $d, $cfg, $s, $cb, $ca, $v)";
            cmd.Parameters.AddWithValue("$c", e.Code);
            cmd.Parameters.AddWithValue("$t", e.Title);
            cmd.Parameters.AddWithValue("$d", e.Desc);
            cmd.Parameters.AddWithValue("$cfg", e.Config);
            cmd.Parameters.AddWithValue("$s", e.Steps);
            cmd.Parameters.AddWithValue("$cb", e.CreatedBy);
            cmd.Parameters.AddWithValue("$ca", e.CreatedAt);
            cmd.Parameters.AddWithValue("$v", e.Views);
            cmd.ExecuteNonQuery();
        }
    }

    public static User? Authenticate(string username, string password)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, username, password_hash, role, full_name FROM users WHERE username = $u";
        cmd.Parameters.AddWithValue("$u", username.Trim());
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var hash = reader.GetString(2);
        if (!BCrypt.Net.BCrypt.Verify(password, hash)) return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Role = reader.GetString(3),
            FullName = reader.GetString(4)
        };
    }

    public static void ChangePassword(int userId, string newPassword)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE users SET password_hash = $h WHERE id = $id";
        cmd.Parameters.AddWithValue("$h", BCrypt.Net.BCrypt.HashPassword(newPassword));
        cmd.Parameters.AddWithValue("$id", userId);
        cmd.ExecuteNonQuery();
    }

    public static List<ErrorEntry> GetErrors(string searchQuery = "", string configFilter = "Все")
    {
        var result = new List<ErrorEntry>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var sql = "SELECT id, error_code, title, description, config_1c, steps, created_by, created_at, views FROM errors WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(searchQuery))
            sql += " AND (error_code LIKE $q OR title LIKE $q OR description LIKE $q)";
        if (configFilter != "Все")
            sql += " AND config_1c = $cfg";
        sql += " ORDER BY id DESC";

        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(searchQuery))
            cmd.Parameters.AddWithValue("$q", $"%{searchQuery.Trim()}%");
        if (configFilter != "Все")
            cmd.Parameters.AddWithValue("$cfg", configFilter);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ErrorEntry
            {
                Id = reader.GetInt32(0),
                ErrorCode = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                Config1C = reader.GetString(4),
                Steps = reader.GetString(5),
                CreatedBy = reader.GetString(6),
                CreatedAt = reader.GetString(7),
                Views = reader.GetInt32(8)
            });
        }
        return result;
    }

    public static void IncrementViews(int errorId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE errors SET views = views + 1 WHERE id = $id";
        cmd.Parameters.AddWithValue("$id", errorId);
        cmd.ExecuteNonQuery();
    }

    public static int AddError(ErrorEntry e)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO errors (error_code, title, description, config_1c, steps, created_by, created_at, views)
                            VALUES ($c, $t, $d, $cfg, $s, $cb, $ca, 0);
                            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$c", e.ErrorCode);
        cmd.Parameters.AddWithValue("$t", e.Title);
        cmd.Parameters.AddWithValue("$d", e.Description);
        cmd.Parameters.AddWithValue("$cfg", e.Config1C);
        cmd.Parameters.AddWithValue("$s", e.Steps);
        cmd.Parameters.AddWithValue("$cb", e.CreatedBy);
        cmd.Parameters.AddWithValue("$ca", e.CreatedAt);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void UpdateError(ErrorEntry e)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE errors SET error_code = $c, title = $t, description = $d,
                            config_1c = $cfg, steps = $s WHERE id = $id";
        cmd.Parameters.AddWithValue("$c", e.ErrorCode);
        cmd.Parameters.AddWithValue("$t", e.Title);
        cmd.Parameters.AddWithValue("$d", e.Description);
        cmd.Parameters.AddWithValue("$cfg", e.Config1C);
        cmd.Parameters.AddWithValue("$s", e.Steps);
        cmd.Parameters.AddWithValue("$id", e.Id);
        cmd.ExecuteNonQuery();
    }
    public static void DeleteError(int errorId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM errors WHERE id = $id";
        cmd.Parameters.AddWithValue("$id", errorId);
        cmd.ExecuteNonQuery();
    }
    public static bool ImportError(ErrorEntry e)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM errors WHERE error_code = $c";
        checkCmd.Parameters.AddWithValue("$c", e.ErrorCode);
        if (Convert.ToInt64(checkCmd.ExecuteScalar()) > 0)
        {
            return false; 
        }

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO errors (error_code, title, description, config_1c, steps, created_by, created_at, views)
                            VALUES ($c, $t, $d, $cfg, $s, $cb, $ca, $v)";
        cmd.Parameters.AddWithValue("$c", e.ErrorCode);
        cmd.Parameters.AddWithValue("$t", e.Title);
        cmd.Parameters.AddWithValue("$d", e.Description);
        cmd.Parameters.AddWithValue("$cfg", e.Config1C);
        cmd.Parameters.AddWithValue("$s", e.Steps);
        cmd.Parameters.AddWithValue("$cb", e.CreatedBy);
        cmd.Parameters.AddWithValue("$ca", e.CreatedAt);
        cmd.Parameters.AddWithValue("$v", e.Views);
        cmd.ExecuteNonQuery();
        return true;
    }
}