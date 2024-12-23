using FluentAssertions;
using IVFSuccessCalculator.BusinessLogic;
using IVFSuccessCalculator.Domain;

namespace IVFSuccessCalculator.Tests.Unit.BusinessLogic
{
    public class SuccessRateRequestValidatorTests
    {
        private readonly SuccessRateRequestValidator _validator;
        private readonly SuccessRateCalculationParameters _request;

        private const int FT = 12;

        public SuccessRateRequestValidatorTests()
        {
            _validator = new SuccessRateRequestValidator();
            _request = new SuccessRateCalculationParameters
            {
                Age = 30,
                Height = 5 * FT + 8,
                Weight = 150,

                ReasonForInfertilityKnown = false,
                InfertilityDiagnosis = new InfertilityFactors(),

                NumLiveBirths = 0,
                NumPriorPregnancies = 0,
            };
        }

        [Theory]
        [InlineData(19)]
        [InlineData(51)]
        public void Validation_Fails_When_Age_Is_Outside_Allowable_Range(int invalidAge)
        {
            _request.Age = invalidAge;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(20)]
        [InlineData(30)]
        [InlineData(50)]
        public void Validation_Succeeds_When_Age_Is_Within_Allowable_Range(int validAge)
        {
            _request.Age = validAge;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(4, 5)]
        [InlineData(6, 1)]
        public void Validation_Fails_When_Height_Is_Outside_Allowable_Range(int invalidFt, int invalidInches)
        {
            _request.Height = invalidFt * FT + invalidInches;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();

            response.Errors.Count().Should().Be(1);
            response.Errors[0].ErrorMessage.Should().Be($"'Height' must be between 4'6\" and 6'. You entered {invalidFt}'{invalidInches}\"");
        }

        [Theory]
        [InlineData(4 * FT + 6)]
        [InlineData(5 * FT + 2)]
        [InlineData(6 * FT)]
        public void Validation_Succeeds_When_Height_Is_Within_Allowable_Range(int validHeight)
        {
            _request.Height = validHeight;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(79)]
        [InlineData(301)]
        public void Validation_Fails_When_Weight_Is_Outside_Allowable_Range(int invalidWeight)
        {
            _request.Weight = invalidWeight;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(80)]
        [InlineData(150)]
        [InlineData(300)]
        public void Validation_Succeeds_When_Weight_Is_Within_Allowable_Range(int validWeight)
        {
            _request.Weight = validWeight;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validation_Fails_When_Num_Prior_Pregnancies_LessThanZero()
        {
            _request.NumPriorPregnancies = -1;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validation_Fails_When_Num_Live_Births_LessThanZero()
        {
            _request.NumLiveBirths = -1;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validation_Fails_When_Num_Live_Births_Exceeds_Num_PriorPregnancies()
        {
            _request.NumPriorPregnancies = 2;
            _request.NumLiveBirths = 3;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();

            response.Errors.Count().Should().Be(1);
            response.Errors[0].ErrorMessage.Should().Be("The number of live births cannot exceed the number of prior pregnancies.");
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 1)]
        public void Validation_Succeeds_When_Num_Live_Births_And_PriorPregnancies_InRange(int priorPregnancies, int numLiveBirths)
        {
            _request.NumPriorPregnancies = priorPregnancies;
            _request.NumLiveBirths = numLiveBirths;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validation_Fails_When_InfFactors_Specified_And_Reason_Unknown()
        {
            _request.ReasonForInfertilityKnown = false;
            _request.InfertilityDiagnosis = new InfertilityFactors
            {
                UterineFactor = true,
                OvulatoryDisorder = true
            };

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();

            response.Errors.Count().Should().Be(1);
            response.Errors[0].ErrorMessage.Should().Be("Infertility factors cannot be specified when reason for infertility is not known.");
        }

        [Fact]
        public void Validation_Fails_When_No_InfFactors_Specified_And_Reason_Known()
        {
            _request.ReasonForInfertilityKnown = true;
            _request.InfertilityDiagnosis = InfertilityFactors.None;

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();

            response.Errors.Count().Should().Be(1);
            response.Errors[0].ErrorMessage.Should().Be("At least one infertility factor must be specified.");
        }

        [Fact]
        public void Validation_Fails_When_Unexplained_And_Other_InfFactors_Specified()
        {
            _request.ReasonForInfertilityKnown = true;
            _request.InfertilityDiagnosis = new InfertilityFactors
            {
                UnexplainedInf = true,
                Endometriosis = true
            };

            var response = _validator.Validate(_request);

            response.IsValid.Should().BeFalse();

            response.Errors.Count().Should().Be(1);
            response.Errors[0].ErrorMessage.Should().Be("Infertility factors cannot be specified if infertility is unexplained.");
        }

        [Theory]
        [MemberData(nameof(ExampleValidRequests))]
        public void Validation_Succeeds_When_Request_Specified(SuccessRateCalculationParameters validRequest)
        {
            var response = _validator.Validate(validRequest);
            response.IsValid.Should().BeTrue();
        }

        public static IEnumerable<object[]> ExampleValidRequests => [
            // Infertility Diagnosis not performed (Reason for infertility is not yet known)
            [new SuccessRateCalculationParameters {
                Age = 20,
                Height = 5*FT + 8,
                Weight = 150,

                ReasonForInfertilityKnown = false,
                InfertilityDiagnosis = new()
            }],

            // Infertility Diagnosis - Infertility cannot be explained after evaluation
            [new SuccessRateCalculationParameters {
                Age = 20,
                Height = 5*FT + 8,
                Weight = 150,

                ReasonForInfertilityKnown = true,
                InfertilityDiagnosis = new InfertilityFactors {
                    UnexplainedInf = true
                }
            }],

            // Infertility Diagnosis - Various known factors specified
            [new SuccessRateCalculationParameters {
                Age = 20,
                Height = 5*FT + 8,
                Weight = 150,

                ReasonForInfertilityKnown = true,
                InfertilityDiagnosis = new InfertilityFactors {
                    MaleFactorInf = true,
                    Endometriosis = true,
                    DiminishedOvarianReserve = true
                }
            }],
        ];
    }
}
