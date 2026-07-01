/**
 * ReportBuilder — generates self-contained HTML strings for each report type.
 *
 * HTML is loaded into the WinForms WebBrowser control for display and printing.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AtRiskTracker.Models;
using AtRiskTracker.Utils;

namespace AtRiskTracker.Reports
{
    public static class ReportBuilder
    {
        private const string Css = @"
            body   { font-family: 'Trebuchet MS', sans-serif; font-size: 11px; margin: 20px; color: #222; }
            h1     { color: #00467f; font-size: 16px; margin-bottom: 4px; }
            h2     { font-size: 13px; margin: 16px 0 4px; color: #333; }
            .meta  { color: #555; font-size: 11px; margin-bottom: 12px; }
            table  { border-collapse: collapse; width: 100%; margin-top: 10px; font-size: 11px; }
            th     { background: #00467f; color: #fff; padding: 5px 8px; text-align: left; white-space: nowrap; }
            td     { padding: 4px 8px; border-bottom: 1px solid #ddd; vertical-align: middle; }
            tr.at-risk td          { background: #fff0f0; }
            tr:nth-child(even) td  { background: #f4f8fc; }
            tr.at-risk:nth-child(even) td { background: #ffe8e8; }
            .badge { display: inline-block; padding: 1px 6px; border-radius: 3px;
                     font-weight: bold; font-size: 10px; white-space: nowrap; }
            .badge-D  { background: #1b5e20; color: #fff; }
            .badge-Ds { background: #1b5e20; color: #fff; }
            .badge-M  { background: #1565c0; color: #fff; }
            .badge-P  { background: #827717; color: #fff; }
            .badge-NP { background: #e65100; color: #fff; }
            .badge-U  { background: #b71c1c; color: #fff; }
            .badge-NS { background: #9e9e9e; color: #fff; }
            .badge-OU { background: #757575; color: #fff; }
            .badge-A  { background: #1b5e20; color: #fff; }
            .badge-NA { background: #b71c1c; color: #fff; }
            .bar-wrap { background: #e0e0e0; border-radius: 2px; height: 10px; min-width: 80px; }
            .bar-fill { background: #00467f; height: 10px; border-radius: 2px; }
            .pct-high { color: #1b5e20; font-weight: bold; }
            .pct-mid  { color: #e65100; font-weight: bold; }
            .pct-low  { color: #b71c1c; font-weight: bold; }
            .foot     { margin-top: 20px; font-size: 10px; color: #888; }
            .summary  { background: #f0f5fb; border: 1px solid #cce; padding: 10px 14px;
                        border-radius: 4px; margin-bottom: 14px; }
            .arrow    { color: #555; }
            @media print { body { margin: 8mm; } }
        ";

        // ----------------------------------------------------------------
        // Cohort-based reports (data from results/index.php)
        // ----------------------------------------------------------------

        public static string Build(string reportName, CourseDto course, GroupDto group, GridDataDto data)
        {
            switch (reportName)
            {
                case "At-Risk Summary":     return AtRiskReport(course, group, data);
                case "Grade Distribution":  return GradeDistReport(course, group, data);
                case "Unit Performance":    return UnitPerfReport(course, group, data);
                case "Assessment Progress": return AssessmentProgressReport(course, group, data);
                case "Outstanding Work":    return OutstandingWorkReport(course, group, data);
                default:                   return AtRiskReport(course, group, data);
            }
        }

        // ----------------------------------------------------------------
        // Report: At-Risk Summary
        // ----------------------------------------------------------------

        private static string AtRiskReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb = new StringBuilder();
            sb.Append(Head($"At-Risk Summary — {group?.Groupname}", course, group));

            sb.Append("<table><thead><tr>");
            sb.Append("<th>CIS No</th><th>Name</th><th>Overall</th><th>NPs</th><th>M/D</th><th>At-Risk Units</th><th>Concern</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var s in data.Students)
            {
                var ar     = AtRiskCalc.Calc(data.Units, s.Results, course?.QualType);
                string ovr = CalcOverallDisplay(s, data.Units, course);
                string cls = ar.IsAtRisk ? " class=\"at-risk\"" : "";
                sb.Append($"<tr{cls}>");
                sb.Append($"<td>{Enc(s.Cisnumber)}</td>");
                sb.Append($"<td>{Enc(s.Firstname)} {Enc(s.Lastname)}</td>");
                sb.Append($"<td>{GradeBadge(ovr)}</td>");
                sb.Append($"<td>{ar.NpsToCompensate}</td>");
                sb.Append($"<td>{ar.MeritsOrDist}</td>");
                sb.Append($"<td>{ar.AtRiskUnits}</td>");
                sb.Append($"<td>{Enc(s.Concern ?? "None")}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            int atRiskCount = data.Students.Count(s =>
                AtRiskCalc.Calc(data.Units, s.Results, course?.QualType).IsAtRisk);
            sb.Append($"<p><strong>{atRiskCount}</strong> of {data.Students.Count} students identified as at risk.</p>");
            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Grade Distribution
        // ----------------------------------------------------------------

        private static string GradeDistReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb = new StringBuilder();
            sb.Append(Head($"Grade Distribution — {group?.Groupname}", course, group));

            bool isNcfe = course?.QualType == "ncfe";
            string[] grades = isNcfe
                ? new[] { "Achieved", "Not Achieved" }
                : new[] { "D*", "D", "M", "P", "NP", "U", "NS" };

            var counts = grades.ToDictionary(g => g, _ => 0);
            int total  = data.Students.Count;

            foreach (var s in data.Students)
            {
                string ovr = CalcOverallDisplay(s, data.Units, course);
                foreach (string g in grades)
                    if (ovr.StartsWith(g, StringComparison.OrdinalIgnoreCase)) { counts[g]++; break; }
            }

            sb.Append("<table><thead><tr><th>Grade</th><th>Count</th><th>%</th><th style='min-width:120px'>Distribution</th></tr></thead><tbody>");
            foreach (string g in grades)
            {
                if (counts[g] == 0) continue;
                double pct  = total > 0 ? Math.Round(100.0 * counts[g] / total, 1) : 0;
                int    bar  = (int)(pct);
                sb.Append($"<tr><td>{GradeBadge(g)}</td><td>{counts[g]}</td><td>{pct}%</td>");
                sb.Append($"<td><div class='bar-wrap'><div class='bar-fill' style='width:{bar}%'></div></div></td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Unit Performance
        // ----------------------------------------------------------------

        private static string UnitPerfReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb = new StringBuilder();
            sb.Append(Head($"Unit Performance — {group?.Groupname}", course, group));
            sb.Append("<table><thead><tr>");
            sb.Append("<th>Code</th><th>Unit Name</th><th>NS</th><th>OU</th><th>U</th><th>NP</th><th>P</th><th>M</th><th>D/D*</th><th>Total</th><th>% Achieved</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var u in data.Units)
            {
                var gc = new Dictionary<string, int>
                    { {"D*",0},{"D",0},{"M",0},{"P",0},{"NP",0},{"U",0},{"NS",0},{"OU",0} };

                foreach (var s in data.Students)
                {
                    string g = GradeCalc.GetGrade(s.Results, u.Id);
                    if (gc.ContainsKey(g)) gc[g]++;
                    else gc["NS"]++;
                }

                int achieved = gc["P"] + gc["M"] + gc["D"] + gc["D*"];
                int ttl      = data.Students.Count;
                double pct   = ttl > 0 ? Math.Round(100.0 * achieved / ttl) : 0;
                string pctCls = pct >= 70 ? "pct-high" : pct >= 50 ? "pct-mid" : "pct-low";

                sb.Append($"<tr><td>{Enc(u.Unitcode)}</td><td>{Enc(u.Unitname)}</td>");
                sb.Append($"<td>{Dash(gc["NS"])}</td><td>{Dash(gc["OU"])}</td><td>{Dash(gc["U"])}</td>");
                sb.Append($"<td>{Dash(gc["NP"])}</td><td>{Dash(gc["P"])}</td><td>{Dash(gc["M"])}</td>");
                sb.Append($"<td>{Dash(gc["D"]+gc["D*"])}</td><td>{ttl}</td>");
                sb.Append($"<td class='{pctCls}'>{pct}%</td></tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Assessment Progress
        // ----------------------------------------------------------------

        private static string AssessmentProgressReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb          = new StringBuilder();
            sb.Append(Head($"Assessment Progress — {group?.Groupname}", course, group));

            var unitsWithDefs = data.Units.Where(u => u.AssessmentDefs?.Count > 0).ToList();
            if (!unitsWithDefs.Any())
            {
                sb.Append("<p>No assessment parts configured for this group's units.</p>");
                sb.Append(Foot()); return sb.ToString();
            }

            foreach (var u in unitsWithDefs)
            {
                sb.Append($"<h2>{Enc(u.Unitcode)} — {Enc(u.Unitname)}</h2>");
                sb.Append("<table><thead><tr><th>Student</th>");
                foreach (var d in u.AssessmentDefs) sb.Append($"<th>{Enc(d.Title)}</th>");
                sb.Append("</tr></thead><tbody>");

                foreach (var s in data.Students)
                {
                    sb.Append($"<tr><td>{Enc(s.Firstname)} {Enc(s.Lastname)}</td>");
                    foreach (var d in u.AssessmentDefs)
                    {
                        string status = "Not Set";
                        if (s.Assessments != null && s.Assessments.TryGetValue(d.Id.ToString(), out var rec))
                            status = rec.Status ?? "NOT_SET";
                        status = NiceStatus(status);
                        sb.Append($"<td>{status}</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
            }

            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Outstanding Work
        // ----------------------------------------------------------------

        private static string OutstandingWorkReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb = new StringBuilder();
            sb.Append(Head($"Outstanding Work — {group?.Groupname}", course, group));
            sb.Append("<table><thead><tr><th>Student</th><th>CIS No</th><th>Outstanding Units</th></tr></thead><tbody>");

            bool any = false;
            foreach (var s in data.Students)
            {
                var outstanding = data.Units
                    .Where(u => GradeCalc.GetGrade(s.Results, u.Id) == "NS")
                    .Select(u => Enc(u.Unitcode))
                    .ToList();
                if (!outstanding.Any()) continue;
                any = true;
                sb.Append($"<tr><td>{Enc(s.Firstname)} {Enc(s.Lastname)}</td><td>{Enc(s.Cisnumber)}</td>");
                sb.Append($"<td>{string.Join(", ", outstanding)}</td></tr>");
            }

            if (!any)
                sb.Append("<tr><td colspan='3' style='color:#555'>No students with outstanding work.</td></tr>");

            sb.Append("</tbody></table>");
            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Grade Audit Trail
        // ----------------------------------------------------------------

        public static string BuildGradeAudit(List<GradeAuditRowDto> rows, int count)
        {
            var sb = new StringBuilder();
            string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            sb.Append($"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Grade Audit Trail</title><style>{Css}</style></head><body>");
            sb.Append($"<h1>Grade Audit Trail</h1>");
            sb.Append($"<p class='meta'><strong>Generated:</strong> {date} &nbsp;&mdash;&nbsp; {count} records");
            if (count >= 500) sb.Append(" &nbsp;<em>(maximum 500 reached — refine filters)</em>");
            sb.Append("</p>");

            if (rows.Count == 0)
            {
                sb.Append("<p>No grade changes match the selected filters.</p>");
            }
            else
            {
                sb.Append("<table><thead><tr>");
                sb.Append("<th>Date</th><th>Time</th><th>CIS No</th><th>Student</th>");
                sb.Append("<th>Course</th><th>Group</th><th>Unit</th><th>Grade</th><th>Changed By</th>");
                sb.Append("</tr></thead><tbody>");

                foreach (var r in rows)
                {
                    string[] parts = (r.UpdatedAt ?? "").Split(' ');
                    string d = parts.Length > 0 ? parts[0] : "";
                    string t = parts.Length > 1 ? parts[1].Substring(0, Math.Min(5, parts[1].Length)) : "";
                    string mark = r.RawMark.HasValue ? $" ({r.RawMark})" : "";
                    sb.Append($"<tr><td>{Enc(d)}</td><td>{Enc(t)}</td>");
                    sb.Append($"<td>{Enc(r.Cisnumber)}</td>");
                    sb.Append($"<td>{Enc(r.Lastname)}, {Enc(r.Firstname)}</td>");
                    sb.Append($"<td>{Enc(r.Coursename)}</td><td>{Enc(r.Groupname)}</td>");
                    sb.Append($"<td>{Enc(r.Unitcode)} — {Enc(r.Unitname)}</td>");
                    sb.Append($"<td>{GradeBadge(r.Result)}{Enc(mark)}</td>");
                    sb.Append($"<td>{Enc(r.ChangedBy)}</td></tr>");
                }
                sb.Append("</tbody></table>");
            }

            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Student Grade History
        // ----------------------------------------------------------------

        public static string BuildStudentAudit(StudentAuditSummaryDto student,
                                               List<StudentAuditRowDto> rows, int count)
        {
            var sb = new StringBuilder();
            string name  = $"{student?.Firstname} {student?.Lastname}";
            string title = $"Grade History — {name}";
            string date  = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            sb.Append($"<!DOCTYPE html><html><head><meta charset='utf-8'><title>{Enc(title)}</title><style>{Css}</style></head><body>");
            sb.Append($"<h1>{Enc(title)}</h1>");
            sb.Append("<div class='summary'>");
            sb.Append($"<strong>CIS No:</strong> {Enc(student?.Cisnumber)} &nbsp;&nbsp; ");
            sb.Append($"<strong>Course:</strong> {Enc(student?.Coursename)} &nbsp;&nbsp; ");
            sb.Append($"<strong>Group:</strong> {Enc(student?.Groupname)}");
            sb.Append("</div>");
            sb.Append($"<p class='meta'><strong>Generated:</strong> {date} &nbsp;&mdash;&nbsp; {count} records</p>");

            if (rows.Count == 0)
            {
                sb.Append("<p>No grade change history found for this student.</p>");
            }
            else
            {
                sb.Append("<table><thead><tr>");
                sb.Append("<th>Date</th><th>Time</th><th>Unit</th><th>Previous</th><th></th><th>New Grade</th><th>Changed By</th>");
                sb.Append("</tr></thead><tbody>");

                foreach (var r in rows)
                {
                    string[] parts = (r.ChangedAt ?? "").Split(' ');
                    string d = parts.Length > 0 ? parts[0] : "";
                    string t = parts.Length > 1 ? parts[1].Substring(0, Math.Min(5, parts[1].Length)) : "";
                    string oldMark = r.OldRawMark.HasValue ? $" ({r.OldRawMark})" : "";
                    string newMark = r.NewRawMark.HasValue ? $" ({r.NewRawMark})" : "";
                    string prev    = string.IsNullOrEmpty(r.OldResult) ? "<span style='color:#aaa'>—</span>" : GradeBadge(r.OldResult) + Enc(oldMark);
                    string arrow   = string.IsNullOrEmpty(r.OldResult) ? "<span class='arrow'>—</span>" : "<span class='arrow'>&rarr;</span>";

                    sb.Append($"<tr><td>{Enc(d)}</td><td>{Enc(t)}</td>");
                    sb.Append($"<td>{Enc(r.Unitcode)} — {Enc(r.Unitname)}</td>");
                    sb.Append($"<td>{prev}</td><td>{arrow}</td>");
                    sb.Append($"<td>{GradeBadge(r.NewResult)}{Enc(newMark)}</td>");
                    sb.Append($"<td>{Enc(r.ChangedBy)}</td></tr>");
                }
                sb.Append("</tbody></table>");
            }

            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private static string CalcOverallDisplay(StudentDto s, List<UnitDto> units, CourseDto course)
        {
            bool isNcfe = course?.QualType == "ncfe";
            bool isBtec = course?.QualType?.StartsWith("btec") ?? false;

            if (isNcfe) return GradeCalc.CalcNcfeGrade(units, s.Results);
            if (isBtec)
            {
                int    pts  = GradeCalc.CalcBtecTotalPoints(units, s.Results, s.RawMarks);
                string gs   = GradeCalc.CalcBtecGradeString(pts);
                string simp = GradeCalc.BtecSimplified.TryGetValue(gs, out string sv) ? sv : "U";
                return $"{simp} ({gs})";
            }
            return GradeCalc.CalcOverallGrade(units, s.Results);
        }

        private static string GradeBadge(string grade)
        {
            if (string.IsNullOrEmpty(grade)) return "<span class='badge badge-NS'>NS</span>";
            string key = grade.Replace("*", "s").Replace(" ", "");
            // Shorten compound grades (MMM → M, DD → D, etc.) for the CSS class key
            if (key.Length > 2) key = key.Substring(0, 1);
            return $"<span class='badge badge-{Enc(key)}'>{Enc(grade)}</span>";
        }

        private static string NiceStatus(string s) =>
            s.Replace("_", " ").Replace("NOT SET", "Not Set")
             .Replace("HANDED IN 1", "Handed In").Replace("HANDED IN 2", "Resubmitted");

        private static string Dash(int n) => n == 0 ? "<span style='color:#bbb'>—</span>" : n.ToString();

        private static string Enc(string s) =>
            System.Net.WebUtility.HtmlEncode(s ?? "");

        private static string Head(string title, CourseDto course, GroupDto group)
        {
            string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<title>{Enc(title)}</title>
<style>{Css}</style></head><body>
<h1>{Enc(title)}</h1>
<p class='meta'><strong>Course:</strong> {Enc(course?.Coursename)} &nbsp;&nbsp;
<strong>Group:</strong> {Enc(group?.Groupname)} &nbsp;&nbsp;
<strong>Generated:</strong> {date}</p>";
        }

        private static string Foot() =>
            "<p class='foot'>© 2026 Exeter College — Creative Commons NC-BY-SA 4.0</p></body></html>";
    }
}
