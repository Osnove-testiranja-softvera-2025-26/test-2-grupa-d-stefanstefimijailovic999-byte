using GymApp.Models;
using GymApp.Services;
using System;

namespace GymApp.Test.Fakes
{
    public class FakePaymentService : IPaymentService
    {
        public int CallCount { get; private set; }
        public int LastTrainerId { get; private set; }
        public BonusPayment LastBonus { get; private set; }

        public void UpdateTrainerBonusPayment(int trainerId, BonusPayment bonus)
        {
            CallCount++;
            LastTrainerId = trainerId;
            LastBonus = bonus;
        }

        void IPaymentService.UpdateTrainerBonusPayment(Guid trianerId, BonusPayment payment)
        {
            throw new NotImplementedException();
        }
    }
}
