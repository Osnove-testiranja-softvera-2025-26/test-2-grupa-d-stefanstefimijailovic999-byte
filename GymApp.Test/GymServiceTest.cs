using GymApp.Exceptions;
using GymApp.Models;
using GymApp.Services;
using GymApp.Test.Fakes;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace GymApp.Test
{
    [TestFixture]
    public class GymServiceTests
    {
        private static List<Training> GroupTrainings(int count)
        {
            var list = new List<Training>();
            for (int i = 0; i < count; i++)
                list.Add(new Training { Type = TrainingType.Group });
            return list;
        }

        [Test]
        public void NoTrainings_ThrowsException()
        {
            var training = new FakeTrainingService { TrainingsToReturn = new List<Training>() };
            var performance = new FakeTrainerPerformanceService();
            var payment = new FakePaymentService();
            var service = new GymService(payment, training, performance);

            Assert.Throws<NoTrainingsInTheLastMonthException>(
                () => service.DoStaffBonusPaymentCalculation(new Trainer { Id = 1 }));
        }

        [Test]
        public void Second_LowNotHeld_ManyFreeDays_Pays160()
        {
            var amount = RunBonus(GroupTrainings(3), PerformanceRank.Second, percentNotHeld: 10, freeDays: 18);
            Assert.That(amount, Is.EqualTo(160.0));
        }
        
        [Test]
        public void Second_LowNotHeld_FewFreeDays_Pays130()
        {
            var amount = RunBonus(GroupTrainings(3), PerformanceRank.Second, percentNotHeld: 14, freeDays: 17);
            Assert.That(amount, Is.EqualTo(130.0));
        }

        [Test]
        public void Second_HighNotHeld_PaysZero()
        {
            var amount = RunBonus(GroupTrainings(3), PerformanceRank.Second, percentNotHeld: 15, freeDays: 18);
            Assert.That(amount, Is.EqualTo(0.0));
        }
        
        [Test]
        public void First_NoBonusConditions_Pays150()
        {
            var amount = RunBonus(GroupTrainings(3), PerformanceRank.First, percentNotHeld: 10, freeDays: 0);
            Assert.That(amount, Is.EqualTo(150.0));
        }

        [Test]
        public void First_ManyGroupTrainings_Pays180()
        {
            var amount = RunBonus(GroupTrainings(11), PerformanceRank.First, percentNotHeld: 10, freeDays: 0);
            Assert.That(amount, Is.EqualTo(180.0));
        }

        [Test]
        public void First_LowNotHeld_Pays180()
        {
            var amount = RunBonus(GroupTrainings(2), PerformanceRank.First, percentNotHeld: 8, freeDays: 0);
            Assert.That(amount, Is.EqualTo(180.0));
        }
        
        private static double RunBonus(List<Training> trainings, PerformanceRank rank, int percentNotHeld, int freeDays)
        {
            var training = new FakeTrainingService { TrainingsToReturn = trainings };
            var performance = new FakeTrainerPerformanceService
            {
                ReportToReturn = new PerformanceReport
                {
                    PerformanceRank = rank,
                    PercentOfTrainingsNotHeld = percentNotHeld,
                    NumberOfFreeDaysLeft = freeDays
                }
            };
            var payment = new FakePaymentService();
            var service = new GymService(payment, training, performance);

            service.DoStaffBonusPaymentCalculation(new Trainer { Id = 1 });

            Assert.That(payment.CallCount, Is.EqualTo(1), "UpdateTrainerBonusPayment mora biti pozvan samio jednom");
            Assert.That(payment.LastTrainerId, Is.EqualTo(1));
            return payment.LastBonus.Amount;
        }

        [Test]
        public void DoStaffBonusPaymentCalculation_First_CallsPaymentServiceWithCorrectBonus()
        {
            var payment = Substitute.For<IPaymentService>();
            var training = Substitute.For<ITrainingService>();
            var performance = Substitute.For<ITrainerPerformanceService>();

            training.GetTrainingsInTheLastMonth(Arg.Any<int>()).Returns(GroupTrainings(11));
            performance.GetTrainerPerformanceReport(Arg.Any<int>()).Returns(new PerformanceReport
            {
                PerformanceRank = PerformanceRank.First,
                PercentOfTrainingsNotHeld = 10,
                NumberOfFreeDaysLeft = 0
            });

            var service = new GymService(payment, training, performance);
            service.DoStaffBonusPaymentCalculation(new Trainer { Id = 42 });

            payment.Received(1).UpdateTrainerBonusPayment(42, Arg.Is<BonusPayment>(b => b.Amount == 180.0));
        }

        [Test]
        public void DoStaffBonusPaymentCalculation_NoTrainings_PaymentNeverCalled()
        {
            var payment = Substitute.For<IPaymentService>();
            var training = Substitute.For<ITrainingService>();
            var performance = Substitute.For<ITrainerPerformanceService>();

            training.GetTrainingsInTheLastMonth(Arg.Any<int>()).Returns(new List<Training>());

            var service = new GymService(payment, training, performance);

            Assert.Throws<NoTrainingsInTheLastMonthException>(
                () => service.DoStaffBonusPaymentCalculation(new Trainer { Id = 7 }));

            payment.DidNotReceive().UpdateTrainerBonusPayment(Arg.Any<int>(), Arg.Any<BonusPayment>());
        }

        [TestCase(TrainingTime.WholeDay, 10, true, 18.0, null)]
        [TestCase(TrainingTime.OnlyMorning, 12, true, 25.0, MembershipType.TypeC)]
        [TestCase(TrainingTime.OnlyNight, 13, true, 26.0, MembershipType.TypeD)]
        [TestCase(TrainingTime.OnlyNight, 12, false, 18.0, MembershipType.TypeD)]
        [TestCase(TrainingTime.OnlyMorning, 10, false, 26.0, MembershipType.TypeC)]
        [TestCase(TrainingTime.WholeDay, 13, false, 25.0, null)]
        [TestCase(TrainingTime.WholeDay, 12, false, 26.0, MembershipType.TypeB)]
        [TestCase(TrainingTime.OnlyMorning, 13, false, 18.0, MembershipType.TypeC)]
        [TestCase(TrainingTime.OnlyNight, 10, false, 25.0, MembershipType.TypeD)]
        [TestCase(TrainingTime.WholeDay, 12, true, 26.0, MembershipType.TypeA)]
        public void GetMemberhipType_ReturnsExpectedType(TrainingTime trainingTime, int numberOfMonths, bool groupTrainings, double monthlyPriceBudget, MembershipType? expected)
        {
            var service = new GymService(
                Substitute.For<IPaymentService>(),
                Substitute.For<ITrainingService>(),
                Substitute.For<ITrainerPerformanceService>());

            var result = service.GetMemberhipType(numberOfMonths, groupTrainings, monthlyPriceBudget, trainingTime);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}