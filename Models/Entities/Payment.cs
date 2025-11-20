using System;
using System.ComponentModel.DataAnnotations;

namespace CondoHub.Models.Entities
{
    public enum PaymentStatus
    {
        Pendente,
        Pago,
        Atrasado
    }

    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public string MesReferencia { get; set; }

        [Required]
        public decimal TaxaCondominial { get; set; }

        [Required]
        public decimal FundoReserva { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pendente;

        public string? ComprovantePath { get; set; }

        public decimal Total => TaxaCondominial + FundoReserva;

        public string? UserId { get; set; }

        public string? QrCodePix { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}