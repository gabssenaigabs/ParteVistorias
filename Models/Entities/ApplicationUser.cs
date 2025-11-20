using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CondoHub.Models.Entities
{
    public enum UserRole
    {
        Gestor,
        Morador,
        Sindico
    }
    public class ApplicationUser : IdentityUser
    {
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Bloco { get; set; }
        public string? Apartamento { get; set; }
        public string? Telefone { get; set; }
        public string? TelefoneComercial { get; set; }
        public string? Empresa { get; set; }
        public string? Cargo { get; set; }
        public UserRole Role { get; set; }
        public DateTime? InicioMandato { get; set; }
        public string? BlocoResidencia { get; set; }
    }
}