using System;
using Microsoft.AspNetCore.Mvc;

namespace booksBackend;

[ApiController]
[Route("[controller]")]
public abstract class BaseController : Controller
{

}
