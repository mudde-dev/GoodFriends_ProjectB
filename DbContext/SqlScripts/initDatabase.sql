USE goodfriendsefc;
GO

--01-create-schema.sql
--create a schema for guest users, i.e. not logged in
CREATE SCHEMA gstusr;
GO

--create a schema for logged in user
CREATE SCHEMA usr;
GO

--02-create-gstusr-view.sql
--create a view that gives overview of the database content
CREATE OR ALTER VIEW gstusr.vwInfoDb AS
    SELECT (SELECT COUNT(*) FROM supusr.Friends WHERE Seeded = 1) as nrSeededFriends, 
        (SELECT COUNT(*) FROM supusr.Friends WHERE Seeded = 0) as nrUnseededFriends,
        (SELECT COUNT(*) FROM supusr.Friends WHERE AddressId IS NOT NULL) as nrFriendsWithAddress,
        (SELECT COUNT(*) FROM supusr.Addresses WHERE Seeded = 1) as nrSeededAddresses, 
        (SELECT COUNT(*) FROM supusr.Addresses WHERE Seeded = 0) as nrUnseededAddresses,
        (SELECT COUNT(*) FROM supusr.Pets WHERE Seeded = 1) as nrSeededPets, 
        (SELECT COUNT(*) FROM supusr.Pets WHERE Seeded = 0) as nrUnseededPets,
        (SELECT COUNT(*) FROM supusr.Quotes WHERE Seeded = 1) as nrSeededQuotes, 
        (SELECT COUNT(*) FROM supusr.Quotes WHERE Seeded = 0) as nrUnseededQuotes;

GO

CREATE OR ALTER VIEW gstusr.vwInfoFriends AS
    SELECT a.Country, a.City, COUNT(*) as NrFriends  FROM supusr.Friends f
    INNER JOIN supusr.Addresses a ON f.AddressId = a.AddressId
    GROUP BY a.Country, a.City WITH ROLLUP;
GO

CREATE OR ALTER VIEW gstusr.vwInfoPets AS
    SELECT a.Country, a.City, COUNT(p.PetId) as NrPets FROM supusr.Friends f
    INNER JOIN supusr.Addresses a ON f.AddressId = a.AddressId
    INNER JOIN supusr.Pets p ON p.FriendId = f.FriendId
    GROUP BY a.Country, a.City WITH ROLLUP;
GO

CREATE OR ALTER VIEW gstusr.vwInfoQuotes AS
    SELECT Author, COUNT(QuoteText) as NrQuotes FROM supusr.Quotes 
    GROUp BY Author;
GO


--03-create-supusr-sp.sql
CREATE OR ALTER PROC supusr.spDeleteAll
    @Seeded BIT = 1,

    @nrFriendsAffected INT OUTPUT,
    @nrAddressesAffected INT OUTPUT,
    @nrPetsAffected INT OUTPUT,
    @nrQuotesAffected INT OUTPUT
    
    AS

    SET NOCOUNT ON;

    SELECT  @nrFriendsAffected = COUNT(*) FROM supusr.Friends WHERE Seeded = @Seeded;
    SELECT  @nrAddressesAffected = COUNT(*) FROM supusr.Addresses WHERE Seeded = @Seeded;
    SELECT  @nrPetsAffected = COUNT(*) FROM supusr.Pets WHERE Seeded = @Seeded;
    SELECT  @nrQuotesAffected = COUNT(*) FROM supusr.Quotes WHERE Seeded = @Seeded;

    DELETE FROM supusr.Friends WHERE Seeded = @Seeded;
    DELETE FROM supusr.Addresses WHERE Seeded = @Seeded;
    DELETE FROM supusr.Pets WHERE Seeded = @Seeded;
    DELETE FROM supusr.Quotes WHERE Seeded = @Seeded;

    SELECT * FROM gstusr.vwInfoDb;

    --throw our own error
    --;THROW 999999, 'my own supusr.spDeleteAll Error directly from SQL Server', 1

    --show return code usage
    RETURN 0;  --indicating success
    --RETURN 1;  --indicating your own error code, in this case 1
GO


--04-create-users.sql
--Create 3 logins
IF SUSER_ID (N'gstusr') IS NOT NULL
DROP LOGIN gstusr;

IF SUSER_ID (N'usr') IS NOT NULL
DROP LOGIN usr;

IF SUSER_ID (N'supusr') IS NOT NULL
DROP LOGIN supusr;

CREATE LOGIN gstusr WITH PASSWORD=N'pa$$Word1', 
    DEFAULT_DATABASE=goodfriendsefc, DEFAULT_LANGUAGE=us_english, 
    CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

CREATE LOGIN usr WITH PASSWORD=N'pa$$Word1', 
DEFAULT_DATABASE=goodfriendsefc, DEFAULT_LANGUAGE=us_english, 
CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

CREATE LOGIN supusr WITH PASSWORD=N'pa$$Word1', 
DEFAULT_DATABASE=goodfriendsefc, DEFAULT_LANGUAGE=us_english, 
CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;


--create 3 users from the logins, we will late set credentials for these
DROP USER IF EXISTS  gstusrUser;
DROP USER IF EXISTS usrUser;
DROP USER IF EXISTS supusrUser;

CREATE USER gstusrUser FROM LOGIN gstusr;
CREATE USER usrUser FROM LOGIN usr;
CREATE USER supusrUser FROM LOGIN supusr;

--05-create-roles-credentials.sql
--create roles
CREATE ROLE goodfriendsefcGstUsr;
CREATE ROLE goodfriendsefcUsr;
CREATE ROLE goodfriendsefcSupUsr;

--assign securables creadentials to the roles
GRANT SELECT, EXECUTE ON SCHEMA::gstusr to goodfriendsefcGstUsr;
GRANT SELECT, UPDATE, INSERT ON SCHEMA::supusr to goodfriendsefcUsr;
GRANT SELECT, UPDATE, INSERT, DELETE, EXECUTE ON SCHEMA::supusr to goodfriendsefcSupUsr;

--finally, add the users to the roles
ALTER ROLE goodfriendsefcGstUsr ADD MEMBER gstusrUser;

ALTER ROLE goodfriendsefcGstUsr ADD MEMBER usrUser;
ALTER ROLE goodfriendsefcUsr ADD MEMBER usrUser;

ALTER ROLE goodfriendsefcGstUsr ADD MEMBER supusrUser;
ALTER ROLE goodfriendsefcUsr ADD MEMBER supusrUser;
ALTER ROLE goodfriendsefcSupUsr ADD MEMBER supusrUser;
GO

--07-create-gstusr-login.sql
CREATE OR ALTER PROC gstusr.spLogin
    @UserNameOrEmail NVARCHAR(100),
    @Password NVARCHAR(200),

    @UserId UNIQUEIDENTIFIER OUTPUT,
    @UserName NVARCHAR(100) OUTPUT,
    @Role NVARCHAR(100) OUTPUT
    
    AS

    SET NOCOUNT ON;
    
    SET @UserId = NULL;
    SET @UserName = NULL;
    SET @Role = NULL;
    
    SELECT Top 1 @UserId = UserId, @UserName = UserName, @Role = [Role] FROM dbo.Users 
    WHERE ((UserName = @UserNameOrEmail) OR
           (Email IS NOT NULL AND (Email = @UserNameOrEmail))) AND ([Password] = @Password);
    
    IF (@UserId IS NULL)
    BEGIN
        ;THROW 999999, 'Login error: wrong user or password', 1
    END

GO


