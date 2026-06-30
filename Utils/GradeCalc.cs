/**
 * Grade calculation utilities — C# port of src/utils/gradeCalc.js.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using AtRiskTracker.Models;

namespace AtRiskTracker.Utils
{
    public static class GradeCalc
    {
        public static readonly Dictionary<string, int> GradePoints = new Dictionary<string, int>
        {
            { "NS", 0 }, { "OU", 0 }, { "U", 0 }, { "NP", 2 }, { "P", 4 }, { "M", 6 }, { "D", 8 }
        };

        public static readonly string[] GradeOptions = { "NS", "OU", "U", "NP", "P", "M", "D" };

        // BTec internal points by GLH
        private static readonly Dictionary<int, Dictionary<string, int>> BtecInternalPoints =
            new Dictionary<int, Dictionary<string, int>>
        {
            { 60,  new Dictionary<string,int>{ {"U",0},{"NP",0},{"P",6}, {"M",10},{"D",16} } },
            { 90,  new Dictionary<string,int>{ {"U",0},{"NP",0},{"P",9}, {"M",15},{"D",24} } },
            { 120, new Dictionary<string,int>{ {"U",0},{"NP",0},{"P",12},{"M",20},{"D",32} } },
            { 150, new Dictionary<string,int>{ {"U",0},{"NP",0},{"P",15},{"M",25},{"D",40} } },
            { 180, new Dictionary<string,int>{ {"U",0},{"NP",0},{"P",18},{"M",30},{"D",48} } },
        };

        // External unit mark boundaries by GLH
        private static readonly Dictionary<int, Dictionary<string, int>> ExternalBoundaries =
            new Dictionary<int, Dictionary<string, int>>
        {
            { 90,  new Dictionary<string,int>{ {"U",0},{"NP",6}, {"P",9}, {"M",15},{"D",24} } },
            { 120, new Dictionary<string,int>{ {"U",0},{"NP",8}, {"P",12},{"M",20},{"D",32} } },
        };

        // BTec Extended Diploma grade thresholds
        public static readonly (int Min, string Grade)[] BtecExtDipGrades =
        {
            (270, "D*D*D*"), (252, "D*D*D"), (234, "D*DD"), (216, "DDD"),
            (196, "DDM"),    (176, "DMM"),    (156, "MMM"),  (140, "MMP"),
            (124, "MPP"),    (108, "PPP"),    (0,   "U"),
        };

        public static readonly Dictionary<string, string> BtecSimplified =
            new Dictionary<string, string>
        {
            { "U",      "U"  }, { "PPP", "P"  }, { "MPP", "P"  }, { "MMP", "P"  },
            { "MMM",    "M"  }, { "DMM", "M"  }, { "DDM", "M"  }, { "DDD", "D"  },
            { "D*DD",   "D"  }, { "D*D*D", "D" }, { "D*D*D*", "D*" },
        };

        // Compound grade thresholds (non-BTec Ext Dip)
        private static readonly (string Grade, double MinPpc)[] CompoundThresholds =
        {
            ("DDD", 8.0), ("DDM", 22.0/3), ("DMM", 20.0/3), ("MMM", 6.0),
            ("MMP", 16.0/3), ("MPP", 14.0/3), ("PPP", 4.0),
        };

        // ----------------------------------------------------------------
        // Generic (non-BTec Ext Dip)
        // ----------------------------------------------------------------

        public static int CalcTotalPoints(IEnumerable<UnitDto> units, Dictionary<string,string> results)
        {
            int total = 0;
            foreach (var u in units)
            {
                string grade = GetGrade(results, u.Id);
                if (GradePoints.TryGetValue(grade, out int pts)) total += pts * u.Credits;
            }
            return total;
        }

        public static string CalcOverallGrade(IEnumerable<UnitDto> units, Dictionary<string,string> results)
        {
            var graded = units.Where(u =>
            {
                string g = GetGrade(results, u.Id);
                return g != "NS" && g != "OU";
            }).ToList();

            if (!graded.Any()) return "-";

            int totalPts     = CalcTotalPoints(graded, results);
            int totalCredits = graded.Sum(u => u.Credits);
            if (totalCredits == 0) return "-";
            double ppc = (double)totalPts / totalCredits;

            foreach (var (grade, minPpc) in CompoundThresholds)
                if (ppc >= minPpc) return grade;
            return "U";
        }

        // ----------------------------------------------------------------
        // BTec Extended Diploma
        // ----------------------------------------------------------------

        public static int BtecInternalPts(string grade, int glh)
        {
            var table = BtecInternalPoints.ContainsKey(glh) ? BtecInternalPoints[glh] : BtecInternalPoints[90];
            return table.ContainsKey(grade) ? table[grade] : 0;
        }

        public static int CalcBtecTotalPoints(IEnumerable<UnitDto> units,
            Dictionary<string,string> results, Dictionary<string,int?> rawMarks)
        {
            int total = 0;
            foreach (var u in units)
            {
                string grade = GetGrade(results, u.Id);
                if (grade == "NS" || grade == "OU") continue;
                if (u.IsExternal != 0)
                {
                    int? raw = null;
                    if (rawMarks != null && rawMarks.TryGetValue(u.Id.ToString(), out int? r)) raw = r;
                    total += raw ?? 0;
                }
                else
                {
                    total += BtecInternalPts(grade, u.Glh);
                }
            }
            return total;
        }

        public static string CalcBtecGradeString(int totalPoints)
        {
            foreach (var (min, grade) in BtecExtDipGrades)
                if (totalPoints >= min) return grade;
            return "U";
        }

        public static string MarkToGrade(int mark, int glh)
        {
            var b = ExternalBoundaries.ContainsKey(glh) ? ExternalBoundaries[glh] : ExternalBoundaries[90];
            foreach (var g in new[] { "D", "M", "P", "NP", "U" })
                if (mark >= b[g]) return g;
            return "U";
        }

        // ----------------------------------------------------------------
        // NCFE
        // ----------------------------------------------------------------

        public static string CalcNcfeGrade(IEnumerable<UnitDto> units, Dictionary<string,string> results)
        {
            if (units == null || !units.Any()) return "Not Achieved";
            return units.All(u => GetGrade(results, u.Id) == "P") ? "Achieved" : "Not Achieved";
        }

        // ----------------------------------------------------------------
        // Helper
        // ----------------------------------------------------------------

        public static string GetGrade(Dictionary<string,string> results, int unitId)
        {
            if (results == null) return "NS";
            return results.TryGetValue(unitId.ToString(), out string g) ? g : "NS";
        }
    }
}
