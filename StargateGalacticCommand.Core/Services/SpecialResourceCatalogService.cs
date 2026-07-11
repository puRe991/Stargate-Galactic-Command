using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    // Lore-Katalog für alle Stargate-Sonderressourcen jenseits der Kernwirtschaft (Naquadah/Trinium/Versorgung/
    // Energie/Personal/Intel bleiben unverändert in ResourceStock/BuildCost). Diese Ressourcen werden pro Basis
    // in PlayerBaseSpecialResource gesammelt (siehe SpecialResourceService) und primär über Gate-Missionen
    // gefunden (siehe GateMissionService).
    public class SpecialResourceCatalogService
    {
        private static readonly IList<SpecialResourceDefinition> Definitions = new List<SpecialResourceDefinition>
        {
            new SpecialResourceDefinition(SpecialResourceType.LiquidNaquadah, SpecialResourceCategory.RawMaterial, "Flüssiges Naquadah", "Raffinierte, flüssige Form von Naquadah für Reaktoren und Legierungen."),
            new SpecialResourceDefinition(SpecialResourceType.Naquadria, SpecialResourceCategory.RawMaterial, "Naquadria", "Instabiles Schwesterelement des Naquadah mit enormer, aber gefährlicher Energiedichte."),
            new SpecialResourceDefinition(SpecialResourceType.AsgardAlloy, SpecialResourceCategory.RawMaterial, "Asgard-Legierung", "Hochfeste Asgard-Metalllegierung für Rumpf- und Schildbauteile."),
            new SpecialResourceDefinition(SpecialResourceType.Kassa, SpecialResourceCategory.RawMaterial, "Kassa", "Begehrtes Handelsgut der Lucian-Allianz, im Schwarzmarkt hoch gehandelt."),

            new SpecialResourceDefinition(SpecialResourceType.DakaraCrystals, SpecialResourceCategory.Crystal, "Dakara-Kristalle", "Kristalle aus der Dakara-Superwaffe, Schlüssel zu antiker Superwaffentechnologie."),
            new SpecialResourceDefinition(SpecialResourceType.AncientCrystals, SpecialResourceCategory.Crystal, "Antiker-Kristalle", "Universelle Steuerkristalle antiker Bauart."),
            new SpecialResourceDefinition(SpecialResourceType.ControlCrystals, SpecialResourceCategory.Crystal, "Kontrollkristalle", "Steuerkristalle für Gate- und Schiffssysteme."),
            new SpecialResourceDefinition(SpecialResourceType.StorageCrystals, SpecialResourceCategory.Crystal, "Speicherkristalle", "Datenspeicher in Kristallform, kompatibel mit den meisten Vorläufer-Technologien."),
            new SpecialResourceDefinition(SpecialResourceType.WraithCrystals, SpecialResourceCategory.Crystal, "Wraith-Kristalle", "Organisch gewachsene Kristalle aus Wraith-Hive-Schiffen."),
            new SpecialResourceDefinition(SpecialResourceType.OriCrystals, SpecialResourceCategory.Crystal, "Ori-Kristalle", "Kristalle der Ori, durchdrungen von Prior-Energie."),

            new SpecialResourceDefinition(SpecialResourceType.ZeroPointModule, SpecialResourceCategory.EnergySource, "ZPM (Zero Point Module)", "Antikes Energiemodul mit nahezu unerschöpflicher Vakuumenergie."),
            new SpecialResourceDefinition(SpecialResourceType.NaquadahReactor, SpecialResourceCategory.EnergySource, "Naquadah-Reaktor", "Kompakter Reaktor auf Naquadah-Basis für Basen und Großschiffe."),
            new SpecialResourceDefinition(SpecialResourceType.NaquadriaReactor, SpecialResourceCategory.EnergySource, "Naquadria-Reaktor", "Hochleistungsreaktor mit Naquadria-Kern; enorme Ausbeute, hohes Risiko."),
            new SpecialResourceDefinition(SpecialResourceType.ArcturusEnergy, SpecialResourceCategory.EnergySource, "Arkturus-Energie", "Vakuumenergie nach Arkturus-Projektbauart, extrem selten."),
            new SpecialResourceDefinition(SpecialResourceType.SolarEnergy, SpecialResourceCategory.EnergySource, "Sonnenenergie", "Über Sonnenkollektoren gewonnene Energie."),
            new SpecialResourceDefinition(SpecialResourceType.GeothermalEnergy, SpecialResourceCategory.EnergySource, "Geothermische Energie", "Aus planetarer Wärme gewonnene Energie."),
            new SpecialResourceDefinition(SpecialResourceType.FusionEnergy, SpecialResourceCategory.EnergySource, "Fusionsenergie", "Energie aus kontrollierter Kernfusion."),
            new SpecialResourceDefinition(SpecialResourceType.AncientEnergyCells, SpecialResourceCategory.EnergySource, "Antiker-Energiezellen", "Tragbare Energiespeicher antiker Bauart."),
            new SpecialResourceDefinition(SpecialResourceType.WraithBioenergy, SpecialResourceCategory.EnergySource, "Wraith-Bioenergie", "Aus Wraith-Biomasse gewonnene organische Energie."),

            new SpecialResourceDefinition(SpecialResourceType.Humans, SpecialResourceCategory.Lifeform, "Menschen", "Zivilbevölkerung und potenzielle Kolonisten aus befreiten oder verbündeten Welten."),
            new SpecialResourceDefinition(SpecialResourceType.Jaffa, SpecialResourceCategory.Lifeform, "Jaffa", "Kriegerkultur mit Symbiontenabhängigkeit, verbündet oder rekrutierbar."),
            new SpecialResourceDefinition(SpecialResourceType.GoauldSymbiotes, SpecialResourceCategory.Lifeform, "Goa'uld-Symbionten", "Parasitäre Symbionten, Grundlage der Goa'uld-Macht."),
            new SpecialResourceDefinition(SpecialResourceType.TokraSymbiotes, SpecialResourceCategory.Lifeform, "Tok'ra-Symbionten", "Symbionten der Tok'ra, die einvernehmliche Wirtsbindungen eingehen."),
            new SpecialResourceDefinition(SpecialResourceType.AsgardClones, SpecialResourceCategory.Lifeform, "Asgard-Klone", "Klonkörper der Asgard zur Fortführung ihrer Zivilisation."),
            new SpecialResourceDefinition(SpecialResourceType.Wraith, SpecialResourceCategory.Lifeform, "Wraith", "Gefangene oder untersuchte Wraith-Individuen."),
            new SpecialResourceDefinition(SpecialResourceType.IratusBugs, SpecialResourceCategory.Lifeform, "Iratus-Käfer", "Ursprungsspezies der Wraith-Regeneration, für Forschungszwecke."),
            new SpecialResourceDefinition(SpecialResourceType.Unas, SpecialResourceCategory.Lifeform, "Unas", "Ursprüngliche Wirtsspezies der Goa'uld."),
            new SpecialResourceDefinition(SpecialResourceType.Nox, SpecialResourceCategory.Lifeform, "Nox", "Eine der vier großen Rassen, pazifistisch und technologisch überlegen."),

            new SpecialResourceDefinition(SpecialResourceType.FurlingTechnology, SpecialResourceCategory.Technology, "Furlinger-Technologie", "Kaum erforschte Technologie der geheimnisvollen Furlinger."),
            new SpecialResourceDefinition(SpecialResourceType.AncientTechnology, SpecialResourceCategory.Technology, "Antiker-Technologie", "Fortschrittliche Technologie der Antiker."),
            new SpecialResourceDefinition(SpecialResourceType.AsgardTechnology, SpecialResourceCategory.Technology, "Asgard-Technologie", "Hochentwickelte Technologie der Asgard."),
            new SpecialResourceDefinition(SpecialResourceType.GoauldTechnology, SpecialResourceCategory.Technology, "Goa'uld-Technologie", "Technologie basierend auf gestohlenem antikem Wissen."),
            new SpecialResourceDefinition(SpecialResourceType.WraithTechnology, SpecialResourceCategory.Technology, "Wraith-Technologie", "Organisch-mechanische Hybridtechnologie der Wraith."),
            new SpecialResourceDefinition(SpecialResourceType.OriTechnology, SpecialResourceCategory.Technology, "Ori-Technologie", "Technologie der Ori, oft mit Prior-Energie verwoben."),
            new SpecialResourceDefinition(SpecialResourceType.ReplicatorTechnology, SpecialResourceCategory.Technology, "Replikatoren-Technologie", "Selbstreplizierende Nanotechnologie, hochgefährlich und begehrt."),
            new SpecialResourceDefinition(SpecialResourceType.AncientAiSystems, SpecialResourceCategory.Technology, "Antiker-KI-Systeme", "Künstliche Intelligenzen antiker Bauart."),
            new SpecialResourceDefinition(SpecialResourceType.AsuranTechnology, SpecialResourceCategory.Technology, "Asuraner-Technologie", "Technologie der Asuraner-Replikatoren aus Pegasus."),
            new SpecialResourceDefinition(SpecialResourceType.GoauldSarcophagus, SpecialResourceCategory.Technology, "Goa'uld-Sarkophage", "Lebensverlängernde Regenerationskammern der Goa'uld."),
            new SpecialResourceDefinition(SpecialResourceType.CommunicationStones, SpecialResourceCategory.Technology, "Kommunikationssteine", "Antike Artefakte zum Bewusstseinsaustausch über große Distanzen."),

            new SpecialResourceDefinition(SpecialResourceType.AncientDatabases, SpecialResourceCategory.KnowledgeArchive, "Antiker-Datenbanken", "Datenarchive der Antiker mit unschätzbarem Wissen."),
            new SpecialResourceDefinition(SpecialResourceType.AsgardDatabases, SpecialResourceCategory.KnowledgeArchive, "Asgard-Datenbanken", "Wissensspeicher der Asgard-Zivilisation."),
            new SpecialResourceDefinition(SpecialResourceType.GoauldKnowledgeArchives, SpecialResourceCategory.KnowledgeArchive, "Goa'uld-Wissensarchive", "Archive der Systemherren mit taktischem und technischem Wissen."),
            new SpecialResourceDefinition(SpecialResourceType.OriKnowledge, SpecialResourceCategory.KnowledgeArchive, "Ori-Wissen", "Religiöses und technisches Wissen der Ori."),
            new SpecialResourceDefinition(SpecialResourceType.StargateAddresses, SpecialResourceCategory.KnowledgeArchive, "Sternentor-Adressen", "Analysierte Gate-Adressen unbekannter Ziele."),
            new SpecialResourceDefinition(SpecialResourceType.StarCharts, SpecialResourceCategory.KnowledgeArchive, "Sternenkarten", "Kartierte Sternsysteme und Hyperraumrouten."),
            new SpecialResourceDefinition(SpecialResourceType.DnaDatabases, SpecialResourceCategory.KnowledgeArchive, "DNA-Datenbanken", "Genetische Kataloge verschiedener Spezies der Milchstraße und Pegasus."),

            new SpecialResourceDefinition(SpecialResourceType.DroneWeapons, SpecialResourceCategory.WeaponSystem, "Drohnenwaffen (Ancient Drones)", "Selbstlenkende Waffendrohnen antiker Bauart."),
            new SpecialResourceDefinition(SpecialResourceType.StaffWeapons, SpecialResourceCategory.WeaponSystem, "Stabwaffen", "Standardbewaffnung der Jaffa-Krieger."),
            new SpecialResourceDefinition(SpecialResourceType.ZatNikTel, SpecialResourceCategory.WeaponSystem, "Zat'nik'tel", "Vielseitige Betäubungs- und Energiewaffe."),
            new SpecialResourceDefinition(SpecialResourceType.GoauldHandDevices, SpecialResourceCategory.WeaponSystem, "Goa'uld-Handgeräte", "Ribbon-Device-artige Handwaffen der Goa'uld-Herren."),

            new SpecialResourceDefinition(SpecialResourceType.ShieldGenerators, SpecialResourceCategory.ShipSystem, "Schildgeneratoren", "Energieschilde für Basen und Schiffe."),
            new SpecialResourceDefinition(SpecialResourceType.Hyperdrives, SpecialResourceCategory.ShipSystem, "Hyperantriebe", "Standard-Hyperraumantriebe für interstellare Reisen."),
            new SpecialResourceDefinition(SpecialResourceType.IntergalacticHyperdrives, SpecialResourceCategory.ShipSystem, "Intergalaktische Hyperantriebe", "Hyperantriebe für Reisen zwischen Galaxien."),
            new SpecialResourceDefinition(SpecialResourceType.RingTransporters, SpecialResourceCategory.ShipSystem, "Transporterringe", "Goa'uld-Ringtransporter für kurze Distanzen."),
            new SpecialResourceDefinition(SpecialResourceType.BeamingTechnology, SpecialResourceCategory.ShipSystem, "Beamer-Technologie", "Asgard-Transporterstrahl-Technologie."),
            new SpecialResourceDefinition(SpecialResourceType.CloakingSystems, SpecialResourceCategory.ShipSystem, "Tarnsysteme", "Tarnvorrichtungen für Schiffe und Basen."),
            new SpecialResourceDefinition(SpecialResourceType.Sensors, SpecialResourceCategory.ShipSystem, "Sensoren", "Hochauflösende Sensorsysteme."),
            new SpecialResourceDefinition(SpecialResourceType.AncientShields, SpecialResourceCategory.ShipSystem, "Antiker-Schutzschilde", "Extrem widerstandsfähige Schildtechnologie der Antiker."),

            new SpecialResourceDefinition(SpecialResourceType.Tritonin, SpecialResourceCategory.MedicalBiological, "Tritonin", "Tok'ra-Medikament zur Stabilisierung sterbender Symbiontenwirte."),
            new SpecialResourceDefinition(SpecialResourceType.WraithHiveBiomass, SpecialResourceCategory.MedicalBiological, "Wraith-Hive-Biomasse", "Organisches Baumaterial aus Wraith-Hive-Schiffen."),
            new SpecialResourceDefinition(SpecialResourceType.WraithFood, SpecialResourceCategory.MedicalBiological, "Wraith-Nahrung (Lebensenergie)", "Von Wraith abgesaugte Lebensenergie."),
            new SpecialResourceDefinition(SpecialResourceType.GeneticSamples, SpecialResourceCategory.MedicalBiological, "Genetische Proben", "Proben für Forschung an Antiker-Genen und Fremdspezies."),
            new SpecialResourceDefinition(SpecialResourceType.HealingPlants, SpecialResourceCategory.MedicalBiological, "Heilpflanzen", "Medizinisch wirksame Pflanzen von entfernten Welten."),
            new SpecialResourceDefinition(SpecialResourceType.MedicalCompounds, SpecialResourceCategory.MedicalBiological, "Medizinische Wirkstoffe", "Verarbeitete Arzneimittel und Wirkstoffe."),

            new SpecialResourceDefinition(SpecialResourceType.Food, SpecialResourceCategory.Logistics, "Nahrung", "Grundlegende Nahrungsmittelversorgung."),
            new SpecialResourceDefinition(SpecialResourceType.Water, SpecialResourceCategory.Logistics, "Wasser", "Trink- und Brauchwasser für Basen und Kolonien."),
            new SpecialResourceDefinition(SpecialResourceType.Oxygen, SpecialResourceCategory.Logistics, "Sauerstoff", "Lebenserhaltender Sauerstoffvorrat."),
            new SpecialResourceDefinition(SpecialResourceType.Fuel, SpecialResourceCategory.Logistics, "Treibstoff", "Treibstoff für konventionelle Antriebe und Fahrzeuge."),
            new SpecialResourceDefinition(SpecialResourceType.SpareParts, SpecialResourceCategory.Logistics, "Ersatzteile", "Ersatzteile für Instandhaltung von Anlagen und Schiffen."),
            new SpecialResourceDefinition(SpecialResourceType.IndustrialGoods, SpecialResourceCategory.Logistics, "Industriegüter", "Verarbeitete Güter für Bau und Produktion."),
            new SpecialResourceDefinition(SpecialResourceType.TradeGoods, SpecialResourceCategory.Logistics, "Handelsgüter", "Allgemeine Handelswaren für den zwischenplanetaren Markt."),

            new SpecialResourceDefinition(SpecialResourceType.Colonists, SpecialResourceCategory.Personnel, "Kolonisten", "Siedler für neue Kolonien und Außenposten."),
            new SpecialResourceDefinition(SpecialResourceType.Scientists, SpecialResourceCategory.Personnel, "Wissenschaftler", "Forschungspersonal für Labore und Expeditionen."),
            new SpecialResourceDefinition(SpecialResourceType.Engineers, SpecialResourceCategory.Personnel, "Ingenieure", "Technisches Personal für Bau und Instandhaltung."),
            new SpecialResourceDefinition(SpecialResourceType.Soldiers, SpecialResourceCategory.Personnel, "Soldaten", "Militärisches Bodenpersonal."),
            new SpecialResourceDefinition(SpecialResourceType.Pilots, SpecialResourceCategory.Personnel, "Piloten", "Ausgebildete Piloten für Flotten und Jäger."),
            new SpecialResourceDefinition(SpecialResourceType.Officers, SpecialResourceCategory.Personnel, "Offiziere", "Führungspersonal für Basen und Missionen."),
            new SpecialResourceDefinition(SpecialResourceType.IntelligencePersonnel, SpecialResourceCategory.Personnel, "Geheimdienstpersonal", "Agenten und Analysten für Spionage und Gegenspionage."),

            new SpecialResourceDefinition(SpecialResourceType.BuildCapacity, SpecialResourceCategory.StrategicPoint, "Baukapazität", "Verfügbare Baukapazität für parallele Bauprojekte."),
            new SpecialResourceDefinition(SpecialResourceType.ResearchPoints, SpecialResourceCategory.StrategicPoint, "Forschungspunkte", "Abstrakte Punkte für zusätzlichen Forschungsfortschritt."),
            new SpecialResourceDefinition(SpecialResourceType.Influence, SpecialResourceCategory.StrategicPoint, "Einfluss", "Politischer und diplomatischer Einfluss in der Galaxie."),
            new SpecialResourceDefinition(SpecialResourceType.Prestige, SpecialResourceCategory.StrategicPoint, "Prestige", "Ansehen einer Fraktion oder eines Kommandanten."),
            new SpecialResourceDefinition(SpecialResourceType.ControlPoints, SpecialResourceCategory.StrategicPoint, "Kontrollpunkte", "Punkte für territoriale Kontrolle und Sektorherrschaft."),
            new SpecialResourceDefinition(SpecialResourceType.PlanetaryInfrastructure, SpecialResourceCategory.StrategicPoint, "Planetare Infrastruktur", "Ausbaustand der planetaren Infrastruktur."),
            new SpecialResourceDefinition(SpecialResourceType.ShipyardCapacity, SpecialResourceCategory.StrategicPoint, "Werftkapazität", "Verfügbare Kapazität der Raumwerften."),
            new SpecialResourceDefinition(SpecialResourceType.ProductionCapacity, SpecialResourceCategory.StrategicPoint, "Produktionskapazität", "Allgemeine industrielle Produktionskapazität."),

            new SpecialResourceDefinition(SpecialResourceType.AncientArtifacts, SpecialResourceCategory.Artifact, "Artefakte der Antiker", "Geborgene Relikte antiker Herkunft."),
            new SpecialResourceDefinition(SpecialResourceType.GoauldArtifacts, SpecialResourceCategory.Artifact, "Artefakte der Goa'uld", "Geborgene Relikte aus Goa'uld-Ruinen."),
            new SpecialResourceDefinition(SpecialResourceType.OriArtifacts, SpecialResourceCategory.Artifact, "Artefakte der Ori", "Geborgene Relikte der Ori."),
            new SpecialResourceDefinition(SpecialResourceType.AsgardArtifacts, SpecialResourceCategory.Artifact, "Artefakte der Asgard", "Geborgene Relikte der Asgard."),
            new SpecialResourceDefinition(SpecialResourceType.WraithArtifacts, SpecialResourceCategory.Artifact, "Artefakte der Wraith", "Geborgene Relikte aus Wraith-Hive-Schiffen."),
            new SpecialResourceDefinition(SpecialResourceType.FurlingArtifacts, SpecialResourceCategory.Artifact, "Artefakte der Furlinger", "Geborgene Relikte der geheimnisvollen Furlinger.")
        };

        public IReadOnlyList<SpecialResourceDefinition> GetAll()
        {
            return Definitions.OrderBy(d => d.Category).ThenBy(d => d.Name).ToList();
        }

        public IEnumerable<SpecialResourceDefinition> GetByCategory(SpecialResourceCategory category)
        {
            return Definitions.Where(d => d.Category == category).OrderBy(d => d.Name);
        }

        public SpecialResourceDefinition Get(SpecialResourceType type)
        {
            var definition = Definitions.FirstOrDefault(d => d.Type == type);
            if (definition == null) throw new ArgumentOutOfRangeException("type", "Unknown special resource type.");
            return definition;
        }

        // Deterministische Auswahl innerhalb einer Kategorie, z. B. um je nach Gate-Adresse
        // eine thematisch passende, aber wechselnde Ressource zu vergeben (siehe GateMissionService).
        public SpecialResourceType PickFromCategory(SpecialResourceCategory category, int seed)
        {
            var pool = Definitions.Where(d => d.Category == category).OrderBy(d => d.Type).ToList();
            if (pool.Count == 0) throw new InvalidOperationException("Keine Ressourcen in dieser Kategorie definiert.");
            int index = ((seed % pool.Count) + pool.Count) % pool.Count;
            return pool[index].Type;
        }
    }
}
