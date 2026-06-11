
using GymApp.Exceptions;
using GymApp.Models;
using GymApp.Test.Fakes;
using System.Collections.Generic;

namespace GymApp.Services
{
    public class GymService
    {
        private readonly IPaymentService _paymentService;
        private readonly ITrainingService _trainingService;
        private readonly ITrainerPerformanceService _trainerPerformanceService;
        private IPaymentService payment;
        private ITrainingService training;
        private ITrainerPerformanceService performance;

        public GymService(FakePaymentService payment, FakeTrainingService training, FakeTrainerPerformanceService performance)
        {
            Payment = payment;
            Training = training;
            Performance = performance;
        }

        public GymService(IPaymentService payment, ITrainingService training, ITrainerPerformanceService performance)
        {
            this.payment = payment;
            this.training = training;
            this.performance = performance;
        }

        public FakePaymentService Payment { get; }
        public FakeTrainingService Training { get; }
        public FakeTrainerPerformanceService Performance { get; }

        public void DoStaffBonusPaymentCalculation(Trainer trainer)
        {
            
            List<Training> trainings = _trainingService.GetTrainingsInTheLastMonth(trainer.Id);
            if(trainings.Count == 0)
            {
                throw new NoTrainingsInTheLastMonthException("Bonus payment cannot be calculated");
            }
            int numberOfGroupTrainigs = trainings.FindAll(x => x.Type.Equals(TrainingType.Group)).Count;
            PerformanceReport performanceReport = _trainerPerformanceService.GetTrainerPerformanceReport(trainer.Id);
            
            BonusPayment bonus = new BonusPayment();

            if (performanceReport.PerformanceRank.Equals(PerformanceRank.Second) && performanceReport.PercentOfTrainingsNotHeld < 15)
            {
                if (performanceReport.NumberOfFreeDaysLeft > 17)
                {
                    bonus.Amount = 160.0;
                }
                else
                {
                    bonus.Amount = 130.0;
                }
            }
            else if (performanceReport.PerformanceRank.Equals(PerformanceRank.First))
            {
                if (numberOfGroupTrainigs > 10 || performanceReport.PercentOfTrainingsNotHeld <= 8)
                {
                    bonus.Amount = 180.0;
                }
                else
                {
                    bonus.Amount = 150.0;
                }
            }
            else
            {
                bonus.Amount = 0;
            }

            _paymentService.UpdateTrainerBonusPayment(trainer.Id, bonus);
        }

        public MembershipType? GetMemberhipType(int numberOfMonths, bool groupTrainings, double monthlyPriceBudget, TrainingTime trainingTime)
        {
            if (trainingTime.Equals(TrainingTime.WholeDay))
            {
                if (numberOfMonths > 10 && monthlyPriceBudget > 25.0)
                {
                    if (groupTrainings)
                        return MembershipType.TypeA;
                    return MembershipType.TypeB;
                }
                return null;
            }
            else if (trainingTime.Equals(TrainingTime.OnlyMorning))
            {
                if (numberOfMonths > 12 || monthlyPriceBudget >= 19.0)
                    return MembershipType.TypeC;
                return null;
            }

            return MembershipType.TypeD;
        }
    }
}
