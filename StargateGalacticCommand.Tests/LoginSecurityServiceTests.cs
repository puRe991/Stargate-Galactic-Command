using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class LoginSecurityServiceTests
    {
        private static GameDbContext CreateDb(SqliteConnection connection)
        {
            connection.Open();
            var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
            var db = new GameDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        [Fact]
        public void CheckLockout_NoPriorAttempts_IsNotLockedOut()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            using (var db = CreateDb(connection))
            {
                var service = new LoginSecurityService();
                var status = service.CheckLockout(db.LoginAttempts, 1, "ip-a", "commander", DateTime.UtcNow);
                Assert.False(status.IsLockedOut);
                Assert.Null(status.RetryAfterUtc);
            }
        }

        [Fact]
        public void CheckLockout_FiveFailedAttemptsWithin15Minutes_LocksAccount()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            using (var db = CreateDb(connection))
            {
                var service = new LoginSecurityService();
                var now = DateTime.UtcNow;
                for (int i = 0; i < 5; i++)
                    db.LoginAttempts.Add(new LoginAttempt { ServerId = 1, IpHash = "ip-a", UsernameKey = "commander", Succeeded = false, AttemptedAtUtc = now.AddMinutes(-i) });
                db.SaveChanges();

                var status = service.CheckLockout(db.LoginAttempts, 1, "ip-b", "commander", now);
                Assert.True(status.IsLockedOut);
                Assert.True(status.RetryAfterUtc > now);
            }
        }

        [Fact]
        public void CheckLockout_SuccessfulAttemptsDoNotCountTowardsLockout()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            using (var db = CreateDb(connection))
            {
                var service = new LoginSecurityService();
                var now = DateTime.UtcNow;
                for (int i = 0; i < 10; i++)
                    db.LoginAttempts.Add(new LoginAttempt { ServerId = 1, IpHash = "ip-a", UsernameKey = "commander", Succeeded = true, AttemptedAtUtc = now.AddMinutes(-i) });
                db.SaveChanges();

                var status = service.CheckLockout(db.LoginAttempts, 1, "ip-a", "commander", now);
                Assert.False(status.IsLockedOut);
            }
        }

        [Fact]
        public void CheckLockout_AttemptsOutsideWindow_AreIgnored()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            using (var db = CreateDb(connection))
            {
                var service = new LoginSecurityService();
                var now = DateTime.UtcNow;
                for (int i = 0; i < 5; i++)
                    db.LoginAttempts.Add(new LoginAttempt { ServerId = 1, IpHash = "ip-a", UsernameKey = "commander", Succeeded = false, AttemptedAtUtc = now.AddHours(-2).AddMinutes(-i) });
                db.SaveChanges();

                var status = service.CheckLockout(db.LoginAttempts, 1, "ip-b", "commander", now);
                Assert.False(status.IsLockedOut);
            }
        }

        [Fact]
        public void CheckLockout_ManyFailuresFromSameIpAcrossDifferentAccounts_LocksIp()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            using (var db = CreateDb(connection))
            {
                var service = new LoginSecurityService();
                var now = DateTime.UtcNow;
                for (int i = 0; i < 20; i++)
                    db.LoginAttempts.Add(new LoginAttempt { ServerId = 1, IpHash = "attacker-ip", UsernameKey = "victim" + i, Succeeded = false, AttemptedAtUtc = now.AddMinutes(-i * 0.1) });
                db.SaveChanges();

                var status = service.CheckLockout(db.LoginAttempts, 1, "attacker-ip", "some-new-target", now);
                Assert.True(status.IsLockedOut);
            }
        }

        [Fact]
        public void CheckLockout_DifferentServer_IsIsolated()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            using (var db = CreateDb(connection))
            {
                var service = new LoginSecurityService();
                var now = DateTime.UtcNow;
                for (int i = 0; i < 5; i++)
                    db.LoginAttempts.Add(new LoginAttempt { ServerId = 1, IpHash = "ip-a", UsernameKey = "commander", Succeeded = false, AttemptedAtUtc = now.AddMinutes(-i) });
                db.SaveChanges();

                var status = service.CheckLockout(db.LoginAttempts, 2, "ip-a", "commander", now);
                Assert.False(status.IsLockedOut);
            }
        }

        [Fact]
        public void NormalizeUsernameKey_TrimsAndLowercases()
        {
            Assert.Equal("commander", LoginSecurityService.NormalizeUsernameKey("  Commander  "));
        }

        [Fact]
        public void HashIp_IsDeterministicAndDoesNotReturnPlainText()
        {
            var hash = LoginSecurityService.HashIp("203.0.113.42");
            Assert.Equal(hash, LoginSecurityService.HashIp("203.0.113.42"));
            Assert.DoesNotContain("203.0.113.42", hash);
        }

        [Fact]
        public void HashIp_NullOrEmpty_ReturnsUnknown()
        {
            Assert.Equal("unknown", LoginSecurityService.HashIp(null));
            Assert.Equal("unknown", LoginSecurityService.HashIp(""));
        }
    }
}
