using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.Entities
{
    public enum PropertyType
    {
        Vago,
        Ocupado,
        Alugado
    }
    public class Property
    {
        public int Id { get; set; }

        public string Bloco { get; set; }
        public string Numero { get; set; }
        public string? TipoImovel { get; set; }
        public PropertyType Type { get; set; }
        public decimal Area { get; set; }
        public string? ProprietarioId { get; set; }
        public ApplicationUser? Proprietario { get; set; }
        public string? ProprietarioNome { get; set; }
    }
}