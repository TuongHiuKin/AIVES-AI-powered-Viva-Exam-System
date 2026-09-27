/*
    FU News Management System - initial SQL Server schema (D00 / ARCH-02).

    Run the complete file in SSMS against your development SQL Server instance.
    GO is a batch separator understood by SSMS/sqlcmd, not an EF command.

    Database name matches StudentNameMVC/appsettings.json: AIVES.
    Creates the database if missing. Refuses to initialize a database that
    already contains user tables. No existing table/data is dropped or altered.
    Run-once initializer: later schema changes need separately reviewed scripts.
    Database creation is outside the schema transaction. If initialization fails,
    the newly created database may remain empty; the table DDL is rolled back.

    Physical-design choices for owner review are documented in:
      docs/business-rules.md (section 6)
    No sample accounts/passwords are inserted. The default Admin remains in
    application configuration. AccountPasswordHash stores an application-produced
    password hash, never the plaintext password entered on the login form.

    Services must still enforce authorization, Staff creator/editor identity,
    immutable creator/creation time, audit updates, email syntax/normalization,
    and full server-side validation. Foreign keys do not replace those checks.
*/

USE [master];
GO

IF DB_ID(N'AIVES') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [AIVES];');
END;
GO

USE [AIVES];
GO

-- If USE failed, do not accidentally initialize tables in master/another DB.
IF DB_NAME() <> N'AIVES'
BEGIN
    ;THROW 51000, 'Select the AIVES database before initializing its schema.', 1;
END;

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    BEGIN
        ;THROW 51001, 'Database already contains user tables. No changes made; use a reviewed upgrade script instead.', 1;
    END;

    CREATE TABLE dbo.SystemAccount
    (
        AccountId INT IDENTITY(1,1) NOT NULL,
        AccountName NVARCHAR(100) NOT NULL,
        AccountEmail NVARCHAR(254) COLLATE Latin1_General_100_CI_AS NOT NULL,
        AccountPasswordHash NVARCHAR(512) NOT NULL,
        AccountRole TINYINT NOT NULL,

        CONSTRAINT PK_SystemAccount PRIMARY KEY (AccountId),
        CONSTRAINT UQ_SystemAccount_AccountEmail UNIQUE (AccountEmail),
        CONSTRAINT CK_SystemAccount_AccountName_NotBlank
            CHECK (LEN(LTRIM(RTRIM(AccountName))) > 0),
        CONSTRAINT CK_SystemAccount_AccountEmail_NotBlank
            CHECK (LEN(LTRIM(RTRIM(AccountEmail))) > 0),
        CONSTRAINT CK_SystemAccount_PasswordHash_NotBlank
            CHECK (LEN(LTRIM(RTRIM(AccountPasswordHash))) > 0),
        CONSTRAINT CK_SystemAccount_AccountRole
            CHECK (AccountRole IN (1, 2))
    );

    -- Single-level categories: no parent/category hierarchy (SC-04).
    CREATE TABLE dbo.Category
    (
        CategoryId INT IDENTITY(1,1) NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        CategoryDescription NVARCHAR(500) NULL,

        CONSTRAINT PK_Category PRIMARY KEY (CategoryId),
        CONSTRAINT CK_Category_CategoryName_NotBlank
            CHECK (LEN(LTRIM(RTRIM(CategoryName))) > 0)
    );

    CREATE TABLE dbo.Tag
    (
        TagId INT IDENTITY(1,1) NOT NULL,
        TagName NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Tag PRIMARY KEY (TagId),
        CONSTRAINT CK_Tag_TagName_NotBlank
            CHECK (LEN(LTRIM(RTRIM(TagName))) > 0)
    );

    CREATE TABLE dbo.NewsArticle
    (
        NewsArticleId INT IDENTITY(1,1) NOT NULL,
        NewsTitle NVARCHAR(200) NOT NULL,
        NewsContent NVARCHAR(MAX) NOT NULL,
        CategoryId INT NOT NULL,
        -- Explicit input required: 0 = Inactive, 1 = Active; no implicit default.
        NewsStatus TINYINT NOT NULL,
        CreatedById INT NOT NULL,
        CreatedDate DATETIME2(3) NOT NULL
            CONSTRAINT DF_NewsArticle_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedById INT NULL,
        ModifiedDate DATETIME2(3) NULL,

        CONSTRAINT PK_NewsArticle PRIMARY KEY (NewsArticleId),
        CONSTRAINT CK_NewsArticle_NewsTitle_NotBlank
            CHECK (LEN(LTRIM(RTRIM(NewsTitle))) > 0),
        CONSTRAINT CK_NewsArticle_NewsContent_NotBlank
            CHECK (LEN(LTRIM(RTRIM(NewsContent))) > 0),
        CONSTRAINT CK_NewsArticle_NewsStatus
            CHECK (NewsStatus IN (0, 1)),
        -- Both are null before the first edit; thereafter both must be present.
        CONSTRAINT CK_NewsArticle_ModificationAudit
            CHECK (
                (UpdatedById IS NULL AND ModifiedDate IS NULL)
                OR
                (UpdatedById IS NOT NULL AND ModifiedDate IS NOT NULL
                 AND ModifiedDate >= CreatedDate)
            ),
        -- SC-01 and BR-11: a used Category cannot be deleted.
        CONSTRAINT FK_NewsArticle_Category
            FOREIGN KEY (CategoryId) REFERENCES dbo.Category (CategoryId)
            ON DELETE NO ACTION ON UPDATE NO ACTION,
        -- SC-05: keep the creator while their News exists.
        CONSTRAINT FK_NewsArticle_CreatedBy
            FOREIGN KEY (CreatedById) REFERENCES dbo.SystemAccount (AccountId)
            ON DELETE NO ACTION ON UPDATE NO ACTION,
        -- Conservative D00 choice: also preserve an editor referenced by News.
        CONSTRAINT FK_NewsArticle_UpdatedBy
            FOREIGN KEY (UpdatedById) REFERENCES dbo.SystemAccount (AccountId)
            ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    -- SC-02/03: 0..many tags per News; duplicate pairs are rejected by the PK.
    CREATE TABLE dbo.NewsTag
    (
        NewsArticleId INT NOT NULL,
        TagId INT NOT NULL,

        CONSTRAINT PK_NewsTag PRIMARY KEY (NewsArticleId, TagId),
        -- SC-07: deleting News removes only its join rows, not shared Tag rows.
        CONSTRAINT FK_NewsTag_NewsArticle
            FOREIGN KEY (NewsArticleId) REFERENCES dbo.NewsArticle (NewsArticleId)
            ON DELETE CASCADE ON UPDATE NO ACTION,
        -- SC-06: deleting a Tag still in use is blocked.
        CONSTRAINT FK_NewsTag_Tag
            FOREIGN KEY (TagId) REFERENCES dbo.Tag (TagId)
            ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    -- Support category lookups/deletion checks, own history, public list/report.
    CREATE INDEX IX_NewsArticle_CategoryId
        ON dbo.NewsArticle (CategoryId);
    CREATE INDEX IX_NewsArticle_CreatedById_CreatedDate
        ON dbo.NewsArticle (CreatedById, CreatedDate DESC);
    CREATE INDEX IX_NewsArticle_UpdatedById
        ON dbo.NewsArticle (UpdatedById);
    CREATE INDEX IX_NewsArticle_CreatedDate
        ON dbo.NewsArticle (CreatedDate DESC);
    CREATE INDEX IX_NewsArticle_NewsStatus_CreatedDate
        ON dbo.NewsArticle (NewsStatus, CreatedDate DESC);
    CREATE INDEX IX_NewsTag_TagId
        ON dbo.NewsTag (TagId);

    COMMIT TRANSACTION;
    PRINT N'AIVES schema created successfully. No accounts or sample data were inserted.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
