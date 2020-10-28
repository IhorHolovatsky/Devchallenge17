USE [master]
GO

IF DB_ID('ArticlesCheck') IS NOT NULL
  set noexec on               -- prevent creation when already exists

CREATE DATABASE [ArticlesCheck];
GO

USE [ArticlesCheck]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Articles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Content] [varchar](max) NOT NULL,
	[Tokens] [varchar](max) NOT NULL,
	[CreatedTs] [datetime] NULL,
	[UpdatedTs] [datetime] NULL,
 CONSTRAINT [PK_Articles] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)) ON [PRIMARY]
GO
