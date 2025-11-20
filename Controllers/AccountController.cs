using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using CondoHub.Models.Entities;
using CondoHub.Models.ViewModels;

namespace GestaoCondominio.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [ActionName("Logout")]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var erros = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ModelState.AddModelError("", "Erros de validação: " + erros);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Senha!, model.LembrarMe, false);
            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user != null && result.Succeeded)
            {
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim("Nome", user.Nome ?? user.Email ?? "Usuário"),
                    new System.Security.Claims.Claim("TipoUsuario", user.Role.ToString())
                };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, "login");
                await _signInManager.SignOutAsync();
                await _signInManager.SignInWithClaimsAsync(user, model.LembrarMe, claims);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Login falhou. Verifique email e senha.");
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterMorador() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterMorador(RegisterMoradorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var erros = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Content("Erros de validação: " + erros);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email!,
                Email = model.Email!,
                Nome = model.Nome!,
                CPF = model.CPF!,
                Bloco = model.Bloco!,
                Telefone = model.Telefone!,
                Role = UserRole.Morador
            };

            var result = await _userManager.CreateAsync(user, model.Senha!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Morador.ToString());
                return RedirectToAction("Login");
            }

            var errosResult = string.Join(", ", result.Errors.Select(e => e.Description));
            return Content("Erros ao criar usuário: " + errosResult);
        }

        [HttpGet]
        public IActionResult RegisterSindico() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterSindico(RegisterSindicoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var erros = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Content("Erros de validação: " + erros);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email!,
                Email = model.Email!,
                Nome = model.Nome!,
                CPF = model.CPF!,
                Telefone = model.Telefone!,
                InicioMandato = model.InicioMandato,
                BlocoResidencia = model.BlocoResidencia!,
                Apartamento = model.Apartamento,
                Role = UserRole.Sindico
            };

            var result = await _userManager.CreateAsync(user, model.Senha!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Sindico.ToString());
                return RedirectToAction("Login");
            }

            var errosResult = string.Join(", ", result.Errors.Select(e => e.Description));
            return Content("Erros ao criar usuário: " + errosResult);
        }

        [HttpGet]
        public IActionResult RegisterGestor() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterGestor(RegisterGestorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email!,
                Email = model.Email!,
                Nome = model.Nome!,
                CPF = model.CPF!,
                TelefoneComercial = model.TelefoneComercial!,
                Empresa = model.Empresa,
                Cargo = model.Cargo,
                Role = UserRole.Gestor
            };
            var result = await _userManager.CreateAsync(user, model.Senha!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Gestor.ToString());
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
}