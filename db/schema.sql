-- Скрипт создания и заполнения базы данных DB_Bykov_Payment
-- УП.01.01, Быков Д.А., 4ИСИП-323

CREATE DATABASE DB_Bykov_Payment
GO

USE DB_Bykov_Payment
GO

-- пользователи
CREATE TABLE [User] (
    ID int IDENTITY(1,1) NOT NULL,
    Login nvarchar(50) NOT NULL,
    Password nvarchar(100) NOT NULL,
    Role nvarchar(20) NOT NULL,
    FIO nvarchar(150) NOT NULL,
    Photo nvarchar(MAX) NULL,
    CONSTRAINT PK_User PRIMARY KEY (ID)
)
GO

-- категории платежей
CREATE TABLE Category (
    ID int IDENTITY(1,1) NOT NULL,
    Name nvarchar(100) NOT NULL,
    CONSTRAINT PK_Category PRIMARY KEY (ID)
)
GO

-- платежи
CREATE TABLE Payment (
    ID int IDENTITY(1,1) NOT NULL,
    CategoryID int NOT NULL,
    UserID int NOT NULL,
    Date date NOT NULL,
    Name nvarchar(100) NOT NULL,
    Num int NOT NULL,
    Price decimal(10, 2) NOT NULL,
    CONSTRAINT PK_Payment PRIMARY KEY (ID)
)
GO

-- связи с каскадным удалением
ALTER TABLE Payment ADD CONSTRAINT FK_Payment_User
    FOREIGN KEY (UserID) REFERENCES [User] (ID) ON DELETE CASCADE
GO

ALTER TABLE Payment ADD CONSTRAINT FK_Payment_Category
    FOREIGN KEY (CategoryID) REFERENCES Category (ID) ON DELETE CASCADE
GO

-- тестовые данные

-- пароли хранятся в виде SHA1-хэша
-- 7C4A8D09CA3762AF61E59520943DC26494F8941B = 123456
-- 7C222FB2927D828AF22F592134E8932480637C0D = 12345678
INSERT INTO [User] (Login, Password, Role, FIO, Photo) VALUES
(N'123456',   N'7C222FB2927D828AF22F592134E8932480637C0D', N'Admin', N'Иванов Иван Иванович', NULL),
(N'petrov',   N'7C4A8D09CA3762AF61E59520943DC26494F8941B', N'User',  N'Петров Петр Петрович', NULL),
(N'sidorova', N'7C4A8D09CA3762AF61E59520943DC26494F8941B', N'Admin', N'Сидорова Анна Николаевна', NULL),
(N'bykov',    N'7C222FB2927D828AF22F592134E8932480637C0D', N'User',  N'Быков Денис Александрович', NULL)
GO

INSERT INTO Category (Name) VALUES
(N'Коммунальные услуги'),
(N'Транспорт'),
(N'Связь и интернет'),
(N'Продукты питания'),
(N'Развлечения')
GO

-- CategoryID: 1 - Коммунальные услуги, 2 - Транспорт, 3 - Связь и интернет, 4 - Продукты питания, 5 - Развлечения
-- UserID: 1 - Иванов, 2 - Петров, 3 - Сидорова, 4 - Быков
INSERT INTO Payment (CategoryID, UserID, Date, Name, Num, Price) VALUES
(1, 1, '2026-08-05', N'Оплата электроэнергии', 1, 1850.40),
(1, 1, '2026-09-05', N'Оплата водоснабжения', 1, 960.00),
(2, 1, '2026-09-12', N'Проездной на метро', 1, 2900.00),
(3, 1, '2026-09-20', N'Мобильная связь', 1, 450.00),
(4, 1, '2026-10-02', N'Продукты в супермаркете', 3, 1240.50),
(5, 1, '2026-10-10', N'Билет в кино', 2, 450.00),

(1, 2, '2026-08-10', N'Квартплата', 1, 5420.00),
(2, 2, '2026-08-15', N'Такси', 4, 380.00),
(3, 2, '2026-09-01', N'Домашний интернет', 1, 650.00),
(4, 2, '2026-09-18', N'Продукты на рынке', 2, 870.00),
(5, 2, '2026-10-05', N'Концерт', 1, 3500.00),

(1, 3, '2026-08-20', N'Оплата газа', 1, 720.30),
(3, 3, '2026-09-03', N'Мобильная связь', 1, 550.00),
(4, 3, '2026-09-25', N'Доставка продуктов', 2, 1680.00),
(5, 3, '2026-10-12', N'Абонемент в бассейн', 1, 4200.00),
(2, 3, '2026-10-15', N'Бензин', 30, 62.40),

(2, 4, '2026-08-25', N'Проездной на автобус', 1, 1500.00),
(3, 4, '2026-09-10', N'Домашний интернет', 1, 600.00),
(4, 4, '2026-09-28', N'Продукты в магазине у дома', 5, 430.00),
(5, 4, '2026-10-08', N'Подписка на онлайн-кинотеатр', 1, 399.00),
(1, 4, '2026-10-18', N'Оплата электроэнергии', 1, 1320.75)
GO
