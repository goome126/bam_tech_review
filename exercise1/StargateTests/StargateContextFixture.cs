using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StargateTests
{
    public class StargateContextFixture : IDisposable
    {
        public StargateContext Context { get; private set; }

        public StargateContextFixture()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new StargateContext(options);

            // Optionally seed data for testing
            SeedData();
        }

        private void SeedData()
        {
            


            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
