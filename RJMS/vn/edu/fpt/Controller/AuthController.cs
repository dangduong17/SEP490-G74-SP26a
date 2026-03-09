using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorToast"] = "Vui lòng kiểm tra lại thông tin đăng nhập.";
                return View(loginDto);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result.Success)
            {
                TempData["SuccessToast"] = "Đăng nhập thành công!";

                // Redirect theo role (cookie đã được AuthService set trước đó)
                var role = Request.Cookies["UserRole"];
                return role switch
                {
                    "Admin"     => RedirectToAction("Index",              "Admin"),
                    "Recruiter" => RedirectToAction("RecruiterDashboard", "Recruiter"),
                    _           => RedirectToAction("Index",              "Home"),
                };
            }

            TempData["ErrorToast"] = result.Message;
            return View(loginDto);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var err in errors)
                {
                    Console.WriteLine($"VALIDATION ERROR: {err}");
                }
                TempData["ErrorToast"] = $"Vui lòng kiểm tra lại thông tin đăng ký. Chi tiết: {string.Join(", ", errors)}";
                return View(registerDto);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success)
            {
                TempData["SuccessToast"] = result.Message;
            }
            else
            {
                TempData["ErrorToast"] = result.Message;
                return View(registerDto);
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult RegisterRecruiter()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterRecruiter(RecruiterRegisterViewModel registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorToast"] = $"Vui lòng kiểm tra lại thông tin đăng ký. Chi tiết: {string.Join(", ", errors)}";
                return View(registerDto);
            }

            var result = await _authService.RegisterRecruiterAsync(registerDto);

            if (result.Success)
            {
                TempData["SuccessToast"] = result.Message;
            }
            else
            {
                TempData["ErrorToast"] = result.Message;
                return View(registerDto);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["SuccessToast"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorToast"] = "Vui lòng kiểm tra lại thông tin email.";
                return View(forgotPasswordDto);
            }

            try
            {
                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

                if (result.Success)
                {
                    TempData["SuccessToast"] = result.Message;
                }
                else
                {
                    TempData["ErrorToast"] = result.Message;
                }
            }
            catch (Exception)
            {
                TempData["ErrorToast"] = "Đã xảy ra lỗi. Vui lòng thử lại sau.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var result = await _authService.ConfirmEmailAsync(token);

            if (result.Success)
            {
                TempData["SuccessToast"] = result.Message;
            }
            else
            {
                TempData["ErrorToast"] = result.Message;
            }

            return RedirectToAction("Login");
        }
    }
}
