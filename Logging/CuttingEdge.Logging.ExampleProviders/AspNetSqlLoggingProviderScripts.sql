SET XACT_ABORT ON

BEGIN TRAN


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[logging_EventTypes](
	[EventTypeId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_logging_EventTypes] PRIMARY KEY CLUSTERED 
(
	[EventTypeId] ASC
) ON [PRIMARY]
) ON [PRIMARY]



CREATE TABLE [dbo].[logging_Events](
	[EventId] [int] IDENTITY(1,1) NOT NULL,
	[EventTypeId] [int] NOT NULL,
	[EventTime] [datetime] NOT NULL CONSTRAINT [DF_logging_Events_eventTime]  DEFAULT (getdate()),
	[Message] [ntext] NULL,
	[Source] [ntext] NULL,
	/* Following columns are added for the AspNetLoggingProviderScripts */
	[UserName] [nvarchar](255) NULL,
	[QueryString] [ntext] NULL,
	[FormData] [ntext] NULL,
	/* End of extra columns */
 CONSTRAINT [PK_logging_Events] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



CREATE TABLE [dbo].[logging_Exceptions](
	[ExceptionId] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[ParentExceptionId] [int] NULL,
	[ExceptionType] [nvarchar](255) NOT NULL,
	[Message] [ntext] NULL,
	[StackTrace] [ntext] NULL,
 CONSTRAINT [PK_logging_Exceptions] PRIMARY KEY CLUSTERED 
(
	[ExceptionId] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


GO



CREATE PROCEDURE [dbo].[logging_AddEvent]
	@EventTypeId int,
	@Message ntext,
	@Source ntext,
	/* Following parameters are added for the AspNetLoggingProviderScripts */
	@UserName nvarchar(255),
	@QueryString ntext,
	@FormData ntext
	/* End of extra parameters */
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.logging_Events (EventTypeId, Message, Source, 
		/* Following columns are added for the AspNetLoggingProviderScripts */
		UserName, QueryString, FormData
		/* End of extra columns */
		)
	VALUES (@EventTypeId, @Message, @Source, 
		/* Following values are added for the AspNetLoggingProviderScripts */
		@UserName, @QueryString, @FormData
		/* End of extra values */
		);

	SELECT CONVERT(int, SCOPE_IDENTITY());
END
GO


CREATE PROCEDURE [dbo].[logging_AddException]
	@EventId int,
	@ParentExceptionId int,
	@ExceptionType nvarchar(255),
	@Message as ntext,
	@StackTrace as ntext
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.logging_Exceptions (EventId, ParentExceptionId, ExceptionType, Message, StackTrace) 
	VALUES (@EventId, @ParentExceptionId, @ExceptionType, @Message, @StackTrace);

	SELECT CONVERT(int, SCOPE_IDENTITY());
END
GO


ALTER TABLE [dbo].[logging_Exceptions] WITH CHECK ADD CONSTRAINT [FK_logging_Exceptions_logging_Events] FOREIGN KEY([EventId])
REFERENCES [dbo].[logging_Events] ([EventId])


ALTER TABLE [dbo].[logging_Exceptions] CHECK CONSTRAINT [FK_logging_Exceptions_logging_Events]


ALTER TABLE [dbo].[logging_Exceptions] WITH CHECK ADD CONSTRAINT [FK_logging_Exceptions_logging_Exceptions] FOREIGN KEY([ParentExceptionId])
REFERENCES [dbo].[logging_Exceptions] ([ExceptionId])


ALTER TABLE [dbo].[logging_Exceptions] CHECK CONSTRAINT [FK_logging_Exceptions_logging_Exceptions]


ALTER TABLE [dbo].[logging_Events] WITH CHECK ADD CONSTRAINT [FK_logging_Events_logging_EventTypes] FOREIGN KEY([EventTypeId])
REFERENCES [dbo].[logging_EventTypes] ([EventTypeId])


ALTER TABLE [dbo].[logging_Events] CHECK CONSTRAINT [FK_logging_Events_logging_EventTypes]



INSERT INTO logging_EventTypes (EventTypeId, Name, Description) 
VALUES (0, 'Debug', 'A debug event. This indicates a verbose event, usefull during development.')

INSERT INTO logging_EventTypes (EventTypeId, Name, Description) 
VALUES (1, 'Information', 'An information event. This indicates a significant, successful operation.')

INSERT INTO logging_EventTypes (EventTypeId, Name, Description) 
VALUES (2, 'Warning', 'A warning event. This indicates a problem that is not immediately significant, but that may signify conditions that could cause future problems.')

INSERT INTO logging_EventTypes (EventTypeId, Name, Description) 
VALUES (3, 'Error', 'An error event. This indicates a significant problem the user should know about; usually a loss of functionality or data.')

INSERT INTO logging_EventTypes (EventTypeId, Name, Description) 
VALUES (4, 'Critical', 'A critical event. This indicates a fatal error or application crash.')


COMMIT