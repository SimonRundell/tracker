namespace AtRiskTracker.Admin
{
    partial class AdminTabPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._tabs                = new System.Windows.Forms.TabControl();
            this._pgStudents          = new System.Windows.Forms.TabPage();
            this._pgEnrollments       = new System.Windows.Forms.TabPage();
            this._pgCourses           = new System.Windows.Forms.TabPage();
            this._pgGroups            = new System.Windows.Forms.TabPage();
            this._pgUnits             = new System.Windows.Forms.TabPage();
            this._pgCourseUnits       = new System.Windows.Forms.TabPage();
            this._pgObjectives        = new System.Windows.Forms.TabPage();
            this._pgAssessmentDefs    = new System.Windows.Forms.TabPage();
            this._pgConcerns          = new System.Windows.Forms.TabPage();
            this._pgUsers             = new System.Windows.Forms.TabPage();
            this._pnlStudents         = new StudentsPanel();
            this._pnlEnrollments      = new EnrollmentsPanel();
            this._pnlCourses          = new CoursesPanel();
            this._pnlGroups           = new GroupsPanel();
            this._pnlUnits            = new UnitsPanel();
            this._pnlCourseUnits      = new CourseUnitsPanel();
            this._pnlObjectives       = new ObjectivesPanel();
            this._pnlAssessmentDefs   = new AssessmentDefsPanel();
            this._pnlConcerns         = new ConcernsPanel();
            this._pnlUsers            = new UsersPanel();
            this._tabs.SuspendLayout();
            this._pgStudents.SuspendLayout();
            this._pgEnrollments.SuspendLayout();
            this._pgCourses.SuspendLayout();
            this._pgGroups.SuspendLayout();
            this._pgUnits.SuspendLayout();
            this._pgCourseUnits.SuspendLayout();
            this._pgObjectives.SuspendLayout();
            this._pgAssessmentDefs.SuspendLayout();
            this._pgConcerns.SuspendLayout();
            this._pgUsers.SuspendLayout();
            this.SuspendLayout();
            //
            // _tabs
            //
            this._tabs.Controls.Add(this._pgStudents);
            this._tabs.Controls.Add(this._pgEnrollments);
            this._tabs.Controls.Add(this._pgCourses);
            this._tabs.Controls.Add(this._pgGroups);
            this._tabs.Controls.Add(this._pgUnits);
            this._tabs.Controls.Add(this._pgCourseUnits);
            this._tabs.Controls.Add(this._pgObjectives);
            this._tabs.Controls.Add(this._pgAssessmentDefs);
            this._tabs.Controls.Add(this._pgConcerns);
            this._tabs.Controls.Add(this._pgUsers);
            this._tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabs.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this._tabs.Name = "_tabs";
            //
            // _pgStudents
            //
            this._pgStudents.Controls.Add(this._pnlStudents);
            this._pgStudents.Name = "_pgStudents";
            this._pgStudents.Text = "Students";
            //
            // _pnlStudents
            //
            this._pnlStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlStudents.Name = "_pnlStudents";
            //
            // _pgEnrollments
            //
            this._pgEnrollments.Controls.Add(this._pnlEnrollments);
            this._pgEnrollments.Name = "_pgEnrollments";
            this._pgEnrollments.Text = "Enrollments";
            //
            // _pnlEnrollments
            //
            this._pnlEnrollments.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlEnrollments.Name = "_pnlEnrollments";
            //
            // _pgCourses
            //
            this._pgCourses.Controls.Add(this._pnlCourses);
            this._pgCourses.Name = "_pgCourses";
            this._pgCourses.Text = "Courses";
            //
            // _pnlCourses
            //
            this._pnlCourses.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlCourses.Name = "_pnlCourses";
            //
            // _pgGroups
            //
            this._pgGroups.Controls.Add(this._pnlGroups);
            this._pgGroups.Name = "_pgGroups";
            this._pgGroups.Text = "Groups";
            //
            // _pnlGroups
            //
            this._pnlGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlGroups.Name = "_pnlGroups";
            //
            // _pgUnits
            //
            this._pgUnits.Controls.Add(this._pnlUnits);
            this._pgUnits.Name = "_pgUnits";
            this._pgUnits.Text = "Units";
            //
            // _pnlUnits
            //
            this._pnlUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlUnits.Name = "_pnlUnits";
            //
            // _pgCourseUnits
            //
            this._pgCourseUnits.Controls.Add(this._pnlCourseUnits);
            this._pgCourseUnits.Name = "_pgCourseUnits";
            this._pgCourseUnits.Text = "Course Units";
            //
            // _pnlCourseUnits
            //
            this._pnlCourseUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlCourseUnits.Name = "_pnlCourseUnits";
            //
            // _pgObjectives
            //
            this._pgObjectives.Controls.Add(this._pnlObjectives);
            this._pgObjectives.Name = "_pgObjectives";
            this._pgObjectives.Text = "Objectives";
            //
            // _pnlObjectives
            //
            this._pnlObjectives.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlObjectives.Name = "_pnlObjectives";
            //
            // _pgAssessmentDefs
            //
            this._pgAssessmentDefs.Controls.Add(this._pnlAssessmentDefs);
            this._pgAssessmentDefs.Name = "_pgAssessmentDefs";
            this._pgAssessmentDefs.Text = "Assessments";
            //
            // _pnlAssessmentDefs
            //
            this._pnlAssessmentDefs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlAssessmentDefs.Name = "_pnlAssessmentDefs";
            //
            // _pgConcerns
            //
            this._pgConcerns.Controls.Add(this._pnlConcerns);
            this._pgConcerns.Name = "_pgConcerns";
            this._pgConcerns.Text = "Concerns";
            //
            // _pnlConcerns
            //
            this._pnlConcerns.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlConcerns.Name = "_pnlConcerns";
            //
            // _pgUsers
            //
            this._pgUsers.Controls.Add(this._pnlUsers);
            this._pgUsers.Name = "_pgUsers";
            this._pgUsers.Text = "Users";
            //
            // _pnlUsers
            //
            this._pnlUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlUsers.Name = "_pnlUsers";
            //
            // AdminTabPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._tabs);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Name = "AdminTabPanel";
            this._pgStudents.ResumeLayout(false);
            this._pgEnrollments.ResumeLayout(false);
            this._pgCourses.ResumeLayout(false);
            this._pgGroups.ResumeLayout(false);
            this._pgUnits.ResumeLayout(false);
            this._pgCourseUnits.ResumeLayout(false);
            this._pgObjectives.ResumeLayout(false);
            this._pgAssessmentDefs.ResumeLayout(false);
            this._pgConcerns.ResumeLayout(false);
            this._pgUsers.ResumeLayout(false);
            this._tabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl  _tabs;
        private System.Windows.Forms.TabPage     _pgStudents;
        private System.Windows.Forms.TabPage     _pgEnrollments;
        private System.Windows.Forms.TabPage     _pgCourses;
        private System.Windows.Forms.TabPage     _pgGroups;
        private System.Windows.Forms.TabPage     _pgUnits;
        private System.Windows.Forms.TabPage     _pgCourseUnits;
        private System.Windows.Forms.TabPage     _pgObjectives;
        private System.Windows.Forms.TabPage     _pgAssessmentDefs;
        private System.Windows.Forms.TabPage     _pgConcerns;
        private System.Windows.Forms.TabPage     _pgUsers;
        private StudentsPanel         _pnlStudents;
        private EnrollmentsPanel      _pnlEnrollments;
        private CoursesPanel          _pnlCourses;
        private GroupsPanel           _pnlGroups;
        private UnitsPanel            _pnlUnits;
        private CourseUnitsPanel      _pnlCourseUnits;
        private ObjectivesPanel       _pnlObjectives;
        private AssessmentDefsPanel   _pnlAssessmentDefs;
        private ConcernsPanel         _pnlConcerns;
        private UsersPanel            _pnlUsers;
    }
}
