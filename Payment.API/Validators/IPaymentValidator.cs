namespace Payment.API.Validators
{
    public interface IPaymentValidator
    {
        bool IsValid(decimal amount);
    }
}