-- ============================================================
-- AtRiskRegister -- MySQL Schema
-- Exeter College
-- License: CC NC-BY-SA 4.0
-- ============================================================

CREATE DATABASE IF NOT EXISTS atriskreg
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE atriskreg;

-- System users (staff only -- no student access)
CREATE TABLE tbluser (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  email       VARCHAR(255) NOT NULL UNIQUE COMMENT 'Used as login username',
  password    VARCHAR(255) NOT NULL       COMMENT 'bcrypt hashed, cost 12',
  fullname    VARCHAR(255) NOT NULL,
  created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- Concern categories (e.g. Attendance, Engagement)
CREATE TABLE tblconcern (
  id       INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  concern  VARCHAR(255) NOT NULL
) ENGINE=InnoDB;

-- Course definitions
CREATE TABLE tblcourse (
  id                  INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  coursename          VARCHAR(255)  NOT NULL,
  qual_type           ENUM('btec_ext_dip','ncfe','t_level','other')
                        NOT NULL DEFAULT 'other'
                        COMMENT 'Qualification family; determines grade point calculation method',
  total_credits       INT UNSIGNED  NOT NULL DEFAULT 0 COMMENT 'Total qualification credits',
  pass_points         INT UNSIGNED  NOT NULL DEFAULT 0 COMMENT 'Min points for overall Pass',
  merit_points        INT UNSIGNED  NOT NULL DEFAULT 0 COMMENT 'Min points for overall Merit',
  distinction_points  INT UNSIGNED  NOT NULL DEFAULT 0 COMMENT 'Min points for overall Distinction'
) ENGINE=InnoDB;

-- Teaching groups / classes
CREATE TABLE tblgroup (
  id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  course_id  INT UNSIGNED NOT NULL,
  groupname  VARCHAR(100) NOT NULL,
  FOREIGN KEY (course_id) REFERENCES tblcourse(id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Unit definitions (shared across courses via junction table)
CREATE TABLE tblunit (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  unitcode    VARCHAR(50)  NOT NULL,
  unitname    VARCHAR(255) NOT NULL,
  unitref     VARCHAR(100),
  credits     INT UNSIGNED NOT NULL DEFAULT 0,
  glh         INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Guided Learning Hours',
  is_external TINYINT(1)   NOT NULL DEFAULT 0 COMMENT '1 = externally assessed unit',
  year        TINYINT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Year of study: 1, 2, 3 or 4',
  section_type ENUM('learning_objectives', 'grade_bands') NULL
               DEFAULT NULL
               COMMENT 'NULL = no criteria configured; use direct grade entry'
) ENGINE=InnoDB;

-- Junction: many-to-many between courses and units
CREATE TABLE tblcourseunit (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  course_id   INT UNSIGNED NOT NULL,
  unit_id     INT UNSIGNED NOT NULL,
  year_taken  INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Year in which the unit is taken (1 or 2)',
  UNIQUE KEY uq_course_unit (course_id, unit_id),
  FOREIGN KEY (course_id) REFERENCES tblcourse(id) ON DELETE CASCADE,
  FOREIGN KEY (unit_id)   REFERENCES tblunit(id)   ON DELETE CASCADE
) ENGINE=InnoDB;

-- Student records (person record only; group enrollment is in tblstudent_group)
CREATE TABLE tblstudent (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  firstname   VARCHAR(100) NOT NULL,
  lastname    VARCHAR(100) NOT NULL,
  cisnumber   VARCHAR(50)  UNIQUE COMMENT 'College CIS reference number',
  notes       LONGTEXT              COMMENT 'Rich-text staff notes (HTML from TipTap editor)'
) ENGINE=InnoDB;

-- Student group enrollment (many-to-many; concern is per enrollment row)
CREATE TABLE tblstudent_group (
  id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  student_id INT UNSIGNED NOT NULL,
  group_id   INT UNSIGNED NOT NULL,
  concern_id INT UNSIGNED NULL DEFAULT NULL,
  UNIQUE KEY uq_student_group (student_id, group_id),
  KEY idx_group_id (group_id),
  FOREIGN KEY (student_id) REFERENCES tblstudent(id)  ON DELETE CASCADE,
  FOREIGN KEY (group_id)   REFERENCES tblgroup(id)    ON DELETE CASCADE,
  FOREIGN KEY (concern_id) REFERENCES tblconcern(id)  ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Student group enrollment; one row per student-group pairing';

-- Unit results per student (one row per student-unit pair)
CREATE TABLE tblresults (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  student_id  INT UNSIGNED NOT NULL,
  unit_id     INT UNSIGNED NOT NULL,
  result      ENUM('NS','OU','U','NP','P','M','D') NOT NULL DEFAULT 'NS'
              COMMENT 'NS=Not Submitted, OU=Ungraded Outstanding, U=Ungraded, NP=Near Pass, P=Pass, M=Merit, D=Distinction',
  raw_mark    SMALLINT UNSIGNED NULL
              COMMENT 'Raw exam mark for externally-assessed BTec units (mark = points directly)',
  updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  updated_by  INT UNSIGNED COMMENT 'FK to tbluser.id -- who last updated this result',
  UNIQUE KEY uq_student_unit (student_id, unit_id),
  FOREIGN KEY (student_id) REFERENCES tblstudent(id) ON DELETE CASCADE,
  FOREIGN KEY (unit_id)    REFERENCES tblunit(id)    ON DELETE CASCADE,
  FOREIGN KEY (updated_by) REFERENCES tbluser(id)    ON DELETE SET NULL
) ENGINE=InnoDB;

-- ============================================================
-- Amendment 01 -- Criteria & Objective Tracking
-- ============================================================

-- Sections within a unit: LOs (LO1, LO2) or grade bands (P, M, D)
CREATE TABLE tblunitsection (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  unit_id     INT UNSIGNED NOT NULL,
  label       VARCHAR(20)  NOT NULL COMMENT 'e.g. LO1, LO2, P, M, D',
  title       VARCHAR(255)          COMMENT 'Optional descriptor',
  sort_order  INT UNSIGNED NOT NULL DEFAULT 0,
  UNIQUE KEY uq_section (unit_id, label),
  FOREIGN KEY (unit_id) REFERENCES tblunit(id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Sections within a unit -- LOs or grade bands';

-- Individual criteria within a section
CREATE TABLE tblcriteria (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  section_id  INT UNSIGNED NOT NULL,
  code        VARCHAR(20)  NOT NULL COMMENT 'e.g. 1.1, P1, M3',
  description TEXT                  COMMENT 'Full criterion wording',
  sort_order  INT UNSIGNED NOT NULL DEFAULT 0,
  UNIQUE KEY uq_criteria (section_id, code),
  FOREIGN KEY (section_id) REFERENCES tblunitsection(id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Individual assessment criteria within a section';

-- Student evidence records
CREATE TABLE tblevidence (
  id            INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  student_id    INT UNSIGNED NOT NULL,
  criteria_id   INT UNSIGNED NOT NULL,
  achieved      TINYINT(1)   NOT NULL DEFAULT 0,
  achieved_date DATE                  COMMENT 'Date criterion was signed off',
  assessor      VARCHAR(100)          COMMENT 'Name or initials of signing assessor',
  portfolio_ref VARCHAR(100)          COMMENT 'Portfolio or page reference',
  updated_at    TIMESTAMP    DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  updated_by    INT UNSIGNED          COMMENT 'FK to tbluser.id',
  UNIQUE KEY uq_evidence (student_id, criteria_id),
  FOREIGN KEY (student_id)  REFERENCES tblstudent(id)  ON DELETE CASCADE,
  FOREIGN KEY (criteria_id) REFERENCES tblcriteria(id) ON DELETE CASCADE,
  FOREIGN KEY (updated_by)  REFERENCES tbluser(id)     ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Student evidence against individual criteria';

-- Append-only grade change history (one row per save event)
CREATE TABLE tblgrade_audit (
  id           INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  student_id   INT UNSIGNED NOT NULL,
  unit_id      INT UNSIGNED NOT NULL,
  old_result   ENUM('NS','OU','U','NP','P','M','D') NULL     COMMENT 'NULL = first time this grade was set',
  new_result   ENUM('NS','OU','U','NP','P','M','D') NOT NULL,
  old_raw_mark SMALLINT UNSIGNED NULL,
  new_raw_mark SMALLINT UNSIGNED NULL,
  changed_by   INT UNSIGNED NOT NULL,
  changed_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (student_id) REFERENCES tblstudent(id) ON DELETE CASCADE,
  FOREIGN KEY (unit_id)    REFERENCES tblunit(id)    ON DELETE CASCADE,
  FOREIGN KEY (changed_by) REFERENCES tbluser(id)    ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Append-only log of every grade change';

-- Assessment part definitions per unit (shared across courses)
CREATE TABLE tblassessment_def (
  id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  unit_id    INT UNSIGNED NOT NULL,
  part_name  VARCHAR(100) NOT NULL            COMMENT 'e.g. "Unit Assessment", "Part A"',
  sort_order TINYINT UNSIGNED NOT NULL DEFAULT 0,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (unit_id) REFERENCES tblunit(id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Assessment part definitions per unit';

-- Per-student, per-part assessment tracking record
CREATE TABLE tblassessment (
  id                 INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  student_id         INT UNSIGNED NOT NULL,
  assessment_def_id  INT UNSIGNED NOT NULL,
  status             ENUM('NOT_SET','SET','HANDED_IN_1','RETURNED','HANDED_IN_2','COMPLETE','INCOMPLETE')
                     NOT NULL DEFAULT 'NOT_SET',
  date_set           DATE NULL,
  date_deadline      DATE NULL,
  date_resubmission  DATE NULL,
  date_completed     DATE NULL,
  set_by_id          INT UNSIGNED NULL         COMMENT 'Staff member who first set the date_set',
  updated_by         INT UNSIGNED NULL         COMMENT 'Staff member who last saved this row',
  updated_at         TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_assessment (student_id, assessment_def_id),
  FOREIGN KEY (student_id)        REFERENCES tblstudent(id)       ON DELETE CASCADE,
  FOREIGN KEY (assessment_def_id) REFERENCES tblassessment_def(id) ON DELETE CASCADE,
  FOREIGN KEY (set_by_id)         REFERENCES tbluser(id)          ON DELETE SET NULL,
  FOREIGN KEY (updated_by)        REFERENCES tbluser(id)          ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Per-student per-part assessment tracking';

-- ============================================================
-- Views
-- ============================================================

CREATE OR REPLACE VIEW vw_criteria_progress AS
SELECT
  cu.course_id,
  u.id                        AS unit_id,
  u.unitcode,
  u.section_type,
  s.label                     AS section_label,
  s.sort_order                AS section_order,
  c.id                        AS criteria_id,
  c.code                      AS criterion_code,
  c.sort_order                AS criterion_order,
  e.student_id,
  COALESCE(e.achieved, 0)     AS achieved,
  e.achieved_date,
  e.portfolio_ref,
  e.assessor
FROM tblcriteria c
JOIN tblunitsection s   ON c.section_id  = s.id
JOIN tblunit u          ON s.unit_id     = u.id
JOIN tblcourseunit cu   ON cu.unit_id    = u.id
LEFT JOIN tblevidence e ON e.criteria_id = c.id
ORDER BY u.id, s.sort_order, c.sort_order, e.student_id;

-- ============================================================
-- Seed Data
-- ============================================================

INSERT INTO tblconcern (concern) VALUES
  ('No Concern'),
  ('Attendance'),
  ('Engagement'),
  ('Late Submissions'),
  ('Grade Borderline'),
  ('Personal Circumstances');

