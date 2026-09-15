using FluentValidation;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validators;

public class CreateIssueDtoValidator : AbstractValidator<CreateIssueDto>
{
    public CreateIssueDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Görev başlığı boş olamaz.")
            .MaximumLength(100).WithMessage("Başlık en fazla 100 karakter olabilir.");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("Geçerli bir proje seçilmelidir.");
    }
}