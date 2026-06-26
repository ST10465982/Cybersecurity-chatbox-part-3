/*
 Cybersecurity Awareness Bot - Part 3 Database Script
 Creates all tables needed for task assistant CRUD, chatbot memory, chat history,
 quiz answer tracking, quiz results and the activity log.
*/

IF DB_ID('CyberSecurityBotDB') IS NULL
BEGIN
    CREATE DATABASE CyberSecurityBotDB;
END;
GO

USE CyberSecurityBotDB;
GO

IF OBJECT_ID('dbo.UserMemory','U') IS NULL
BEGIN
 CREATE TABLE dbo.UserMemory(
  MemoryID INT IDENTITY(1,1) PRIMARY KEY,
  MemoryKey VARCHAR(100) NOT NULL UNIQUE,
  MemoryValue VARCHAR(1000) NULL,
  UpdatedDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
GO

IF OBJECT_ID('dbo.ChatMessages','U') IS NULL
BEGIN
 CREATE TABLE dbo.ChatMessages(
  MessageID INT IDENTITY(1,1) PRIMARY KEY,
  Sender VARCHAR(30) NOT NULL,
  MessageText VARCHAR(2000) NOT NULL,
  Intent VARCHAR(100) NULL,
  MessageDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
GO

IF OBJECT_ID('dbo.ReminderTasks','U') IS NULL
BEGIN
 CREATE TABLE dbo.ReminderTasks(
  TaskID INT IDENTITY(1,1) PRIMARY KEY,
  TaskDescription VARCHAR(500) NOT NULL,
  ReminderDate DATE NOT NULL,
  TaskStatus VARCHAR(30) NOT NULL DEFAULT 'Pending',
  CreatedDateTime DATETIME NOT NULL DEFAULT GETDATE(),
  UpdatedDateTime DATETIME NULL,
  CompletedDateTime DATETIME NULL
 );
END;
GO

IF OBJECT_ID('dbo.QuizResults','U') IS NULL
BEGIN
 CREATE TABLE dbo.QuizResults(
  ResultID INT IDENTITY(1,1) PRIMARY KEY,
  Score INT NOT NULL,
  TotalQuestions INT NOT NULL,
  PercentageScore DECIMAL(5,2) NOT NULL,
  CompletedDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
GO

IF OBJECT_ID('dbo.QuizAnswers','U') IS NULL
BEGIN
 CREATE TABLE dbo.QuizAnswers(
  AnswerID INT IDENTITY(1,1) PRIMARY KEY,
  Topic VARCHAR(100) NOT NULL,
  QuestionText VARCHAR(1000) NOT NULL,
  SelectedAnswer VARCHAR(500) NOT NULL,
  CorrectAnswer VARCHAR(500) NOT NULL,
  WasCorrect BIT NOT NULL,
  Feedback VARCHAR(1000) NOT NULL,
  AnsweredDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
GO

IF OBJECT_ID('dbo.ActivityLog','U') IS NULL
BEGIN
 CREATE TABLE dbo.ActivityLog(
  LogID INT IDENTITY(1,1) PRIMARY KEY,
  ActivityDescription VARCHAR(1000) NOT NULL,
  ActivityDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
GO

-- Useful checks for the lecturer/demonstration
SELECT * FROM dbo.ReminderTasks;
SELECT * FROM dbo.UserMemory;
SELECT * FROM dbo.QuizResults;
SELECT * FROM dbo.ActivityLog;
