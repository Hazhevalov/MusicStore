using MusicStore.Data;
using Microsoft.EntityFrameworkCore;
using MusicStore.Helpers;

namespace MusicStore.Data
{
    public class DatabaseInitializer
    {
        // Applies database migrations and verifies that the database is available.
        public bool Initialize()
        {
            try
            {
                using var db = new ApplicationContext();

                Console.WriteLine("Connecting to the database...");

                db.Database.Migrate();

                // Older installations may contain passwords stored as plain text.
                // Upgrade those records after migration and before the first login.
                foreach (var user in db.Users.ToList())
                {
                    if (!PasswordSecurity.IsHash(user.PasswordHash))
                        user.PasswordHash = PasswordSecurity.Hash(user.PasswordHash);
                }
                db.SaveChanges();

                if (!db.Database.CanConnect())
                {
                    Console.WriteLine("Could not connect to the database.");
                    return false;
                }

                Console.WriteLine("The database is ready!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database connection error:");
                Console.WriteLine(ex.Message);

                return false;
            }
        }
    }
}
