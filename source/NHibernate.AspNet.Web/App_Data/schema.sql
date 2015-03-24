
    PRAGMA foreign_keys = OFF

    drop table if exists AspNetUsers

    drop table if exists AspNetUserRoles

    drop table if exists AspNetUserLogins

    drop table if exists IdentityRole`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]

    drop table if exists AspNetUserClaims

    drop table if exists IdentityRole

    drop table if exists ApplicationUser

    PRAGMA foreign_keys = ON

    create table AspNetUsers (
        Id TEXT not null,
       AccessFailedCount INT,
       Email TEXT,
       EmailConfirmed BOOL,
       LockoutEnabled BOOL,
       LockoutEndDateUtc DATETIME,
       PasswordHash TEXT,
       PhoneNumber TEXT,
       PhoneNumberConfirmed BOOL,
       TwoFactorEnabled BOOL,
       UserName TEXT not null unique,
       SecurityStamp TEXT,
       identityrole`1_key TEXT,
       primary key (Id),
       constraint FK3485576BC9070869 foreign key (identityrole`1_key) references IdentityRole`1[[System.String,
       mscorlib,
       Version=4.0.0.0,
       Culture=neutral,
       PublicKeyToken=b77a5c561934e089]]
    )
