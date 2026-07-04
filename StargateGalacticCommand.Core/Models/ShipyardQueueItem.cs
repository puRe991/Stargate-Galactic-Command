using System;
namespace StargateGalacticCommand.Core.Models
{
 public class ShipyardQueueItem{ public int Id{get;set;} public int PlayerBaseId{get;set;} public PlayerBase PlayerBase{get;set;} public ShipType ShipType{get;set;} public int Quantity{get;set;} public DateTime StartedAtUtc{get;set;} public DateTime CompletesAtUtc{get;set;} }
}
