package com.stargate.sgw3d;

import java.util.ArrayList;
import java.util.List;

/**
 * Mock server list -- fixed placeholder worlds. Mirrors the existing web
 * game's "/Server/Select" concept (independent worlds, some paused/in
 * maintenance) but has no real backend query yet.
 */
public class ServerSelectState extends ListMenuState {

    private final GameSession session;

    ServerSelectState(GameSession session) {
        this.session = session;
    }

    @Override
    protected String getTitle() {
        return "SERVER AUSWAHL";
    }

    @Override
    protected String getSubtitle() {
        return "Angemeldet als " + session.username + "  --  Prototyp: feste Testwelten";
    }

    @Override
    protected List<Item> buildItems() {
        List<Item> items = new ArrayList<>();
        items.add(new Item("Alpha-Welt   (Online, 42 Spieler)", () -> selectServer("Alpha-Welt")));
        items.add(new Item("Beta-Welt    (Online, 17 Spieler)", () -> selectServer("Beta-Welt")));
        items.add(new Item("Gamma-Welt   (pausiert -- keine neuen Sitzungen)", () -> { }));
        return items;
    }

    private void selectServer(String serverName) {
        session.selectedServer = serverName;
        getStateManager().attach(new CharacterSelectState(session));
        setEnabled(false);
    }

    @Override
    protected void onBack() {
        LoginState login = getStateManager().getState(LoginState.class);
        getStateManager().detach(this);
        if (login != null) {
            login.setEnabled(true);
        }
    }
}
