package com.stargate.sgw3d;

import java.util.ArrayList;
import java.util.List;

/**
 * Carries the player's choices across menu screens for one run of the app.
 * Purely in-memory -- no persistence, no backend calls yet.
 */
class GameSession {
    String username;
    String selectedServer;
    final List<CharacterProfile> characters = new ArrayList<>();
    CharacterProfile activeCharacter;
}
