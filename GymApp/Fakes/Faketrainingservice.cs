using GymApp.Models;
using GymApp.Services;
using System;
using System.Collections.Generic;

namespace GymApp.Test.Fakes
{

    public class FakeTrainingService : ITrainingService
    {
        public List<Training> TrainingsToReturn { get; set; } = new List<Training>();

        public List<Training> GetTrainingsInTheLastMonth(int trainerId) => TrainingsToReturn;

        public List<Training> GetTrainingsInTheLastMonth(Guid trainerId)
        {
            throw new NotImplementedException();
        }
    }
}
