using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ResourceService
    {
        public bool HasEnough(ResourceStock stock, BuildCost cost)
        {
            if (stock == null) throw new ArgumentNullException("stock");
            if (cost == null) throw new ArgumentNullException("cost");
            return stock.Naquadah >= cost.Naquadah && stock.Trinium >= cost.Trinium && stock.Supplies >= cost.Supplies && stock.Energy >= cost.Energy && stock.Personnel >= cost.Personnel && stock.Intel >= cost.Intel;
        }

        public void Spend(ResourceStock stock, BuildCost cost)
        {
            if (!HasEnough(stock, cost)) throw new InvalidOperationException("Not enough resources.");
            stock.Naquadah -= cost.Naquadah;
            stock.Trinium -= cost.Trinium;
            stock.Supplies -= cost.Supplies;
            stock.Energy -= cost.Energy;
            stock.Personnel -= cost.Personnel;
            stock.Intel -= cost.Intel;
        }
    }
}
