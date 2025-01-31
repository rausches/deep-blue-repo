DROP SCHEMA IF EXISTS uxcheckmate;
CREATE SCHEMA uxcheckmate;
USE uxcheckmate;

CREATE TABLE UserAccounts (
    UserID      INT             NOT NULL AUTO_INCREMENT,
    Email       VARCHAR(255)    NOT NULL UNIQUE,
    Password    VARCHAR(255)    NOT NULL,
    RoleID      INT             NOT NULL,
    PRIMARY KEY (UserID),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE RESTRICT
);