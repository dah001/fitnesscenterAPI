// FitnessCenterr.Migrator – One-time migration: MySQL → MongoDB + Neo4j

using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;

// ── 1. KONFIGURATION ────────────────────────────────────────────────────────
// Læser connection strings og credentials fra appsettings.json.
// Intet migreres endnu her — det er kun opsætning.
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var mysqlConn   = config.GetConnectionString("DefaultConnection")!;  
var mongoConn   = config["MongoDB:ConnectionString"]!;                // MongoDB Atlas (mål)
var mongoDbName = config["MongoDB:Database"]!;
var neo4jUri    = config["Neo4j:Uri"]!;                               // Neo4j AuraDB (mål)
var neo4jUser   = config["Neo4j:Username"]!;
var neo4jPass   = config["Neo4j:Password"]!;

Console.WriteLine("Henter data fra MySQL...");

// ── 2. LÆSNING FRA MYSQL ─────────────────────────────────────────────────────
// Migration starter 
var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseMySql(mysqlConn, new MySqlServerVersion(new Version(8, 0, 0)))
    .Options;

using var db = new AppDbContext(dbOptions);

// n+1 problem
var trainers      = await db.Trainers.ToListAsync();
var members       = await db.Members.Include(m => m.Trainer).ToListAsync();
var subscriptions = await db.Subscriptions.ToListAsync();
var memberships   = await db.Memberships.Include(m => m.Member).Include(m => m.Subscription).ToListAsync();
var classes       = await db.Classes.Include(c => c.Trainer).Include(c => c.Hall).Include(c => c.Location).ToListAsync();
var bookings      = await db.ClassBookings.Include(b => b.Member).Include(b => b.Class).ToListAsync();
var locations     = await db.Locations.ToListAsync();
var centers       = await db.Centers.Include(c => c.Location).ToListAsync();
var halls         = await db.Halls.ToListAsync();
var equipments    = await db.Equipments.ToListAsync();
var vending       = await db.VendingMachines.ToListAsync();
var stocks        = await db.VendingMachineStocks.Include(s => s.VendingMachine).ToListAsync();
var staffs        = await db.Staffs.ToListAsync();
var payments      = await db.Payments.Include(p => p.Member).ToListAsync();
var users         = await db.Users.ToListAsync();

Console.WriteLine($"  Trainers: {trainers.Count}, Members: {members.Count}, Classes: {classes.Count}");
Console.WriteLine($"  Locations: {locations.Count}, Centers: {centers.Count}, Halls: {halls.Count}");
Console.WriteLine($"  Staff: {staffs.Count}, Payments: {payments.Count}, Users: {users.Count}");

// 3. MIGRERING TIL MONGODB 

// MONGODB START
Console.WriteLine("\nMigrerer til MongoDB...");

try
{
    var mongoClient = new MongoClient(mongoConn);
    var mongoDB     = mongoClient.GetDatabase(mongoDbName);

 
    await mongoDB.DropCollectionAsync("members");
    await mongoDB.DropCollectionAsync("classes");
    await mongoDB.DropCollectionAsync("subscriptions");
    await mongoDB.DropCollectionAsync("centers");
    await mongoDB.DropCollectionAsync("staff");
    await mongoDB.DropCollectionAsync("payments");
    
    var membersCol = mongoDB.GetCollection<BsonDocument>("members");
    var memberDocs = members.Select(m =>
    {
        var membership     = memberships.FirstOrDefault(ms => ms.MemberID == m.MemberID);
        var memberPayments = payments.Where(p => p.MemberID == m.MemberID).ToList();
        var memberUser     = users.FirstOrDefault(u => u.MemberID == m.MemberID);
        return new BsonDocument
        {
            { "_id", m.MemberID },
            { "name", m.Name },
            { "email", m.Email ?? "" },
            { "birthDate", m.BirthDate.HasValue ? m.BirthDate.Value.ToDateTime(TimeOnly.MinValue) : BsonNull.Value },

          
            { "user", memberUser != null
                ? new BsonDocument { { "userID", memberUser.UserID }, { "username", memberUser.Username }, { "role", memberUser.Role ?? "" } }
                : BsonNull.Value },

            
            { "trainer", m.Trainer != null
                ? new BsonDocument { { "trainerID", m.TrainerID ?? 0 }, { "name", m.Trainer.Name } }
                : BsonNull.Value },

           
            { "membership", membership != null
                ? new BsonDocument
                {
                    { "membershipID", membership.MembershipID },
                    { "subscriptionType", membership.Subscription.Type },
                    { "price", (double)membership.Subscription.Price },
                    { "startDate", membership.StartDate.ToDateTime(TimeOnly.MinValue) }
                }
                : BsonNull.Value },

            
            { "bookings", new BsonArray(
                bookings.Where(b => b.MemberID == m.MemberID)
                        .Select(b => new BsonDocument
                        {
                            { "bookingID", b.BookingID },
                            { "classID", b.ClassID },
                            { "className", b.Class.Name }
                        })) },

        
            { "payments", new BsonArray(
                memberPayments.Select(p => new BsonDocument
                {
                    { "paymentID", p.PaymentID },
                    { "amount", p.Amount.HasValue ? (double)p.Amount.Value : 0 },
                    { "paymentDate", p.PaymentDate ?? DateTime.MinValue },
                    { "paymentType", p.PaymentType ?? "" }
                })) }
        };
    }).ToList();

    await membersCol.InsertManyAsync(memberDocs);
    Console.WriteLine($"  MongoDB: {memberDocs.Count} members indsat.");
    
    var classesCol = mongoDB.GetCollection<BsonDocument>("classes");
    var classDocs = classes.Select(c => new BsonDocument
    {
        { "_id", c.ClassID },
        { "name", c.Name },
        { "classDate", c.ClassDate },
        { "trainer", new BsonDocument { { "trainerID", c.TrainerID }, { "name", c.Trainer.Name } } },
        { "hall", c.Hall != null ? new BsonDocument { { "hallID", c.HallID ?? 0 }, { "name", c.Hall.Name ?? "" } } : BsonNull.Value },
        { "location", c.Location != null ? new BsonDocument { { "locationID", c.LocationID ?? 0 }, { "city", c.Location.City } } : BsonNull.Value },
        { "participantCount", bookings.Count(b => b.ClassID == c.ClassID) } // derived value
    }).ToList();

    await classesCol.InsertManyAsync(classDocs);
    Console.WriteLine($"  MongoDB: {classDocs.Count} classes indsat.");
    
    var subsCol = mongoDB.GetCollection<BsonDocument>("subscriptions");
    var subDocs = subscriptions.Select(s => new BsonDocument
    {
        { "_id", s.SubscriptionID },
        { "type", s.Type },
        { "price", (double)s.Price },
        { "memberCount", memberships.Count(ms => ms.SubscriptionID == s.SubscriptionID) }
    }).ToList();

    await subsCol.InsertManyAsync(subDocs);
    Console.WriteLine($"  MongoDB: {subDocs.Count} subscriptions indsat.");
    
    var centersCol = mongoDB.GetCollection<BsonDocument>("centers");
    var centerDocs = centers.Select(c => new BsonDocument
    {
        { "_id", c.CenterID },
        { "location", new BsonDocument { { "locationID", c.LocationID }, { "city", c.Location.City } } },
        { "halls", new BsonArray(halls.Where(h => h.CenterID == c.CenterID).Select(h => new BsonDocument { { "hallID", h.HallID }, { "name", h.Name ?? "" } })) },
        { "equipment", new BsonArray(equipments.Where(e => e.CenterID == c.CenterID).Select(e => new BsonDocument { { "equipmentID", e.EquipmentID }, { "name", e.Name ?? "" } })) },
        { "vendingMachines", new BsonArray(vending.Where(v => v.CenterID == c.CenterID).Select(v => new BsonDocument
        {
            { "vendingMachineID", v.VendingMachineID },
            { "name", v.Name ?? "" },
            { "location", v.Location ?? "" },
            // Nested array inde i et embedded objekt — VendingMachineStock-rækker
            { "stock", new BsonArray(stocks.Where(s => s.VendingMachineID == v.VendingMachineID).Select(s => new BsonDocument
            {
                { "product", s.ProductName ?? "" },
                { "quantity", s.Quantity ?? 0 },
                { "price", s.Price.HasValue ? (double)s.Price.Value : 0 }
            })) }
        })) }
    }).ToList();

    await centersCol.InsertManyAsync(centerDocs);
    Console.WriteLine($"  MongoDB: {centerDocs.Count} centers indsat.");

  
    var staffCol = mongoDB.GetCollection<BsonDocument>("staff");
    var staffDocs = staffs.Select(s => new BsonDocument
    {
        { "_id", s.StaffID },
        { "name", s.Name },
        { "role", s.Role ?? "" }
    }).ToList();

    await staffCol.InsertManyAsync(staffDocs);
    Console.WriteLine($"  MongoDB: {staffDocs.Count} staff indsat.");
    Console.WriteLine("  MongoDB: Migration fuldført.");
}
catch (Exception ex)
{
    Console.WriteLine($"  ⚠️  MongoDB fejlede: {ex.Message}");
    Console.WriteLine("  MongoDB springes over – fortsætter med Neo4j...");
}
// MONGODB SLUT

// 4. MIGRERING TIL NEO4J 
// NEO4J START
Console.WriteLine("\nMigrerer til Neo4j...");

var neo4jDriver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPass));
var session = neo4jDriver.AsyncSession();

await session.RunAsync("MATCH (n) DETACH DELETE n");


foreach (var t in trainers)
    await session.RunAsync("CREATE (t:Trainer {trainerID: $id, name: $name})",
        new { id = t.TrainerID, name = t.Name });
Console.WriteLine($"  Neo4j: {trainers.Count} Trainer noder oprettet.");


foreach (var s in subscriptions)
    await session.RunAsync("CREATE (s:Subscription {subscriptionID: $id, type: $type, price: $price})",
        new { id = s.SubscriptionID, type = s.Type, price = (double)s.Price });
Console.WriteLine($"  Neo4j: {subscriptions.Count} Subscription noder oprettet.");


foreach (var l in locations)
    await session.RunAsync("CREATE (l:Location {locationID: $id, city: $city})",
        new { id = l.LocationID, city = l.City });
Console.WriteLine($"  Neo4j: {locations.Count} Location noder oprettet.");


foreach (var c in centers)
    await session.RunAsync(
        "MATCH (l:Location {locationID: $lId}) CREATE (c:Center {centerID: $id})-[:LOCATED_IN]->(l)",
        new { id = c.CenterID, lId = c.LocationID });
Console.WriteLine($"  Neo4j: {centers.Count} Center noder oprettet.");


foreach (var h in halls)
    await session.RunAsync(
        "MATCH (c:Center {centerID: $cId}) CREATE (h:Hall {hallID: $id, name: $name})-[:PART_OF]->(c)",
        new { id = h.HallID, name = h.Name ?? "", cId = h.CenterID });
Console.WriteLine($"  Neo4j: {halls.Count} Hall noder oprettet.");


foreach (var e in equipments)
    await session.RunAsync(
        "MATCH (c:Center {centerID: $cId}) CREATE (e:Equipment {equipmentID: $id, name: $name})-[:BELONGS_TO]->(c)",
        new { id = e.EquipmentID, name = e.Name ?? "", cId = e.CenterID ?? 0 });
Console.WriteLine($"  Neo4j: {equipments.Count} Equipment noder oprettet.");


foreach (var v in vending)
    await session.RunAsync(
        "MATCH (c:Center {centerID: $cId}) CREATE (v:VendingMachine {vendingMachineID: $id, name: $name, location: $loc})-[:LOCATED_IN]->(c)",
        new { id = v.VendingMachineID, name = v.Name ?? "", loc = v.Location ?? "", cId = v.CenterID ?? 0 });
Console.WriteLine($"  Neo4j: {vending.Count} VendingMachine noder oprettet.");


foreach (var s in stocks)
    await session.RunAsync(
        "MATCH (v:VendingMachine {vendingMachineID: $vId}) CREATE (p:Product {stockID: $id, productName: $name, quantity: $qty, price: $price})-[:SOLD_BY]->(v)",
        new { id = s.StockID, name = s.ProductName ?? "", qty = s.Quantity ?? 0, price = (double)(s.Price ?? 0), vId = s.VendingMachineID });
Console.WriteLine($"  Neo4j: {stocks.Count} Product noder oprettet.");

foreach (var s in staffs)
    await session.RunAsync("CREATE (s:Staff {staffID: $id, name: $name, role: $role})",
        new { id = s.StaffID, name = s.Name, role = s.Role ?? "" });
Console.WriteLine($"  Neo4j: {staffs.Count} Staff noder oprettet.");


foreach (var u in users)
{
    await session.RunAsync(
        "CREATE (u:User {userID: $id, username: $username, role: $role, enabled: $enabled})",
        new { id = u.UserID, username = u.Username, role = u.Role ?? "", enabled = u.Enabled });

    if (u.MemberID.HasValue)
        await session.RunAsync(
            "MATCH (u:User {userID: $uId}), (m:Member {memberID: $mId}) CREATE (m)-[:HAS_USER]->(u)",
            new { uId = u.UserID, mId = u.MemberID.Value });

    if (u.TrainerID.HasValue)
        await session.RunAsync(
            "MATCH (u:User {userID: $uId}), (t:Trainer {trainerID: $tId}) CREATE (t)-[:HAS_USER]->(u)",
            new { uId = u.UserID, tId = u.TrainerID.Value });
    
}
Console.WriteLine($"  Neo4j: {users.Count} User noder oprettet.");

foreach (var m in members)
{
    await session.RunAsync("CREATE (m:Member {memberID: $id, name: $name, email: $email})",
        new { id = m.MemberID, name = m.Name, email = m.Email ?? "" });
    if (m.TrainerID.HasValue)
        await session.RunAsync(
            "MATCH (m:Member {memberID: $mId}), (t:Trainer {trainerID: $tId}) CREATE (m)-[:TRAINED_BY]->(t)",
            new { mId = m.MemberID, tId = m.TrainerID.Value });
}
Console.WriteLine($"  Neo4j: {members.Count} Member noder oprettet.");

foreach (var c in classes)
{
    await session.RunAsync("CREATE (c:FitnessClass {classID: $id, name: $name, classDate: $date})",
        new { id = c.ClassID, name = c.Name, date = c.ClassDate.ToString("yyyy-MM-dd HH:mm") });
    await session.RunAsync(
        "MATCH (c:FitnessClass {classID: $cId}), (t:Trainer {trainerID: $tId}) CREATE (t)-[:TEACHES]->(c)",
        new { cId = c.ClassID, tId = c.TrainerID });
    if (c.LocationID.HasValue)
        await session.RunAsync(
            "MATCH (c:FitnessClass {classID: $cId}), (l:Location {locationID: $lId}) CREATE (c)-[:HELD_AT]->(l)",
            new { cId = c.ClassID, lId = c.LocationID.Value });
}
Console.WriteLine($"  Neo4j: {classes.Count} FitnessClass noder oprettet.");

foreach (var b in bookings)
{
    await session.RunAsync(
        "CREATE (cb:ClassBooking {bookingID: $id, memberID: $mId, classID: $cId})",
        new { id = b.BookingID, mId = b.MemberID, cId = b.ClassID });
    await session.RunAsync(
        "MATCH (m:Member {memberID: $mId}), (cb:ClassBooking {bookingID: $id}) CREATE (m)-[:HAS_BOOKING]->(cb)",
        new { mId = b.MemberID, id = b.BookingID });
    await session.RunAsync(
        "MATCH (cb:ClassBooking {bookingID: $id}), (c:FitnessClass {classID: $cId}) CREATE (cb)-[:BOOKED]->(c)",
        new { id = b.BookingID, cId = b.ClassID });
}
Console.WriteLine($"  Neo4j: {bookings.Count} ClassBooking noder oprettet.");

foreach (var ms in memberships)
{
    await session.RunAsync(
        "CREATE (mb:Membership {membershipID: $id, memberID: $mId, subscriptionID: $sId, startDate: $since})",
        new { id = ms.MembershipID, mId = ms.MemberID, sId = ms.SubscriptionID, since = ms.StartDate.ToString("yyyy-MM-dd") });
    await session.RunAsync(
        "MATCH (m:Member {memberID: $mId}), (mb:Membership {membershipID: $id}) CREATE (m)-[:HAS_MEMBERSHIP]->(mb)",
        new { mId = ms.MemberID, id = ms.MembershipID });
    await session.RunAsync(
        "MATCH (mb:Membership {membershipID: $id}), (s:Subscription {subscriptionID: $sId}) CREATE (mb)-[:FOR_SUBSCRIPTION]->(s)",
        new { id = ms.MembershipID, sId = ms.SubscriptionID });
}
Console.WriteLine($"  Neo4j: {memberships.Count} Membership noder oprettet.");

await session.RunAsync("MATCH (p:Payment) DETACH DELETE p");
foreach (var p in payments)
    await session.RunAsync(
        @"CREATE (p:Payment {
            paymentID:   $paymentID,
            memberID:    $memberID,
            amount:      $amount,
            paymentDate: $paymentDate,
            paymentType: $paymentType
          })",
        new {
            paymentID   = p.PaymentID,
            memberID    = p.MemberID,
            amount      = (double)(p.Amount ?? 0),
            paymentDate = p.PaymentDate != null ? p.PaymentDate.Value.ToString("yyyy-MM-dd") : "",
            paymentType = p.PaymentType ?? ""
        });
Console.WriteLine($"  Neo4j: {payments.Count} Payment noder oprettet.");

foreach (var p in payments)
    await session.RunAsync(
        "MATCH (m:Member {memberID: $mId}), (p:Payment {paymentID: $pId}) CREATE (m)-[:MADE_PAYMENT]->(p)",
        new { mId = p.MemberID, pId = p.PaymentID });
Console.WriteLine($"  Neo4j: {payments.Count} MADE_PAYMENT relationer oprettet.");
// >>> NEO4J SLUT <<<

// ── 5. OPRYDNING ─────────────────────────────────────────────────────────────
// Lukker Neo4j-session og driver pænt. Fejl her ignoreres bevidst (safe to ignore),
// da migrationen allerede er gennemført på dette tidspunkt.
try
{
    await session.CloseAsync();
    await neo4jDriver.DisposeAsync();
}
catch (Exception)
{
    // Safe to ignore
}

Console.WriteLine("\n✅ Migration fuldført! Data er nu i MySQL, MongoDB og Neo4j.");