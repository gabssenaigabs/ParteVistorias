using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.ViewModels
{
    public class RegisterMoradorViewModel
    {
        [Required] public string? Nome { get; set; }
        [Required, EmailAddress] public string? Email { get; set; }
        [Required] public string? CPF { get; set; }
        [Required] public string? Bloco { get; set; }
        public string? Apartamento { get; set; }
        [Required, DataType(DataType.Password)] public string? Senha { get; set; }
        [Required] public string? Telefone { get; set; }
        [DataType(DataType.Password)] public string? ConfirmarSenha { get; set; }
    }
}