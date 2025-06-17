using System.ComponentModel.DataAnnotations;


namespace NewGaras.Infrastructure.DTO.Hotel.Validators
{
    public class FeatureDate : ValidationAttribute
    {
        public override bool IsValid(object? value)
            => value is DateTime startDate && startDate >= DateTime.Today;
    }
}
