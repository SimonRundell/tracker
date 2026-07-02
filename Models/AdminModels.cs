/**
 * DTOs for admin panel endpoints.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AtRiskTracker.Models
{
    // Generic list wrapper
    public class ListResponse<T>
    {
        [JsonProperty("data")] public List<T> Data { get; set; }
    }

    public class SingleResponse<T>
    {
        [JsonProperty("data")] public T Data { get; set; }
    }

    // Students
    public class StudentAdminDto
    {
        [JsonProperty("id")]         public int    Id        { get; set; }
        [JsonProperty("cisnumber")]  public string Cisnumber { get; set; }
        [JsonProperty("firstname")]  public string Firstname { get; set; }
        [JsonProperty("lastname")]   public string Lastname  { get; set; }
        [JsonProperty("sg_id")]      public int?   SgId      { get; set; }
        [JsonProperty("concern_id")] public int?   ConcernId { get; set; }
        [JsonProperty("concern")]    public string Concern   { get; set; }
        public override string ToString() => $"{Lastname}, {Firstname} ({Cisnumber})";
    }

    // Units (admin)
    public class UnitAdminDto
    {
        [JsonProperty("id")]          public int    Id          { get; set; }
        [JsonProperty("unitcode")]    public string Unitcode    { get; set; }
        [JsonProperty("unitname")]    public string Unitname    { get; set; }
        [JsonProperty("credits")]     public int    Credits     { get; set; }
        [JsonProperty("glh")]         public int    Glh         { get; set; }
        [JsonProperty("is_external")] public int    IsExternal  { get; set; }
        public override string ToString() => $"{Unitcode} - {Unitname}";
    }

    // Course units (linking course <-> unit)
    public class CourseUnitDto
    {
        [JsonProperty("id")]          public int    Id         { get; set; }
        [JsonProperty("course_id")]   public int    CourseId   { get; set; }
        [JsonProperty("unit_id")]     public int    UnitId     { get; set; }
        [JsonProperty("year_taken")]  public int?   YearTaken  { get; set; }
        [JsonProperty("coursename")]  public string Coursename { get; set; }
        [JsonProperty("unitcode")]    public string Unitcode   { get; set; }
        [JsonProperty("unitname")]    public string Unitname   { get; set; }
    }

    // Objectives
    public class ObjectiveDto
    {
        [JsonProperty("id")]          public int    Id          { get; set; }
        [JsonProperty("unit_id")]     public int    UnitId      { get; set; }
        [JsonProperty("grade")]       public string Grade       { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("unitcode")]    public string Unitcode    { get; set; }
    }

    // Concerns
    public class ConcernDto
    {
        [JsonProperty("id")]      public int    Id      { get; set; }
        [JsonProperty("concern")] public string Concern { get; set; }
        public override string ToString() => Concern;
    }

    // Users
    public class UserAdminDto
    {
        [JsonProperty("id")]       public int    Id       { get; set; }
        [JsonProperty("fullname")] public string Fullname { get; set; }
        [JsonProperty("email")]    public string Email    { get; set; }
        [JsonProperty("role")]     public string Role     { get; set; }
    }

    // Enrollments
    public class EnrollmentDto
    {
        [JsonProperty("id")]         public int    Id        { get; set; }
        [JsonProperty("student_id")] public int    StudentId { get; set; }
        [JsonProperty("group_id")]   public int    GroupId   { get; set; }
        [JsonProperty("course_id")]  public int    CourseId  { get; set; }
        [JsonProperty("concern_id")] public int?   ConcernId { get; set; }
        [JsonProperty("firstname")]  public string Firstname { get; set; }
        [JsonProperty("lastname")]   public string Lastname  { get; set; }
        [JsonProperty("cisnumber")]  public string Cisnumber { get; set; }
        [JsonProperty("groupname")]  public string Groupname { get; set; }
        [JsonProperty("coursename")] public string Coursename{ get; set; }
        [JsonProperty("concern")]    public string Concern   { get; set; }
        public override string ToString() => $"{Lastname}, {Firstname} ({Cisnumber})";
    }

    // Unit as returned by /units/index.php (includes section_type and course grouping fields)
    public class UnitForObjectivesDto
    {
        [JsonProperty("id")]           public int    Id          { get; set; }
        [JsonProperty("unitcode")]     public string Unitcode    { get; set; }
        [JsonProperty("unitname")]     public string Unitname    { get; set; }
        [JsonProperty("unitref")]      public string Unitref     { get; set; }
        [JsonProperty("credits")]      public int    Credits     { get; set; }
        [JsonProperty("glh")]          public int    Glh         { get; set; }
        [JsonProperty("is_external")]  public int    IsExternal  { get; set; }
        [JsonProperty("section_type")] public string SectionType { get; set; }
        [JsonProperty("year_taken")]   public int?   YearTaken   { get; set; }
        [JsonProperty("courses")]      public string Courses     { get; set; }
        [JsonProperty("course_ids")]   public string CourseIds   { get; set; }
        public override string ToString() => $"{Unitcode} — {Unitname}";
    }

    // Section (one item from /criteria/index.php?unit_id=X)
    public class SectionDto
    {
        [JsonProperty("section_id")]    public int                SectionId    { get; set; }
        [JsonProperty("section_label")] public string             SectionLabel { get; set; }
        [JsonProperty("section_title")] public string             SectionTitle { get; set; }
        [JsonProperty("sort_order")]    public int                SortOrder    { get; set; }
        [JsonProperty("criteria")]      public List<CriterionDto> Criteria     { get; set; }
    }

    // Criterion within a section
    public class CriterionDto
    {
        [JsonProperty("id")]          public int    Id          { get; set; }
        [JsonProperty("section_id")]  public int    SectionId   { get; set; }
        [JsonProperty("code")]        public string Code        { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("sort_order")]  public int    SortOrder   { get; set; }
    }

    // Assessment defs (admin) — mirrors tblassessment_def
    public class AssessmentDefAdminDto
    {
        [JsonProperty("id")]         public int    Id        { get; set; }
        [JsonProperty("unit_id")]    public int    UnitId    { get; set; }
        [JsonProperty("part_name")]  public string PartName  { get; set; }
        [JsonProperty("sort_order")] public int    SortOrder { get; set; }
    }

    // Result of POST /students/import.php
    public class StudentImportResultDto
    {
        [JsonProperty("imported")] public int                        Imported { get; set; }
        [JsonProperty("updated")]  public int                        Updated  { get; set; }
        [JsonProperty("errors")]   public List<StudentImportErrorDto> Errors  { get; set; }
    }

    public class StudentImportErrorDto
    {
        [JsonProperty("row")]     public int    Row     { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }
}
