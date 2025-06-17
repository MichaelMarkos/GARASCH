
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.Hotel.Validators
{
    public class IsAfterStartDate : ValidationAttribute
    {
        private readonly DateTime _startDate;
        public IsAfterStartDate(DateTime startDate) 
        {
            _startDate = startDate;
        }

        public override bool IsValid(object? value)
            => value is DateTime startDate && startDate > _startDate;
    }
}
