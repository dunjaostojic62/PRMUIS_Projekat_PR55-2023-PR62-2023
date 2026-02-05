using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation.Domain.Entities;

public class Bill
{
    public int OrderId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Change { get; set; }

    private const decimal VatRate = 0.2m;

    public Bill(int orderId, decimal subtotal)
    {
        OrderId = orderId;
        Subtotal = subtotal;
        Vat = subtotal * VatRate;
        Total = Subtotal + Vat;
        PaidAmount = 0;
        Change = 0;
    }

    public void Pay(decimal amount)
    {
        PaidAmount = amount;

        if (PaidAmount >= Total)
        {
            Change = PaidAmount - Total;
        }
        else
        {
            throw new InvalidOperationException("Nedovoljno novca za placanje racuna.");
        }
    }
}
