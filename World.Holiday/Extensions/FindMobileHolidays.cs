﻿using System;
using System.Collections.Generic;
using World.Holidays.Entities;
using World.Holidays.Enum;
using static World.Holidays.Enum.Culture;

namespace World.Holidays.Extensions
{
    internal static class FindMobileHolidays
    {
        /// <summary>
        /// The days on week
        /// </summary>
        private static readonly int DaysOnWeek = 7;

        private static DateTime EasterDay;

        /// <summary>
        /// Determines whether [is mobile holiday] [the specified culture].
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static IEnumerable<Holiday> GetMobileHoliday(long ticks, ECulture culture)
        {
            var holidays = new List<Holiday>();
            var date = new DateTime(ticks);
            var month = date.Month;
            var year = date.Year;

            var parentDate = GetMothersDay(ticks, year, month, culture) ?? GetFathersDay(ticks, year, month, culture);

            if (parentDate != null)
            {
                holidays.Add(parentDate);
            }

            foreach (var holiday in Holidays(date, culture))
            {
                if (((culture & holiday.Culture) == culture) && holiday.Ticks == ticks)
                {
                    holidays.Add(holiday);
                }
            }

            return holidays;
        }

        /// <summary>
        /// Calculates the mobile holidays.
        /// </summary>
        /// <param name="date">The date.</param>
        private static void CalculateEasterDay(DateTime date)
        {
            var year = date.Year;
            var c = year / 100;
            var n = year - (19 * (year / 19));
            var k = (c - 17) / 25;
            var i = c - c / 4 - ((c - k) / 3) + (19 * n) + 15;
            i = i - (30 * (i / 30));
            i = i - ((i / 28) * (1 - (i / 28)) * (29 / (i + 1)) * ((21 - n) / 11));
            var j = year + year / 4 + i + 2 - c + c / 4;
            j = j - ((7 * (j / 7)));
            var l = i - j;
            var month = 3 + ((l + 40) / 44);
            var day = l + 28 - (31 * (month / 4));

            EasterDay = new DateTime(year, month, day);
        }

        /// <summary>
        /// Gets the days left to.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="sundaysUntilHoliday">The sundays until holiday.</param>
        /// <returns></returns>
        private static DateTime? GetDaysLeftTo(long ticks, int year, int month, int sundaysUntilHoliday)
        {
            var firstDay = new DateTime(year, month, 1);

            var dayOfWeek = firstDay.DayOfWeek;

            var daysLeft = 0;

            if (dayOfWeek != DayOfWeek.Sunday)
            {
                daysLeft = (DaysOnWeek * sundaysUntilHoliday) - (int)dayOfWeek;
            }

            var parentDate = firstDay.AddDays(daysLeft).Ticks;

            if (ticks == parentDate)
            {
                return new DateTime(parentDate);
            }

            return null;
        }

        /// <summary>
        /// Gets the fathers day.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        private static Holiday GetFathersDay(long ticks, int year, int month, ECulture culture)
        {
            if ((culture & (EnCulture | ECulture.ptBR)) != culture)
            {
                return null;
            }
            if ((culture & ECulture.ptBR) == culture && month != 8)
            {
                return null;
            }

            if ((culture & EnCulture) == culture && month != 6)
            {
                return null;
            }

            var holiday = GetParentDay(ticks, year, month, culture, EHolidayName.FatherDay, false);

            return holiday;
        }

        /// <summary>
        /// Gets the mothers day.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        private static Holiday GetMothersDay(long ticks, int year, int month, ECulture culture)
        {
            if (month != 5)
            {
                return null;
            }

            var holiday = GetParentDay(ticks, year, month, culture, EHolidayName.MotherDay, true);

            return holiday;
        }

        /// <summary>
        /// Gets the parent day.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="holidayName">Name of the holiday.</param>
        /// <param name="isNational">if set to <c>true</c> [is national].</param>
        /// <returns></returns>
        private static Holiday GetParentDay(long ticks, int year, int month, ECulture culture, EHolidayName holidayName, bool isNational)
        {
            var sundays = GetTotalSunday(holidayName, culture);

            var parentDay = GetDaysLeftTo(ticks, year, month, sundays);

            if (!parentDay.HasValue)
            {
                return null;
            }

            return new Holiday(parentDay.Value, isNational, holidayName.GetDescription(culture), culture);
        }

        /// <summary>
        /// Gets the total sunday.
        /// </summary>
        /// <param name="holidayName">Name of the parent date.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        private static int GetTotalSunday(EHolidayName holidayName, ECulture culture)
        {
            var sundaysInMonth = 2;

            switch (culture)
            {
                case ECulture.enCA:
                case ECulture.enUS:
                    if (holidayName == EHolidayName.FatherDay)
                    {
                        sundaysInMonth = 3;
                    }
                    break;

                case ECulture.esES:
                case ECulture.ptPT:
                    if (holidayName == EHolidayName.MotherDay)
                    {
                        sundaysInMonth = 1;
                    }
                    break;
            }
            return sundaysInMonth;
        }

        /// <summary>
        /// The holidays
        /// </summary>
        private static Holiday[] Holidays(DateTime dateTime, ECulture culture)
        {
            CalculateEasterDay(dateTime);

            return new[]
            {
                new Holiday(EasterDay.AddDays(-3), false, EHolidayName.HolyThursday.GetDescription(culture), ECulture.esES),
                new Holiday(EasterDay.AddDays(-2), true, EHolidayName.GoodFriday.GetDescription(culture), EasterMonday | ECulture.ptBR),
                new Holiday(EasterDay.AddDays(-46), true, EHolidayName.AshWednesday.GetDescription(culture), ECulture.ptBR),
                new Holiday(EasterDay.AddDays(-47), true, EHolidayName.MardiGrass.GetDescription(culture), ECulture.ptBR),
                new Holiday(EasterDay.AddDays(-48), culture != ECulture.ptPT, EHolidayName.MardiGrass.GetDescription(culture), PtCulture),
                new Holiday(EasterDay.AddDays(60), false, EHolidayName.CorpusChristi.GetDescription(culture), PtCulture | ECulture.esES),
                new Holiday(EasterDay.AddDays(39), false, EHolidayName.AscensionThursday.GetDescription(culture), ECulture.ptPT),
                new Holiday(EasterDay.AddDays(0), true, EHolidayName.EasterDay.GetDescription(culture), PtCulture | ECulture.esES),
                new Holiday(EasterDay.AddDays(1), false, EHolidayName.EasterMonday.GetDescription(culture), EasterMonday)
            };
        }
    }
}