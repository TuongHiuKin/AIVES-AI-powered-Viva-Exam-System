/*
    AIVES - SC-10 / BR-26: add the Account soft-delete flag.

    Run this complete file in SSMS on the development SQL Server after 001.
    Existing DB: run only this upgrade; do not rerun 001-create-schema.sql.
    Fresh DB: run 001-create-schema.sql, then this file.

    Adds dbo.SystemAccount.IsDeleted BIT NOT NULL DEFAULT (0).
    Existing accounts receive 0 (not deleted) when the column is first added.
    Reruns preserve all existing flag values, including 1 (already deleted).
    If an existing column has an incompatible definition, stop for review.
    Does not delete accounts/news, seed data, or change foreign keys.

    Application follow-up: map the bool property, reject deleted accounts in
    authentication/session validation, and filter account lists appropriately.
    Adding this column alone does not implement these application behaviors.
*/

USE [AIVES];
GO

-- Protect against USE failing and the session remaining in another database.
IF DB_NAME() <> N'AIVES'
BEGIN
    ;THROW 51020, 'Expected the existing AIVES database. No upgrade was performed.', 1;
END;

-- Do not commit or roll back a transaction belonging to the caller.
IF @@TRANCOUNT <> 0
BEGIN
    ;THROW 51021, 'Run this upgrade outside an existing transaction.', 1;
END;

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @accountTableId INT = OBJECT_ID(N'dbo.SystemAccount', N'U');
    IF @accountTableId IS NULL
    BEGIN
        ;THROW 51022, 'dbo.SystemAccount is missing. Create the baseline schema before running this upgrade.', 1;
    END;

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = @accountTableId AND name = N'IsDeleted'
    )
    BEGIN
        -- Compile DDL only when this branch runs (safe on subsequent executions).
        EXEC(N'ALTER TABLE dbo.SystemAccount
            ADD IsDeleted BIT NOT NULL
                CONSTRAINT DF_SystemAccount_IsDeleted DEFAULT (0) WITH VALUES;');
    END;

    -- Also validate an already-present column rather than assuming a rerun is safe.
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = @accountTableId
          AND name = N'IsDeleted'
          AND user_type_id = TYPE_ID(N'bit')
          AND is_nullable = 0
          AND is_computed = 0
    )
    BEGIN
        ;THROW 51023, 'Existing IsDeleted must be a non-computed BIT NOT NULL column. Review its definition manually.', 1;
    END;

    DECLARE @defaultObjectId INT;
    SELECT @defaultObjectId = default_object_id
    FROM sys.columns
    WHERE object_id = @accountTableId AND name = N'IsDeleted';

    IF @defaultObjectId = 0
    BEGIN
        -- Add only the missing default; do not reset any existing flag values.
        EXEC(N'ALTER TABLE dbo.SystemAccount
            ADD CONSTRAINT DF_SystemAccount_IsDeleted DEFAULT (0) FOR IsDeleted;');
    END;

    DECLARE @defaultDefinition NVARCHAR(MAX);
    SELECT @defaultDefinition = d.definition
    FROM sys.default_constraints AS d
    INNER JOIN sys.columns AS c ON c.default_object_id = d.object_id
    WHERE c.object_id = @accountTableId AND c.name = N'IsDeleted';

    -- Accept SQL Server's usual (0)/((0)) form and any constraint name.
    IF @defaultDefinition IS NULL
       OR REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
           @defaultDefinition, N'(', N''), N')', N''), N' ', N''),
           NCHAR(9), N''), NCHAR(10), N''), NCHAR(13), N'') <> N'0'
    BEGIN
        ;THROW 51024, 'IsDeleted has an unexpected default; expected constant (0). Review it manually.', 1;
    END;

    COMMIT TRANSACTION;
    PRINT N'AIVES: IsDeleted BIT NOT NULL DEFAULT (0) is ready. Existing data has been preserved.';

    SELECT c.name AS ColumnName, TYPE_NAME(c.user_type_id) AS SqlType,
           c.is_nullable AS IsNullable, d.name AS DefaultConstraint,
           d.definition AS DefaultDefinition
    FROM sys.columns AS c
    LEFT JOIN sys.default_constraints AS d ON d.object_id = c.default_object_id
    WHERE c.object_id = @accountTableId AND c.name = N'IsDeleted';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
