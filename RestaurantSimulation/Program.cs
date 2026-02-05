using RestaurantSimulation.Application.Interfaces;
using RestaurantSimulation.Application.Services;
using RestaurantSimulation.Domain.Entities;
using RestaurantSimulation.Domain.Enums;

IRestaurantServer server = new RestaurantServer();

Console.WriteLine("=== RESTORAN SIMULACIJA (FAZA 1 - bez soketa) ===");

int? lastOrderId = null;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1) Check-in stola (konobar)");
    Console.WriteLine("2) Kreiraj porudzbinu (konobar)");
    Console.WriteLine("3) Dispatch rada (server -> kuhinja/bar)");
    Console.WriteLine("4) Oznaci spremno (kuvar/barmen)");
    Console.WriteLine("5) Plati racun (konobar)");
    Console.WriteLine("6) Prikazi stanje (server)");
    Console.WriteLine("7) Kreiraj rezervaciju (menadzer)");
    Console.WriteLine("8) Otkazi rezervaciju (menadzer)");
    Console.WriteLine("0) Izlaz");
    Console.Write("Izbor: ");

    string? choice = Console.ReadLine();

    try
    {
        if (choice == "0") break;

        if (choice == "1")
        {
            Console.Write("Unesi ID stola (1-3): ");
            int tableId = int.Parse(Console.ReadLine()!);

            Console.Write("Unesi broj gostiju: ");
            int guests = int.Parse(Console.ReadLine()!);

            server.CheckInTable(tableId, guests);
            Console.WriteLine("OK: Sto je zauzet.");
        }
        else if (choice == "2")
        {
            Console.Write("Unesi ID stola (1-3): ");
            int tableId = int.Parse(Console.ReadLine()!);

            var items = new List<OrderItem>();

            Console.Write("Koliko stavki zelis da uneses? ");
            int n = int.Parse(Console.ReadLine()!);

            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($"--- Stavka {i + 1} ---");
                Console.Write("Naziv: ");
                string name = Console.ReadLine()!;

                Console.Write("Kolicina: ");
                int qty = int.Parse(Console.ReadLine()!);

                Console.Write("Cena (npr 350): ");
                decimal price = decimal.Parse(Console.ReadLine()!);

                Console.Write("Tip (F = Food, D = Drink): ");
                string t = (Console.ReadLine() ?? "").Trim().ToUpper();

                ItemType type = (t == "D") ? ItemType.Drink : ItemType.Food;

                items.Add(new OrderItem(name, qty, price, type));
            }

            var order = server.PlaceOrder(tableId, items);
            lastOrderId = order.Id;

            Console.WriteLine($"OK: Porudzbina kreirana. OrderId = {order.Id}");
        }
        else if (choice == "3")
        {
            server.DispatchWork();
            Console.WriteLine("OK: Dispatch izvrsen (izvuceno iz redova ako ima).");
        }
        else if (choice == "4")
        {
            int orderId = AskOrderId(lastOrderId);

            Console.Write("Sta oznacavas spremno? (F = Food, D = Drink): ");
            string t = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (t == "D")
            {
                server.MarkDrinkReady(orderId);
                Console.WriteLine("OK: Drink oznacen spremnim.");
            }
            else
            {
                server.MarkFoodReady(orderId);
                Console.WriteLine("OK: Food oznacen spremnim.");
            }

            Console.WriteLine("Napomena: Kad su svi potrebni delovi spremni, porudzbina postaje READY.");
        }
        else if (choice == "5")
        {
            int orderId = AskOrderId(lastOrderId);

            Console.Write("Koliko je gost platio? ");
            decimal paid = decimal.Parse(Console.ReadLine()!);

            Bill bill = server.Pay(orderId, paid);

            Console.WriteLine($"UKUPNO (sa PDV): {bill.Total}");
            Console.WriteLine($"PLACENO: {bill.PaidAmount}");
            Console.WriteLine($"KUSUR: {bill.Change}");
            Console.WriteLine("OK: Porudzbina je placena.");
        }
        else if (choice == "6")
        {
            Console.WriteLine("=== STOLOVI ===");
            foreach (var t in server.GetTables())
            {
                Console.WriteLine($"Sto {t.Id} | Kapacitet: {t.Capacity} | Gosti: {t.CurrentGuests} | Status: {t.Status}");
            }

            Console.WriteLine();
            Console.WriteLine("=== REZERVACIJE ===");
            foreach (var r in server.GetReservations())
            {
                Console.WriteLine($"Rez {r.Id} | Sto: {r.TableId} | {r.GuestName} | {r.GuestCount} | {r.From:g}-{r.To:g} | Active: {r.IsActive}");
            }

            Console.WriteLine();
            Console.WriteLine("=== PORUDZBINE ===");
            foreach (var o in server.GetOrders())
            {
                Console.WriteLine($"Order {o.Id} | Sto: {o.TableId} | Status: {o.Status} | Iznos: {o.TotalAmount()}");
            }
        }
        else if (choice == "7")
        {
            Console.Write("TableId (1-3): ");
            int tableId = int.Parse(Console.ReadLine()!);

            Console.Write("Ime gosta: ");
            string guest = Console.ReadLine()!;

            Console.Write("Broj gostiju: ");
            int guestCount = int.Parse(Console.ReadLine()!);

            Console.Write("Od (minute od sada, npr 0): ");
            int fromMin = int.Parse(Console.ReadLine()!);

            Console.Write("Do (minute od sada, npr 60): ");
            int toMin = int.Parse(Console.ReadLine()!);

            DateTime from = DateTime.Now.AddMinutes(fromMin);
            DateTime to = DateTime.Now.AddMinutes(toMin);

            var r = server.CreateReservation(tableId, guest, guestCount, from, to);
            Console.WriteLine($"OK: Rezervacija kreirana. Id = {r.Id}");
        }
        else if (choice == "8")
        {
            Console.Write("Unesi ReservationId: ");
            int reservationId = int.Parse(Console.ReadLine()!);

            server.CancelReservation(reservationId);
            Console.WriteLine("OK: Rezervacija otkazana.");
        }
        else
        {
            Console.WriteLine("Nepoznat izbor.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"GRESKA: {ex.Message}");
    }
}

static int AskOrderId(int? lastOrderId)
{
    Console.Write($"Unesi OrderId{(lastOrderId.HasValue ? $" (ENTER za {lastOrderId})" : "")}: ");
    string? s = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(s))
    {
        if (!lastOrderId.HasValue)
            throw new InvalidOperationException("Nema poslednjeg OrderId. Unesi rucno.");

        return lastOrderId.Value;
    }

    return int.Parse(s);
}
