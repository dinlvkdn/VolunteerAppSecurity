using Org.BouncyCastle.Bcpg;
using VolunteerAppSecurity.DataAccess;
using VolunteerAppSecurity.Models;

namespace VolunteerAppSecurity
{
    public class DataSeeder
    {
        private readonly SecurityDBContext  _dbContext;

        public DataSeeder(SecurityDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Seed()
        {
            if(!_dbContext.Users.Any()) 
            {
                var user = new List<User>()
                {
                    new User()
                    {
                        Id = new Guid("78ae91f4-6d5e-40cc-bc60-e27e84219661"),
                        UserName = "nalivaykodiana@gmail.com",
                        Email = "nalivaykodiana@gmail.com"
                    },
                    new User()
                    {
                        Id = new Guid("78ae91f4-6d5e-40cc-bc60-e27e84219662"),
                        UserName = "nalivaykodiana@gmail.com",
                        Email = "nalivaykodiana@gmail.com"
                    },
                     new User()
                    {
                         Id = new Guid("78ae91f4-6d5e-40cc-bc60-e27e84219663"),
                        UserName = "nalivaykodiana@gmail.com",
                        Email = "nalivaykodiana@gmail.com"
                    }
                };
                _dbContext.Users.AddRange(user);
                _dbContext.SaveChanges();
            }

        }    
    }
}
