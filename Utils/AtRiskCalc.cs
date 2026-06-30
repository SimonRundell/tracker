/**
 * At-risk flag calculations — C# port of src/utils/atRiskCalc.js.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using AtRiskTracker.Models;

namespace AtRiskTracker.Utils
{
    public struct AtRiskResult
    {
        public int  NpsToCompensate;
        public int  MeritsOrDist;
        public int  AtRiskUnits;
        public bool IsAtRisk;
    }

    public static class AtRiskCalc
    {
        private const int AtRiskThreshold = 2;

        /// <summary>
        /// Calculates the NP/M+D/at-risk counts for a student.
        /// NCFE courses flag at-risk if any unit is NS or U (threshold = 1).
        /// All others flag at AT_RISK_THRESHOLD (2) NS/U units or any U grade.
        /// </summary>
        public static AtRiskResult Calc(
            IEnumerable<UnitDto> units,
            Dictionary<string, string> results,
            string qualType = "other")
        {
            int  nps    = 0, merits = 0, atRiskCount = 0;
            bool hasU   = false;

            foreach (var unit in units)
            {
                string grade = GradeCalc.GetGrade(results, unit.Id);
                if (grade == "NP")               nps++;
                if (grade == "M" || grade == "D") merits++;
                if (grade == "U" || grade == "NS") atRiskCount++;
                if (grade == "U")                 hasU = true;
            }

            bool isAtRisk = qualType == "ncfe"
                ? atRiskCount > 0
                : hasU || atRiskCount >= AtRiskThreshold;

            return new AtRiskResult
            {
                NpsToCompensate = nps,
                MeritsOrDist    = merits,
                AtRiskUnits     = atRiskCount,
                IsAtRisk        = isAtRisk,
            };
        }
    }
}
