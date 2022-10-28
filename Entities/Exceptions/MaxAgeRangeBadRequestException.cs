namespace Entities.Exceptions
{
    public sealed class MaxAgeRangeBadRequestException : BadRequestException
    {
        public MaxAgeRangeBadRequestException() : base($"Max age cannot be less than min age.") { }
    }
}
