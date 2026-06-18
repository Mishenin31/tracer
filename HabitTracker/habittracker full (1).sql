-- ============================================================
--  Трекер привычек и продуктивности — ПОЛНАЯ схема БД (SQL Server)
--  Базовые таблицы: Users, Categories, Habits, HabitLogs, Tasks
--  Дополнительные:  Goals, Reminders, MoodLogs, Achievements,
--                    UserAchievements
-- ============================================================

IF DB_ID('HabitTracker') IS NULL
    CREATE DATABASE HabitTracker;
GO

USE HabitTracker;
GO

-- ---------- Удаление существующих объектов (в порядке зависимостей) ----------
IF OBJECT_ID('dbo.UserAchievements', 'U') IS NOT NULL DROP TABLE dbo.UserAchievements;
IF OBJECT_ID('dbo.Achievements', 'U')     IS NOT NULL DROP TABLE dbo.Achievements;
IF OBJECT_ID('dbo.MoodLogs', 'U')         IS NOT NULL DROP TABLE dbo.MoodLogs;
IF OBJECT_ID('dbo.Reminders', 'U')        IS NOT NULL DROP TABLE dbo.Reminders;
IF OBJECT_ID('dbo.Goals', 'U')            IS NOT NULL DROP TABLE dbo.Goals;
IF OBJECT_ID('dbo.HabitLogs', 'U')        IS NOT NULL DROP TABLE dbo.HabitLogs;
IF OBJECT_ID('dbo.Habits', 'U')           IS NOT NULL DROP TABLE dbo.Habits;
IF OBJECT_ID('dbo.Tasks', 'U')            IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Categories', 'U')       IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users', 'U')            IS NOT NULL DROP TABLE dbo.Users;
GO

/* ============================================================
   БАЗОВЫЕ ТАБЛИЦЫ
   ============================================================ */

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
    CONSTRAINT FK_Tasks_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Tasks_Prio CHECK (Priority IN (0, 1, 2))
);
GO

/* ============================================================
   ДОПОЛНИТЕЛЬНЫЕ ТАБЛИЦЫ
   ============================================================ */

-- ---------- Цели ----------
CREATE TABLE dbo.Goals (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL,
    Title       NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    TargetDate  DATE NULL,
    IsAchieved  BIT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Goals_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

-- Привязка привычки к цели (необязательная связь)
ALTER TABLE dbo.Habits
    ADD GoalId INT NULL
        CONSTRAINT FK_Habits_Goal FOREIGN KEY (GoalId) REFERENCES dbo.Goals(Id) ON DELETE SET NULL;
GO

-- ---------- Напоминания ----------
CREATE TABLE dbo.Reminders (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL,
    HabitId      INT NULL,
    TaskId       INT NULL,
    RemindTime   TIME NOT NULL,
    DaysOfWeek   NVARCHAR(20) NOT NULL DEFAULT N'1234567',  -- цифры 1-7 = дни недели
    IsActive     BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Reminders_User   FOREIGN KEY (UserId)  REFERENCES dbo.Users(Id)  ON DELETE CASCADE,
    CONSTRAINT FK_Reminders_Habit  FOREIGN KEY (HabitId) REFERENCES dbo.Habits(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Reminders_Task   FOREIGN KEY (TaskId)  REFERENCES dbo.Tasks(Id)  ON DELETE CASCADE,
    CONSTRAINT CK_Reminders_Target CHECK (HabitId IS NOT NULL OR TaskId IS NOT NULL)
);
GO

-- ---------- Дневник настроения ----------
CREATE TABLE dbo.MoodLogs (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    LogDate   DATE NOT NULL,
    MoodScore TINYINT NOT NULL,   -- 1 = очень плохо ... 5 = отлично
    Note      NVARCHAR(300) NULL,
    CONSTRAINT FK_MoodLogs_User  FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_MoodLogs_Score CHECK (MoodScore BETWEEN 1 AND 5),
    CONSTRAINT UQ_MoodLogs_UserDate UNIQUE (UserId, LogDate)
);
GO

-- ---------- Достижения (справочник) ----------
CREATE TABLE dbo.Achievements (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    Description NVARCHAR(300) NULL,
    IconUrl     NVARCHAR(300) NULL,
    CONSTRAINT UQ_Achievements_Name UNIQUE (Name)
);
GO

-- ---------- Достижения пользователей (many-to-many) ----------
CREATE TABLE dbo.UserAchievements (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT NOT NULL,
    AchievementId INT NOT NULL,
    EarnedAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_UserAch_User        FOREIGN KEY (UserId)        REFERENCES dbo.Users(Id)        ON DELETE CASCADE,
    CONSTRAINT FK_UserAch_Achievement FOREIGN KEY (AchievementId) REFERENCES dbo.Achievements(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserAch_UserAchievement UNIQUE (UserId, AchievementId)
);
GO

-- ---------- Индексы ----------
CREATE INDEX IX_Habits_UserId           ON dbo.Habits(UserId);
CREATE INDEX IX_HabitLogs_HabitId       ON dbo.HabitLogs(HabitId);
CREATE INDEX IX_Tasks_UserId            ON dbo.Tasks(UserId);
CREATE INDEX IX_Goals_UserId            ON dbo.Goals(UserId);
CREATE INDEX IX_Reminders_UserId        ON dbo.Reminders(UserId);
CREATE INDEX IX_MoodLogs_UserId         ON dbo.MoodLogs(UserId);
CREATE INDEX IX_UserAchievements_UserId ON dbo.UserAchievements(UserId);
GO

/* ============================================================
   ТЕСТОВЫЕ ДАННЫЕ
   ============================================================ */

-- Пользователи --------------------------------------------------------------
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, Login, Password, FullName, Role) VALUES
    (1, N'admin', N'admin', N'Администратор', 0),
    (2, N'user',  N'user',  N'Иван Петров',   1);
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- Категории -------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (Id, Name, ColorHex) VALUES
    (1, N'Здоровье',  '#6FB98F'),
    (2, N'Работа',    '#5B8DEF'),
    (3, N'Развитие',  '#C77DD6'),
    (4, N'Быт',       '#E8A14B');
SET IDENTITY_INSERT dbo.Categories OFF;
GO

-- Привычки -------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Habits ON;
INSERT INTO dbo.Habits (Id, UserId, Name, CategoryId, Frequency, TargetPerWeek) VALUES
    (1, 2, N'Бег по утрам',          1, 1, 3),
    (2, 2, N'Чтение книг',           3, 0, 7),
    (3, 2, N'Уборка квартиры',       4, 1, 1),
    (4, 2, N'Планирование рабочего дня', 2, 0, 5);
SET IDENTITY_INSERT dbo.Habits OFF;
GO

-- Отметки выполнения привычек -------------------------------------------------
INSERT INTO dbo.HabitLogs (HabitId, LogDate, Completed) VALUES
    (1, '2026-06-15', 1),
    (1, '2026-06-17', 1),
    (2, '2026-06-15', 1),
    (2, '2026-06-16', 1),
    (2, '2026-06-17', 1),
    (4, '2026-06-16', 1),
    (4, '2026-06-17', 1);
GO

-- Задачи -------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Tasks ON;
INSERT INTO dbo.Tasks (Id, UserId, Title, Priority, DueDate, IsDone) VALUES
    (1, 2, N'Подготовить отчёт по проекту', 2, '2026-06-20', 0),
    (2, 2, N'Купить продукты',              0, '2026-06-19', 0),
    (3, 2, N'Записаться к врачу',           1, '2026-06-25', 0);
SET IDENTITY_INSERT dbo.Tasks OFF;
GO

-- Цели --------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Goals ON;
INSERT INTO dbo.Goals (Id, UserId, Title, Description, TargetDate, IsAchieved) VALUES
    (1, 2, N'Пробежать полумарафон',    N'Подготовка к забегу на 21 км', '2026-09-15', 0),
    (2, 2, N'Прочитать 12 книг за год', N'Одна книга в месяц',           '2026-12-31', 0);
SET IDENTITY_INSERT dbo.Goals OFF;
GO

-- Привязываем привычки к целям
UPDATE dbo.Habits SET GoalId = 1 WHERE Id = 1;  -- Бег по утрам -> полумарафон
UPDATE dbo.Habits SET GoalId = 2 WHERE Id = 2;  -- Чтение книг -> 12 книг за год
GO

-- Напоминания ---------------------------------------------------------------
INSERT INTO dbo.Reminders (UserId, HabitId, TaskId, RemindTime, DaysOfWeek, IsActive) VALUES
    (2, 1, NULL, '07:00', N'135',     1),
    (2, 2, NULL, '21:00', N'1234567', 1),
    (2, NULL, 1, '09:30', N'12345',   1);
GO

-- Дневник настроения ---------------------------------------------------------
INSERT INTO dbo.MoodLogs (UserId, LogDate, MoodScore, Note) VALUES
    (2, '2026-06-15', 4, N'Хороший продуктивный день'),
    (2, '2026-06-16', 3, N'Устал, но в целом нормально'),
    (2, '2026-06-17', 5, N'Отличное настроение, всё успел');
GO

-- Достижения (справочник) -----------------------------------------------
SET IDENTITY_INSERT dbo.Achievements ON;
INSERT INTO dbo.Achievements (Id, Name, Description, IconUrl) VALUES
    (1, N'Первый шаг',       N'Создана первая привычка',                 N'https://img.icons8.com/color/96/medal2.png'),
    (2, N'Неделя подряд',    N'Привычка выполнена 7 дней без перерыва',  N'https://img.icons8.com/color/96/fire-element.png'),
    (3, N'Месяц дисциплины', N'Привычка выполнена 30 дней без перерыва', N'https://img.icons8.com/color/96/trophy.png'),
    (4, N'Цель достигнута',  N'Завершена хотя бы одна цель',             N'https://img.icons8.com/color/96/goal.png');
SET IDENTITY_INSERT dbo.Achievements OFF;
GO

-- Достижения пользователей -----------------------------------------------
INSERT INTO dbo.UserAchievements (UserId, AchievementId) VALUES
    (2, 1),
    (2, 2);
GO

/* ============================================================
   ПРОВЕРОЧНЫЕ ЗАПРОСЫ
   ============================================================ */

-- Привычки пользователя с категорией и целью
SELECT h.Name AS Привычка, c.Name AS Категория, g.Title AS Цель, h.TargetPerWeek
FROM   dbo.Habits h
LEFT JOIN dbo.Categories c ON c.Id = h.CategoryId
LEFT JOIN dbo.Goals g      ON g.Id = h.GoalId
WHERE  h.UserId = 2;
GO

-- Прогресс по привычкам (число выполнений)
SELECT h.Name AS Привычка, COUNT(hl.Id) AS Выполнений
FROM   dbo.Habits h
LEFT JOIN dbo.HabitLogs hl ON hl.HabitId = h.Id AND hl.Completed = 1
WHERE  h.UserId = 2
GROUP BY h.Name;
GO

-- Активные напоминания пользователя
SELECT u.FullName AS Пользователь,
       COALESCE(h.Name, t.Title) AS Объект,
       r.RemindTime, r.DaysOfWeek
FROM   dbo.Reminders r
JOIN   dbo.Users u ON u.Id = r.UserId
LEFT JOIN dbo.Habits h ON h.Id = r.HabitId
LEFT JOIN dbo.Tasks  t ON t.Id = r.TaskId
WHERE  r.IsActive = 1;
GO

-- Динамика настроения пользователя
SELECT LogDate AS Дата, MoodScore AS Настроение, Note AS Заметка
FROM   dbo.MoodLogs
WHERE  UserId = 2
ORDER BY LogDate;
GO

-- Достижения пользователя
SELECT u.FullName AS Пользователь, a.Name AS Достижение, ua.EarnedAt AS Получено
FROM   dbo.UserAchievements ua
JOIN   dbo.Users u        ON u.Id = ua.UserId
JOIN   dbo.Achievements a ON a.Id = ua.AchievementId
ORDER BY ua.EarnedAt;
GO

-- Невыполненные задачи по приоритету
SELECT Title AS Задача,
       CASE Priority WHEN 0 THEN N'Низкий' WHEN 1 THEN N'Средний' ELSE N'Высокий' END AS Приоритет,
       DueDate AS Срок
FROM   dbo.Tasks
WHERE  UserId = 2 AND IsDone = 0
ORDER BY Priority DESC, DueDate;
GO
