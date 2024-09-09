using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StargateTests
{
    public static class DbSetMockingHelper
    {
        public static Mock<DbSet<TEntity>> CreateDbSetMock<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            var queryable = entities.AsQueryable();

            var dbSetMock = new Mock<DbSet<TEntity>>();

            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            dbSetMock.Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
                     .Callback<TEntity, CancellationToken>((entity, cancellationToken) =>
                     {
                         var list = entities.ToList();
                         list.Add(entity);
                         queryable = list.AsQueryable();
                     })
                     .Returns((TEntity entity, CancellationToken cancellationToken) =>
                         Task.FromResult((EntityEntry<TEntity>)null));

            return dbSetMock;
        }
    }
}
