using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.ViewModels
{
    public class RegisterGestorViewModel
    {
        [Required] public string? Nome { get; set; }
        [Required, EmailAddress] public string? Email { get; set; }
        [Required] public string? CPF { get; set; }
        [Required, DataType(DataType.Password)] public string? Senha { get; set; }
        [Required] public string? TelefoneComercial { get; set; }
        public string? Empresa { get; set; }
        public string? Cargo { get; set; }
    }
}