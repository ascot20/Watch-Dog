CREATE TYPE user_role AS ENUM ('SuperAdmin', 'User');
CREATE TYPE project_status AS ENUM ('NotStarted', 'InProgress', 'Completed', 'Closed');
CREATE TYPE subtask_status AS ENUM ('NotStarted', 'InProgress', 'Completed', 'OnHold', 'Closed');
CREATE TYPE message_type AS ENUM ('Update', 'Announcement', 'Milestone', 'Question');

CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role user_role NOT NULL DEFAULT 'User',
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Projects (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    StartDate TIMESTAMP NOT NULL,
    EndDate TIMESTAMP,
    Status project_status NOT NULL DEFAULT 'NotStarted',
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Tasks (
    Id SERIAL PRIMARY KEY,
    TaskDescription VARCHAR(255) NOT NULL,
    Remarks TEXT,
    StartDate TIMESTAMP,
    CompletedDate TIMESTAMP,
    PercentageComplete INT NOT NULL DEFAULT 0,
    ProjectId INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    AssignedUserId INT NOT NULL REFERENCES Users(Id),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS SubTasks (
    Id SERIAL PRIMARY KEY,
    Description VARCHAR(255) NOT NULL,
    StartDate TIMESTAMP,
    CompletedDate TIMESTAMP,
    Status subtask_status NOT NULL DEFAULT 'NotStarted',
    TaskId INT NOT NULL REFERENCES Tasks(Id) ON DELETE CASCADE,
    CreatedById INT NOT NULL REFERENCES Users(Id),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS TimeLineMessages (
    Id SERIAL PRIMARY KEY,
    Content TEXT NOT NULL,
    Type message_type NOT NULL,
    IsPinned BOOLEAN NOT NULL DEFAULT FALSE,
    ProjectId INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    AuthorId INT REFERENCES Users(Id),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS TimeLineReplies (
    Id SERIAL PRIMARY KEY,
    Content TEXT NOT NULL,
    TimeLineMessageId INT NOT NULL REFERENCES TimeLineMessages(Id) ON DELETE CASCADE,
    AuthorId INT REFERENCES Users(Id),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ProgressionMessages (
    Id SERIAL PRIMARY KEY,
    Content TEXT NOT NULL,
    SubTaskId INT NOT NULL REFERENCES SubTasks(Id) ON DELETE CASCADE,
    AuthorId INT REFERENCES Users(Id),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS UserProjects (
    Id SERIAL PRIMARY KEY,
    UserId INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    ProjectId INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    JoinedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, ProjectId)
);

CREATE INDEX idx_tasks_project ON Tasks(ProjectId);
CREATE INDEX idx_subtasks_task ON SubTasks(TaskId);
CREATE INDEX idx_timeline_messages_project ON TimeLineMessages(ProjectId);
CREATE INDEX idx_timeline_replies_message ON TimeLineReplies(TimeLineMessageId);
CREATE INDEX idx_progression_messages_subtask ON ProgressionMessages(SubTaskId);
CREATE INDEX idx_user_projects_user ON UserProjects(UserId);
CREATE INDEX idx_user_projects_project ON UserProjects(ProjectId);

INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedDate)
VALUES (
    'admin',
    'admin@xyztechsolutions.com',
    '$2a$11$iu7d5i8NjkpQcRkshAM83OPMmAuGzM77P.OOuzkxXaxTsBnYRKKg.',
    'SuperAdmin',
    CURRENT_TIMESTAMP
) ON CONFLICT (Email) DO NOTHING;

INSERT INTO Projects (Title, Description, StartDate, Status, CreatedDate)
VALUES (
           'WatchDog Development',
           'Development of the WatchDog project management application',
           CURRENT_TIMESTAMP,
           'InProgress', 
           CURRENT_TIMESTAMP
       ) ON CONFLICT DO NOTHING;

INSERT INTO UserProjects (UserId, ProjectId, JoinedDate)
VALUES (1, 1, CURRENT_TIMESTAMP)
    ON CONFLICT DO NOTHING;


BEGIN;

DELETE FROM TimeLineReplies WHERE id > 0;
DELETE FROM ProgressionMessages WHERE id > 0;
DELETE FROM TimeLineMessages  WHERE id > 0;
DELETE FROM SubTasks WHERE id > 0;
DELETE FROM Tasks WHERE id > 0;
DELETE FROM UserProjects WHERE ProjectId > 1 OR UserId > 1;
DELETE FROM Projects WHERE Id > 1;
DELETE FROM Users WHERE Id > 1;

-- Insert sample data
INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedDate)
VALUES
    ('johndoe', 'john@example.com', '$2a$11$iu7d5i8NjkpQcRkshAM83OPMmAuGzM77P.OOuzkxXaxTsBnYRKKg.', 'User', CURRENT_TIMESTAMP),
    ('janedoe', 'jane@example.com', '$2a$11$iu7d5i8NjkpQcRkshAM83OPMmAuGzM77P.OOuzkxXaxTsBnYRKKg.', 'User', CURRENT_TIMESTAMP),
    ('projectlead', 'lead@example.com', '$2a$11$iu7d5i8NjkpQcRkshAM83OPMmAuGzM77P.OOuzkxXaxTsBnYRKKg.', 'User', CURRENT_TIMESTAMP)
    ON CONFLICT (Email) DO NOTHING;

INSERT INTO Projects (Title, Description, StartDate, EndDate, Status, CreatedDate)
VALUES
    ('Website Redesign', 'Complete redesign of company website', CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE + INTERVAL '60 days', 'InProgress', CURRENT_TIMESTAMP),
    ('Mobile App Development', 'Create a new mobile app for customers', CURRENT_DATE - INTERVAL '15 days', CURRENT_DATE + INTERVAL '90 days', 'NotStarted', CURRENT_TIMESTAMP),
    ('Database Migration', 'Migrate from legacy database to new system', CURRENT_DATE + INTERVAL '5 days', CURRENT_DATE + INTERVAL '45 days', 'NotStarted', CURRENT_TIMESTAMP),
    ('API Integration', 'Integrate with third-party payment APIs', CURRENT_DATE - INTERVAL '10 days', NULL, 'InProgress', CURRENT_TIMESTAMP),
    ('Marketing Campaign', 'Q4 digital marketing campaign', CURRENT_DATE - INTERVAL '60 days', CURRENT_DATE - INTERVAL '10 days', 'Completed', CURRENT_TIMESTAMP)
    ON CONFLICT DO NOTHING;

INSERT INTO UserProjects (UserId, ProjectId, JoinedDate)
VALUES
    (2, 1, CURRENT_TIMESTAMP),
    (3, 1, CURRENT_TIMESTAMP),
    (2, 2, CURRENT_TIMESTAMP),
    (3, 2, CURRENT_TIMESTAMP),
    (2, 3, CURRENT_TIMESTAMP),
    (3, 4, CURRENT_TIMESTAMP),
    (2, 5, CURRENT_TIMESTAMP)
    ON CONFLICT DO NOTHING;

INSERT INTO Tasks (TaskDescription, Remarks, StartDate, CompletedDate, PercentageComplete, ProjectId, AssignedUserId, CreatedDate)
VALUES
    ('Design mockups', 'Create wireframes and high-fidelity designs', CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE - INTERVAL '15 days', 100, 1, 2, CURRENT_TIMESTAMP),
    ('Frontend implementation', 'Implement new design in HTML/CSS/JS', CURRENT_DATE - INTERVAL '14 days', NULL, 75, 1, 2, CURRENT_TIMESTAMP),
    ('Backend API changes', 'Update API to support new features', CURRENT_DATE - INTERVAL '10 days', NULL, 40, 1, 3, CURRENT_TIMESTAMP),
    ('Design mobile UI', 'Create app UI designs', CURRENT_DATE - INTERVAL '15 days', CURRENT_DATE - INTERVAL '5 days', 100, 2, 2, CURRENT_TIMESTAMP),
    ('Develop iOS app', 'Build native iOS application', CURRENT_DATE - INTERVAL '5 days', NULL, 30, 2, 3, CURRENT_TIMESTAMP),
    ('Develop Android app', 'Build native Android application', CURRENT_DATE - INTERVAL '5 days', NULL, 20, 2, 2, CURRENT_TIMESTAMP),
    ('Data schema design', 'Design new database schema', CURRENT_DATE + INTERVAL '5 days', NULL, 0, 3, 3, CURRENT_TIMESTAMP),
    ('Data migration scripts', 'Write scripts to migrate existing data', CURRENT_DATE + INTERVAL '15 days', NULL, 0, 3, 2, CURRENT_TIMESTAMP),
    ('API documentation', 'Document API endpoints and usage', CURRENT_DATE - INTERVAL '10 days', CURRENT_DATE - INTERVAL '2 days', 100, 4, 3, CURRENT_TIMESTAMP),
    ('Payment gateway integration', 'Integrate with payment processor', CURRENT_DATE - INTERVAL '5 days', NULL, 60, 4, 3, CURRENT_TIMESTAMP),
    ('Content creation', 'Create campaign content', CURRENT_DATE - INTERVAL '60 days', CURRENT_DATE - INTERVAL '40 days', 100, 5, 2, CURRENT_TIMESTAMP),
    ('Campaign analytics', 'Analyze campaign performance', CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE - INTERVAL '10 days', 100, 5, 2, CURRENT_TIMESTAMP)
    ON CONFLICT DO NOTHING;

INSERT INTO SubTasks (Description, StartDate, CompletedDate, Status, TaskId, CreatedById, CreatedDate)
VALUES
    ('Create homepage mockup', CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE - INTERVAL '28 days', 'Completed', 1, 2, CURRENT_TIMESTAMP),
    ('Create product page mockup', CURRENT_DATE - INTERVAL '28 days', CURRENT_DATE - INTERVAL '25 days', 'Completed', 1, 2, CURRENT_TIMESTAMP),
    ('Create contact page mockup', CURRENT_DATE - INTERVAL '25 days', CURRENT_DATE - INTERVAL '22 days', 'Completed', 1, 2, CURRENT_TIMESTAMP),
    ('Create user account pages mockup', CURRENT_DATE - INTERVAL '22 days', CURRENT_DATE - INTERVAL '15 days', 'Completed', 1, 2, CURRENT_TIMESTAMP),
    ('Implement responsive navigation', CURRENT_DATE - INTERVAL '14 days', CURRENT_DATE - INTERVAL '12 days', 'Completed', 2, 2, CURRENT_TIMESTAMP),
    ('Implement homepage', CURRENT_DATE - INTERVAL '12 days', CURRENT_DATE - INTERVAL '8 days', 'Completed', 2, 2, CURRENT_TIMESTAMP),
    ('Implement product pages', CURRENT_DATE - INTERVAL '8 days', NULL, 'Completed', 2, 2, CURRENT_TIMESTAMP),
    ('Implement contact form', CURRENT_DATE - INTERVAL '6 days', NULL, 'Completed', 2, 2, CURRENT_TIMESTAMP),
    ('Update user API endpoints', CURRENT_DATE - INTERVAL '10 days', CURRENT_DATE - INTERVAL '7 days', 'Completed', 3, 3, CURRENT_TIMESTAMP),
    ('Update product API endpoints', CURRENT_DATE - INTERVAL '7 days', NULL, 'Completed', 3, 3, CURRENT_TIMESTAMP),
    ('Create order API endpoints', CURRENT_DATE - INTERVAL '4 days', NULL, 'InProgress', 3, 3, CURRENT_TIMESTAMP),
    ('Design login screen', CURRENT_DATE - INTERVAL '15 days', CURRENT_DATE - INTERVAL '13 days', 'Completed', 4, 2, CURRENT_TIMESTAMP),
    ('Design main navigation', CURRENT_DATE - INTERVAL '13 days', CURRENT_DATE - INTERVAL '10 days', 'Completed', 4, 2, CURRENT_TIMESTAMP),
    ('Design product screens', CURRENT_DATE - INTERVAL '10 days', CURRENT_DATE - INTERVAL '5 days', 'Completed', 4, 2, CURRENT_TIMESTAMP),
    ('Implement iOS login screen', CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE - INTERVAL '3 days', 'Completed', 5, 3, CURRENT_TIMESTAMP),
    ('Implement iOS navigation', CURRENT_DATE - INTERVAL '3 days', NULL, 'InProgress', 5, 3, CURRENT_TIMESTAMP),
    ('Implement iOS product screens', CURRENT_DATE - INTERVAL '1 day', NULL, 'NotStarted', 5, 3, CURRENT_TIMESTAMP),
    ('Implement Android login screen', CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE - INTERVAL '2 days', 'Completed', 6, 2, CURRENT_TIMESTAMP),
    ('Implement Android navigation', CURRENT_DATE - INTERVAL '2 days', NULL, 'InProgress', 6, 2, CURRENT_TIMESTAMP),
    ('Document payment API endpoints', CURRENT_DATE - INTERVAL '10 days', CURRENT_DATE - INTERVAL '8 days', 'Completed', 9, 3, CURRENT_TIMESTAMP),
    ('Document user API endpoints', CURRENT_DATE - INTERVAL '8 days', CURRENT_DATE - INTERVAL '5 days', 'Completed', 9, 3, CURRENT_TIMESTAMP),
    ('Document order API endpoints', CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE - INTERVAL '2 days', 'Completed', 9, 3, CURRENT_TIMESTAMP),
    ('Integrate payment provider SDK', CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE - INTERVAL '2 days', 'Completed', 10, 3, CURRENT_TIMESTAMP),
    ('Implement payment workflows', CURRENT_DATE - INTERVAL '2 days', NULL, 'InProgress', 10, 3, CURRENT_TIMESTAMP),
    ('Create email newsletter content', CURRENT_DATE - INTERVAL '60 days', CURRENT_DATE - INTERVAL '55 days', 'Completed', 11, 2, CURRENT_TIMESTAMP),
    ('Create social media content', CURRENT_DATE - INTERVAL '55 days', CURRENT_DATE - INTERVAL '50 days', 'Completed', 11, 2, CURRENT_TIMESTAMP),
    ('Create landing page content', CURRENT_DATE - INTERVAL '50 days', CURRENT_DATE - INTERVAL '40 days', 'Completed', 11, 2, CURRENT_TIMESTAMP)
    ON CONFLICT DO NOTHING;


INSERT INTO TimeLineMessages (Content, Type, IsPinned, ProjectId, AuthorId, CreatedDate)
VALUES
    ('Welcome to the Website Redesign project! Let''s make this our best site yet.', 'Update', TRUE, 1, 1, CURRENT_TIMESTAMP - INTERVAL '30 days'),
    ('I''ve completed all the design mockups. Please review and provide feedback.', 'Update', FALSE, 1, 2, CURRENT_TIMESTAMP - INTERVAL '15 days'),
    ('Backend API changes are taking longer than expected. I might need help.', 'Update', FALSE, 1, 3, CURRENT_TIMESTAMP - INTERVAL '7 days'),
    ('Mobile app development project is now underway. Please check tasks assigned to you.', 'Update', TRUE, 2, 1, CURRENT_TIMESTAMP - INTERVAL '15 days'),
    ('The iOS implementation is going well, but we might need to extend the timeline.', 'Update', FALSE, 2, 3, CURRENT_TIMESTAMP - INTERVAL '3 days'),
    ('Update on the payment integration: We''re at 60% completion and on track.', 'Update', FALSE, 4, 3, CURRENT_TIMESTAMP - INTERVAL '2 days'),
    ('Great work on the marketing campaign! The metrics look excellent.', 'Update', TRUE, 5, 1, CURRENT_TIMESTAMP - INTERVAL '15 days')
ON CONFLICT DO NOTHING;

INSERT INTO TimeLineReplies (Content, TimeLineMessageId, AuthorId, CreatedDate)
VALUES
    ('The designs look great! I''ll start implementing the frontend right away.', 2, 3, CURRENT_TIMESTAMP - INTERVAL '14 days'),
    ('I can help with the API work. Let''s sync up tomorrow.', 3, 1, CURRENT_TIMESTAMP - INTERVAL '7 days')
ON CONFLICT DO NOTHING;

INSERT INTO ProgressionMessages (Content, SubTaskId, AuthorId, CreatedDate)
VALUES
    ('Homepage mockup is complete and ready for review', 1, 2, CURRENT_TIMESTAMP - INTERVAL '28 days'),
    ('Integrated payment provider SDK successfully', 23, 3, CURRENT_TIMESTAMP - INTERVAL '2 days')
ON CONFLICT DO NOTHING;

COMMIT;
