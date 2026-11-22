using EmployeeManagement.Application.Commands;
using FluentValidation;

namespace EmployeeManagement.Application.Validators;

public sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(command => command.Department).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Designation).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Salary).GreaterThan(0);
        RuleFor(command => command.DateOfJoining)
            .Must(date => date <= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("Date of joining cannot be in the future.");
    }
}
