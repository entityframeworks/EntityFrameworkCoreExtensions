﻿// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace AutoModelSample
{
    using System.Reflection;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.Extensions.DependencyInjection;

    using EntityFrameworkCoreExtensions;
    using EntityFrameworkCoreExtensions.Builder;

    public class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<CatalogDbContext>(options => options.UseInMemoryDatabase())
                .AddEntityFrameworkCoreExtensions(b => b
                    .AddModelBuildersFromAssembly(typeof(Program).GetTypeInfo().Assembly)
                    .AddAutoModel()
                );
            var provider = services.BuildServiceProvider();

            var context = provider.GetService<CatalogDbContext>();

            var product = new Product { Name = "Test Product" };
            product.Variants = new List<ProductVariant>()
            {
                new ProductVariant { Name = "Test Product Variant" }
            };

            context.Add(product);
            context.SaveChanges();
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ProductVariant> Variants { get; set; }
    }

    public class ProductVariant
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }

    [AutoModel]
    public class CatalogDbContext : ExtendedDbContext
    {
        public CatalogDbContext(IEnumerable<IDbContextHook> hooks, DbContextOptions options) : base(hooks, options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductVariant> Variants { get; set; }
    }

    public class ProductEntityTypeBuilder : EntityTypeBuilderBase<Product>
    {
        public override void BuildEntity(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

            builder.HasMany(p => p.Variants)
                .WithOne(pv => pv.Product)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(pv => pv.ProductId);
        }
    }

    public class ProductVariantEntityTypeBuilder : EntityTypeBuilderBase<ProductVariant>
    {
        public override void BuildEntity(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.HasAlternateKey(pv => pv.Id);
            builder.Property(pv => pv.Name).IsRequired().HasMaxLength(200);
        }
    }
}
