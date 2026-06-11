using GymApp.Models;
using GymApp.Services;
using System;

namespace GymApp.Test.Fakes
{
    public class FakeTrainerPerformanceService : ITrainerPerformanceService
    {
        public PerformanceReport ReportToReturn { get; set; }

        public PerformanceReport GetTrainerPerformanceReport(int trainerId) => ReportToReturn;

        public PerformanceReport GetTrainerPerformanceReport(Guid trainerId)
        {
            throw new NotImplementedException();
        }
    }
}
