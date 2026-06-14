-- create database База_Мероприятий COLLATE Cyrillic_General_CI_AS;
USE База_Мероприятий;
GO
CREATE PROCEDURE СоздатьАккаунт
@Логин NVARCHAR(50),
@Пароль NVARCHAR(100),
@Почта NVARCHAR(100) null,
@Номер VARCHAR(50) null
AS
BEGIN
    DECLARE @PasswordHash BINARY(32);
    SET @PasswordHash = HASHBYTES('SHA2_256', @Пароль);
    
    INSERT INTO Аккаунты (Логин, Почта, Номер, Пароль)
    VALUES (@Логин, @Почта, @Номер, @PasswordHash);
END;

GO
CREATE PROCEDURE Авторизация_Пользователя
    @Ввод NVARCHAR(100), -- Сюда передаем либо логин, либо почту, либо телефон
    @Пароль NVARCHAR(100),
    @Успех BIT OUTPUT
AS
BEGIN
    DECLARE @PasswordHash BINARY(32) = HASHBYTES('SHA2_256', @Пароль);
    
    IF EXISTS (
        SELECT 1 FROM Аккаунты 
        WHERE (Логин = @Ввод OR Почта = @Ввод OR Номер = @Ввод) 
          AND Пароль = @PasswordHash
    )
        SET @Успех = 1;
    ELSE
        SET @Успех = 0;
END;

GO
CREATE PROCEDURE Оформить_Билет_Место
    @Логин NVARCHAR(50),
    @Событие_ИД NVARCHAR(50),
    @Планировка_ИД NVARCHAR(50),
    @Место_ИД NVARCHAR(50)
AS
BEGIN
    -- Начинаем транзакцию, чтобы никто не перехватил место во время оформления
    BEGIN TRANSACTION;

    -- 1. Проверяем, не куплено ли уже это место
    IF EXISTS (SELECT 1 FROM Билеты WHERE Событие_ИД = @Событие_ИД AND Место_ИД = @Место_ИД)
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Место уже занято.', 16, 1);
        RETURN;
    END

    -- 2. Узнаем цену места через маппинг категорий
    DECLARE @Цена MONEY;
    SELECT @Цена = ЦП.Цена
    FROM Событие_Места_Категории СМК
    JOIN Ценовая_Политика ЦП ON СМК.Событие_ИД = ЦП.Событие_ИД AND СМК.Категория_ИД = ЦП.Категория_ИД
    WHERE СМК.Событие_ИД = @Событие_ИД AND СМК.Планировка_ИД = @Планировка_ИД AND СМК.Место_ИД = @Место_ИД;

    IF @Цена IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Место не предназначено для продажи или цена не найдена.', 16, 1);
        RETURN;
    END

    -- 3. Генерируем уникальный ИД заказа и записываем билет
    DECLARE @Заказ_ИД NVARCHAR(50) = NEWID();
    
    INSERT INTO Билеты (Заказ_ИД, Логин, Событие_ИД, Место_ИД, Зона_ИД, Сумма_Оплаты)
    VALUES (@Заказ_ИД, @Логин, @Событие_ИД, @Место_ИД, NULL, @Цена);

    COMMIT TRANSACTION;
END;
GO


CREATE OR ALTER PROCEDURE sp_Authenticate
    @Identity NVARCHAR(100), -- Логин, почта или телефон
    @Password NVARCHAR(100),
    @IsRegistration BIT
AS
BEGIN
    IF @IsRegistration = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM Аккаунты WHERE (Логин=@Identity OR Почта=@Identity OR Номер=@Identity) AND Пароль=HASHBYTES('SHA2_256', @Password))
            SELECT 1 AS Result, Логин FROM Аккаунты WHERE Логин=@Identity OR Почта=@Identity OR Номер=@Identity;
        ELSE
            SELECT 0 AS Result;
    END
    ELSE
    BEGIN
        -- Логика регистрации (проверка уникальности и INSERT)
        IF NOT EXISTS (SELECT 1 FROM Аккаунты WHERE Логин=@Identity)
        BEGIN
            INSERT INTO Аккаунты (Логин, Пароль) VALUES (@Identity, HASHBYTES('SHA2_256', @Password));
            SELECT 1 AS Result, @Identity AS Логин;
        END
        ELSE SELECT 0 AS Result;
    END
END


