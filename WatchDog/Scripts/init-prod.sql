CREATE TYPE user_role AS ENUM ('SuperAdmin', 'User');
CREATE TYPE project_status AS ENUM ('NotStarted', 'InProgress', 'Completed', 'Closed');
CREATE TYPE message_type AS ENUM ('Update', 'Announcement', 'Milestone', 'Question');

CREATE TABLE IF NOT EXISTS Users
(
    Id           SERIAL PRIMARY KEY,
    Username     VARCHAR(100) NOT NULL UNIQUE,
    Email        VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role         user_role    NOT NULL DEFAULT 'User',
    CreatedDate  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Projects
(
    Id          SERIAL PRIMARY KEY,
    Title       VARCHAR(255)   NOT NULL,
    Description TEXT,
    StartDate   TIMESTAMP      NOT NULL,
    EndDate     TIMESTAMP,
    Status      project_status NOT NULL DEFAULT 'NotStarted',
    CreatedDate TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Tasks
(
    Id                 SERIAL PRIMARY KEY,
    TaskDescription    VARCHAR(255) NOT NULL,
    Remarks            TEXT,
    StartDate          TIMESTAMP,
    CompletedDate      TIMESTAMP,
    PercentageComplete INT          NOT NULL DEFAULT 0,
    ProjectId          INT          NOT NULL REFERENCES Projects (Id) ON DELETE CASCADE,
    AssignedUserId     INT          NOT NULL REFERENCES Users (Id),
    CreatedDate        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS SubTasks
(
    Id            SERIAL PRIMARY KEY,
    Description   VARCHAR(255) NOT NULL,
    StartDate     TIMESTAMP,
    CompletedDate TIMESTAMP,
    IsComplete    BOOLEAN      NOT NULL DEFAULT FALSE,
    TaskId        INT          NOT NULL REFERENCES Tasks (Id) ON DELETE CASCADE,
    CreatedById   INT          NOT NULL REFERENCES Users (Id),
    CreatedDate   TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS TimeLineMessages
(
    Id          SERIAL PRIMARY KEY,
    Content     TEXT         NOT NULL,
    Type        message_type NOT NULL,
    IsPinned    BOOLEAN      NOT NULL DEFAULT FALSE,
    ProjectId   INT          NOT NULL REFERENCES Projects (Id) ON DELETE CASCADE,
    AuthorId    INT REFERENCES Users (Id),
    CreatedDate TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ProgressionMessages
(
    Id          SERIAL PRIMARY KEY,
    Content     TEXT      NOT NULL,
    TaskId      INT       NOT NULL REFERENCES Tasks (Id) ON DELETE CASCADE,
    AuthorId    INT       NOT NULL REFERENCES Users (Id),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS UserProjects
(
    Id         SERIAL PRIMARY KEY,
    UserId     INT       NOT NULL REFERENCES Users (Id) ON DELETE CASCADE,
    ProjectId  INT       NOT NULL REFERENCES Projects (Id) ON DELETE CASCADE,
    JoinedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (UserId, ProjectId)
);

CREATE INDEX idx_tasks_project ON Tasks (ProjectId);
CREATE INDEX idx_subtasks_task ON SubTasks (TaskId);
CREATE INDEX idx_timeline_messages_project ON TimeLineMessages (ProjectId);
CREATE INDEX idx_progression_messages_subtask ON ProgressionMessages (TaskId);
CREATE INDEX idx_user_projects_user ON UserProjects (UserId);
CREATE INDEX idx_user_projects_project ON UserProjects (ProjectId);

INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedDate)
VALUES ('admin',
        'admin@xyztechsolutions.com',
        '$2y$10$7u0OOaMjRFEH5DETDMiw4eayOCpT6MwNTbOgcZ6JJlWY5/nZGqrba',
        'SuperAdmin',
        CURRENT_TIMESTAMP)
ON CONFLICT (Email) DO NOTHING;
