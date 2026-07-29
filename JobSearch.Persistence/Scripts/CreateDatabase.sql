CREATE TABLE IF NOT EXISTS Users (
    Id              TEXT        NOT NULL PRIMARY KEY,
    Email           TEXT        NOT NULL,
    Name            TEXT        NOT NULL,
    CreatedAt       TEXT        NOT NULL,
    UpdatedAt       TEXT        NOT NULL,
    IsActive        INTEGER     NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

-- ADR-0002: Worker resolves the target user by most recent UpdatedAt.
CREATE INDEX IF NOT EXISTS IX_Users_UpdatedAt
    ON Users (UpdatedAt DESC);

CREATE TABLE IF NOT EXISTS UserProfiles (
    Id                  TEXT    NOT NULL PRIMARY KEY,
    UserId              TEXT    NOT NULL,
    ClaudeReadyProfile  TEXT    NOT NULL,
    DesiredRoles        TEXT    NOT NULL DEFAULT '',
    DesiredSalaryMin    INTEGER,
    DesiredSalaryMax    INTEGER,
    SalaryCurrency      TEXT    NOT NULL DEFAULT 'USD',
    LocationPreference  TEXT    NOT NULL DEFAULT '',
    CvParsedAt          TEXT    NOT NULL,
    CvFileHash          TEXT    NOT NULL DEFAULT '',
    UpdatedAt           TEXT    NOT NULL,
    CONSTRAINT FK_UserProfiles_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserProfiles_UserId UNIQUE (UserId)
);

CREATE TABLE IF NOT EXISTS UserSkills (
    Id                  TEXT    NOT NULL PRIMARY KEY,
    UserId              TEXT    NOT NULL,
    SkillName           TEXT    NOT NULL,
    ProficiencyLevel    TEXT    NOT NULL DEFAULT 'NotSpecified',
    YearsOfExperience   REAL,
    ExtractedByClaude   INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT FK_UserSkills_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_UserSkills_UserId_SkillName
    ON UserSkills (UserId, SkillName);

CREATE TABLE IF NOT EXISTS JobSites (
    Id          TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    BaseUrl     TEXT    NOT NULL,
    IsActive    INTEGER NOT NULL DEFAULT 1,
    ScrapeConfig TEXT   NOT NULL DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS Jobs (
    Id              TEXT    NOT NULL PRIMARY KEY,
    JobSiteId       TEXT    NOT NULL,
    -- ADR-0008: nullable and unpopulated for now — reserved for future
    -- use (secondary dedup key / display value). SaveJobTool always
    -- passes null; this was previously NOT NULL, which risked a
    -- constraint violation on every job save.
    ExternalId      TEXT,
    Url             TEXT    NOT NULL,
    Title           TEXT    NOT NULL,
    Company         TEXT    NOT NULL,
    Location        TEXT,
    SalaryRaw       TEXT,
    DescriptionRaw  TEXT    NOT NULL,
    PostedAt        TEXT,
    FoundAt         TEXT    NOT NULL,
    UrlHash         TEXT    NOT NULL,
    CONSTRAINT FK_Jobs_JobSites
        FOREIGN KEY (JobSiteId) REFERENCES JobSites(Id) ON DELETE RESTRICT,
    CONSTRAINT UQ_Jobs_UrlHash
        UNIQUE (UrlHash),
    -- Safe while ExternalId is always NULL: SQL treats every NULL as
    -- distinct for UNIQUE purposes, so multiple NULL rows per JobSiteId
    -- don't conflict. Starts enforcing real uniqueness once populated.
    CONSTRAINT UQ_Jobs_JobSiteId_ExternalId
        UNIQUE (JobSiteId, ExternalId)
);

CREATE INDEX IF NOT EXISTS IX_Jobs_UrlHash
    ON Jobs (UrlHash);

CREATE INDEX IF NOT EXISTS IX_Jobs_JobSiteId_ExternalId
    ON Jobs (JobSiteId, ExternalId);

CREATE TABLE IF NOT EXISTS UserJobMatches (
    Id              TEXT    NOT NULL PRIMARY KEY,
    UserId          TEXT    NOT NULL,
    JobId           TEXT    NOT NULL,
    RelevanceScore  REAL    NOT NULL,
    RelevanceReason TEXT,
    WasNotified     INTEGER NOT NULL DEFAULT 0,
    NotifiedAt      TEXT,
    FoundInRunAt    TEXT    NOT NULL,
    -- ADR-0007: persistence foundation only — no Business/UI writes yet.
    IsApplied       INTEGER NOT NULL DEFAULT 0,
    AppliedAt       TEXT,
    CONSTRAINT FK_UserJobMatches_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserJobMatches_Jobs
        FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserJobMatches_UserId_JobId
        UNIQUE (UserId, JobId)
);

CREATE INDEX IF NOT EXISTS IX_UserJobMatches_UserId_JobId
    ON UserJobMatches (UserId, JobId);

CREATE INDEX IF NOT EXISTS IX_UserJobMatches_WasNotified
    ON UserJobMatches (WasNotified);

CREATE INDEX IF NOT EXISTS IX_UserJobMatches_FoundInRunAt
    ON UserJobMatches (FoundInRunAt);

-- ADR-0009: jobs Claude analyzed and scored below RelevanceThreshold.
-- Parallel structure to UserJobMatches, without WasNotified/IsApplied —
-- those concepts don't apply to a rejected job.
CREATE TABLE IF NOT EXISTS UserJobRejections (
    Id              TEXT    NOT NULL PRIMARY KEY,
    UserId          TEXT    NOT NULL,
    JobId           TEXT    NOT NULL,
    RelevanceScore  REAL    NOT NULL,
    RelevanceReason TEXT,
    AnalyzedAt      TEXT    NOT NULL,
    CONSTRAINT FK_UserJobRejections_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserJobRejections_Jobs
        FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserJobRejections_UserId_JobId
        UNIQUE (UserId, JobId)
);

CREATE INDEX IF NOT EXISTS IX_UserJobRejections_UserId_AnalyzedAt
    ON UserJobRejections (UserId, AnalyzedAt);

-- Log of every attempted email send. Status/AttemptCount/ErrorMessage let
-- the retry policy (Polly, 3 attempts) record outcome without needing a
-- separate audit mechanism.
CREATE TABLE IF NOT EXISTS SentEmails (
    Id              TEXT    NOT NULL PRIMARY KEY,
    UserId          TEXT    NOT NULL,
    ToAddress       TEXT    NOT NULL,
    Subject         TEXT    NOT NULL,
    Body            TEXT    NOT NULL,
    Status          TEXT    NOT NULL DEFAULT 'Pending', -- Pending | Sent | Failed
    AttemptCount    INTEGER NOT NULL DEFAULT 0,
    ErrorMessage    TEXT,
    SentAt          TEXT,
    CreatedAt       TEXT    NOT NULL,
    CONSTRAINT FK_SentEmails_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_SentEmails_UserId_CreatedAt
    ON SentEmails (UserId, CreatedAt);

CREATE INDEX IF NOT EXISTS IX_SentEmails_Status
    ON SentEmails (Status);

-- SMTP settings editable via the new WPF form. Deliberately does NOT
-- include a password/credential column — that stays in user-secrets /
-- env vars only (EmailSettings:SmtpPassword), never written by the UI.
-- Single-row table per ADR-0003 (single-user local deployment) — the
-- application always upserts/reads the one row with Id = the well-known
-- singleton GUID, rather than querying "all settings".
CREATE TABLE IF NOT EXISTS EmailSettings (
    Id                  TEXT    NOT NULL PRIMARY KEY,
    SmtpHost            TEXT    NOT NULL,
    SmtpPort            INTEGER NOT NULL,
    UseSsl              INTEGER NOT NULL DEFAULT 1,
    SmtpUsername        TEXT    NOT NULL DEFAULT '',
    FromAddress         TEXT    NOT NULL,
    FromDisplayName     TEXT    NOT NULL DEFAULT '',
    UpdatedAt           TEXT    NOT NULL
);