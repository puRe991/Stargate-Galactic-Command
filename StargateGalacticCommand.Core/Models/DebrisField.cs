using System;
namespace StargateGalacticCommand.Core.Models { public class DebrisField { public int Id{get;set;} public int PlayerBaseId{get;set;} public PlayerBase PlayerBase{get;set;} public int Naquadah{get;set;} public int Trinium{get;set;} public DateTime CreatedAtUtc{get;set;} public bool IsRecycled{get;set;} } }
