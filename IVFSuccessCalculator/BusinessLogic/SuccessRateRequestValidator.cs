using FluentValidation;
using IVFSuccessCalculator.Domain;

namespace IVFSuccessCalculator.BusinessLogic
{
    public class SuccessRateRequestValidator : AbstractValidator<SuccessRateCalculationParameters>
    {
        public SuccessRateRequestValidator() 
        {
            RuleFor(r => r.Age)
                .InclusiveBetween(20, 50);

            RuleFor(r => r.Weight)
                .InclusiveBetween(80, 300);

            RuleFor(r => r.Height)
                // Input range mirrored from CDC website
                .InclusiveBetween(54 /*4'6"*/, 72 /*6'*/)
                .WithMessage(r => $"'Height' must be between 4'6\" and 6'. You entered {r.Height / 12}'{r.Height % 12}\"");

            RuleFor(r => r.NumPriorPregnancies)
                .GreaterThanOrEqualTo(0);

            RuleFor(r => r.NumLiveBirths)
                .GreaterThanOrEqualTo(0);

            RuleFor(r => r.NumLiveBirths)
                .LessThanOrEqualTo(r => r.NumPriorPregnancies)
                .WithMessage("The number of live births cannot exceed the number of prior pregnancies.");

            RuleFor(r => r.InfertilityDiagnosis)
                .SetValidator(r => new InfertilityFactorsValidator(r.ReasonForInfertilityKnown));
        }

        private class InfertilityFactorsValidator : AbstractValidator<InfertilityFactors>
        {
            public InfertilityFactorsValidator(bool reasonForInfertilityKnown)
            {
                if (!reasonForInfertilityKnown)
                {
                    RuleFor(f => f)
                        .Must(be => be == InfertilityFactors.None)
                        .WithMessage("Infertility factors cannot be specified when reason for infertility is not known.");

                    return;
                }

                RuleFor(f => f)
                    .Must(be => be != InfertilityFactors.None)
                    .WithMessage("At least one infertility factor must be specified.");

                RuleFor(f => f)
                    .Must(be => be == InfertilityFactors.None with { UnexplainedInf = true })
                    .When(f => f.UnexplainedInf)
                    .WithMessage("Infertility factors cannot be specified if infertility is unexplained.");
            }
        }
    }
}
