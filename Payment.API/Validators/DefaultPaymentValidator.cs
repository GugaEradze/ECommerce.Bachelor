namespace Payment.API.Validators
{
    public class DefaultPaymentValidator : IPaymentValidator
    {
        public bool IsValid(decimal amount)
        {
            return amount != 999;
        }
    }
}