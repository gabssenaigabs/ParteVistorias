using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CondoHub.Models.Entities
{
    // DTO para receber dados do frontend
    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }
    public enum ItemStatus
    {
        NA = 1,
        OK = 2,
        Atenção = 3,
        Critico = 4
    }

    public enum NoticeStatus
    {
        Pendente = 1,
        EmProgresso = 2,
        Resolvido = 3
    }

    public class Notice
    {
        public int Id { get; set; }

        public string SolicitanteId { get; set; }
        public virtual ApplicationUser Solicitante { get; set; }

        public string? GestorId { get; set; }
        public virtual ApplicationUser? Gestor { get; set; }

        public int? PropertyId { get; set; }
        public virtual Property? Property { get; set; }

        public NoticeStatus Status { get; set; } = NoticeStatus.Pendente;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }

        public List<ChecklistItem> Itens { get; set; } = new List<ChecklistItem>();
        public List<NoticeFoto> Fotos { get; set; } = new List<NoticeFoto>();
    }

    public class ChecklistItem
    {
        public int Id { get; set; }

        public string? Categoria { get; set; }
        public string? Descricao { get; set; }

        public ItemStatus Status { get; set; } = ItemStatus.NA;

        public int NoticeId { get; set; }
        public virtual Notice? Notice { get; set; }

        public List<NoticeFoto> Fotos { get; set; } = new List<NoticeFoto>();
    }

    public class NoticeFoto
    {
        public int Id { get; set; }

        public string? FotoFile { get; set; }
        public DateTime DataUpload { get; set; } = DateTime.UtcNow;

        public int NoticeId { get; set; }
        public virtual Notice? Notice { get; set; }

        public int? ChecklistItemId { get; set; }
        public virtual ChecklistItem? ChecklistItem { get; set; }
    }
}
