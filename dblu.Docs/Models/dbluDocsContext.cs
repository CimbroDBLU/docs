using Microsoft.Extensions.Configuration;
using System;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata;

namespace dblu.Docs.Models
{
    public partial class dbluDocsContext //: DbContext
    {
        public string Connessione { get; set; } 
        public dbluDocsContext(IConfiguration config)
        {
            Connessione = config.GetConnectionString("dblu.Docs");
        }

        //string connessione = "Server=192.168.44.120\\sql2017;uid=sa;pwd=sa;Database=dbluDocs;";
        //        public dbluDocsContext()
        //        {
        //            Connessione = "";
        //        }
        //        public dbluDocsContext(string Conn)
        //        {
        //            Connessione = Conn;
        //        }
        //        public dbluDocsContext(DbContextOptions<dbluDocsContext> options)
        //            : base(options)
        //        {
        //            Connessione = this.Database.GetDbConnection().ConnectionString;
        //        }

        //        public virtual DbSet<Soggetti> Soggetti { get; set; }
        //        public virtual DbSet<Allegati> Allegati { get; set; }
        //        public virtual DbSet<Categorie> Categorie { get; set; }
        //        public virtual DbSet<Elementi> Elementi { get; set; }
        //        public virtual DbSet<EmailSoggetti> EmailSoggetti { get; set; }
        //        public virtual DbSet<EmailServer> EmailServer { get; set; }
        //        public virtual DbSet<Fascicoli> Fascicoli { get; set; }
        //        public virtual DbSet<TipiAllegati> TipiAllegati { get; set; }
        //        public virtual DbSet<TipiElementi> TipiElementi { get; set; }
        //        public virtual DbSet<VDocs> VDocs { get; set; }

        //        // Unable to generate entity type for table 'dbo.emailFT'. Please see the warning messages.

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //        {
        //            if (!optionsBuilder.IsConfigured)
        //            {
        //                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //                //                optionsBuilder.UseSqlServer("Server=.\\sql17;Database=dbluDocs;uid=sa;pwd=as;");
        //                 optionsBuilder.UseSqlServer(Connessione);
        //            }
        //        }

        //        protected override void OnModelCreating(ModelBuilder modelBuilder)
        //        {

        //             modelBuilder.Entity<Allegati>(entity =>
        //            {
        //                entity.Property(e => e.Id)
        //                    .HasColumnName("ID")
        //                    .HasMaxLength(36);

        //                entity.Property(e => e.DataC)
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.DataUM)
        //                    .HasColumnName("DataUM")
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.Descrizione)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");


        //                entity.Property(e => e.IdElemento)
        //                    .HasColumnName("IDElemento")
        //                    .HasMaxLength(36);

        //                entity.Property(e => e.IdFascicolo)
        //                    .HasColumnName("IDFascicolo")
        //                    .HasMaxLength(36);

        //                entity.Property(e => e.NomeFile).HasMaxLength(255);

        //                entity.Property(e => e.Tipo)
        //                    .IsRequired()
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(c => c.Stato)
        //                    .HasConversion<Int16>()
        //                    .HasDefaultValueSql($"{(int)StatoAllegato.Attivo}");

        //                entity.Property(e => e._attributi)
        //                    .IsRequired()
        //                    .HasColumnName("Attributi")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e._note)
        //                    .IsRequired()
        //                    .HasColumnName("Note")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteC)
        //                    .IsRequired()
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteUM)
        //                    .IsRequired()
        //                    .HasColumnName("UtenteUM")
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.HasOne(d => d.IdFascicoloNavigation)
        //                    .WithMany(p => p.Allegati)
        //                    .HasForeignKey(d => d.IdFascicolo)
        //                    .OnDelete(DeleteBehavior.Cascade)
        //                    .HasConstraintName("FK_Allegati_Fascicoli");

        //                entity.HasOne(d => d.TipoNavigation)
        //                    .WithMany(p => p.Allegati)
        //                    .HasForeignKey(d => d.Tipo)
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_Allegati_TipiAllegati");

        //                entity.Property(e => e.Chiave1)
        //                .IsRequired()
        //                .HasMaxLength(255)
        //                .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave2)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave3)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave4)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave5)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");


        //                entity.Property(e => e.Origine)
        //                    .IsRequired()
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //            });

        //            modelBuilder.Entity<Categorie>(entity =>
        //            {
        //                entity.HasKey(e => e.Codice);

        //                entity.Property(e => e.Codice).HasMaxLength(20);

        //                entity.Property(e => e.Descrizione).HasMaxLength(255);

        //                entity.Property(e => e._listaattributi)
        //                    .IsRequired()
        //                    .HasColumnName("ListaAttributi")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.ViewAttributi).HasMaxLength(255)
        //                      .HasDefaultValueSql("('')");
        //            });

        //            modelBuilder.Entity<Elementi>(entity =>
        //            {
        //                entity.HasKey(e => new { e.Id, e.Revisione });

        //                entity.Property(e => e.Id)
        //                    .HasColumnName("ID")
        //                    .HasMaxLength(36);

        //                entity.Property(e => e._attributi)
        //                    .IsRequired()
        //                    .HasColumnName("Attributi")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.DataC)
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.DataUM)
        //                    .HasColumnName("DataUM")
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.Descrizione)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.IdFascicolo)
        //                    .HasColumnName("IDFascicolo")
        //                    .HasMaxLength(36);

        //                entity.Property(e => e.Tipo)
        //                    .IsRequired()
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteC)
        //                    .IsRequired()
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteUM)
        //                    .IsRequired()
        //                    .HasColumnName("UtenteUM")
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.HasOne(d => d.IdFascicoloNavigation)
        //                    .WithMany(p => p.Elementi)
        //                    .HasForeignKey(d => d.IdFascicolo)
        //                    .OnDelete(DeleteBehavior.Cascade)
        //                    .HasConstraintName("FK_Elementi_Fascicoli");

        //                entity.HasOne(d => d.TipoNavigation)
        //                    .WithMany(p => p.Elementi)
        //                    .HasForeignKey(d => d.Tipo)
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_Elementi_TipiElementi");

        //                entity.Property(e => e.Chiave1)
        //                   .IsRequired()
        //                   .HasMaxLength(255)
        //                   .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave2)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave3)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave4)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave5)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(c => c.Stato)
        //                .HasConversion<Int16>()
        //                .HasDefaultValueSql($"{(int)StatoElemento.Attivo}");
        //            });

        //            modelBuilder.Entity<EmailSoggetti>(entity =>
        //            {
        //                entity.HasKey(e => e.Email)
        //                    .HasName("PK_EmailSoggetti");


        //                entity.Property(e => e.Email)
        //                    .IsRequired()
        //                    .HasColumnName("email")
        //                    .HasMaxLength(255);
        ///*
        //           		entity.HasOne(d => d.CodiceSoggettoNavigation)
        //	                .WithMany(f => f.Emails)
        //	                .HasForeignKey(d => d.CodiceSoggetto)
        //	                .OnDelete(DeleteBehavior.ClientSetNull)
        //	                .HasConstraintName("FK_Email_Soggetti");
        //*/
        //            });

        //            modelBuilder.Entity<EmailServer>(entity =>
        //            {
        //                entity.HasKey(e => e.Nome)
        //                    .HasName("PK_EmailServer");

        //                entity.Property(e => e.Nome).HasMaxLength(50);

        //                entity.Property(e => e.Cartella)
        //                    .IsRequired()
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Email)
        //                    .IsRequired()
        //                    .HasColumnName("email")
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Intervallo).HasDefaultValueSql("((1000))");

        //                entity.Property(e => e.Password)
        //                    .IsRequired()
        //                    .HasMaxLength(50);

        //                entity.Property(e => e.Server)
        //                    .IsRequired()
        //                    .HasMaxLength(50);

        //                entity.Property(e => e.Ssl).HasColumnName("SSL");

        //                entity.Property(e => e.Utente)
        //                    .IsRequired()
        //                    .HasMaxLength(50);

        //                entity.Property(e => e.NomeProcesso)
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //            });

        //            modelBuilder.Entity<Fascicoli>(entity =>
        //            {
        //                entity.Property(e => e.Id)
        //                    .HasColumnName("ID")
        //                    .HasMaxLength(36);

        //                entity.Property(e => e._attributi)
        //                    .IsRequired()
        //                    .HasColumnName("Attributi")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Categoria)
        //                    .IsRequired()
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave1)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave2)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave3)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave4)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Chiave5)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.DataC)
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.DataUM)
        //                    .HasColumnName("DataUM")
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.Descrizione)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteC)
        //                    .IsRequired()
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteUM)
        //                    .IsRequired()
        //                    .HasColumnName("UtenteUM")
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.HasOne(d => d.CategoriaNavigation)
        //                    .WithMany(p => p.Fascicoli)
        //                    .HasForeignKey(d => d.Categoria)
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_Fascicoli_Categorie");

        //                //entity.HasOne(d => d.CodiceSoggettoNavigation)
        //                //    .WithMany(f => f.Fascicoli)
        //                //    .HasForeignKey(d => d.CodiceSoggetto )
        //                //    .OnDelete(DeleteBehavior.ClientSetNull)
        //                //    .HasConstraintName("FK_Fascicoli_Soggetti");

        //            });

        //            modelBuilder.Entity<TipiAllegati>(entity =>
        //            {
        //                entity.HasKey(e => e.Codice);

        //                entity.Property(e => e.Codice).HasMaxLength(20);

        //                entity.Property(e => e.Descrizione).HasMaxLength(255);

        //                entity.Property(e => e.Cartella).HasMaxLength(255);

        //                entity.Property(e => e._listaattributi)
        //                    .IsRequired()
        //                    .HasColumnName("ListaAttributi")
        //                    .HasDefaultValueSql("('')");
                
        //                entity.Property(e => e.ViewAttributi).HasMaxLength(255)
        //                      .HasDefaultValueSql("('')");
        //            });

        //            modelBuilder.Entity<TipiElementi>(entity =>
        //            {
        //                entity.HasKey(e => e.Codice);

        //                entity.Property(e => e.Codice).HasMaxLength(20);

        //                entity.Property(e => e.Descrizione).HasMaxLength(255);

        //                entity.Property(e => e.Categoria)
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("(NULL)");

        //                entity.Property(e => e._listaattributi)
        //                    .IsRequired()
        //                    .HasColumnName("ListaAttributi")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.ViewAttributi).HasMaxLength(255)
        //                      .HasDefaultValueSql("('')");


        //                entity.HasOne(d => d.CategoriaNavigation)
        //                .WithMany(t => t.TipiElementi)
        //                .HasForeignKey(d => d.Categoria)
        //                .OnDelete(DeleteBehavior.ClientSetNull)
        //                .HasConstraintName("FK_TipiElementi_Categorie");
        //            });

        //            modelBuilder.Entity<VDocs>(entity =>
        //            {
        //                entity.HasNoKey();

        //                entity.ToView("vDocs");

        //                entity.Property(e => e.CachedFileSize).HasColumnName("cached_file_size");

        //                entity.Property(e => e.CreationTime).HasColumnName("creation_time");

        //                entity.Property(e => e.FileStream).HasColumnName("file_stream");

        //                entity.Property(e => e.FileType)
        //                    .HasColumnName("file_type")
        //                    .HasMaxLength(255);

        //                entity.Property(e => e.IsArchive).HasColumnName("is_archive");

        //                entity.Property(e => e.IsDirectory).HasColumnName("is_directory");

        //                entity.Property(e => e.IsHidden).HasColumnName("is_hidden");

        //                entity.Property(e => e.IsOffline).HasColumnName("is_offline");

        //                entity.Property(e => e.IsReadonly).HasColumnName("is_readonly");

        //                entity.Property(e => e.IsSystem).HasColumnName("is_system");

        //                entity.Property(e => e.IsTemporary).HasColumnName("is_temporary");

        //                entity.Property(e => e.LastAccessTime).HasColumnName("last_access_time");

        //                entity.Property(e => e.LastWriteTime).HasColumnName("last_write_time");

        //                entity.Property(e => e.Name)
        //                    .IsRequired()
        //                    .HasColumnName("name")
        //                    .HasMaxLength(255);

        //                entity.Property(e => e.ParentPathLocator)
        //                    .HasColumnName("parent_path_locator")
        //                    .HasMaxLength(4000);

        //                entity.Property(e => e.PathLocator)
        //                    .HasColumnName("path_locator")
        //                    .HasMaxLength(4000);
                
        //                entity.Property(e => e.StreamId).HasColumnName("stream_id");
        //            });

        //            modelBuilder.Entity<Soggetti>(entity =>
        //            {
        //                //entity.Property(e => e.Id)
        //                //    .HasColumnName("ID")
        //                //    .HasDefaultValueSql("(newsequentialid())");

        //                entity.HasKey(e => e.Codice);

        //                entity.Property(e => e.Codice)
        //                    .IsRequired()
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e._attributi)
        //                     .HasColumnName("Attributi")
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Nome)
        //                    .IsRequired()
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Indirizzo)
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");
                
        //                entity.Property(e => e.CAP)
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Localita)
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Provincia)
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Nazione)
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Note)
        //                    .HasMaxLength(255)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.PartitaIVA)
        //                   .HasMaxLength(20)
        //                   .HasDefaultValueSql("('')");

        //                entity.Property(e => e.Nomignolo)
        //                    .HasMaxLength(20)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.DataC)
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.DataUM)
        //                    .HasColumnName("DataUM")
        //                    .HasColumnType("datetime")
        //                    .HasDefaultValueSql("(getdate())");

        //                entity.Property(e => e.UtenteC)
        //                    .IsRequired()
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //                entity.Property(e => e.UtenteUM)
        //                    .IsRequired()
        //                    .HasColumnName("UtenteUM")
        //                    .HasMaxLength(50)
        //                    .HasDefaultValueSql("('')");

        //            });

        //            OnModelCreatingPartial(modelBuilder);
        //        }

        //        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
