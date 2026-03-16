using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Users.Application.Validators
{
    public static class CountryValidator
    {
        private static readonly HashSet<string> CountryCodes =
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new RegionInfo(c.LCID))
                .Select(r => r.TwoLetterISORegionName)
                .Distinct()
                .ToHashSet();

        public static bool IsValid(string code)
        {
            return CountryCodes.Contains(code.ToUpper());
        }
    }

}
