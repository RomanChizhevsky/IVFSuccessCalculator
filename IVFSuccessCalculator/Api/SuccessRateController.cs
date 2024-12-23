using FluentValidation;
using IVFSuccessCalculator.BusinessLogic;
using IVFSuccessCalculator.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IVFSuccessCalculator.Api
{
    [ApiController]
    [Route("/success-rate")]
    public class SuccessRateController : ControllerBase
    {
        private readonly ISuccessRateCalculator _calculator;
        private readonly IValidator<SuccessRateCalculationParameters> _validator;

        public SuccessRateController(ISuccessRateCalculator calculator, IValidator<SuccessRateCalculationParameters> validator)
        {
            _calculator = calculator;
            _validator = validator;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Get([FromBody] SuccessRateCalculationRequest request)
        {
            var parameters = request.ToParameters();

            var validationResult = _validator.Validate(parameters);
            if (!validationResult.IsValid)
            {
                var formatErrors = validationResult
                                                .Errors
                                                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                                                .ToDictionary(e => e.Key, messages => messages);

                return BadRequest(formatErrors);
            }

            var result = _calculator.Calculate(parameters);
            return Ok(result);
        }
    }
}
