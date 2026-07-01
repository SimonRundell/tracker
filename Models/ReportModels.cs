/**
 * DTOs for report-specific API responses (audit reports).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AtRiskTracker.Models
{
    public class GradeAuditRowDto
    {
        [JsonProperty("updated_at")]  public string UpdatedAt  { get; set; }
        [JsonProperty("cisnumber")]   public string Cisnumber  { get; set; }
        [JsonProperty("firstname")]   public string Firstname  { get; set; }
        [JsonProperty("lastname")]    public string Lastname   { get; set; }
        [JsonProperty("coursename")]  public string Coursename { get; set; }
        [JsonProperty("groupname")]   public string Groupname  { get; set; }
        [JsonProperty("unitcode")]    public string Unitcode   { get; set; }
        [JsonProperty("unitname")]    public string Unitname   { get; set; }
        [JsonProperty("result")]      public string Result     { get; set; }
        [JsonProperty("raw_mark")]    public int?   RawMark    { get; set; }
        [JsonProperty("changed_by")]  public string ChangedBy  { get; set; }
    }

    public class GradeAuditResponse
    {
        [JsonProperty("success")] public bool                   Success { get; set; }
        [JsonProperty("data")]    public List<GradeAuditRowDto> Data    { get; set; }
        [JsonProperty("count")]   public int                    Count   { get; set; }
    }

    public class StudentAuditRowDto
    {
        [JsonProperty("changed_at")]   public string ChangedAt  { get; set; }
        [JsonProperty("unitcode")]     public string Unitcode   { get; set; }
        [JsonProperty("unitname")]     public string Unitname   { get; set; }
        [JsonProperty("old_result")]   public string OldResult  { get; set; }
        [JsonProperty("old_raw_mark")] public int?   OldRawMark { get; set; }
        [JsonProperty("new_result")]   public string NewResult  { get; set; }
        [JsonProperty("new_raw_mark")] public int?   NewRawMark { get; set; }
        [JsonProperty("changed_by")]   public string ChangedBy  { get; set; }
    }

    public class StudentAuditSummaryDto
    {
        [JsonProperty("firstname")]  public string Firstname  { get; set; }
        [JsonProperty("lastname")]   public string Lastname   { get; set; }
        [JsonProperty("cisnumber")]  public string Cisnumber  { get; set; }
        [JsonProperty("coursename")] public string Coursename { get; set; }
        [JsonProperty("groupname")]  public string Groupname  { get; set; }
    }

    public class StudentAuditResponse
    {
        [JsonProperty("success")] public bool                    Success { get; set; }
        [JsonProperty("student")] public StudentAuditSummaryDto  Student { get; set; }
        [JsonProperty("data")]    public List<StudentAuditRowDto> Data   { get; set; }
        [JsonProperty("count")]   public int                     Count  { get; set; }
    }
}
