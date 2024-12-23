using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using IVFSuccessCalculator.Api;
using IVFSuccessCalculator.BusinessLogic;
using IVFSuccessCalculator.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;

namespace IVFSuccessCalculator.Tests.Unit.Api
{
    public class SuccessRateControllerUnitTests
    {
        private readonly Mock<IValidator<SuccessRateCalculationParameters>> _validator = new();
        private readonly Mock<ISuccessRateCalculator> _calculator = new();

        private readonly SuccessRateController _controller;

        public SuccessRateControllerUnitTests()
        {
            _controller = new SuccessRateController(_calculator.Object, _validator.Object);
        }

        [Fact]
        public void Request_That_DoesNot_Pass_Validation_Returns_BadRequest()
        {
            // ARRANGE
            var request = new SuccessRateCalculationRequest
            {
                Age = 20,
                Height = 68,
                Weight = 150,

                InfertilityDiagnosis = new(),

                UsingOwnEggs = true,
                UsedIvfBefore = false,
                ReasonForInfertilityKnown = true,

                NumLiveBirths = 1,
                NumPriorPregnancies = 1
            };

            _validator
                .Setup(v => v.Validate(It.IsAny<SuccessRateCalculationParameters>()))
                .Returns(new ValidationResult
                {
                    Errors = new List<ValidationFailure>
                    {
                        new ValidationFailure("Age", "Error")
                    }
                });

            // ACT
            var response = _controller.Get(request);

            // ASSERT
            response.As<IStatusCodeActionResult>().StatusCode.Should().Be(400);
            response.As<BadRequestObjectResult>().Value.Should().BeEquivalentTo(new Dictionary<string, string[]>
            {
                { "Age", ["Error"] }
            });
        }

        [Fact]
        public void Endpoint_Returns_Result_Of_Calculation()
        {
            // ARRANGE
            var request = new SuccessRateCalculationRequest
            {
                Age = 20,
                Height = 68,
                Weight = 150,

                InfertilityDiagnosis = new(),

                UsingOwnEggs = true,
                UsedIvfBefore = false,
                ReasonForInfertilityKnown = true,

                NumLiveBirths = 1,
                NumPriorPregnancies = 1
            };

            _validator
                .Setup(v => v.Validate(It.IsAny<SuccessRateCalculationParameters>()))
                .Returns(new ValidationResult());

            _calculator
                .Setup(c => c.Calculate(It.IsAny<SuccessRateCalculationParameters>()))
                .Returns(0.64);

            // ACT
            var response = _controller.Get(request);

            // ASSERT
            response.As<IStatusCodeActionResult>().StatusCode.Should().Be(200);
            response.As<OkObjectResult>().Value.Should().BeEquivalentTo(0.64);
        }
    }
}
