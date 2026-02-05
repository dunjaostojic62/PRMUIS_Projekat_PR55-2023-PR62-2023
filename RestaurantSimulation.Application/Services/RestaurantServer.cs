using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantSimulation.Application.Interfaces;
using RestaurantSimulation.Domain.Entities;
using RestaurantSimulation.Domain.Enums;

namespace RestaurantSimulation.Application.Services;

public class RestaurantServer : IRestaurantServer
{
    
    private readonly List<Table> _tables = new();
    private readonly List<Reservation> _reservations = new();
    private readonly List<Order> _orders = new();

    // Redovi za kuhinju i bar (FIFO)
    private readonly Queue<int> _kitchenQueue = new(); // čuvamo orderId
    private readonly Queue<int> _barQueue = new();

    // “Ready” flagovi
    private readonly HashSet<int> _foodReady = new();
    private readonly HashSet<int> _drinkReady = new();

    private int _nextReservationId = 1;
    private int _nextOrderId = 1;

    public RestaurantServer()
    {
        // TEST stolovi (posle ćemo imati init servis/rep)
        _tables.Add(new Table(id: 1, capacity: 2));
        _tables.Add(new Table(id: 2, capacity: 4));
        _tables.Add(new Table(id: 3, capacity: 6));
    }

    public Reservation CreateReservation(int tableId, string guestName, int guestCount, DateTime from, DateTime to)
    {
        var table = FindTable(tableId);

        // Minimalna validacija
        if (guestCount > table.Capacity)
            throw new InvalidOperationException("Broj gostiju je veci od kapaciteta stola.");

        var reservation = new Reservation(_nextReservationId++, tableId, guestName, guestCount, from, to);
        _reservations.Add(reservation);

        table.Status = TableStatus.Reserved;
        return reservation;
    }

    public void CancelReservation(int reservationId)
    {
        var r = _reservations.FirstOrDefault(x => x.Id == reservationId)
            ?? throw new InvalidOperationException("Rezervacija ne postoji.");

        r.Cancel();

       
        var table = FindTable(r.TableId);
        bool hasAnyActive = _reservations.Any(x => x.TableId == table.Id && x.IsActive);
        if (!hasAnyActive && table.Status == TableStatus.Reserved)
            table.Status = TableStatus.Free;
    }

    public void CheckInTable(int tableId, int guestCount)
    {
        var table = FindTable(tableId);

        if (guestCount > table.Capacity)
            throw new InvalidOperationException("Previse gostiju za ovaj sto.");

        // Provera da li postoji AKTIVNA rezervacija za ovaj sto u ovom trenutku
        var now = DateTime.Now;
        var hasValidReservation = _reservations.Any(r =>
            r.TableId == tableId &&
            r.IsActive &&
            r.GuestCount == guestCount &&
            r.IsValidAt(now)
        );

        // Ako je sto rezervisan, dozvoli check-in samo uz validnu rezervaciju
        if (table.Status == TableStatus.Reserved && !hasValidReservation)
            throw new InvalidOperationException("Sto je rezervisan - nema validne rezervacije.");

        table.Occupy(guestCount);
    }


    public void FreeTable(int tableId)
    {
        var table = FindTable(tableId);
        table.Free();
    }

    public Order PlaceOrder(int tableId, List<OrderItem> items)
    {
        var table = FindTable(tableId);
        if (table.Status != TableStatus.Occupied)
            throw new InvalidOperationException("Sto nije zauzet - ne moze porudzbina.");

        var order = new Order(_nextOrderId++, tableId);
        foreach (var it in items)
            order.AddItem(it);

        order.Status = OrderStatus.Queued;
        _orders.Add(order);

        // raspodela: hrana u kitchenQueue, piće u barQueue
        if (order.HasFood()) _kitchenQueue.Enqueue(order.Id);
        if (order.HasDrink()) _barQueue.Enqueue(order.Id);

        return order;
    }

    public void DispatchWork()
    {
        // Za sad: samo “izvadi iz reda i kreni pripremu”
        if (_kitchenQueue.Count > 0)
        {
            int orderId = _kitchenQueue.Dequeue();
            // ovde bi kuvar “preuzeo”
            SetOrderStatus(orderId, OrderStatus.InPreparation);
        }

        if (_barQueue.Count > 0)
        {
            int orderId = _barQueue.Dequeue();
            SetOrderStatus(orderId, OrderStatus.InPreparation);
        }
    }

    public void MarkFoodReady(int orderId)
    {
        _foodReady.Add(orderId);
        TryMarkOrderReady(orderId);
    }

    public void MarkDrinkReady(int orderId)
    {
        _drinkReady.Add(orderId);
        TryMarkOrderReady(orderId);
    }

    public Bill Pay(int orderId, decimal paidAmount)
    {
        var order = FindOrder(orderId);

        if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Ready)
            throw new InvalidOperationException("Porudzbina nije spremna/isporucena.");

        var bill = new Bill(order.Id, order.TotalAmount());
        bill.Pay(paidAmount);

        order.Status = OrderStatus.Paid;
        return bill;
    }

    // ------- pomoćne metode -------

    private Table FindTable(int tableId)
        => _tables.FirstOrDefault(t => t.Id == tableId)
           ?? throw new InvalidOperationException("Sto ne postoji.");

    private Order FindOrder(int orderId)
        => _orders.FirstOrDefault(o => o.Id == orderId)
           ?? throw new InvalidOperationException("Porudzbina ne postoji.");

    private void SetOrderStatus(int orderId, OrderStatus status)
    {
        var order = FindOrder(orderId);
        if (order.Status < status) order.Status = status;
    }

    private void TryMarkOrderReady(int orderId)
    {
        var order = FindOrder(orderId);

        bool needFood = order.HasFood();
        bool needDrink = order.HasDrink();

        bool foodOk = !needFood || _foodReady.Contains(orderId);
        bool drinkOk = !needDrink || _drinkReady.Contains(orderId);

        if (foodOk && drinkOk)
        {
            order.Status = OrderStatus.Ready;
        }
    }

    public IReadOnlyList<Table> GetTables() => _tables;
    public IReadOnlyList<Reservation> GetReservations() => _reservations;
    public IReadOnlyList<Order> GetOrders() => _orders;
}

