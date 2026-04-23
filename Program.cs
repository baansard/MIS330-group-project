using MySqlConnector;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Read connection string from environment variable (set in Railway)
var connStr = Environment.GetEnvironmentVariable("DATABASE_URL") ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddScoped(_ => new MySqlConnection(connStr));

var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// ─────────────────────────────────────────────
//  HELPER
// ─────────────────────────────────────────────
static async Task<List<Dictionary<string, object?>>> Query(MySqlConnection db, string sql, object? p = null)
{
    await db.OpenAsync();
    await using var cmd = db.CreateCommand();
    cmd.CommandText = sql;
    if (p != null)
        foreach (var prop in p.GetType().GetProperties())
            cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(p) ?? DBNull.Value);

    var rows = new List<Dictionary<string, object?>>();
    await using var rdr = await cmd.ExecuteReaderAsync();
    while (await rdr.ReadAsync())
    {
        var row = new Dictionary<string, object?>();
        for (int i = 0; i < rdr.FieldCount; i++)
            row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
        rows.Add(row);
    }
    return rows;
}

static async Task<long> Exec(MySqlConnection db, string sql, object? p = null)
{
    if (db.State != System.Data.ConnectionState.Open) await db.OpenAsync();
    await using var cmd = db.CreateCommand();
    cmd.CommandText = sql;
    if (p != null)
        foreach (var prop in p.GetType().GetProperties())
            cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(p) ?? DBNull.Value);
    await cmd.ExecuteNonQueryAsync();
    return cmd.LastInsertedId;
}

// ─────────────────────────────────────────────
//  TRIPS
// ─────────────────────────────────────────────
app.MapGet("/api/trips", async (MySqlConnection db) =>
{
    var rows = await Query(db, @"
        SELECT t.*,
               e.employeefirst, e.employeelast,
               COALESCE(SUM(CASE WHEN r.reservationstatus <> 'Cancelled' THEN r.spotsreserved ELSE 0 END),0) AS enrolledSpots
        FROM trip t
        LEFT JOIN employee e ON e.employeeid = t.empid
        LEFT JOIN reservationtrip rt ON rt.tripid = t.tripid
        LEFT JOIN reservation r ON r.reservationid = rt.reservationid
        GROUP BY t.tripid
        ORDER BY t.tripname");
    return Results.Ok(rows);
});

app.MapGet("/api/trips/{id:int}", async (int id, MySqlConnection db) =>
{
    var rows = await Query(db, @"
        SELECT t.*,
               e.employeefirst, e.employeelast,
               COALESCE(SUM(CASE WHEN r.reservationstatus <> 'Cancelled' THEN r.spotsreserved ELSE 0 END),0) AS enrolledSpots
        FROM trip t
        LEFT JOIN employee e ON e.employeeid = t.empid
        LEFT JOIN reservationtrip rt ON rt.tripid = t.tripid
        LEFT JOIN reservation r ON r.reservationid = rt.reservationid
        WHERE t.tripid = @id
        GROUP BY t.tripid", new { id });
    return rows.Count > 0 ? Results.Ok(rows[0]) : Results.NotFound();
});

app.MapPost("/api/trips", async (HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();

    // Auto-generate next ID
    var maxRows = await Query(db, "SELECT COALESCE(MAX(tripid),10000) + 1 AS nextId FROM trip");
    int nextId = Convert.ToInt32(maxRows[0]["nextId"]);

    await Exec(db, @"INSERT INTO trip (tripid,tripname,tripdate,tripstatus,starttime,tripdescription,maxcapacity,distancemiles,lengthhours,tripprice,street,city,state,zip,empid)
        VALUES (@tripid,@tripname,@tripdate,@tripstatus,@starttime,@tripdescription,@maxcapacity,@distancemiles,@lengthhours,@tripprice,@street,@city,@state,@zip,@empid)",
        new {
            tripid = nextId,
            tripname    = b["tripname"].GetString(),
            tripdate    = b["tripdate"].GetString(),
            tripstatus  = b.ContainsKey("tripstatus") ? b["tripstatus"].GetString() : "Scheduled",
            starttime   = b["starttime"].GetString(),
            tripdescription = b.ContainsKey("tripdescription") ? b["tripdescription"].GetString() : "",
            maxcapacity = b["maxcapacity"].GetInt32(),
            distancemiles = b["distancemiles"].GetDouble(),
            lengthhours = b["lengthhours"].GetInt32(),
            tripprice   = b["tripprice"].GetDouble(),
            street      = b.ContainsKey("street") ? b["street"].GetString() : "",
            city        = b["city"].GetString(),
            state       = b["state"].GetString(),
            zip         = b["zip"].GetString(),
            empid       = b["empid"].GetInt32()
        });

    return Results.Ok(new { tripid = nextId });
});

app.MapPut("/api/trips/{id:int}", async (int id, HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();

    await Exec(db, @"UPDATE trip SET
        tripname=@tripname, tripdate=@tripdate, tripstatus=@tripstatus, starttime=@starttime,
        tripdescription=@tripdescription, maxcapacity=@maxcapacity, distancemiles=@distancemiles,
        lengthhours=@lengthhours, tripprice=@tripprice, street=@street, city=@city,
        state=@state, zip=@zip, empid=@empid
        WHERE tripid=@tripid",
        new {
            tripid = id,
            tripname    = b["tripname"].GetString(),
            tripdate    = b["tripdate"].GetString(),
            tripstatus  = b["tripstatus"].GetString(),
            starttime   = b["starttime"].GetString(),
            tripdescription = b.ContainsKey("tripdescription") ? b["tripdescription"].GetString() : "",
            maxcapacity = b["maxcapacity"].GetInt32(),
            distancemiles = b["distancemiles"].GetDouble(),
            lengthhours = b["lengthhours"].GetInt32(),
            tripprice   = b["tripprice"].GetDouble(),
            street      = b.ContainsKey("street") ? b["street"].GetString() : "",
            city        = b["city"].GetString(),
            state       = b["state"].GetString(),
            zip         = b["zip"].GetString(),
            empid       = b["empid"].GetInt32()
        });

    return Results.Ok();
});

app.MapDelete("/api/trips/{id:int}", async (int id, MySqlConnection db) =>
{
    // Delete junction rows first
    await Exec(db, "DELETE FROM reservationtrip WHERE tripid=@id", new { id });
    await Exec(db, "DELETE FROM trip WHERE tripid=@id", new { id });
    return Results.Ok();
});

// ─────────────────────────────────────────────
//  AUTH  (login, register)
// ─────────────────────────────────────────────
app.MapPost("/api/login", async (HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();
    var username = b["username"].GetString() ?? "";
    var password = b["password"].GetString() ?? "";

    // Try employee first
    var empRows = await Query(db,
        "SELECT employeeid AS id, employeefirst AS firstName, employeelast AS lastName, employeeemail AS email, employeeuser AS username FROM employee WHERE employeeuser=@u AND employeepassword=@p",
        new { u = username, p = password });
    if (empRows.Count > 0)
    {
        var emp = empRows[0];
        emp["role"] = "Employee";
        return Results.Ok(emp);
    }

    // Try customer
    var custRows = await Query(db,
        "SELECT customerid AS id, custfirstname AS firstName, custlastname AS lastName, custemail AS email, custusername AS username, custphone AS phone FROM customer WHERE custusername=@u AND custpassword=@p",
        new { u = username, p = password });
    if (custRows.Count > 0)
    {
        var cust = custRows[0];
        cust["role"] = "Customer";
        return Results.Ok(cust);
    }

    return Results.Unauthorized();
});

app.MapPost("/api/register", async (HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();

    var username = b["username"].GetString() ?? "";
    var email    = b["email"].GetString() ?? "";

    // Check uniqueness
    var exists = await Query(db, "SELECT 1 FROM customer WHERE custusername=@u OR custemail=@e", new { u = username, e = email });
    if (exists.Count > 0) return Results.Conflict(new { error = "Username or email already in use." });

    var maxRows = await Query(db, "SELECT COALESCE(MAX(customerid),11114) + 1 AS nextId FROM customer");
    int nextId = Convert.ToInt32(maxRows[0]["nextId"]);

    await Exec(db, @"INSERT INTO customer (customerid,custfirstname,custlastname,custemail,custusername,custpassword,custphone)
        VALUES (@id,@fn,@ln,@email,@uname,@pwd,@phone)",
        new {
            id    = nextId,
            fn    = b["firstName"].GetString(),
            ln    = b["lastName"].GetString(),
            email = email,
            uname = username,
            pwd   = b["password"].GetString(),
            phone = b.ContainsKey("phone") ? b["phone"].GetString() : ""
        });

    return Results.Ok(new {
        id        = nextId,
        firstName = b["firstName"].GetString(),
        lastName  = b["lastName"].GetString(),
        email     = email,
        username  = username,
        role      = "Customer"
    });
});

// ─────────────────────────────────────────────
//  CUSTOMERS  (profile update)
// ─────────────────────────────────────────────
app.MapPut("/api/customers/{id:int}", async (int id, HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();

    await Exec(db, @"UPDATE customer SET custfirstname=@fn, custlastname=@ln, custemail=@email, custphone=@phone WHERE customerid=@id",
        new {
            id    = id,
            fn    = b["firstName"].GetString(),
            ln    = b["lastName"].GetString(),
            email = b["email"].GetString(),
            phone = b.ContainsKey("phone") ? b["phone"].GetString() : ""
        });
    return Results.Ok();
});

// ─────────────────────────────────────────────
//  EMPLOYEES  (list for dropdowns)
// ─────────────────────────────────────────────
app.MapGet("/api/employees", async (MySqlConnection db) =>
{
    var rows = await Query(db, "SELECT employeeid AS id, employeefirst AS firstName, employeelast AS lastName, employeeemail AS email, employeeuser AS username FROM employee ORDER BY employeelast");
    return Results.Ok(rows);
});

// ─────────────────────────────────────────────
//  RESERVATIONS
// ─────────────────────────────────────────────
app.MapGet("/api/reservations", async (HttpContext ctx, MySqlConnection db) =>
{
    // optional ?customerId=X filter
    var custIdStr = ctx.Request.Query["customerId"].FirstOrDefault();

    string sql = @"
        SELECT r.reservationid AS id, r.spotsreserved AS participants, r.reservationdate, r.reservationstatus,
               r.customerid,
               c.custfirstname AS firstName, c.custlastname AS lastName, c.custemail AS email, c.custphone AS phone,
               t.tripid, t.tripname, t.tripdate, t.tripprice,
               (r.spotsreserved * t.tripprice) AS total
        FROM reservation r
        JOIN customer c ON c.customerid = r.customerid
        LEFT JOIN reservationtrip rt ON rt.reservationid = r.reservationid
        LEFT JOIN trip t ON t.tripid = rt.tripid";

    if (!string.IsNullOrEmpty(custIdStr) && int.TryParse(custIdStr, out int custId))
        sql += " WHERE r.customerid = " + custId;

    sql += " ORDER BY r.reservationdate DESC";

    var rows = await Query(db, sql);
    return Results.Ok(rows);
});

app.MapPost("/api/reservations", async (HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();

    // Check capacity
    int tripId     = b["tripId"].GetInt32();
    int customerId = b["customerId"].GetInt32();
    int spots      = b["participants"].GetInt32();

    var capRows = await Query(db, @"
        SELECT t.maxcapacity,
               COALESCE(SUM(CASE WHEN r.reservationstatus <> 'Cancelled' THEN r.spotsreserved ELSE 0 END),0) AS enrolled
        FROM trip t
        LEFT JOIN reservationtrip rt ON rt.tripid = t.tripid
        LEFT JOIN reservation r ON r.reservationid = rt.reservationid
        WHERE t.tripid = @id
        GROUP BY t.tripid", new { id = tripId });

    if (capRows.Count == 0) return Results.NotFound(new { error = "Trip not found" });
    int max      = Convert.ToInt32(capRows[0]["maxcapacity"]);
    int enrolled = Convert.ToInt32(capRows[0]["enrolled"]);
    if (enrolled + spots > max) return Results.BadRequest(new { error = "Not enough spots available." });

    var maxResRows = await Query(db, "SELECT COALESCE(MAX(reservationid),0) + 1 AS nextId FROM reservation");
    int nextId = Convert.ToInt32(maxResRows[0]["nextId"]);

    await Exec(db, @"INSERT INTO reservation (reservationid,spotsreserved,reservationdate,reservationstatus,customerid)
        VALUES (@id,@spots,@rdate,@status,@custid)",
        new {
            id     = nextId,
            spots  = spots,
            rdate  = DateTime.Now.ToString("yyyy-MM-dd"),
            status = "Pending",
            custid = customerId
        });

    await Exec(db, "INSERT INTO reservationtrip (reservationid,tripid) VALUES (@rid,@tid)",
        new { rid = nextId, tid = tripId });

    return Results.Ok(new { id = nextId, reservationstatus = "Pending" });
});

app.MapPut("/api/reservations/{id:int}/status", async (int id, HttpContext ctx, MySqlConnection db) =>
{
    var b = await ctx.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    if (b == null) return Results.BadRequest();
    var status = b["status"].GetString();
    await Exec(db, "UPDATE reservation SET reservationstatus=@s WHERE reservationid=@id", new { s = status, id });
    return Results.Ok();
});

// ─────────────────────────────────────────────
//  FALLBACK  — serve index.html for all non-API routes
// ─────────────────────────────────────────────
app.MapFallbackToFile("index.html");

app.Run();
