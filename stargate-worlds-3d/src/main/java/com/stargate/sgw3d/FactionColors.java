package com.stargate.sgw3d;

import com.jme3.math.ColorRGBA;

/** Maps the four playable factions to their avatar tint. */
final class FactionColors {

    private FactionColors() {
    }

    static ColorRGBA of(CharacterProfile profile) {
        if (profile == null) {
            return new ColorRGBA(0.2f, 0.4f, 0.85f, 1f);
        }
        switch (profile.faction) {
            case TAURI: return new ColorRGBA(0.2f, 0.4f, 0.85f, 1f);
            case FREE_JAFFA: return new ColorRGBA(0.8f, 0.6f, 0.15f, 1f);
            case TOKRA: return new ColorRGBA(0.15f, 0.7f, 0.6f, 1f);
            case LUCIAN: return new ColorRGBA(0.75f, 0.2f, 0.2f, 1f);
            default: return new ColorRGBA(0.2f, 0.4f, 0.85f, 1f);
        }
    }
}
