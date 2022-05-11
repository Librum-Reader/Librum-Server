using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("{name}")]
    public ActionResult<string> TestRequest(string name)
    {
        return "Hello" + name;
    }
}