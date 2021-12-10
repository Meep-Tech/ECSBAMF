using Microsoft.EntityFrameworkCore;

namespace Meep.Tech.Data {

  public partial class Model {
    public static partial class Serializer {
      /// <summary>
      /// The default db context class for the model serializer.
      /// If you use your own, make sure to call modelBuilder.SetUpEcsbamModels();
      /// </summary>
      public class DbContext : Microsoft.EntityFrameworkCore.DbContext {
        internal DbContext(DbContextOptions<DbContext> options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
          base.OnModelCreating(modelBuilder);
          modelBuilder.SetUpEcsbamModels();
        }
      }
    }
  }
}