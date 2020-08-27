namespace SteppingStone.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mg1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        ActivityId = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        ParentId = c.Int(),
                        StudentId = c.Int(),
                        MessageId = c.Int(),
                        EventId = c.Int(),
                        BankId = c.Int(),
                        ClassLevelId = c.Int(),
                        PaymentId = c.Int(),
                        ExpenseId = c.Int(),
                        TermId = c.Int(),
                        Title = c.String(maxLength: 128),
                        Description = c.String(),
                        Recorded = c.DateTime(nullable: false),
                        RecordedById = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ActivityId)
                .ForeignKey("dbo.AspNetUsers", t => t.RecordedById)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.RecordedById);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DisplayId = c.Int(nullable: false, identity: true),
                        Title = c.String(maxLength: 4),
                        FirstName = c.String(maxLength: 60),
                        LastName = c.String(maxLength: 50),
                        DOB = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Banks",
                c => new
                    {
                        BankId = c.Int(nullable: false, identity: true),
                        AccountNo = c.String(maxLength: 20),
                        Name = c.String(maxLength: 200),
                        DeActivated = c.DateTime(),
                    })
                .PrimaryKey(t => t.BankId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        TermId = c.Int(nullable: false),
                        ClassLevelId = c.Int(nullable: false),
                        Amount = c.Double(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Recorded = c.DateTime(nullable: false),
                        BankId = c.Int(nullable: false),
                        SlipNo = c.Int(nullable: false),
                        Method = c.Int(nullable: false),
                        PaidInBy = c.String(maxLength: 200),
                        Deleted = c.DateTime(),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Banks", t => t.BankId, cascadeDelete: true)
                .ForeignKey("dbo.ClassLevels", t => t.ClassLevelId, cascadeDelete: true)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .ForeignKey("dbo.Terms", t => t.TermId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.TermId)
                .Index(t => t.ClassLevelId)
                .Index(t => t.BankId);
            
            CreateTable(
                "dbo.ClassLevels",
                c => new
                    {
                        ClassLevelId = c.Int(nullable: false, identity: true),
                        Level = c.Int(nullable: false),
                        StudyMode = c.Int(nullable: false),
                        SchoolFee = c.Double(nullable: false),
                        SchoolLevel = c.Int(nullable: false),
                        HalfDay = c.Boolean(nullable: false),
                        Added = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                        Deleted = c.DateTime(),
                    })
                .PrimaryKey(t => t.ClassLevelId);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        StudentId = c.Int(nullable: false, identity: true),
                        CurrentLevelId = c.Int(),
                        CurrentTermId = c.Int(),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 60),
                        DOB = c.DateTime(),
                        Gender = c.Int(nullable: false),
                        Transport = c.Double(nullable: false),
                        Swimming = c.Double(nullable: false),
                        Uniforms = c.Double(nullable: false),
                        Medical = c.Double(nullable: false),
                        BreaktimeFee = c.Double(nullable: false),
                        ClubFee = c.Double(nullable: false),
                        RegistrationFee = c.Double(nullable: false),
                        HalfDay = c.Boolean(nullable: false),
                        StudyMode = c.Int(nullable: false),
                        Stream = c.Int(nullable: false),
                        Terminated = c.DateTime(),
                        OldDebt = c.Double(nullable: false),
                        Joined = c.DateTime(nullable: false),
                        Balance = c.Double(nullable: false),
                        Dp = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.StudentId)
                .ForeignKey("dbo.ClassLevels", t => t.CurrentLevelId)
                .ForeignKey("dbo.Terms", t => t.CurrentTermId)
                .Index(t => t.CurrentLevelId)
                .Index(t => t.CurrentTermId);
            
            CreateTable(
                "dbo.Terms",
                c => new
                    {
                        TermId = c.Int(nullable: false, identity: true),
                        Year = c.Int(nullable: false),
                        Position = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsCurrentTerm = c.Boolean(nullable: false),
                        Added = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                        Deleted = c.DateTime(),
                    })
                .PrimaryKey(t => t.TermId);
            
            CreateTable(
                "dbo.StudentParents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Parents", t => t.ParentId, cascadeDelete: true)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.Parents",
                c => new
                    {
                        ParentId = c.Int(nullable: false, identity: true),
                        Notified = c.DateTime(),
                        RemindDate = c.DateTime(),
                        RemindCount = c.Int(nullable: false),
                        Added = c.DateTime(nullable: false),
                        Terminated = c.DateTime(),
                        Title = c.String(maxLength: 4),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 60),
                        Email = c.String(maxLength: 80),
                        PhoneNumber = c.String(maxLength: 20),
                        Mobile = c.String(maxLength: 20),
                        Organisation = c.String(maxLength: 80),
                        JobTitle = c.String(maxLength: 80),
                        Line1 = c.String(maxLength: 50),
                        Line2 = c.String(maxLength: 50),
                        Line3 = c.String(maxLength: 50),
                        Line4 = c.String(maxLength: 50),
                        Line5 = c.String(maxLength: 50),
                        PostCode = c.String(maxLength: 12),
                        CountryId = c.Int(),
                    })
                .PrimaryKey(t => t.ParentId)
                .ForeignKey("dbo.Countries", t => t.CountryId)
                .Index(t => t.CountryId);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        CountryId = c.Int(nullable: false, identity: true),
                        CountryName = c.String(nullable: false, maxLength: 80),
                    })
                .PrimaryKey(t => t.CountryId);
            
            CreateTable(
                "dbo.StudentEvents",
                c => new
                    {
                        StudentEventId = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StudentEventId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        EventDate = c.DateTime(),
                        Description = c.String(maxLength: 200),
                        NotificationDate = c.DateTime(),
                        Notified = c.DateTime(),
                        StudentList = c.String(),
                        Added = c.DateTime(nullable: false),
                        Cancelled = c.Boolean(nullable: false),
                        IsGeneral = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventId);
            
            CreateTable(
                "dbo.Expenses",
                c => new
                    {
                        ExpenseId = c.Int(nullable: false, identity: true),
                        Amount = c.Double(nullable: false),
                        Description = c.String(maxLength: 1000),
                        Category = c.Int(nullable: false),
                        By = c.String(maxLength: 100),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ExpenseId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        MessageDescription = c.String(maxLength: 160),
                        Scheduled = c.DateTime(),
                        SendNow = c.Boolean(nullable: false),
                        Sent = c.DateTime(),
                        Failed = c.DateTime(),
                        Added = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MessageId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Payments", "TermId", "dbo.Terms");
            DropForeignKey("dbo.Payments", "StudentId", "dbo.Students");
            DropForeignKey("dbo.Payments", "ClassLevelId", "dbo.ClassLevels");
            DropForeignKey("dbo.StudentEvents", "StudentId", "dbo.Students");
            DropForeignKey("dbo.StudentEvents", "EventId", "dbo.Events");
            DropForeignKey("dbo.StudentParents", "StudentId", "dbo.Students");
            DropForeignKey("dbo.StudentParents", "ParentId", "dbo.Parents");
            DropForeignKey("dbo.Parents", "CountryId", "dbo.Countries");
            DropForeignKey("dbo.Students", "CurrentTermId", "dbo.Terms");
            DropForeignKey("dbo.Students", "CurrentLevelId", "dbo.ClassLevels");
            DropForeignKey("dbo.Payments", "BankId", "dbo.Banks");
            DropForeignKey("dbo.Activities", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Activities", "RecordedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.StudentEvents", new[] { "EventId" });
            DropIndex("dbo.StudentEvents", new[] { "StudentId" });
            DropIndex("dbo.Parents", new[] { "CountryId" });
            DropIndex("dbo.StudentParents", new[] { "ParentId" });
            DropIndex("dbo.StudentParents", new[] { "StudentId" });
            DropIndex("dbo.Students", new[] { "CurrentTermId" });
            DropIndex("dbo.Students", new[] { "CurrentLevelId" });
            DropIndex("dbo.Payments", new[] { "BankId" });
            DropIndex("dbo.Payments", new[] { "ClassLevelId" });
            DropIndex("dbo.Payments", new[] { "TermId" });
            DropIndex("dbo.Payments", new[] { "StudentId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Activities", new[] { "RecordedById" });
            DropIndex("dbo.Activities", new[] { "UserId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Messages");
            DropTable("dbo.Expenses");
            DropTable("dbo.Events");
            DropTable("dbo.StudentEvents");
            DropTable("dbo.Countries");
            DropTable("dbo.Parents");
            DropTable("dbo.StudentParents");
            DropTable("dbo.Terms");
            DropTable("dbo.Students");
            DropTable("dbo.ClassLevels");
            DropTable("dbo.Payments");
            DropTable("dbo.Banks");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Activities");
        }
    }
}
