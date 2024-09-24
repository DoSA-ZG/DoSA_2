
//using Microsoft.EntityFrameworkCore;
//using RPPP_WebApp.ModelsPartial;

//namespace RPPP_WebApp.Models;

//public partial class RPPP13Context
//{
//    //public DbSet<PlantsDenorm> TenFirstPlotResults { get; set; }
//    //public DbSet<PlantsDenorm> PlantsDenorm { get; set; }
//    //public IQueryable<PlantsDenorm> TenFirstPlot(int count) =>
//    //    FromExpression(() => TenFirstPlotResults.Take(count));


//    //public IQueryable<PlantsDenorm> TenFirstPlot(int count)
//    //{
//    //    var sql = $"SELECT TOP {count} * FROM fn_TenFirstPlot";
//    //    return PlantsDenorm
//    //}


//    //partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
//    //{
//    //    modelBuilder.Entity<PlantsDenorm>(entity =>
//    //    {
//    //        entity.HasNoKey();
//    //    });

//    //    modelBuilder.HasDbFunction(typeof(RPPP13Context).GetMethod(nameof(TenFirstPlot), new[] { typeof(int) }))
//    //                .HasName("fn_TenFirstPlot");
//    //    //modelBuilder.Entity<RPPP13Context>();
//    //}
////    public List<PlantsDenorm> GetPlantsDenormData(int count)
////    {
////        var sql = $"SELECT TOP {count} * FROM Plot"; // Replace YourTable with the actual table name or query

////        return Set<PlantsDenorm>().FromSqlRaw(sql).ToList();
////    }
//}
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.ModelsPartial;
namespace RPPP_WebApp.Models;

public partial class RPPP13Context
{
    public IQueryable<PlantsDenorm> BiggestPurchases(int count) =>
    FromExpression(() => BiggestPurchases(count));
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlantsDenorm>(entity =>
        {
            entity.HasNoKey();
        });
        modelBuilder.HasDbFunction(typeof(RPPP13Context).GetMethod(
        nameof(BiggestPurchases), new[] { typeof(int) }))
        .HasName("fn_BiggestPurchases");
    }
}