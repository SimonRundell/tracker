/**
 * DTOs for the grade grid / results endpoint (/results/index.php).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AtRiskTracker.Models
{
    public class AssessmentDefDto
    {
        [JsonProperty("id")]          public int    Id         { get; set; }
        [JsonProperty("part_name")]   public string Title      { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("max_mark")]    public int?   MaxMark    { get; set; }
    }

    public class UnitDto
    {
        [JsonProperty("id")]              public int    Id             { get; set; }
        [JsonProperty("unitcode")]        public string Unitcode       { get; set; }
        [JsonProperty("unitname")]        public string Unitname       { get; set; }
        [JsonProperty("credits")]         public int    Credits        { get; set; }
        [JsonProperty("glh")]             public int    Glh            { get; set; }
        [JsonProperty("year_taken")]      public int?   YearTaken      { get; set; }
        [JsonProperty("is_external")]     public int    IsExternal     { get; set; }
        [JsonProperty("assessment_defs")] public List<AssessmentDefDto> AssessmentDefs { get; set; }
    }

    public class AssessmentRecordDto
    {
        [JsonProperty("id")]                public int?   Id              { get; set; }
        [JsonProperty("mark")]              public int?   Mark            { get; set; }
        [JsonProperty("grade")]             public string Grade           { get; set; }
        [JsonProperty("feedback")]          public string Feedback        { get; set; }
        [JsonProperty("status")]            public string Status          { get; set; }
        [JsonProperty("date_set")]          public string DateSet         { get; set; }
        [JsonProperty("date_deadline")]     public string DateDeadline    { get; set; }
        [JsonProperty("date_resubmission")] public string DateResubmission { get; set; }
        [JsonProperty("date_completed")]    public string DateCompleted   { get; set; }
    }

    public class StudentDto
    {
        [JsonProperty("id")]          public int    Id          { get; set; }
        [JsonProperty("cisnumber")]   public string Cisnumber   { get; set; }
        [JsonProperty("firstname")]   public string Firstname   { get; set; }
        [JsonProperty("lastname")]    public string Lastname    { get; set; }
        [JsonProperty("concern")]     public string Concern     { get; set; }
        [JsonProperty("notes")]       public string Notes       { get; set; }
        [JsonProperty("results")]     public Dictionary<string, string> Results     { get; set; }
        [JsonProperty("rawMarks")]    public Dictionary<string, int?>   RawMarks    { get; set; }
        [JsonProperty("assessments")] public Dictionary<string, AssessmentRecordDto> Assessments { get; set; }
    }

    public class GridDataDto
    {
        [JsonProperty("units")]    public List<UnitDto>    Units    { get; set; }
        [JsonProperty("students")] public List<StudentDto> Students { get; set; }
    }

    public class GridResponse
    {
        [JsonProperty("data")] public GridDataDto Data { get; set; }
    }

    // Course group selector models
    public class CourseDto
    {
        [JsonProperty("id")]                 public int    Id                { get; set; }
        [JsonProperty("coursename")]         public string Coursename        { get; set; }
        [JsonProperty("qual_type")]          public string QualType          { get; set; }
        [JsonProperty("pass_points")]        public int?   PassPoints        { get; set; }
        [JsonProperty("merit_points")]       public int?   MeritPoints       { get; set; }
        [JsonProperty("distinction_points")] public int?   DistinctionPoints { get; set; }
        public override string ToString() => Coursename;
    }

    public class GroupDto
    {
        [JsonProperty("id")]        public int    Id        { get; set; }
        [JsonProperty("groupname")] public string Groupname { get; set; }
        [JsonProperty("course_id")] public int    CourseId  { get; set; }
        public override string ToString() => Groupname;
    }

    public class CoursesResponse
    {
        [JsonProperty("data")] public List<CourseDto> Data { get; set; }
    }

    public class GroupsResponse
    {
        [JsonProperty("data")] public List<GroupDto> Data { get; set; }
    }

    // QualType model (used for is_ncfe / show_predict flags)
    public class QualTypeDto
    {
        [JsonProperty("id")]               public int    Id             { get; set; }
        [JsonProperty("name")]             public string Name           { get; set; }
        [JsonProperty("slug")]             public string Slug           { get; set; }
        [JsonProperty("show_predict")]     public int    ShowPredict    { get; set; }
        [JsonProperty("is_ncfe")]          public int    IsNcfe         { get; set; }
        [JsonProperty("btec_overall_grades")] public int BtecOverallGrades { get; set; }
        public override string ToString() => Name;

        // The backend stores qual_type as a plain string on each course record;
        // there is no separate qualtypes table or API endpoint. Flags are derived
        // locally to match the React app's hardcoded logic in GradeGrid.jsx.
        public static QualTypeDto FromSlug(string slug)
        {
            switch (slug ?? "other")
            {
                case "btec_ext_dip": return new QualTypeDto { Id=1, Name="BTec Extended Diploma", Slug="btec_ext_dip", ShowPredict=1, IsNcfe=0, BtecOverallGrades=1 };
                case "ncfe":         return new QualTypeDto { Id=2, Name="NCFE",                  Slug="ncfe",         ShowPredict=0, IsNcfe=1, BtecOverallGrades=0 };
                case "t_level":      return new QualTypeDto { Id=3, Name="T-Level",               Slug="t_level",      ShowPredict=1, IsNcfe=0, BtecOverallGrades=0 };
                default:             return new QualTypeDto { Id=4, Name="Other",                  Slug="other",        ShowPredict=0, IsNcfe=0, BtecOverallGrades=0 };
            }
        }

        /// <summary>The complete fixed list of supported qual types.</summary>
        public static List<QualTypeDto> All() => new List<QualTypeDto>
        {
            FromSlug("btec_ext_dip"),
            FromSlug("ncfe"),
            FromSlug("t_level"),
            FromSlug("other"),
        };
    }

    public class QualTypesResponse
    {
        [JsonProperty("data")] public List<QualTypeDto> Data { get; set; }
    }
}
