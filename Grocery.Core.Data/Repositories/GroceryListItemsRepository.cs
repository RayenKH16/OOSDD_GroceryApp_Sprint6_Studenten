using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Grocery.Core.Data.Helpers;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : IGroceryListItemsRepository
    {
        private readonly string _connectionString;

        public GroceryListItemsRepository()
        {
            _connectionString = ConnectionHelper.ConnectionStringValue("GroceryAppDb")
                ?? throw new InvalidOperationException("Connection string 'GroceryAppDb' ontbreekt in appsettings.json.");

            using var conn = CreateConnection();
            conn.Open();
            EnsureSchema(conn);
            SeedIfEmpty(conn);
        }

        private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

        // ---------------- GET ALL ----------------
        public List<GroceryListItem> GetAll()
        {
            using var conn = CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT gli.Id, gli.GroceryListId, gli.ProductId, gli.Amount,
                       p.Id, p.Name, p.Price
                FROM GroceryListItems gli
                LEFT JOIN Products p ON p.Id = gli.ProductId
                ORDER BY gli.Id;
            ";

            var list = new List<GroceryListItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        // -------------- GET BY LIST --------------
        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            using var conn = CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT gli.Id, gli.GroceryListId, gli.ProductId, gli.Amount,
                       p.Id, p.Name, p.Price
                FROM GroceryListItems gli
                LEFT JOIN Products p ON p.Id = gli.ProductId
                WHERE gli.GroceryListId = @id
                ORDER BY gli.Id;
            ";
            cmd.Parameters.Add(new SqliteParameter("@id", groceryListId));

            var list = new List<GroceryListItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        // ----------------- ADD -----------------
        public GroceryListItem Add(GroceryListItem item)
        {
            using var conn = CreateConnection();
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO GroceryListItems (GroceryListId, ProductId, Amount)
                    VALUES (@listId, @prodId, @amount);
                ";
                cmd.Parameters.Add(new SqliteParameter("@listId", item.GroceryListId));
                cmd.Parameters.Add(new SqliteParameter("@prodId", item.ProductId));
                cmd.Parameters.Add(new SqliteParameter("@amount", item.Amount));
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_insert_rowid();";
                item.Id = Convert.ToInt32((long)cmd.ExecuteScalar()!);
            }

            return Get(item.Id)!;
        }

        // ---------------- GET ONE ----------------
        public GroceryListItem? Get(int id)
        {
            using var conn = CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT gli.Id, gli.GroceryListId, gli.ProductId, gli.Amount,
                       p.Id, p.Name, p.Price
                FROM GroceryListItems gli
                LEFT JOIN Products p ON p.Id = gli.ProductId
                WHERE gli.Id = @id;
            ";
            cmd.Parameters.Add(new SqliteParameter("@id", id));

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        // ---------------- UPDATE ----------------
        public GroceryListItem? Update(GroceryListItem item)
        {
            using var conn = CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE GroceryListItems
                SET GroceryListId = @listId,
                    ProductId = @prodId,
                    Amount = @amount
                WHERE Id = @id;
            ";
            cmd.Parameters.Add(new SqliteParameter("@listId", item.GroceryListId));
            cmd.Parameters.Add(new SqliteParameter("@prodId", item.ProductId));
            cmd.Parameters.Add(new SqliteParameter("@amount", item.Amount));
            cmd.Parameters.Add(new SqliteParameter("@id", item.Id));

            var changed = cmd.ExecuteNonQuery();
            return changed > 0 ? Get(item.Id) : null;
        }

        // ---------------- DELETE ----------------
        public GroceryListItem? Delete(GroceryListItem item)
        {
            using var conn = CreateConnection();
            conn.Open();

            var existing = Get(item.Id);
            if (existing is null) return null;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM GroceryListItems WHERE Id = @id;";
            cmd.Parameters.Add(new SqliteParameter("@id", item.Id));

            var changed = cmd.ExecuteNonQuery();
            return changed > 0 ? existing : null;
        }

        // ------------ MAP TO MODEL --------------
        private static GroceryListItem Map(SqliteDataReader r)
        {
            var id = r.GetInt32(0);
            var listId = r.GetInt32(1);
            var prodId = r.GetInt32(2);
            var amount = r.GetInt32(3);

            var item = new GroceryListItem(id, listId, prodId, amount);

            if (!r.IsDBNull(4))
            {
                var pId = r.GetInt32(4);
                var pName = r.GetString(5);
                var pPrice = Convert.ToDecimal(r.GetDouble(6));
                item.Product = new Product(pId, pName, 0, default, pPrice);
            }

            return item;
        }

        // ------------- SCHEMA CREATION ----------
        private static void EnsureSchema(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products(
                    Id      INTEGER PRIMARY KEY,
                    Name    TEXT NOT NULL,
                    Stock   INTEGER NOT NULL DEFAULT 0,
                    Price   REAL NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS GroceryListItems(
                    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    GroceryListId INTEGER NOT NULL,
                    ProductId     INTEGER NOT NULL,
                    Amount        INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY(ProductId) REFERENCES Products(Id)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // ------------- INITIAL DATA --------------
        private static void SeedIfEmpty(SqliteConnection conn)
        {
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Products;";
            var count = Convert.ToInt32((long)check.ExecuteScalar()!);
            if (count > 0) return;

            using var trans = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = trans;

            cmd.CommandText = @"
                INSERT INTO Products(Id, Name, Stock, Price)
                VALUES
                (1, 'Melk', 10, 1.19),
                (2, 'Brood', 15, 1.89),
                (3, 'Boter', 8, 2.49);

                INSERT INTO GroceryListItems(GroceryListId, ProductId, Amount)
                VALUES
                (1, 1, 3),
                (1, 2, 1),
                (1, 3, 4),
                (2, 1, 2),
                (2, 2, 5);
            ";
            cmd.ExecuteNonQuery();
            trans.Commit();
        }
    }
}
