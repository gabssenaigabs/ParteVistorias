using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.Entities
{
    public enum ContractType
    {
        LocatarioProprietario = 1,
        Condomino = 2,
        Funcionario = 3
    }
    public class Contract
    {
        public int Id { get; set; }

        public ContractType Type { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? LocatarioNome { get; set; }
        public string? ProprietarioNome { get; set; }

        public int? PropertyId { get; set; }
        public Property? Property { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal? ValorMensal { get; set; }

        public string? CPF { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? Unidade { get; set; }
        public string? TipoCondomino { get; set; }

        public string? Cargo { get; set; }
        public string? Turno { get; set; }
        public DateTime? DataAdmissao { get; set; }
    }
}