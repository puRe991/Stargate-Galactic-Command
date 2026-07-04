using System;
namespace StargateGalacticCommand.Core.Models { public class AllianceMember { public int Id{get;set;} public int AllianceId{get;set;} public Alliance Alliance{get;set;} public int UserId{get;set;} public User User{get;set;} public AllianceRank Rank{get;set;} public DateTime JoinedAtUtc{get;set;} } }
