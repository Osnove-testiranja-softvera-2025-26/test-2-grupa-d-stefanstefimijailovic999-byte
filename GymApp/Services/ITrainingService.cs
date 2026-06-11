using GymApp.Models;
using NSubstitute.Core;
using System;
using System.Collections.Generic;

namespace GymApp.Services
{
    public interface ITrainingService
    {
        List<Training> GetTrainingsInTheLastMonth(Guid trainerId);
        void GetTrainingsInTheLastMonth(ConfiguredCall configuredCall);
    }
}
