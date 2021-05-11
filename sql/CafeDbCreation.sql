-- Кофейная компания выращивает, перевозит и продает перекупщикам кофе
-- Компания владеет несколькими плантациями в Южной Америке
-- Каждая плантация поставляет кофе лишь в один порт, но один порт может принимать поставки от нескольких плантаций
CREATE TABLE IF NOT EXISTS plantation
(
    plantation_id SERIAL PRIMARY KEY,
    country       VARCHAR(100),
    manager       VARCHAR(100),
    port_name     VARCHAR(100) REFERENCES american_port (american_port_name)
    );

-- Кофе упаковывается на плантации в мешки и отправляется партиями в один из южноамериканских портов
-- Датой перевозки считается дата отправления корабля из американского порта (2004-10-22)
-- По прибытии в порт назначения весь груз кофе тут же отгружается покупателю
CREATE TABLE IF NOT EXISTS plantation_to_port_delivery
(
    plantation_to_port_delivery_id SERIAL PRIMARY KEY,
    plantation_id                  INT REFERENCES plantation (plantation_id),
    coffee_amount_in_ton           INT CHECK ( coffee_amount_in_ton > 0 ) NOT NULL,
    date                           DATE                                   NOT NULL
    );

CREATE TABLE IF NOT EXISTS port_to_port_delivery
(
    port_to_port_delivery_id SERIAL PRIMARY KEY,
    delivery_cost_id         INT REFERENCES ship_america_to_europe_costs (cost_id),
    buyer_name               VARCHAR(100) REFERENCES buyer (buyer_name),
    date                     DATE NOT NULL
    );

CREATE TABLE IF NOT EXISTS american_port
(
    american_port_name VARCHAR(100) UNIQUE
    );

CREATE TABLE IF NOT EXISTS ship
(
    ship_name                VARCHAR(100) PRIMARY KEY,
    carrying_capacity_in_ton INT CHECK ( carrying_capacity_in_ton > 0 )
    );

CREATE TABLE IF NOT EXISTS european_port
(
    european_port_name VARCHAR(100) UNIQUE
    );

-- Стоимость перевозки между двумя данными портами на данном корабле фиксирована (в тысячах долларов)
CREATE TABLE IF NOT EXISTS ship_america_to_europe_costs
(
    cost_id            SERIAL PRIMARY KEY,
    ship_name          VARCHAR(100) REFERENCES ship (ship_name),
    american_port_name VARCHAR(100) REFERENCES american_port (american_port_name),
    european_port_name VARCHAR(100) REFERENCES european_port (european_port_name),
    cost               INT
    );

-- Покупатель характеризуется именем и он всегда приобретает кофе по фиксированной цене (в тысячах долларов за тонну)
CREATE TABLE IF NOT EXISTS buyer
(
    buyer_name    VARCHAR(100) UNIQUE,
    purchase_cost INT CHECK ( purchase_cost > 0 )
    );
