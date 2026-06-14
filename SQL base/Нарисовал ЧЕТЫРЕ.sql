USE База_Мероприятий;
GO
SET NOCOUNT OFF;

-- ОЧИСТКА СТАРЫХ ДАННЫХ ПЛАНИРОВКИ --
-- ЗАКОММЕНТИРОВАНО ВО ИЗБЕЖАНИЕ СЛУЧАЙНЫХ УДАЛЕНИЙ
ALTER TABLE Событие_Места_Категории NOCHECK CONSTRAINT ALL;
ALTER TABLE Событие_Лимиты_Зон NOCHECK CONSTRAINT ALL;
ALTER TABLE Планировки_Мест NOCHECK CONSTRAINT ALL;
ALTER TABLE Планировки_Зон NOCHECK CONSTRAINT ALL;
ALTER TABLE События NOCHECK CONSTRAINT ALL;
ALTER TABLE Событие_Категории NOCHECK CONSTRAINT ALL;
ALTER TABLE Ценовая_Политика NOCHECK CONSTRAINT ALL;
ALTER TABLE Билеты NOCHECK CONSTRAINT ALL;
DELETE FROM Событие_Места_Категории WHERE Планировка_ИД = 'CROP-ARENA-01';
DELETE FROM Событие_Лимиты_Зон WHERE Планировка_ИД = 'CROP-ARENA-01';
DELETE FROM Планировки_Мест WHERE Планировка_ИД = 'CROP-ARENA-01';
DELETE FROM Планировки_Зон WHERE Планировка_ИД = 'CROP-ARENA-01';

BEGIN TRANSACTION;
BEGIN TRY;

    DECLARE @PlanID NVARCHAR(50) = 'CROP-ARENA-01';
        -- ГЕОМЕТРИЯ ЗОН И ТЕКСТА --
    INSERT INTO Планировки_Зон (Планировка_ИД, Зона_ИД, Название_Зоны, Тип_Зоны, Коорд_X, Коорд_Y, Координаты_Полигона, Ориентация_Текста, Размер_Текста) VALUES
    (@PlanID, 'TXT-f0cc70', 'Балкон', 'Текст', 663, 100, NULL, 'L-T', 23),
    (@PlanID, 'Z-BALCONY', 'Балкон', 'Зона', 957, 522, 'M 100 100 L 1696 100 L 1696 1342 L 1504 1342 L 1504 337 L 100 337 L 100 100 Z', 'C-T', 22),
    (@PlanID, 'Z-DANCE', 'Танцпол', 'Танцпол', 744, 683, 'M 474 443 L 1150 443 L 1150 1044 L 474 1044 L 474 443 Z', 'C-C', 22),
    (@PlanID, 'Z-FAN', 'Фан-зона', 'Танцпол', 744, 1163, 'M 474 1044 L 1150 1044 L 1150 1342 L 474 1342 L 474 1044 Z', 'C-C', 22),
    (@PlanID, 'TXT-a2547c', 'Фан-зона', 'Текст', 604, 1074, NULL, 'L-T', 35),
    (@PlanID, 'TXT-b19b3e', 'Танцпол', 'Текст', 619, 619, NULL, 'L-T', 35),
    (@PlanID, 'Z-LEFT-VIP', 'Столы', 'Зона', 290, 905, 'M 418 615 L 418 1342 L 100 1342 L 100 615 L 418 615 Z', 'L-B', 34),
    (@PlanID, 'TXT-500583', '1', 'Текст', 154, 1179, NULL, 'L-T', 18),
    (@PlanID, 'TXT-0653ae', '2', 'Текст', 157, 932, NULL, 'L-T', 18),
    (@PlanID, 'TXT-d7bb42', '3', 'Текст', 156, 685, NULL, 'L-T', 18),
    (@PlanID, 'Z-RIGHT-VIP', 'Столы', 'Зона', 1384, 1045, 'M 1504 847 L 1504 1342 L 1206 1342 L 1206 847 L 1504 847 Z', 'R-B', 20),
    (@PlanID, 'TXT-5ab6d4', '5', 'Текст', 1264, 930, NULL, 'L-T', 18),
    (@PlanID, 'TXT-301079', '6', 'Текст', 1265, 1176, NULL, 'L-T', 18),
    (@PlanID, 'TXT-36bce9', '1', 'Текст', 1456, 1034, NULL, 'L-T', 14),
    (@PlanID, 'TXT-b551b6', '2', 'Текст', 1510, 1173, NULL, 'L-T', 14),
    (@PlanID, 'TXT-bc726b', '2', 'Текст', 102, 187, NULL, 'L-T', 14),
    (@PlanID, 'TXT-73468f', '1', 'Текст', 100, 238, NULL, 'L-T', 14);
    -- ГЕОМЕТРИЯ МЕСТ --
    INSERT INTO Планировки_Мест (Планировка_ИД, Зона_ИД, Место_ИД, Ряд, Номер, Коорд_X, Коорд_Y) VALUES
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S1', '1', '1', 221, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S2', '1', '2', 267, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S3', '1', '3', 313, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S4', '1', '4', 362, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S5', '1', '5', 407, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S6', '1', '6', 456, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S7', '1', '7', 501, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S8', '1', '8', 550, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S9', '1', '9', 600, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S10', '1', '10', 644, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S11', '1', '11', 689, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S12', '1', '12', 739, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S13', '1', '13', 788, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S14', '1', '14', 833, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S15', '1', '15', 882, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S16', '1', '16', 927, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S17', '1', '17', 976, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S18', '1', '18', 1026, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S19', '1', '19', 1075, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S20', '1', '20', 1120, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S21', '1', '21', 1164, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S22', '1', '22', 1214, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S23', '1', '23', 1263, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S24', '1', '24', 1308, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S25', '1', '25', 1352, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S26', '1', '26', 1402, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S27', '1', '27', 1451, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S28', '1', '28', 1496, 275),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S29', '1', '29', 1540, 325),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S30', '1', '30', 1539, 373),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S31', '1', '31', 1539, 418),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S32', '1', '32', 1539, 464),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S33', '1', '33', 1539, 509),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S34', '1', '34', 1539, 560),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S35', '1', '35', 1539, 610),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S36', '1', '36', 1539, 656),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S37', '1', '37', 1539, 701),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S38', '1', '38', 1539, 746),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S39', '1', '39', 1539, 792),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S40', '1', '40', 1539, 842),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S41', '1', '41', 1539, 888),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S42', '1', '42', 1539, 938),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S43', '1', '43', 1539, 984),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R1-S44', '1', '44', 1539, 1034),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S1', '2', '1', 221, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S2', '2', '2', 267, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S3', '2', '3', 312, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S4', '2', '4', 363, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S5', '2', '5', 408, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S6', '2', '6', 459, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S7', '2', '7', 504, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S8', '2', '8', 549, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S9', '2', '9', 600, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S10', '2', '10', 645, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S11', '2', '11', 691, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S12', '2', '12', 736, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S13', '2', '13', 787, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S14', '2', '14', 835, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S15', '2', '15', 883, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S16', '2', '16', 928, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S17', '2', '17', 984, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S18', '2', '18', 1029, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S19', '2', '19', 1075, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S20', '2', '20', 1120, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S21', '2', '21', 1166, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S22', '2', '22', 1216, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S23', '2', '23', 1262, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S24', '2', '24', 1307, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S25', '2', '25', 1357, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S26', '2', '26', 1403, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S27', '2', '27', 1453, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S28', '2', '28', 1504, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S29', '2', '29', 1549, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S30', '2', '30', 1595, 226),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S31', '2', '31', 1595, 277),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S32', '2', '32', 1595, 327),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S33', '2', '33', 1595, 373),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S34', '2', '34', 1595, 418),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S35', '2', '35', 1595, 464),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S36', '2', '36', 1595, 514),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S37', '2', '37', 1595, 560),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S38', '2', '38', 1595, 610),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S39', '2', '39', 1595, 656),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S40', '2', '40', 1595, 701),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S41', '2', '41', 1595, 751),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S42', '2', '42', 1595, 797),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S43', '2', '43', 1595, 842),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S44', '2', '44', 1595, 888),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S45', '2', '45', 1595, 943),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S46', '2', '46', 1595, 989),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S47', '2', '47', 1595, 1029),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S48', '2', '48', 1595, 1075),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S49', '2', '49', 1595, 1125),
    (@PlanID, 'Z-BALCONY', 'Z-BALCONY-R2-S50', '2', '50', 1595, 1171),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R1-S1', '1', '1', 312, 1241),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R1-S2', '1', '2', 166, 1241),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R1-S3', '1', '3', 166, 1196),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R1-S4', '1', '4', 166, 1150),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R1-S5', '1', '5', 216, 1150),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R1-S6', '1', '6', 262, 1150),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R2-S1', '2', '1', 312, 994),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R2-S2', '2', '2', 166, 994),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R2-S3', '2', '3', 166, 948),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R2-S4', '2', '4', 166, 903),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R2-S5', '2', '5', 216, 903),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R2-S6', '2', '6', 257, 903),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R3-S1', '3', '1', 307, 751),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R3-S2', '3', '2', 166, 751),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R3-S3', '3', '3', 166, 701),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R3-S4', '3', '4', 166, 656),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R3-S5', '3', '5', 221, 656),
    (@PlanID, 'Z-LEFT-VIP', 'Z-LEFT-VIP-R3-S6', '3', '6', 262, 656),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R5-S1', '5', '1', 1277, 994),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R5-S2', '5', '2', 1413, 994),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R5-S3', '5', '3', 1413, 948),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R5-S4', '5', '4', 1413, 903),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R5-S5', '5', '5', 1368, 903),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R5-S6', '5', '6', 1327, 903),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R6-S1', '6', '1', 1277, 1241),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R6-S2', '6', '2', 1418, 1241),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R6-S3', '6', '3', 1418, 1196),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R6-S4', '6', '4', 1418, 1145),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R6-S5', '6', '5', 1373, 1145),
    (@PlanID, 'Z-RIGHT-VIP', 'Z-RIGHT-VIP-R6-S6', '6', '6', 1322, 1145);
    -- Переменные для перебора дней (0 — сегодня, от 1 до 7 следующие дни)
    DECLARE @DayOffset INT = 0; 
    DECLARE @ПлощадкаИД INT = (SELECT TOP 1 Площадка_ИД FROM Площадки WHERE Планировка_ИД = 'CROP-ARENA-01');
    
    WHILE @DayOffset <= 7
    BEGIN
        -- Вычисляем дату для текущего шага цикла (без времени)
        DECLARE @TargetDate DATE = CAST(DATEADD(day, @DayOffset, GETDATE()) AS DATE);
        DECLARE @DateStr NVARCHAR(8) = CONVERT(NVARCHAR(8), @TargetDate, 112); -- Формат YYYYMMDD

        -- Внутренний цикл: создаем ровно 3 события на этот день
        DECLARE @EventIndex INT = 1;
        WHILE @EventIndex <= 3
        BEGIN
            -- Определение параметров конкретного сеанса
            DECLARE @CurrentEvID NVARCHAR(50) = 'EV-CROP-' + @DateStr + '-' + CAST(@EventIndex AS NVARCHAR);
            DECLARE @EventTime DATETIME;
            DECLARE @CurrentInfoID NVARCHAR(50);
            DECLARE @BasePrice MONEY;

            -- Распределяем события по времени суток и типам релизов
            IF @EventIndex = 1
            BEGIN
                SET @EventTime = CAST(@TargetDate AS DATETIME) + CAST('13:00:00' AS DATETIME); -- Дневной сеанс
                SET @CurrentInfoID = 'INFO-ROCK-02';
                SET @BasePrice = 2500.00;
            END
            ELSE IF @EventIndex = 2
            BEGIN
                SET @EventTime = CAST(@TargetDate AS DATETIME) + CAST('17:30:00' AS DATETIME); -- Вечерний сеанс
                SET @CurrentInfoID = 'INFO-PUNK-01';
                SET @BasePrice = 3500.00;
            END
            ELSE
            BEGIN
                SET @EventTime = CAST(@TargetDate AS DATETIME) + CAST('21:00:00' AS DATETIME); -- Ночной сеанс
                SET @CurrentInfoID = 'INFO-RAP-03';
                SET @BasePrice = 4000.00;
            END


            -- Дополнительные удаления
                DELETE FROM События WHERE Событие_ИД = @CurrentEvID;
                DELETE FROM Событие_Категории WHERE Событие_ИД = @CurrentEvID;
                DELETE FROM Ценовая_Политика WHERE Событие_ИД = @CurrentEvID;
                DELETE FROM Событие_Лимиты_Зон WHERE Событие_ИД = @CurrentEvID;
                DELETE FROM Событие_Места_Категории WHERE Событие_ИД = @CurrentEvID;

            -- 1. Добавляем само событие, если его еще нет

            IF NOT EXISTS (SELECT 1 FROM События WHERE Событие_ИД = @CurrentEvID)
            BEGIN
                INSERT INTO События (Событие_ИД, Информация_ИД, Дата_Время, Площадка_ИД, Номер_Зала, Планировка_ИД)
                VALUES (@CurrentEvID, @CurrentInfoID, @EventTime, @ПлощадкаИД, 0, @PlanID);
            END

    -- КОНЕЦ ЦЕНОВОЙ ПОЛИТИКИ --



    -- КАТЕГОРИИ И ЦЕНЫ --
    INSERT INTO Событие_Категории (Событие_ИД, Категория_ИД, Название, Цвет, Описание) VALUES
    (@CurrentEvID, 1, N'VIP', '#FF8040', N'Места на трибунах'),
    (@CurrentEvID, 2, N'Балкон', '#80FFFF', N'Верхний ярус'),
    (@CurrentEvID, 3, N'Балкон', '#FFFF80', N'Верхний ярус'),
    (@CurrentEvID, 4, N'Танцпол', '#0080FF', N'Общий танцпол'),
    (@CurrentEvID, 5, N'Фан-зона', '#80FF80', N'Ближе всего к сцене');
    INSERT INTO Ценовая_Политика (Событие_ИД, Категория_ИД, Цена, Актуальность) VALUES
    (@CurrentEvID, 1, 5000, @EventTime),
    (@CurrentEvID, 2, 4049, @EventTime),
    (@CurrentEvID, 3, 4549, @EventTime),
    (@CurrentEvID, 4, 1549, @EventTime),
    (@CurrentEvID, 5, 2649, @EventTime);

    -- ПРИВЯЗКА ЛИМИТОВ ЗОН --
    INSERT INTO Событие_Лимиты_Зон (Событие_ИД, Планировка_ИД, Зона_ИД, Макс_Мест) VALUES
    (@CurrentEvID, @PlanID, 'Z-DANCE', 300),
    (@CurrentEvID, @PlanID, 'Z-FAN', 200);

    -- ПРИВЯЗКА МЕСТ К КАТЕГОРИЯМ --
    INSERT INTO Событие_Места_Категории (Событие_ИД, Планировка_ИД, Место_ИД, Категория_ИД) VALUES
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S1', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S2', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S3', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S4', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S5', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S6', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S7', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S8', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S9', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S10', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S11', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S12', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S13', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S14', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S15', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S16', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S17', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S18', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S19', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S20', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S21', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S22', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S23', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S24', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S25', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S26', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S27', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S28', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S29', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S30', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S31', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S32', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S33', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S34', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S35', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S36', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S37', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S38', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S39', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S40', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S41', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S42', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S43', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R1-S44', 3),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S1', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S2', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S3', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S4', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S5', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S6', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S7', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S8', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S9', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S10', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S11', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S12', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S13', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S14', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S15', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S16', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S17', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S18', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S19', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S20', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S21', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S22', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S23', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S24', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S25', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S26', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S27', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S28', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S29', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S30', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S31', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S32', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S33', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S34', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S35', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S36', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S37', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S38', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S39', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S40', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S41', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S42', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S43', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S44', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S45', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S46', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S47', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S48', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S49', 2),
    (@CurrentEvID, @PlanID, 'Z-BALCONY-R2-S50', 2),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R1-S1', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R1-S2', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R1-S3', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R1-S4', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R1-S5', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R1-S6', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R2-S1', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R2-S2', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R2-S3', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R2-S4', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R2-S5', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R2-S6', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R3-S1', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R3-S2', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R3-S3', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R3-S4', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R3-S5', 1),
    (@CurrentEvID, @PlanID, 'Z-LEFT-VIP-R3-S6', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R5-S1', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R5-S2', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R5-S3', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R5-S4', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R5-S5', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R5-S6', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R6-S1', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R6-S2', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R6-S3', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R6-S4', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R6-S5', 1),
    (@CurrentEvID, @PlanID, 'Z-RIGHT-VIP-R6-S6', 1);
    
SET @EventIndex = @EventIndex + 1;

        end
        
SET @DayOffset = @DayOffset + 1;
     end

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Ошибка: ' + ERROR_MESSAGE();
END CATCH


ALTER TABLE Событие_Места_Категории CHECK CONSTRAINT ALL;
ALTER TABLE Событие_Лимиты_Зон CHECK CONSTRAINT ALL;
ALTER TABLE Планировки_Мест CHECK CONSTRAINT ALL;
ALTER TABLE Планировки_Зон CHECK CONSTRAINT ALL;
ALTER TABLE События CHECK CONSTRAINT ALL;
ALTER TABLE Событие_Категории CHECK CONSTRAINT ALL;
ALTER TABLE Ценовая_Политика CHECK CONSTRAINT ALL;
ALTER TABLE Билеты CHECK CONSTRAINT ALL;
