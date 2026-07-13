package com.stargate.sgw3d;

import java.util.ArrayList;
import java.util.List;

/**
 * Lists characters created this session plus an entry to create a new one.
 * Rebuilt every time it becomes visible (see {@link ListMenuState}), so a
 * character just created shows up immediately without extra wiring.
 */
public class CharacterSelectState extends ListMenuState {

    private final GameSession session;

    CharacterSelectState(GameSession session) {
        this.session = session;
    }

    @Override
    protected String getTitle() {
        return "CHARAKTER AUSWAHL";
    }

    @Override
    protected String getSubtitle() {
        return "Welt: " + session.selectedServer;
    }

    @Override
    protected List<Item> buildItems() {
        List<Item> items = new ArrayList<>();
        for (CharacterProfile profile : session.characters) {
            String label = profile.name + "   --   " + profile.factionLabel() + " / " + profile.roleLabel();
            items.add(new Item(label, () -> enterWorld(profile)));
        }
        items.add(new Item("+ Neuen Charakter erstellen", this::createCharacter));
        return items;
    }

    private void enterWorld(CharacterProfile profile) {
        session.activeCharacter = profile;
        getStateManager().attach(new MissionState(session));
        setEnabled(false);
    }

    private void createCharacter() {
        getStateManager().attach(new CharacterCreateState(session));
        setEnabled(false);
    }

    @Override
    protected void onBack() {
        ServerSelectState serverSelect = getStateManager().getState(ServerSelectState.class);
        getStateManager().detach(this);
        if (serverSelect != null) {
            serverSelect.setEnabled(true);
        }
    }
}
