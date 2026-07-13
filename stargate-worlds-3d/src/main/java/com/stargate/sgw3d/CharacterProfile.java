package com.stargate.sgw3d;

/** A locally-created character (no backend persistence yet). */
class CharacterProfile {

    enum Faction { TAURI, FREE_JAFFA, TOKRA, LUCIAN }

    enum Role { MILITAER, WISSENSCHAFT, DIPLOMATIE }

    final String name;
    final Faction faction;
    final Role role;

    CharacterProfile(String name, Faction faction, Role role) {
        this.name = name;
        this.faction = faction;
        this.role = role;
    }

    String factionLabel() {
        switch (faction) {
            case TAURI: return "Tau'ri / SGC";
            case FREE_JAFFA: return "Freie Jaffa";
            case TOKRA: return "Tok'ra";
            case LUCIAN: return "Lucian Alliance";
            default: return faction.name();
        }
    }

    String roleLabel() {
        switch (role) {
            case MILITAER: return "Militaer";
            case WISSENSCHAFT: return "Wissenschaft";
            case DIPLOMATIE: return "Diplomatie";
            default: return role.name();
        }
    }
}
