using System;
namespace StargateGalacticCommand.Core.Models
{
 public class FleetReport{ public int Id{get;set;} public int UserId{get;set;} public User User{get;set;} public int? FleetMovementId{get;set;} public FleetMovement FleetMovement{get;set;} public DateTime CreatedAtUtc{get;set;} public string Title{get;set;} public string Body{get;set;} }
}
