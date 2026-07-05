
# PART B — SQL Query (do this first, it's quick)

### Step B1: Create tables

```sql
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY IDENTITY(1,1),
    EmployeeName VARCHAR(100)
);

CREATE TABLE ItemMaster (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    ItemName VARCHAR(100)
);

CREATE TABLE OrderMaster (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId INT FOREIGN KEY REFERENCES Employees(EmployeeId),
    ItemId INT FOREIGN KEY REFERENCES ItemMaster(ItemId)
);
```

### Step B2: Insert sample data (to match the expected output)

```sql
INSERT INTO Employees (EmployeeName) VALUES ('A'), ('B'), ('C'), ('D');

INSERT INTO ItemMaster (ItemName) VALUES ('Monitor'), ('Mouse'), ('Key Board'), ('Processor');

-- EmployeeId: 1=A, 2=B, 3=C, 4=D
-- ItemId: 1=Monitor, 2=Mouse, 3=Key Board, 4=Processor

INSERT INTO OrderMaster (EmployeeId, ItemId) VALUES
(1, 1), (1, 2),   -- A: Monitor, Mouse
(2, 3), (2, 4),   -- B: Key Board, Processor
(3, 1), (3, 2),   -- C: Monitor, Mouse
(4, 4);           -- D: Processor
```

### Step B3: The report query

Simplest approach — `STRING_AGG` (SQL Server 2017+):

```sql
SELECT 
    E.EmployeeName AS [Employee Name],
    STRING_AGG(I.ItemName, ', ') AS Items
FROM Employees E
INNER JOIN OrderMaster O ON O.EmployeeId = E.EmployeeId
INNER JOIN ItemMaster I ON I.ItemId = O.ItemId
GROUP BY E.EmployeeName;
```

If your SQL Server version is older than 2017, use this instead (works everywhere):

```sql
SELECT 
    E.EmployeeName AS [Employee Name],
    STUFF((
        SELECT ', ' + I2.ItemName
        FROM OrderMaster O2
        INNER JOIN ItemMaster I2 ON I2.ItemId = O2.ItemId
        WHERE O2.EmployeeId = E.EmployeeId
        FOR XML PATH('')
    ), 1, 2, '') AS Items
FROM Employees E
GROUP BY E.EmployeeName, E.EmployeeId;
```

Both give you:

| Employee Name | Items |
|---|---|
| A | Monitor, Mouse |
| B | Key Board, Processor |
| C | Monitor, Mouse |
| D | Processor |

---

