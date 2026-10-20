-- 草稿：依 Models/Member.cs 與 Models/Product.cs 推導，欄位型別為推測。
-- 請與實際資料庫比對後再使用。執行順序：本檔 -> 1131740/SQLQuery2.sql -> 1131740/SQLQuery3.sql

CREATE TABLE TableMembers1131740 (
    Username      NVARCHAR(50)  NOT NULL PRIMARY KEY,
    Password      NVARCHAR(100) NOT NULL,
    Name          NVARCHAR(100) NOT NULL,
    Balance       INT NULL,
    TotalConsumed INT NULL
);
GO

CREATE TABLE TableProducts1131740 (
    ProductID   INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200) NULL,
    Price       INT NOT NULL,
    Stock       INT NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Category    NVARCHAR(50)  NULL,
    Tag         NVARCHAR(50)  NULL,
    ImageUrl    NVARCHAR(500) NULL
);
GO

-- 示範用管理員帳號（程式以 Username = 'admin' 判斷管理員）。請自行設定密碼。
-- INSERT INTO TableMembers1131740 (Username, Password, Name, Balance, TotalConsumed)
-- VALUES (N'admin', N'YOUR_PASSWORD', N'Administrator', 0, 0);
