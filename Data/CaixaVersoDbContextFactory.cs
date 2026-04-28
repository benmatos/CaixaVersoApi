using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CaixaVersoApi.Data;

public class CaixaVersoDbContextFactory : IDesignTimeDbContextFactory<CaixaVersoDbContext>
{
    public CaixaVersoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CaixaVersoDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=CaixaVerso;Trusted_Connection=True;TrustServerCertificate=True;");
        return new CaixaVersoDbContext(optionsBuilder.Options);
    }
}
