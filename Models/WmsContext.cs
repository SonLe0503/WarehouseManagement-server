using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace warehouseManagement.Models;

public partial class WmsContext : DbContext
{
    public WmsContext()
    {
    }

    public WmsContext(DbContextOptions<WmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Approval> Approvals { get; set; }

    public virtual DbSet<ApprovalLog> ApprovalLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<InboundItem> InboundItems { get; set; }

    public virtual DbSet<InboundRequest> InboundRequests { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<OutboundItem> OutboundItems { get; set; }

    public virtual DbSet<OutboundRequest> OutboundRequests { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<StockTransferItem> StockTransferItems { get; set; }

    public virtual DbSet<StockTransferRequest> StockTransferRequests { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<UnitConversion> UnitConversions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Approval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Approval__3214EC079EEA8943");

            entity.Property(e => e.RefType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<ApprovalLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Approval__3214EC079D570F04");

            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.ActionAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Comment).HasMaxLength(255);

            entity.HasOne(d => d.ActionByNavigation).WithMany(p => p.ApprovalLogs)
                .HasForeignKey(d => d.ActionBy)
                .HasConstraintName("FK__ApprovalL__Actio__0A9D95DB");

            entity.HasOne(d => d.Approval).WithMany(p => p.ApprovalLogs)
                .HasForeignKey(d => d.ApprovalId)
                .HasConstraintName("FK__ApprovalL__Appro__09A971A2");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07CCCEB394");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Categorie__Paren__46E78A0C");
        });

        modelBuilder.Entity<InboundItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InboundI__3214EC075798DABA");

            entity.Property(e => e.LineNote).HasMaxLength(255);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StoragePosition).HasMaxLength(100);

            entity.HasOne(d => d.InboundRequest).WithMany(p => p.InboundItems)
                .HasForeignKey(d => d.InboundRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InboundIt__Inbou__6D0D32F4");

            entity.HasOne(d => d.Product).WithMany(p => p.InboundItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InboundIt__Produ__6E01572D");
        });

        modelBuilder.Entity<InboundRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InboundR__3214EC07AF35A290");

            entity.HasIndex(e => e.RequestNo, "UQ__InboundR__33A869A4045CBE22").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.RequestNo).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.SupplierName).HasMaxLength(150);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.InboundRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__InboundRe__Appro__6A30C649");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InboundRequestCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InboundRe__Creat__693CA210");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InboundRequests)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InboundRe__Wareh__68487DD7");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC07F523929A");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }, "UQ_Product_Warehouse").IsUnique();

            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StoragePosition).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventori__Produ__5DCAEF64");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventori__Wareh__5EBF139D");
        });

        modelBuilder.Entity<OutboundItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Outbound__3214EC0700E162EE");

            entity.Property(e => e.LineNote).HasMaxLength(255);
            entity.Property(e => e.PickedQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StoragePosition).HasMaxLength(100);

            entity.HasOne(d => d.OutboundRequest).WithMany(p => p.OutboundItems)
                .HasForeignKey(d => d.OutboundRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutboundI__Outbo__778AC167");

            entity.HasOne(d => d.Product).WithMany(p => p.OutboundItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutboundI__Produ__787EE5A0");
        });

        modelBuilder.Entity<OutboundRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Outbound__3214EC0780210F71");

            entity.HasIndex(e => e.RequestNo, "UQ__Outbound__33A869A40FF14DA6").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CustomerName).HasMaxLength(150);
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.RequestNo).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.OutboundRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__OutboundR__Appro__74AE54BC");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OutboundRequestCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutboundR__Creat__73BA3083");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.OutboundRequests)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutboundR__Wareh__72C60C4A");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC072D8DC5C3");

            entity.HasIndex(e => e.Sku, "UQ__Products__CA1ECF0D3196D262").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.BaseUnit).WithMany(p => p.Products)
                .HasForeignKey(d => d.BaseUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_BaseUnit");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Catego__5165187F");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0783C615EE");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6625A8735").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockMov__3214EC072F62B318");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.QuantityChange).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefType).HasMaxLength(50);
            entity.Property(e => e.StoragePosition).HasMaxLength(100);

            entity.HasOne(d => d.Product).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockMove__Produ__628FA481");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockMove__Wareh__6383C8BA");
        });

        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockTra__3214EC07B8B4D4CA");

            entity.Property(e => e.FromStoragePosition).HasMaxLength(100);
            entity.Property(e => e.LineNote).HasMaxLength(255);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ToStoragePosition).HasMaxLength(100);

            entity.HasOne(d => d.Product).WithMany(p => p.StockTransferItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockTran__Produ__03F0984C");

            entity.HasOne(d => d.StockTransferRequest).WithMany(p => p.StockTransferItems)
                .HasForeignKey(d => d.StockTransferRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockTran__Stock__02FC7413");
        });

        modelBuilder.Entity<StockTransferRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockTra__3214EC07DE6315E3");

            entity.HasIndex(e => e.TransferNo, "UQ__StockTra__9548BE629861B165").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransferNo).HasMaxLength(50);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.StockTransferRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__StockTran__Appro__00200768");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StockTransferRequestCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockTran__Creat__7F2BE32F");

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.StockTransferRequestFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockTran__FromW__7D439ABD");

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.StockTransferRequestToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockTran__ToWar__7E37BEF6");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Units__3214EC07DF9EB686");

            entity.HasIndex(e => e.Code, "UQ__Units__A25C5AA767180E21").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<UnitConversion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UnitConv__3214EC07806BF8FC");

            entity.HasIndex(e => new { e.ProductId, e.FromUnitId }, "UQ_Product_FromUnit").IsUnique();

            entity.Property(e => e.ConversionFactor).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.BaseUnit).WithMany(p => p.UnitConversionBaseUnits)
                .HasForeignKey(d => d.BaseUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UnitConversions_BaseUnit");

            entity.HasOne(d => d.FromUnit).WithMany(p => p.UnitConversionFromUnits)
                .HasForeignKey(d => d.FromUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UnitConversions_FromUnit");

            entity.HasOne(d => d.Product).WithMany(p => p.UnitConversions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_UnitConversions_Product");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC071C4F9126");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4A48DD635").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Users)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK_Users_Warehouse");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__RoleI__3F466844"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__UserI__3E52440B"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760ADE460EA6E");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC0768FE66B8");

            entity.HasIndex(e => e.Code, "UQ__Warehous__A25C5AA7BBD28221").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
