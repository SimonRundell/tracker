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
        [JsonProperty("id")]        public int    Id        { get; set; }
        [JsonProperty("cisnumber")] public string Cisnumber { get; set; }
        [JsonProperty("firstname")] public string Firstname { get; set; }
        [JsonProperty("lastname")]  public string Lastname  { get; set; }
        [JsonProperty("email")]     public string Email     { get; set; }
        [JsonProperty("concern")]   public string Concern   { get; set; }
        [JsonProperty("notes")]     public string Notes     { get; set; }
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
        [JsonProperty("id")]    public int    Id    { get; set; }
        [JsonProperty("label")] public string Label { get; set; }
        [JsonProperty("color")] public string Color { get; set; }
        public override string ToString() => Label;
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
        [JsonProperty("firstname")]  public string Firstname { get; set; }
        [JsonProperty("lastname")]   public string Lastname  { get; set; }
        [JsonProperty("cisnumber")]  public string Cisnumber { get; set; }
    }

    // Assessment defs (admin)
    public class AssessmentDefAdminDto
    {
        [JsonProperty("id")]          public int    Id          { get; set; }
        [JsonProperty("unit_id")]     public int    UnitId      { get; set; }
        [JsonProperty("title")]       public string Title       { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("max_mark")]    public int?   MaxMark     { get; set; }
        [JsonProperty("unitcode")]    public string Unitcode    { get; set; }
    }
}
