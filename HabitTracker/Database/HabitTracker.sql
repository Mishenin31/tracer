-- ============================================================
--  Трекер привычек и продуктивности — схема БД (SQL Server)
-- ============================================================

IF DB_ID('HabitTracker') IS NULL
    CREATE DATABASE HabitTracker;
GO

USE HabitTracker;
GO

-- ---------- Удаление существующих объектов ----------
IF OBJECT_ID('dbo.HabitLogs', 'U')   IS NOT NULL DROP TABLE dbo.HabitLogs;
IF OBJECT_ID('dbo.Habits', 'U')      IS NOT NULL DROP TABLE dbo.Habits;
IF OBJECT_ID('dbo.Tasks', 'U')       IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Categories', 'U')  IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users', 'U')       IS NOT NULL DROP TABLE dbo.Users;
GO

-- ---------- Пользователи ----------
CREATE TABLE dbo.Users (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Login     NVARCHAR(50)  NOT NULL,
    Password  NVARCHAR(100) NOT NULL,
    FullName  NVARCHAR(100) NOT NULL,
    Role      INT           NOT NULL DEFAULT 1,  -- 0 = Администратор, 1 = Пользователь
    CONSTRAINT UQ_Users_Login UNIQUE (Login),
    CONSTRAINT CK_Users_Role  CHECK (Role IN (0, 1))
);
GO

-- ---------- Категории ----------
CREATE TABLE dbo.Categories (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL,
    ColorHex CHAR(7)       NOT NULL DEFAULT '#7C6BAD',
    CONSTRAINT UQ_Categories_Name UNIQUE (Name),
    CONSTRAINT CK_Categories_Color CHECK (ColorHex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);
GO

-- ---------- Привычки ----------
CREATE TABLE dbo.Habits (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT NOT NULL,
    Name          NVARCHAR(150) NOT NULL,
    CategoryId    INT NULL,
    Frequency     INT NOT NULL DEFAULT 0,  -- 0 = Ежедневно, 1 = Еженедельно
    TargetPerWeek INT NOT NULL DEFAULT 7,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Habits_User     FOREIGN KEY (UserId)     REFERENCES dbo.Users(Id)      ON DELETE CASCADE,
    CONSTRAINT FK_Habits_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id) ON DELETE SET NULL,
    CONSTRAINT CK_Habits_Freq     CHECK (Frequency IN (0, 1)),
    CONSTRAINT CK_Habits_Target   CHECK (TargetPerWeek BETWEEN 1 AND 7)
);
GO

-- ---------- Отметки выполнения привычек ----------
CREATE TABLE dbo.HabitLogs (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    HabitId   INT NOT NULL,
    LogDate   DATE NOT NULL,
    Completed BIT  NOT NULL DEFAULT 1,
    CONSTRAINT FK_HabitLogs_Habit FOREIGN KEY (HabitId) REFERENCES dbo.Habits(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_HabitLogs_HabitDate UNIQUE (HabitId, LogDate)
);
GO

-- ---------- Задачи ----------
CREATE TABLE dbo.Tasks (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    UserId   INT NOT NULL,
    Title    NVARCHAR(200) NOT NULL,
    Priority INT NOT NULL DEFAULT 1,  -- 0 = Низкий, 1 = Средний, 2 = Высокий
    DueDate  DATE NULL,
    IsDone   BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Tasks_User   FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Tasks_Prio   CHECK (Priority IN (0, 1, 2))
);
GO

-- ---------- Индексы ----------
CREATE INDEX IX_Habits_UserId    ON dbo.Habits(UserId);
CREATE INDEX IX_HabitLogs_HabitId ON dbo.HabitLogs(HabitId);
CREATE INDEX IX_Tasks_UserId     ON dbo.Tasks(UserId);
GO

-- ============================================================
--  Тестовые данные
-- ============================================================

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, Login, Password, FullName, Role) VALUES
    (1, N'admin', N'admin', N'Администратор', 0),
    (2, N'user',  N'user',  N'Иван Петров',   1);
SET IDENTITY_INSERT dbo.Users OFF;
GO

SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (Id, Name, ColorHex) VALUES
    (1, N'Здоровье',  '#6FB98F'),
    (2, N'Работа',    '#5B8DEF'),
    (3, N'Развитие',  '#C77DD6'),
    (4, N'Быт',       '#E8A14B');
SET IDENTITY_INSERT dbo.Categories OFF;
GO

SET IDENTITY_INSERT dbo.Habits ON;
INSERT INTO dbo.Habits (Id, UserId, Name, CategoryId, Frequency, TargetPerWeek) VALUES
    (1, 2, N'Зарядка утром',    1, 0, 7),
    (2, 2, N'Читать 30 минут',  3, 0, 7),
    (3, 2, N'Пить 2 л воды',    1, 0, 7),
    (4, 2, N'Планёрка недели',  2, 1, 1),
    (5, 2, N'Уборка',           4, 1, 2);
SET IDENTITY_INSERT dbo.Habits OFF;
GO

-- Отметки за последние дни (серии)
INSERT INTO dbo.HabitLogs (HabitId, LogDate, Completed) VALUES
    (1, CAST(DATEADD(day, -1, GETDATE()) AS DATE), 1),
    (1, CAST(DATEADD(day, -2, GETDATE()) AS DATE), 1),
    (2, CAST(DATEADD(day, -1, GETDATE()) AS DATE), 1),
    (2, CAST(DATEADD(day, -2, GETDATE()) AS DATE), 1),
    (2, CAST(DATEADD(day, -3, GETDATE()) AS DATE), 1),
    (3, CAST(DATEADD(day, -1, GETDATE()) AS DATE), 1);
GO

SET IDENTITY_INSERT dbo.Tasks ON;
INSERT INTO dbo.Tasks (Id, UserId, Title, Priority, DueDate, IsDone) VALUES
    (1, 2, N'Сдать квартальный отчёт', 2, CAST(DATEADD(day, 2, GETDATE()) AS DATE), 0),
    (2, 2, N'Купить продукты',         1, CAST(GETDATE() AS DATE),                  0),
    (3, 2, N'Записаться к врачу',       2, CAST(DATEADD(day, 1, GETDATE()) AS DATE), 0),
    (4, 2, N'Разобрать почту',          0, NULL,                                    1);
SET IDENTITY_INSERT dbo.Tasks OFF;
GO

PRINT N'База данных HabitTracker успешно создана и заполнена тестовыми данными.';
GO
