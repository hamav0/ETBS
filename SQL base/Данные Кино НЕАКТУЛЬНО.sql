USE База_Мероприятий;
GO

SET NOCOUNT ON;

--- =======================================================================
--- БЛОК НАСТРОЕК СЕАНСОВ И ЦЕН (КОНФИГУРИРУЙТЕ ДАННЫЕ ЗДЕСЬ)
--- =======================================================================
DECLARE @MovieID NVARCHAR(50) = 'INFO-BACKROOMS';
DECLARE @CinemaPlanID NVARCHAR(50) = 'KINO-IMYA-MAIN';

-- Базовое время для сеансов (часы и минуты)
DECLARE @TimeZ1 TIME = '19:30:00'; -- Зал 1
DECLARE @TimeZ2 TIME = '20:15:00'; -- Зал 2 (7 рядов)
DECLARE @TimeZ3 TIME = '21:00:00'; -- Зал 3 (9 рядов)

-- Настройка стоимости билетов на категории
DECLARE @PriceStandard MONEY = 400.00;  -- Обычное кресло
DECLARE @PriceSofa     MONEY = 850.00;  -- Диван (LoveSeat)

--- =======================================================================
--- 1. РЕГИСТРАЦИЯ ПЛОЩАДКИ И КИНОРЕЛИЗА (ЕСЛИ ИХ НЕТ)
--- =======================================================================
DECLARE @CinemaID INT = (SELECT TOP 1 Площадка_ИД FROM Площадки WHERE Планировка_ИД = @CinemaPlanID);

IF @CinemaID IS NULL
BEGIN
    INSERT INTO Площадки (Название, Адрес, Номер, Описание, Планировка_ИД)
    VALUES ('Кинотеатр "Имя"', 'Россия, г. Краснодар, ул. Красная, 100', '+7 (800) 123-45-67', 'Главный мультиплекс города', @CinemaPlanID);
    SET @CinemaID = SCOPE_IDENTITY();
END

IF NOT EXISTS (SELECT 1 FROM Информация WHERE Информация_ИД = @MovieID)
BEGIN
    INSERT INTO Информация (Информация_ИД, Название, Оригинальное_Название, Возрастной_Рейтинг, Продолжительность_Минут, Описание, Год_Производства)
    VALUES (@MovieID, 'Закулисье', 'The Backrooms', '16+', 115, 'Психологический триллер про бесконечные коридоры', '2026-05-01');
END

--- =======================================================================
--- 2. АВТОМАТИЧЕСКАЯ ГЕНЕРАЦИЯ СЕАНСОВ НА СЕГОДНЯ И СЛЕДУЮЩИЕ 7 ДНЕЙ
--- =======================================================================
BEGIN
    DECLARE @DayOffset INT = 0;
    DECLARE @MaxDays INT = 7; -- Сегодня (0) + 7 дней = всего 8 дней проката

    -- Извлекаем только чистую дату без времени
    DECLARE @BaseDate DATE = CAST(GETDATE() AS DATE);

    WHILE @DayOffset <= @MaxDays
    BEGIN
        -- Вычисляем дату для текущего шага цикла
        DECLARE @TargetDate DATE = DATEADD(DAY, @DayOffset, @BaseDate);
        
        -- Вычисляем время окончания онлайн-продаж (за 1 час до начала первого сеанса текущего дня)
        DECLARE @SaleUntil DATETIME = CAST(@TargetDate AS DATETIME) + CAST('18:30:00' AS DATETIME);

        -- Идентификаторы сессий для каждого зала на этот день
        DECLARE @EvID_Z1 NVARCHAR(50) = 'EV-BACKROOMS-Z1-D' + CAST(@DayOffset AS NVARCHAR);
        DECLARE @EvID_Z2 NVARCHAR(50) = 'EV-BACKROOMS-Z2-D' + CAST(@DayOffset AS NVARCHAR);
        DECLARE @EvID_Z3 NVARCHAR(50) = 'EV-BACKROOMS-Z3-D' + CAST(@DayOffset AS NVARCHAR);

        -- Конкретные DateTime начала сеансов
        DECLARE @DT_Z1 DATETIME = CAST(@TargetDate AS DATETIME) + CAST(@TimeZ1 AS DATETIME);
        DECLARE @DT_Z2 DATETIME = CAST(@TargetDate AS DATETIME) + CAST(@TimeZ2 AS DATETIME);
        DECLARE @DT_Z3 DATETIME = CAST(@TargetDate AS DATETIME) + CAST(@TimeZ3 AS DATETIME);

        -------------------------------------------------------------------
        -- ЗАЛ 1: Генерация сеанса, категорий и цен
        -------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM События WHERE Событие_ИД = @EvID_Z1)
        BEGIN
            INSERT INTO События (Событие_ИД, Информация_ИД, Дата_Время, Площадка_ИД, Номер_Зала, Планировка_ИД)
            VALUES (@EvID_Z1, @MovieID, @DT_Z1, @CinemaID, 1, 'KINO-IMYA-ZAL1');

            INSERT INTO Событие_Категории (Событие_ИД, Категория_ИД, Название, Цвет, Описание) 
            VALUES (@EvID_Z1, 1, 'Стандарт', '#00BFFF', 'Обычное место');

            INSERT INTO Ценовая_Политика (Событие_ИД, Категория_ИД, Цена, Актуальность) 
            VALUES (@EvID_Z1, 1, @PriceStandard, @SaleUntil);

            INSERT INTO Событие_Места_Категории (Событие_ИД, Планировка_ИД, Место_ИД, Категория_ИД)
            SELECT @EvID_Z1, Планировка_ИД, Место_ИД, 1
            FROM Планировки_Мест WHERE Планировка_ИД = 'KINO-IMYA-ZAL1';
        END

        -------------------------------------------------------------------
        -- ЗАЛ 2: Генерация сеанса, категорий и цен (Диваны 7 ряд и центр 6-7)
        -------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM События WHERE Событие_ИД = @EvID_Z2)
        BEGIN
            INSERT INTO События (Событие_ИД, Информация_ИД, Дата_Время, Площадка_ИД, Номер_Зала, Планировка_ИД)
            VALUES (@EvID_Z2, @MovieID, @DT_Z2, @CinemaID, 2, 'KINO-IMYA-ZAL2');

            INSERT INTO Событие_Категории (Событие_ИД, Категория_ИД, Название, Цвет, Описание) VALUES 
            (@EvID_Z2, 1, 'Стандарт', '#00BFFF', 'Обычное место'),
            (@EvID_Z2, 2, 'Диван', '#FFA500', 'Уютный LoveSeat');

            INSERT INTO Ценовая_Политика (Событие_ИД, Категория_ИД, Цена, Актуальность) VALUES 
            (@EvID_Z2, 1, @PriceStandard, @SaleUntil),
            (@EvID_Z2, 2, @PriceSofa, @SaleUntil);

            INSERT INTO Событие_Места_Категории (Событие_ИД, Планировка_ИД, Место_ИД, Категория_ИД)
            SELECT 
                @EvID_Z2, Планировка_ИД, Место_ИД,
                CASE WHEN Ряд = 7 THEN 2 WHEN Номер BETWEEN 6 AND 7 THEN 2 ELSE 1 END
            FROM Планировки_Мест WHERE Планировка_ИД = 'KINO-IMYA-ZAL2';
        END

        -------------------------------------------------------------------
        -- ЗАЛ 3: Генерация сеанса, категорий и цен (Шахматные диваны)
        -------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM События WHERE Событие_ИД = @EvID_Z3)
        BEGIN
            INSERT INTO События (Событие_ИД, Информация_ИД, Дата_Время, Площадка_ИД, Номер_Зала, Планировка_ИД)
            VALUES (@EvID_Z3, @MovieID, @DT_Z3, @CinemaID, 3, 'KINO-IMYA-ZAL3');

            INSERT INTO Событие_Категории (Событие_ИД, Категория_ИД, Название, Цвет, Описание) VALUES 
            (@EvID_Z3, 1, 'Стандарт', '#00BFFF', 'Обычное место'),
            (@EvID_Z3, 2, 'Диван', '#FFA500', 'Комфортный диван');

            INSERT INTO Ценовая_Политика (Событие_ИД, Категория_ИД, Цена, Актуальность) VALUES 
            (@EvID_Z3, 1, @PriceStandard, @SaleUntil),
            (@EvID_Z3, 2, @PriceSofa, @SaleUntil);

            INSERT INTO Событие_Места_Категории (Событие_ИД, Планировка_ИД, Место_ИД, Категория_ИД)
            SELECT 
                @EvID_Z3, Планировка_ИД, Место_ИД,
                CASE 
                    WHEN Ряд = 9 THEN 2 
                    WHEN Ряд IN (7, 8) AND Номер BETWEEN 9 AND 10 THEN 2 
                    WHEN Номер BETWEEN 7 AND 8 AND Ряд NOT IN (7, 8) THEN 2 
                    ELSE 1 
                END
            FROM Планировки_Мест WHERE Планировка_ИД = 'KINO-IMYA-ZAL3';
        END

        -- Переходим к следующему дню проката
        SET @DayOffset = @DayOffset + 1;
    END
END
GO

--- =======================================================================
--- УДОБНЫЙ КОНТРОЛЬНЫЙ ВЫВОД ДЛЯ SSMS
--- =======================================================================
SELECT 
    S.Событие_ИД AS [ID Сеанса],
    S.Номер_Зала AS [Номер Зала],
    CONVERT(NVARCHAR, S.Дата_Время, 104) AS [Дата Сеанса],
    CONVERT(NVARCHAR, S.Дата_Время, 108) AS [Время Начала],
    (SELECT COUNT(*) FROM Событие_Места_Категории WHERE Событие_ИД = S.Событие_ИД) AS [Всего мест в зале],
    (SELECT COUNT(*) FROM Событие_Места_Категории WHERE Событие_ИД = S.Событие_ИД AND Категория_ИД = 2) AS [Из них Диванов]
FROM События S 
WHERE S.Информация_ИД = 'INFO-BACKROOMS'
ORDER BY S.Дата_Время ASC, S.Номер_Зала ASC;
GO