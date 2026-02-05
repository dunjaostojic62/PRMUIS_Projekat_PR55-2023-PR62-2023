using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantSimulation.Domain.Entities;

namespace RestaurantSimulation.Application.Interfaces;

public interface IRestaurantServer
{
    // Rezervacije (menadžer)
    Reservation CreateReservation(int tableId, string guestName, int guestCount, DateTime from, DateTime to);
    void CancelReservation(int reservationId);

    // Sto (konobar)
    void CheckInTable(int tableId, int guestCount);
    void FreeTable(int tableId);

    // Porudžbine (konobar)
    Order PlaceOrder(int tableId, List<OrderItem> items);

    // Obrada rada (kuhinja/bar)
    void DispatchWork();

    // Označavanje spremno (kuvar/barmen)
    void MarkFoodReady(int orderId);
    void MarkDrinkReady(int orderId);

    // Plaćanje (konobar)
    Bill Pay(int orderId, decimal paidAmount);
}

