﻿using CreateCalendar;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using CreateCalendar.ProcessXlCalendar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TestRosterToCal
{
    [TestClass]
    public class TestFromFileSystem
    {
        [TestMethod]
        public async Task PCCUFromFileSystem()
        {
            var fn = "PCCU SMO Roster.xlsx";
            var folder = "./TestFiles";
            var calPath = Directory.CreateDirectory(Path.Combine(folder, "Calendars")).FullName;
            var xlRosterSettings = new ExcelRosterSettings
            {
                DateCommentsCol = 3,
                SpecialShiftHeaders = new[] {
                        "2nd"
                    },
                IgnoreShifts = new[] { "RDO", "PH" },
                NonAvailableShifts = new[] { "Leave", "PDL" }
            };
            var xlDataReader = new XlDataReader(
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<XlDataReader>(),
                xlRosterSettings
            );
            EveryoneRoster everyoneRoster;
            using (var rosterFileStream = File.OpenRead(Path.Combine(folder, fn)))
            {
                everyoneRoster = xlDataReader.Process(rosterFileStream);
            }
            var eventSettings = new MockEventSettings {
                IcsFilename = "{0} PCCU SMO Roster.ics",
                FormatShift = "{0} PCCU",
                
                AppointmentKeyValues = new Dictionary<string, string>
                {
                    { "LOCATION", "SCUH" },
                    { "ORGANIZER;CN=Paula Lister", "mailto:Paula.Lister@health.qld.gov.au" }
                }
            };
            await ProcessAndOutput.ProcessCalendar(Guid.Empty,
                PerUserRosters.Create(everyoneRoster, xlRosterSettings),
                eventSettings,
                fileName => Task.FromResult <Stream> (fileName.StartsWith("McSharry")
                    ? File.OpenRead(Path.Combine(folder, fileName))
                    : null),
                async (filename, ms) =>
                {
                    var outPath = Path.Combine(calPath, filename);
                    Debug.WriteLine(outPath);
                    using (var icsFs = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        await ms.CopyToAsync(icsFs);
                    }
                });
        }

        [TestMethod]
        public async Task ICUFromFileSystem()
        {
            var fn = "ICU SMO Roster.xlsx";
            var folder = "./TestFiles";
            var calPath = Directory.CreateDirectory(Path.Combine(folder, "Calendars")).FullName;
            var xlRosterSettings = new ExcelRosterSettings
            {
                DateCommentsCol = 3,
                IgnoreShifts = new[] { "RDO", "SD" },
                NonAvailableShifts = new[] { "Leave", "PDL" }
            };
            var xlDataReader = new XlDataReader(
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<XlDataReader>(),
                xlRosterSettings
            );
            EveryoneRoster everyoneRoster;
            using (var rosterFileStream = File.OpenRead(Path.Combine(folder, fn)))
            {
                everyoneRoster = xlDataReader.Process(rosterFileStream);
            }
            var eventSettings = new MockEventSettings
            {
                IcsFilename = "{0} ICU SMO Roster.ics",
                FormatShift = "{0} SCUH ICU",
                AppointmentKeyValues = new Dictionary<string, string>
                {
                    { "LOCATION", "SCUH" },
                    { "ORGANIZER;CN=Alex Grosso", "mailto:Alex.Grosso@health.qld.gov.au" }
                }
            };
            await ProcessAndOutput.ProcessCalendar(Guid.Empty,
                PerUserRosters.Create(everyoneRoster, xlRosterSettings),
                eventSettings,
                fileName => Task.FromResult<Stream>(null),
                async (fileName, ms) =>
                {
                    var outPath = Path.Combine(calPath, fileName);
                    Debug.WriteLine(outPath);
                    using (var icsFs = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        await ms.CopyToAsync(icsFs);
                    }
                });
        }
    }

    internal class MockEventSettings : IEventSettings
    {
        public string IcsFilename { get; set; } 
        public string FormatShift { get; set; } 
        public Dictionary<string, string> AppointmentKeyValues { get; set; }
    }
}