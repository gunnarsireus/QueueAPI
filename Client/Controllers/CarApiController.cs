using Client.Interfaces;
using Client.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Requests;
using Shared.Responses;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Client.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarApiController : ControllerBase
    {
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IClientMessageHub _serverMessageHub;

        public CarApiController(SignInManager<ApplicationUser> signInManager, IClientMessageHub serverMessageHub)
        {
            _signInManager = signInManager;
            _serverMessageHub = serverMessageHub;
        }

        [HttpGet("getallcars")]
        [SwaggerOperation(Summary = "Get all cars", Description = "Get all cars")]
        [SwaggerResponse(200, "Respons was successfully generated")]
        [SwaggerResponse(500, "An unexpected error occurred")]
        public async Task<IActionResult> GetAllCars()
        {
            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCarsRequest(), correlationId);
            var getCarsResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCarsResponse>(correlationId);

            return Ok(getCarsResponse.Cars);
        }

        [HttpPost("updateonline")]
        [SwaggerOperation(Summary = "Update property online", Description = "Update property online")]
        [SwaggerResponse(200, "Update was successfull")]
        [SwaggerResponse(500, "An unexpected error occurred")]
        public async Task<IActionResult> UpdateOnline([FromBody] Car car)
        {
            if (!ModelState.IsValid) return Ok(new { success = false });

            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCarRequest(car.Id), correlationId);
            var getCarResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCarResponse>(correlationId);

            var oldCar = getCarResponse.Car;
            oldCar.Online = car.Online;

            correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new UpdateCarRequest(oldCar), correlationId);
            var upadeCarResponse = await _serverMessageHub.ReceiveFromServerMessage<UpdateCarResponse>(correlationId);

            return Ok(new { success = true });
        }
    }
}
