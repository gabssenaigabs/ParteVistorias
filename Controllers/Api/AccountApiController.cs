using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondoHub.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CondoHub.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountApiController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public class GestorRegisterDto
        {
            public string? Nome { get; set; }
            public string? Email { get; set; }
            public string? CPF { get; set; }
            public string? Senha { get; set; }
            public string? ConfirmarSenha { get; set; }
            public string? TelefoneComercial { get; set; }
            public string? Empresa { get; set; }
            public string? Cargo { get; set; }
        }

        [HttpPost("register-gestor")]
        public async Task<IActionResult> RegisterGestor([FromBody] GestorRegisterDto dto)
        {
            if (dto == null)
                return BadRequest(new { errors = new[] { "Dados inválidos." } });

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Senha) ||
                string.IsNullOrWhiteSpace(dto.ConfirmarSenha) ||
                string.IsNullOrWhiteSpace(dto.Nome) ||
                string.IsNullOrWhiteSpace(dto.CPF) ||
                string.IsNullOrWhiteSpace(dto.TelefoneComercial))
                return BadRequest(new { errors = new[] { "Todos os campos obrigatórios devem ser preenchidos." } });

            if (dto.Senha != dto.ConfirmarSenha)
                return BadRequest(new { errors = new[] { "As senhas não coincidem." } });

            if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 6)
                return BadRequest(new { errors = new[] { "A senha deve ter pelo menos 6 caracteres." } });

            var user = new ApplicationUser
            {
                UserName = dto.Email!.Trim(),
                Email = dto.Email!.Trim(),
                Nome = dto.Nome!.Trim(),
                CPF = dto.CPF!.Trim(),
                TelefoneComercial = dto.TelefoneComercial!.Trim(),
                Empresa = dto.Empresa?.Trim(),
                Cargo = dto.Cargo?.Trim(),
            };

            var result = await _userManager.CreateAsync(user, dto.Senha!);
            if (!result.Succeeded)
            {
                return BadRequest
                (
                    new
                    {
                        errors = result.Errors.Select(e => e.Description).ToArray()
                    }
                );
            }

            await _userManager.AddToRoleAsync(user, UserRole.Gestor.ToString());

            return Ok(new { success = true });
        }
    }
}