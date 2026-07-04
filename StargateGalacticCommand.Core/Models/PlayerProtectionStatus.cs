using System;
namespace StargateGalacticCommand.Core.Models { public class PlayerProtectionStatus { public int Id{get;set;} public int UserId{get;set;} public User User{get;set;} public DateTime ProtectedUntilUtc{get;set;} public int Score{get;set;} public DateTime? LastAttackedAtUtc{get;set;} public bool IsUnderBeginnerProtection(DateTime now)=>ProtectedUntilUtc>now; } }
