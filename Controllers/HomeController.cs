using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Registration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Registration.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // INDEX
    public IActionResult Index()
    {
        return View();
    }

    // FETCH
    [HttpGet]
    public JsonResult fetch()
    {
        Record? record;

        using (RegistrationContext db = new())
        {
            if(_fetch_record(db, out record) == false)
            {
                return null!;
            }
        }

        var recordDictionary = new Dictionary<string, string>()
            {
                {"Code", record!.Code!},
                {"Name", record!.Name},
                {"Cpf", record!.Cpf},
                {"Address", record!.Address!},
                {"Phone", record!.Phone!}
            };

            // Return user's record 
            return new JsonResult(recordDictionary, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
        }

    // UPDATE
    [HttpPost]
    public JsonResult update([FromBody] FormModel formData)
    {
        // Ensure mandatory fields were filled
        if (formData is null || formData.Name is null || formData.Cpf is null)
        {
            return _jsonError("Erro: O nome e o CPF devem ser preenchidos.");
        }

        // Validate incoming name
        if (!Regex.IsMatch(formData.Name, @"^[A-Za-zãáâéêíõóúÃÁÂÉÊÕÓÚ ]+$"))
        {
            return _jsonError("Erro: Nome inválido.");
        }

        // Validate incoming CPF
        if (!Regex.IsMatch(formData.Cpf, @"^([0-9]{3}\.){2}([0-9]){3}(\-)([0-9]){2}$"))
        {
            return _jsonError("Erro: CPF inválido.");
        }

        // Validate phone number if filled
        if (formData.Phone is not null)
        {
            if (!Regex.IsMatch(formData.Phone, @"^((\+)|([0-9\(]))(([0-9\-\(\) ]){6,32})$"))
            {
                return _jsonError("Erro: Telefone inválido.");
            }
        }

        using (RegistrationContext db = new())
        {
            Record? record;

            // Search for record in database
            if(_fetch_record(db, out record))
            {
                // If found, update data
                record!.Code = formData.Code;
                record!.Name = formData.Name;
                record!.Cpf = formData.Cpf;
                record!.Address = formData.Address;
                record!.Phone = formData.Phone;
            }
            
            // Otherwise, add new record
            else
            {
                string? username = _getCurrentUser();

                // Return null if user is not authenticated
                if (username is null)
                {
                    return _jsonError("Erro: Não foi possível salvar os dados.");
                }

                // Query database for user with username
                IQueryable<UserInfo> usersQuery = db.UserInfos.Where(u => u.Username == username);
                var user = usersQuery.FirstOrDefault();

                if (user is null)
                {
                    return _jsonError("Erro: Não foi possível salvar os dados.");
                }

                // Create new record
                record = new()
                {
                    Code = formData.Code,
                    Name = formData.Name,
                    Cpf = formData.Cpf,
                    Address = formData.Address,
                    Phone = formData.Phone,
                    User = user
                };

                // Add record to database
                db.Records.Add(record);
            }

            int affected = db.SaveChanges();

            if (affected <= 0)
            {
                return _jsonError("Erro: Não foi possível salvar os dados.");

            }

            // Return success message
            var rgx = new Regex(@"[^0-9]");
            string numericCpf = rgx.Replace(formData.Cpf, "");

            string cpfString = numericCpf.ToString();
            return _jsonSucess(cpfString.Substring(cpfString.Length - 4));
        }
    }

    // IS-AUTHENTICATED
    [AllowAnonymous]
    [HttpGet]
    public bool isAuthenticated()
    {
        return _isAuthenticated();
    }

    // LOG-IN (POST)
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> LogIn(string? username, string? password)
    {
        _logger.LogInformation($"Usuário = {username}, Senha = {password}");

        // Ensure username and password are not null
        if (username is null || password is null)
        {
            _logger.LogError("EMPTY FIELDS");
            return StatusCode(400);
        }

        // Validate username and password
        using (RegistrationContext db = new())
        {
            // Query database for user
            IQueryable<UserInfo> usersQuery = db.UserInfos.Where(u => u.Username == username);
            var user = usersQuery.FirstOrDefault();

            // Return error in case no user was found
            if (user == null)
            {
                return StatusCode(400);
            }
            
            // Verify password
            if (BCrypt.Net.BCrypt.Verify(password.ToString(), user.PasswordHash.ToString()) == false)
            {
                _logger.LogError("INVALID PASSWORD");
                return StatusCode(400);
            }
        }

        // Authenticate
        await Task.Run(() => _authenticate(username!));

        // Redirect to homepage
        return Redirect("/");
    }

    // LOG-IN (GET)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> LogIn()
    {
        if (_isAuthenticated())
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // Return form web page
        return View();
    }

    // LOG-OUT
    [AllowAnonymous]
    public async Task<IActionResult> LogOut()
    {
        // Log out and redirect to homepage
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    // ERROR
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        // Gerate user-friendly error pages
        if (statusCode.HasValue)
        {
            if (statusCode.Value == 400)
            {
                return View(statusCode.ToString());
            }
        }
        return View();
    }

    /// <summary>
    /// Fetch the record belonging to the current user in session
    /// </summary>
    /// <param name="db">dbContext within which method is called</param>
    /// <param name="record">Output variable contaning the method</param>
    /// <returns>Boolean value indicating if fetch was successful</returns>
    private bool _fetch_record(RegistrationContext db, out Record? record)
    {
        record = null;
        string? username = _getCurrentUser();

        // Return false if user is not authenticated
        if (username is null)
        {
            return false;
        }

        // Query database for user's record
        IQueryable<UserInfo> usersQuery = db.UserInfos.Include(u => u.Record).Where(u => u.Username == username);
        var user = usersQuery.FirstOrDefault();
    
        // If no record is found, return false
        if (user is null ||user.Record is null)
        {
            return false;
        }

        // Otherwise, output record and return true
        record = user.Record;
        return true;
    }

    /// <summary>
    /// Returns a JSON error message
    /// </summary>
    /// <param name="errorMessage">Message to be displayed</param>
    /// <returns></returns>
    private JsonResult _jsonError(string errorMessage)
    {
        var answer = new Dictionary<string, string>()
        {
            {"status", "danger"},
            {"message", errorMessage}
        };

        return new JsonResult(answer, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });
    }

    /// <summary>
    /// Returns a JSON sucess message
    /// </summary>
    /// <param name="userCode">Last 4 digits of CPF</param>
    /// <returns></returns>
    private JsonResult _jsonSucess(string userCode)
    {
        var answer = new Dictionary<string, string>()
        {
            {"status", "success"},
            {"message", "Pessoa cadastrada com sucesso, código " + userCode}
        };

        return new JsonResult(answer, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });
    }
    
    /// <summary>
    /// Check if user is authenticated through private method _getCurrentUser
    /// </summary>
    /// <returns>Status of authentication.</returns>
    private bool _isAuthenticated()
    {
        // Return true if session is defined by cookies
        if (_getCurrentUser() is null)
        {
            return false;
        }
        else 
        {
            return true;
        }
    }

    /// <summary>
    /// Use cookies to get current user
    /// </summary>
    /// <returns>Current user's username</returns>
    private string _getCurrentUser()
    {
        string? username;

        // Get username from cookies
        try
        {
            username = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name)?.Value;
            return username!;
        }
        // Return null if user is not authenticated (no cookies)
        catch (InvalidOperationException)
        {
            return null!;
        }
    }

    /// <summary>
    /// Perform cookie authentication using username (not ideal for real apps)
    /// </summary>
    /// <param name="username">Used for the claims</param>
    private async void _authenticate(string username)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties();

        await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        authProperties);
    }
}
