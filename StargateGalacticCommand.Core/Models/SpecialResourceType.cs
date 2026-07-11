namespace StargateGalacticCommand.Core.Models
{
    // Naquadah und Trinium bleiben die bestehenden Kern-Ressourcen (ResourceStock/BuildCost/TradeResourceType)
    // und werden hier bewusst nicht dupliziert. Dieser Katalog deckt alle übrigen Stargate-Ressourcen ab,
    // die als Sonderbestand pro Basis geführt werden (siehe PlayerBaseSpecialResource/SpecialResourceService).
    public enum SpecialResourceType
    {
        // Rohstoffe
        LiquidNaquadah = 1,
        Naquadria = 2,
        AsgardAlloy = 3,
        Kassa = 4,

        // Kristalle
        DakaraCrystals = 10,
        AncientCrystals = 11,
        ControlCrystals = 12,
        StorageCrystals = 13,
        WraithCrystals = 14,
        OriCrystals = 15,

        // Energiequellen
        ZeroPointModule = 20,
        NaquadahReactor = 21,
        NaquadriaReactor = 22,
        ArcturusEnergy = 23,
        SolarEnergy = 24,
        GeothermalEnergy = 25,
        FusionEnergy = 26,
        AncientEnergyCells = 27,
        WraithBioenergy = 28,

        // Lebensformen
        Humans = 30,
        Jaffa = 31,
        GoauldSymbiotes = 32,
        TokraSymbiotes = 33,
        AsgardClones = 34,
        Wraith = 35,
        IratusBugs = 36,
        Unas = 37,
        Nox = 38,

        // Technologie
        FurlingTechnology = 40,
        AncientTechnology = 41,
        AsgardTechnology = 42,
        GoauldTechnology = 43,
        WraithTechnology = 44,
        OriTechnology = 45,
        ReplicatorTechnology = 46,
        AncientAiSystems = 47,
        AsuranTechnology = 48,
        GoauldSarcophagus = 49,
        CommunicationStones = 50,

        // Wissensarchive
        AncientDatabases = 60,
        AsgardDatabases = 61,
        GoauldKnowledgeArchives = 62,
        OriKnowledge = 63,
        StargateAddresses = 64,
        StarCharts = 65,
        DnaDatabases = 66,

        // Waffensysteme
        DroneWeapons = 70,
        StaffWeapons = 71,
        ZatNikTel = 72,
        GoauldHandDevices = 73,

        // Schiffssysteme
        ShieldGenerators = 80,
        Hyperdrives = 81,
        IntergalacticHyperdrives = 82,
        RingTransporters = 83,
        BeamingTechnology = 84,
        CloakingSystems = 85,
        Sensors = 86,
        AncientShields = 87,

        // Medizinisch/Biologisch
        Tritonin = 90,
        WraithHiveBiomass = 91,
        WraithFood = 92,
        GeneticSamples = 93,
        HealingPlants = 94,
        MedicalCompounds = 95,

        // Logistik/Grundversorgung
        Food = 100,
        Water = 101,
        Oxygen = 102,
        Fuel = 103,
        SpareParts = 104,
        IndustrialGoods = 105,
        TradeGoods = 106,

        // Personal
        Colonists = 110,
        Scientists = 111,
        Engineers = 112,
        Soldiers = 113,
        Pilots = 114,
        Officers = 115,
        IntelligencePersonnel = 116,

        // Abstrakte Kontrollpunkte
        BuildCapacity = 120,
        ResearchPoints = 121,
        Influence = 122,
        Prestige = 123,
        ControlPoints = 124,
        PlanetaryInfrastructure = 125,
        ShipyardCapacity = 126,
        ProductionCapacity = 127,

        // Artefakte
        AncientArtifacts = 130,
        GoauldArtifacts = 131,
        OriArtifacts = 132,
        AsgardArtifacts = 133,
        WraithArtifacts = 134,
        FurlingArtifacts = 135
    }
}
