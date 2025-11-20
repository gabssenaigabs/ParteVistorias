using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CondoHub.Data;
using CondoHub.Models.Entities;
using System.Collections.Generic;

namespace CondoHub.Controllers
{
    public class FinancialController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var isSindico = User.IsInRole("SINDICO");
            var isGestor = User.IsInRole("GESTOR");

            await UpdatePaymentStatuses();

            if (isGestor)
            {
                var pagamentos = await _context.Payments.ToListAsync();

                ViewBag.TotalEsperado = pagamentos.Sum(p => p.Total);
                ViewBag.TotalRecebido = pagamentos.Where(p => p.Status == PaymentStatus.Pago).Sum(p => p.Total);
                ViewBag.TotalPendente = pagamentos.Where(p => p.Status == PaymentStatus.Pendente).Sum(p => p.Total);
                ViewBag.TotalAtrasado = pagamentos.Where(p => p.Status == PaymentStatus.Atrasado).Sum(p => p.Total);

                var userIds = pagamentos.Select(p => p.UserId).Where(id => !string.IsNullOrEmpty(id)).Distinct();
                var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                var userMap = new Dictionary<string, (string Nome, string Apartamento)>();

                foreach (var u in users)
                {
                    if (!string.IsNullOrEmpty(u.Id))
                    {
                        var display = !string.IsNullOrEmpty(u.Nome) ? u.Nome : (u.Email ?? "");
                        var apt = u.Apartamento ?? "-";
                        userMap[u.Id] = (display, apt);
                    }
                }

                ViewBag.PaymentUserMap = userMap;

                return View("Gestor", pagamentos);
            }

            await GenerateMonthlyPayment(userId);

            var userPayments = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.DataVencimento)
                .ToListAsync();

            var latestPayment = userPayments.FirstOrDefault();

            if (latestPayment == null)
            {
                latestPayment = new Payment
                {
                    UserId = userId,
                    MesReferencia = DateTime.Now.ToString("MM/yyyy"),
                    TaxaCondominial = 1452.00m,
                    FundoReserva = 200.00m,
                    DataVencimento = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 20),
                    QrCodePix = GenerateQrCode("000201PIXDATAAQUI"),
                    Status = PaymentStatus.Pendente
                };
            }

            if (isSindico)
                latestPayment.TaxaCondominial *= 0.8m;

            return View("Index", latestPayment);
        }

        private async Task GenerateMonthlyPayment(string userId)
        {
            var currentMonth = DateTime.Now.ToString("MM/yyyy");
            var exists = await _context.Payments
                .AnyAsync(p => p.MesReferencia == currentMonth && p.UserId == userId);

            if (!exists)
            {
                var payment = new Payment
                {
                    UserId = userId,
                    MesReferencia = currentMonth,
                    TaxaCondominial = 1452.00m,
                    FundoReserva = 200.00m,
                    DataVencimento = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 20),
                    QrCodePix = GenerateQrCode("000201PIXDATAAQUI"),
                    Status = PaymentStatus.Pendente
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }
        }

        private async Task UpdatePaymentStatuses()
        {
            var pagamentos = await _context.Payments.ToListAsync();
            var hoje = DateTime.Now;

            foreach (var p in pagamentos)
            {
                if (p.Status != PaymentStatus.Pago)
                {
                    if (hoje > p.DataVencimento)
                        p.Status = PaymentStatus.Atrasado;
                    else
                        p.Status = PaymentStatus.Pendente;
                }
            }

            await _context.SaveChangesAsync();
        }

        private string GenerateQrCode(string pixData)
        {
            var encoded = System.Net.WebUtility.UrlEncode(pixData);
            return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={encoded}";
        }

        [HttpPost]
        public async Task<IActionResult> UploadComprovante(int id, IFormFile comprovante)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null || comprovante == null)
                return RedirectToAction(nameof(Index));

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(comprovante.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/comprovantes", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await comprovante.CopyToAsync(stream);

            payment.ComprovantePath = "/uploads/comprovantes/" + fileName;
            payment.Status = PaymentStatus.Pago;
            payment.DataPagamento = DateTime.Now;

            _context.Update(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}