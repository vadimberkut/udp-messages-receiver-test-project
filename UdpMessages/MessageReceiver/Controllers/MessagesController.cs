using MessageReceiver.Dtos;
using MessageReceiver.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MessageReceiver.Controllers
{
    [AllowAnonymous]
    [Route("api/v1/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;

        public MessagesController(
            IMessagesService messagesService    
        )
        {
            _messagesService = messagesService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MessageDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMessages(
            [FromQuery] string ip,
            [FromQuery] DateTimeOffset? from, 
            [FromQuery] DateTimeOffset? to,
            [FromQuery] int? offset,
            [FromQuery] int? limit

        )
        {
            var result = await _messagesService.GetMessagesAsync(ip, from, to, offset, limit);
            return new JsonResult(result);
        }
    }
}
