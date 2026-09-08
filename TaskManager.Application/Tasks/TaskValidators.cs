using FluentValidation;
using TaskManager.Application.Abstractions;

namespace TaskManager.Application.Tasks;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(IClock clock)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(clock.Today)
            .WithMessage("Due date cannot be in the past.");
    }
}

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator(IClock clock)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(clock.Today)
            .WithMessage("Due date cannot be in the past.");
    }
}
