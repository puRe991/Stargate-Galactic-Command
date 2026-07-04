using System;
namespace StargateGalacticCommand.Core.Models
{
 public class FleetMovement{ public int Id{get;set;} public int UserId{get;set;} public int OriginBaseId{get;set;} public PlayerBase OriginBase{get;set;} public int TargetBaseId{get;set;} public PlayerBase TargetBase{get;set;} public FleetMissionType MissionType{get;set;} public FleetMovementStatus Status{get;set;} public ShipType ShipType{get;set;} public int ShipCount{get;set;} public int Naquadah{get;set;} public int Trinium{get;set;} public int Supplies{get;set;} public int Energy{get;set;} public int Personnel{get;set;} public DateTime StartedAtUtc{get;set;} public DateTime ArrivesAtUtc{get;set;} public DateTime? CompletedAtUtc{get;set;} public int Distance{get;set;} public int FuelCost{get;set;} }
}
