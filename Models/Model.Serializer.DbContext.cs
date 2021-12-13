using Microsoft.EntityFrameworkCore;
using System;

namespace Meep.Tech.Data {

  public partial class Model {
    public partial class Serializer {

      /// <summary>
      /// The default db context class for the model serializer.
      /// If you use your own, make sure to call modelBuilder.SetUpEcsbamModels();
      /// </summary>
      public class DbContext : Microsoft.EntityFrameworkCore.DbContext {

        Action<DbContextOptionsBuilder> _onConfiguring {
          get;
        }

        /// <summary>
        /// The universe this is for
        /// </summary>
        Universe _universe {
          get;
        }
        public DbContext(Action<DbContextOptionsBuilder> onConfiguring, DbContextOptions<DbContext> options, Universe universe)
            : base(options) {
          _onConfiguring = onConfiguring;
          _universe = universe;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
          base.OnModelCreating(modelBuilder);
          modelBuilder.SetUpEcsbamModels(_universe);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
          base.OnConfiguring(optionsBuilder);
          _onConfiguring(optionsBuilder);
        }
      }
    }
  }
}