-- ЗАПРОСЫ --
-- 1) Найти фамилии управляющих, которые заведуют плантациями,
-- когда-либо поставлявшими кофе в заданный порт ("Рио-де-Жанейро")
SELECT split_part(manager, ' ', 2)
FROM (plantation_to_port_delivery
    inner join plantation p on p.plantation_id = plantation_to_port_delivery.plantation_id)
WHERE p.port_name = 'Santos';

-- 2) Найти все корабли грузоподъемностью более 1000 тонн,
-- которые смогут перевезти кофе из Рио-де-Жанейро в Лиссабон по цене меньше чем 10000 долларов

SELECT ship_name
FROM ship_america_to_europe_costs
WHERE american_port_name = 'Santos'
  AND european_port_name = 'Rotterdam'
  AND cost < 10000;

-- 3) Управляющий плантации "Мачетес" в день X потерял свой любимый кошелек.
-- Расследование длилось несколько дней и наконец в день Y выяснилось, что кошелек случайно попал в мешок с кофе и поехал в порт.
-- В порт сразу же позвонили, но за истекший срок из порта ушло несколько кораблей с кофе и скорее всего кошелек был в одном из них.
-- Выясните, с кем в Европе надо связаться, чтоб найти кошелек

CREATE OR REPLACE FUNCTION get_american_port_name_by_plantation_id(id INT, OUT name VARCHAR(100)) RETURNS VARCHAR(100)
AS
$$
BEGIN
SELECT port_name INTO name FROM plantation WHERE plantation_id = id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE VIEW plantation_to_port_deliveries_info AS
SELECT port_name, date AS plantation_date
FROM (plantation
    inner join plantation_to_port_delivery ptpd on ptpd.plantation_id = plantation.plantation_id);

SELECT *
FROM plantation_to_port_deliveries_info;

CREATE OR REPLACE VIEW port_to_port_deliveries_info AS
SELECT american_port_name, european_port_name, date AS port_date, delivery_cost_id, buyer_name
FROM (port_to_port_delivery
    inner join ship_america_to_europe_costs satec on satec.cost_id = port_to_port_delivery.delivery_cost_id);

SELECT *
FROM port_to_port_deliveries_info;

SELECT european_port_name
FROM (plantation_to_port_deliveries_info
    inner join port_to_port_deliveries_info ptpdi
    on ptpdi.american_port_name = plantation_to_port_deliveries_info.port_name)
WHERE plantation_date >= '2021-04-21'
  AND port_date <= '2021-04-26';

-- ПРЕДСТАВЛЕНИЯ --
-- 1) Аналитикам хочется знать "товарооборот" между каждой парой европейских и южноамериканских портов.
-- Товарооборот -- это суммарный вес перевезенного кофе между двумя портами. Напишите им простенькое представление, показывающее эти цифры

CREATE OR REPLACE VIEW costs_ship_info AS
SELECT cost_id, cost, american_port_name, european_port_name, carrying_capacity_in_ton, s.ship_name
FROM (ship_america_to_europe_costs
    inner join ship s on s.ship_name = ship_america_to_europe_costs.ship_name);

DROP VIEW costs_ship_info;
SELECT *
FROM costs_ship_info;

CREATE OR REPLACE VIEW port_to_port_delivery_costs_info AS
SELECT port_to_port_deliveries_info.delivery_cost_id,
       port_to_port_deliveries_info.american_port_name,
       port_to_port_deliveries_info.european_port_name,
       carrying_capacity_in_ton,
       buyer_name
FROM (port_to_port_deliveries_info
    inner join costs_ship_info csi on port_to_port_deliveries_info.delivery_cost_id = csi.cost_id);

SELECT *
FROM port_to_port_delivery_costs_info;

SELECT american_port_name, european_port_name, SUM(carrying_capacity_in_ton) AS turnover
FROM port_to_port_delivery_costs_info
GROUP BY delivery_cost_id, american_port_name, european_port_name;

-- 2) Тем же аналитикам для каждого европейского порта хочется знать распределение доставленного в него кофе по покупателям.
-- Что-то в духе "50% кофе, доставленного в Гамбург, покупает "Чибо", 25% "Нескафе" и 25% "Якобс".
-- В Марселе 70% покупает "Нескафе", а "Чибо" и "Якобс" по 15%.

WITH european_ports_turnover AS (
    SELECT european_port_name, SUM(carrying_capacity_in_ton) AS turnover
    FROM port_to_port_delivery_costs_info
    GROUP BY european_port_name
)
SELECT port_to_port_delivery_costs_info.european_port_name,
       SUM(carrying_capacity_in_ton)::float / turnover * 100 as percentageofbuy,
       buyer_name
FROM (port_to_port_delivery_costs_info
    INNER JOIN european_ports_turnover
    ON european_ports_turnover.european_port_name = port_to_port_delivery_costs_info.european_port_name)
GROUP BY port_to_port_delivery_costs_info.european_port_name, port_to_port_delivery_costs_info.buyer_name,
         european_ports_turnover.turnover;

-- ПРОЦЕДУРА --
-- У вас в неком южноамериканском порту K лежит некий груз кофе весом X тонн.
-- У вас есть два покупателя A и B. Покупатель A согласен приехать за кофе в европейский порт L, а покупатель B согласен приехать в порт M.
-- Посчитайте, кому из них выгоднее везти ваш кофе и на каком корабле.


CREATE OR REPLACE FUNCTION find_best_ship_between_ports_with_weight(
    american_port_name american_port.american_port_name%type,
    european_port_name european_port.european_port_name%type,
    cargo_weight INT
) RETURNS costs_ship_info AS
$$
DECLARE
row            costs_ship_info%rowtype;
    best_row       costs_ship_info%rowtype default NULL;
    best_ship_cost float8 default '+infinity';
BEGIN
FOR row IN
SELECT * FROM costs_ship_info
                  LOOP
    IF row.american_port_name = $1 AND row.european_port_name = $2 AND
               row.carrying_capacity_in_ton >= cargo_weight THEN
                IF row.cost < best_ship_cost THEN
                    best_ship_cost := row.cost;
best_row := row;
END IF;
END IF;
END LOOP;
    IF best_row IS NULL THEN
        RAISE EXCEPTION 'There is no compatible ship for your request';
END IF;
RETURN best_row;
END;
$$ LANGUAGE plpgsql;

SELECT *
FROM find_best_ship_between_ports_with_weight('Santos', 'Rotterdam', 10000);

CREATE OR REPLACE FUNCTION find_more_profitable_buyer(
    american_port_name american_port.american_port_name%type,
    cargo_weight INT,
    first_buyer_name buyer.buyer_name%type,
    first_buyer_port_european_port_name european_port.european_port_name%type,
    second_buyer_name buyer.buyer_name%type,
    second_buyer_port_european_port_name european_port.european_port_name%type,
    OUT profitable_buyer_name buyer.buyer_name%type,
    OUT profitable_buyer_ship_name costs_ship_info.ship_name%type
) AS
$$
DECLARE
first_european_port_best_cost  costs_ship_info%rowtype;
    second_european_port_best_cost costs_ship_info%rowtype;
BEGIN
    first_european_port_best_cost := find_best_ship_between_ports_with_weight($1, $4, $2);
    second_european_port_best_cost := find_best_ship_between_ports_with_weight($1, $6, $2);

    IF first_european_port_best_cost.cost <= second_european_port_best_cost.cost THEN
        profitable_buyer_name := $3;
        profitable_buyer_ship_name := first_european_port_best_cost.ship_name;
ELSE
        profitable_buyer_name := $5;
        profitable_buyer_ship_name := second_european_port_best_cost.ship_name;
END IF;
END;
$$ LANGUAGE plpgsql;

SELECT *
FROM find_more_profitable_buyer('Santos', 100, 'Emir', 'Valencia', 'Egor', 'Hamburg');
