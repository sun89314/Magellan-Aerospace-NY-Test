CREATE DATABASE Part;

DROP DATABASE IF EXISTS item;
CREATE TABLE item (
    id SERIAL PRIMARY KEY,
    item_name VARCHAR(50) NOT NULL,
    parent_item INTEGER REFERENCES item(id),
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL
);


INSERT INTO item (id, item_name, parent_item, cost, req_date) VALUES
(1, 'Item1', NULL, 500, '2024-02-20'),
(2, 'Sub1', 1, 200, '2024-02-10'),
(3, 'Sub2', 1, 300, '2024-01-05'),
(4, 'Sub3', 2, 300, '2024-01-02'),
(5, 'Sub4', 2, 400, '2024-01-02'),
(6, 'Item2', NULL, 600, '2024-03-15'),
(7, 'Sub1', 6, 200, '2024-02-25');

CREATE OR REPLACE FUNCTION Get_Total_Cost(itemName VARCHAR(50))
RETURNS INTEGER AS $$
DECLARE
    totalCost INTEGER;
    itemCount INTEGER;
BEGIN
    SELECT COUNT(*) INTO itemCount
    FROM item
    WHERE item_name = itemName;
    IF itemCount > 1 THEN
        RETURN NULL;
    end if;

    WITH RECURSIVE CostCTE AS (
    
        SELECT id, item_name, parent_item, cost
        FROM item
        WHERE item_name = itemName

        UNION ALL

        SELECT i.id, i.item_name, i.parent_item, i.cost
        FROM item i
        INNER JOIN CostCTE cte ON i.parent_item = cte.id
    )

    SELECT SUM(cost) INTO totalCost
    FROM CostCTE;
    
    RETURN totalCost;
END;
$$ LANGUAGE plpgsql;
-- the code I've used for database administration
CREATE ROLE leting WITH LOGIN PASSWORD '123456';
GRANT CONNECT ON DATABASE Part TO leting;
GRANT USAGE ON SCHEMA public TO leting;
GRANT SELECT ON item TO leting;
GRANT INSERT ON item TO leting;
GRANT UPDATE ON item TO leting;
GRANT DELETE ON item TO leting;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE item_id_seq TO leting;
SELECT setval('item_id_seq', (SELECT MAX(id) FROM item));
INSERT INTO item (item_name, parent_item, cost, req_date) VALUES
('Item5', NULL, 500, '2024-02-20');
