/**
 * ReportBuilder — generates HTML strings for each report type.
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
            body { font-family: 'Trebuchet MS', sans-serif; font-size: 11px; margin: 20px; color: #222; }
            h1   { color: #00467f; font-size: 16px; }
            h2   { font-size: 13px; margin-bottom: 4px; }
            table { border-collapse: collapse; width: 100%; margin-top: 12px; font-size: 11px; }
            th   { background: #00467f; color: #fff; padding: 5px 8px; text-align: left; }
            td   { padding: 4px 8px; border-bottom: 1px solid #ddd; }
            tr.at-risk td { background: #fff0f0; }
            tr:nth-child(even) td { background: #f4f8fc; }
            .grade-D  { color: #006600; font-weight: bold; }
            .grade-M  { color: #004088; font-weight: bold; }
            .grade-P  { color: #888800; }
            .grade-U  { color: #cc0000; font-weight: bold; }
            .grade-NS { color: #aaa; }
            @media print { body { margin: 8mm; } }
        ";

        public static string Build(string reportName, CourseDto course, GroupDto group, GridDataDto data)
        {
            switch (reportName)
            {
                case "At-Risk Summary":       return AtRiskReport(course, group, data);
                case "Grade Distribution":    return GradeDistReport(course, group, data);
                case "Unit Performance":      return UnitPerfReport(course, group, data);
                case "Assessment Progress":   return AssessmentProgressReport(course, group, data);
                case "Outstanding Work":      return OutstandingWorkReport(course, group, data);
                default:                      return AtRiskReport(course, group, data);
            }
        }

        // ----------------------------------------------------------------
        // Report: At-Risk Summary
        // ----------------------------------------------------------------

        private static string AtRiskReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb = new StringBuilder();
            sb.Append(Head($"At-Risk Summary — {group.Groupname}", course, group));

            sb.Append("<table><thead><tr>");
            sb.Append("<th>CIS No</th><th>Name</th><th>NPs</th><th>M/D</th><th>At Risk Units</th><th>Overall</th><th>Concern</th>");
            sb.Append("</tr></thead><tbody>");

            bool isNcfe = course?.QualType == "ncfe";

            foreach (var s in data.Students)
            {
                var ar = AtRiskCalc.Calc(data.Units, s.Results, course?.QualType);
                string overall = CalcOverallDisplay(s, data.Units, course);
                string cls    = ar.IsAtRisk ? " class=\"at-risk\"" : "";
                sb.Append($"<tr{cls}>");
                sb.Append($"<td>{s.Cisnumber}</td>");
                sb.Append($"<td>{s.Firstname} {s.Lastname}</td>");
                sb.Append($"<td>{ar.NpsToCompensate}</td>");
                sb.Append($"<td>{ar.MeritsOrDist}</td>");
                sb.Append($"<td>{ar.AtRiskUnits}</td>");
                sb.Append($"<td>{overall}</td>");
                sb.Append($"<td>{s.Concern ?? "None"}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            int atRiskCount = data.Students.Count(s => AtRiskCalc.Calc(data.Units, s.Results, course?.QualType).IsAtRisk);
            sb.Append($"<p><strong>{atRiskCount}</strong> of {data.Students.Count} students at risk.</p>");
            sb.Append(Foot());
            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Report: Grade Distribution
        // ----------------------------------------------------------------

        private static string GradeDistReport(CourseDto course, GroupDto group, GridDataDto data)
        {
            var sb = new StringBuilder();
            sb.Append(Head($"Grade Distribution — {group.Groupname}", course, group));

            var grades = new[] { "D*", "D", "M", "P", "NP", "U", "NS", "OU" };
            var counts = grades.ToDictionary(g => g, g => 0);

            foreach (var s in data.Students)
            {
                string overall = CalcOverallDisplay(s, data.Units, course);
                // Simplified for distribution
                foreach (var g in grades)
                    if (overall.StartsWith(g)) { counts[g]++; break; }
            }

            sb.Append("<table><thead><tr><th>Grade</th><th>Count</th><th>%</th></tr></thead><tbody>");
            int total = data.Students.Count;
            foreach (var g in grades)
            {
                if (counts[g] == 0) continue;
                double pct = total > 0 ? Math.Round(100.0 * counts[g] / total, 1) : 0;
                string cls = $"grade-{g.Replace("*","")}";
                sb.Append($"<tr><td class=\"{cls}\">{g}</td><td>{counts[g]}</td><td>{pct}%</td></tr>");
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
            sb.Append(Head($"Unit Performance — {group.Groupname}", course, group));
            sb.Append("<table><thead><tr><th>Unit</th><th>D/D*</th><th>M</th><th>P</th><th>NP</th><th>U</th><th>NS</th></tr></thead><tbody>");

            foreach (var u in data.Units)
            {
                var gc = new Dictionary<string, int> { {"D",0},{"D*",0},{"M",0},{"P",0},{"NP",0},{"U",0},{"NS",0} };
                foreach (var s in data.Students)
                {
                    string g = GradeCalc.GetGrade(s.Results, u.Id);
                    if (gc.ContainsKey(g)) gc[g]++;
                    else gc["NS"]++;
                }
                sb.Append($"<tr><td>{u.Unitcode}</td><td>{gc["D"]+gc["D*"]}</td><td>{gc["M"]}</td><td>{gc["P"]}</td><td>{gc["NP"]}</td><td>{gc["U"]}</td><td>{gc["NS"]}</td></tr>");
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
            var sb = new StringBuilder();
            sb.Append(Head($"Assessment Progress — {group.Groupname}", course, group));

            var unitsWithAssess = data.Units.Where(u => u.AssessmentDefs?.Count > 0).ToList();
            if (!unitsWithAssess.Any())
            {
                sb.Append("<p>No assessment parts configured for this group's units.</p>");
                sb.Append(Foot()); return sb.ToString();
            }

            foreach (var u in unitsWithAssess)
            {
                sb.Append($"<h2>{u.Unitcode} — {u.Unitname}</h2>");
                sb.Append("<table><thead><tr><th>Student</th>");
                foreach (var d in u.AssessmentDefs) sb.Append($"<th>{d.Title}</th>");
                sb.Append("</tr></thead><tbody>");

                foreach (var s in data.Students)
                {
                    sb.Append($"<tr><td>{s.Firstname} {s.Lastname}</td>");
                    foreach (var d in u.AssessmentDefs)
                    {
                        string status = "Not Set";
                        if (s.Assessments != null && s.Assessments.TryGetValue(d.Id.ToString(), out var rec))
                            status = rec.Status ?? "NOT_SET";
                        status = status.Replace("_", " ").Replace("HANDED IN 1", "Handed In")
                                       .Replace("HANDED IN 2", "Resub");
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
            sb.Append(Head($"Outstanding Work — {group.Groupname}", course, group));
            sb.Append("<table><thead><tr><th>Student</th><th>Outstanding Units</th></tr></thead><tbody>");

            foreach (var s in data.Students)
            {
                var outstanding = data.Units
                    .Where(u => GradeCalc.GetGrade(s.Results, u.Id) == "NS")
                    .Select(u => u.Unitcode)
                    .ToList();

                if (!outstanding.Any()) continue;
                sb.Append($"<tr><td>{s.Firstname} {s.Lastname}</td><td>{string.Join(", ", outstanding)}</td></tr>");
            }

            sb.Append("</tbody></table>");
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

            if (isNcfe)  return GradeCalc.CalcNcfeGrade(units, s.Results);
            if (isBtec)
            {
                int pts = GradeCalc.CalcBtecTotalPoints(units, s.Results, s.RawMarks);
                string gs = GradeCalc.CalcBtecGradeString(pts);
                string simp = GradeCalc.BtecSimplified.TryGetValue(gs, out string sv) ? sv : "U";
                return $"{simp} ({gs})";
            }
            return GradeCalc.CalcOverallGrade(units, s.Results);
        }

        private static string Head(string title, CourseDto course, GroupDto group)
        {
            string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<title>{title}</title>
<style>{Css}</style></head><body>
<h1>{title}</h1>
<p><strong>Course:</strong> {course?.Coursename} &nbsp;&nbsp; <strong>Group:</strong> {group?.Groupname} &nbsp;&nbsp; <strong>Generated:</strong> {date}</p>";
        }

        private static string Foot() =>
            "<p style='margin-top:20px;font-size:10px;color:#888;'>© 2026 Exeter College — Creative Commons NC-BY-SA 4.0</p></body></html>";
    }
}
