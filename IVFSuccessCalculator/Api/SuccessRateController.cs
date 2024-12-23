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
        private readonly IValidator<SuccessRateCalculationRequest> _validator;

        public SuccessRateController(ISuccessRateCalculator calculator, IValidator<SuccessRateCalculationRequest> validator)
        {
            _calculator = calculator;
            _validator = validator;
        }

        [HttpPost]
        public IActionResult Get([FromBody] SuccessRateCalculationRequest request)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var formatErrors = validationResult
                                                .Errors
                                                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                                                .ToDictionary(e => e.Key, messages => messages);

                return BadRequest(formatErrors);
            }

            var result = _calculator.Calculate(request);
            return Ok(result);
        }
    }
}
