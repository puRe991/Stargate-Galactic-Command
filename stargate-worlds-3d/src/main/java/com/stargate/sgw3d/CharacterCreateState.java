package com.stargate.sgw3d;

import com.jme3.app.Application;
import com.jme3.app.SimpleApplication;
import com.jme3.app.state.BaseAppState;
import com.jme3.font.BitmapFont;
import com.jme3.font.BitmapText;
import com.jme3.input.InputManager;
import com.jme3.input.KeyInput;
import com.jme3.input.RawInputListener;
import com.jme3.input.controls.ActionListener;
import com.jme3.input.controls.KeyTrigger;
import com.jme3.input.event.JoyAxisEvent;
import com.jme3.input.event.JoyButtonEvent;
import com.jme3.input.event.KeyInputEvent;
import com.jme3.input.event.MouseButtonEvent;
import com.jme3.input.event.MouseMotionEvent;
import com.jme3.input.event.TouchEvent;
import com.jme3.math.ColorRGBA;
import com.jme3.scene.Node;

/**
 * Minimal character creation: name (typed), faction (Left/Right), role
 * (Up/Down). Not a visual customizer like the original Stargate Worlds
 * screen -- we have no 3D character assets yet -- but mirrors its flow:
 * pick a faction/class, name the character, confirm.
 */
public class CharacterCreateState extends BaseAppState implements ActionListener, RawInputListener {

    private static final ColorRGBA ACCENT = new ColorRGBA(0.4f, 0.85f, 1f, 1f);

    private final GameSession session;
    private SimpleApplication app;
    private Node root;
    private TextField nameField;
    private BitmapText factionText;
    private BitmapText roleText;
    private BitmapText errorText;
    private int factionIndex;
    private int roleIndex;
    private final CharacterProfile.Faction[] factions = CharacterProfile.Faction.values();
    private final CharacterProfile.Role[] roles = CharacterProfile.Role.values();

    CharacterCreateState(GameSession session) {
        this.session = session;
    }

    @Override
    protected void initialize(Application application) {
        this.app = (SimpleApplication) application;
        BitmapFont font = app.getAssetManager().loadFont("Interface/Fonts/Default.fnt");
        int width = app.getCamera().getWidth();
        int height = app.getCamera().getHeight();
        float left = width / 2f - 180;

        root = new Node("character-create");

        BitmapText title = new BitmapText(font);
        title.setSize(font.getCharSet().getRenderedSize() * 1.8f);
        title.setColor(ColorRGBA.White);
        title.setText("CHARAKTER ERSTELLEN");
        title.setLocalTranslation(centered(title.getLineWidth(), width), height - 100, 0);
        root.attachChild(title);

        root.attachChild(label(font, "Name:", left, height / 2f + 90));
        nameField = new TextField(font, "Namen eingeben", false);
        nameField.getDisplay().setLocalTranslation(left, height / 2f + 65, 0);
        root.attachChild(nameField.getDisplay());

        root.attachChild(label(font, "Fraktion (Pfeiltasten links/rechts):", left, height / 2f + 20));
        factionText = new BitmapText(font);
        factionText.setSize(font.getCharSet().getRenderedSize() * 1.2f);
        factionText.setColor(ACCENT);
        factionText.setLocalTranslation(left, height / 2f - 5, 0);
        root.attachChild(factionText);

        root.attachChild(label(font, "Rolle (Pfeiltasten hoch/runter):", left, height / 2f - 45));
        roleText = new BitmapText(font);
        roleText.setSize(font.getCharSet().getRenderedSize() * 1.2f);
        roleText.setColor(ACCENT);
        roleText.setLocalTranslation(left, height / 2f - 70, 0);
        root.attachChild(roleText);

        BitmapText createButton = new BitmapText(font);
        createButton.setSize(font.getCharSet().getRenderedSize() * 1.2f);
        createButton.setColor(ACCENT);
        createButton.setText("[ Erstellen ]");
        createButton.setLocalTranslation(centered(createButton.getLineWidth(), width), height / 2f - 120, 0);
        root.attachChild(createButton);

        BitmapText backHint = new BitmapText(font);
        backHint.setSize(font.getCharSet().getRenderedSize());
        backHint.setColor(ColorRGBA.LightGray);
        backHint.setText("Enter: erstellen  |  Esc: abbrechen");
        backHint.setLocalTranslation(centered(backHint.getLineWidth(), width), height / 2f - 155, 0);
        root.attachChild(backHint);

        errorText = new BitmapText(font);
        errorText.setSize(font.getCharSet().getRenderedSize());
        errorText.setColor(new ColorRGBA(1f, 0.5f, 0.5f, 1f));
        errorText.setText("Name erforderlich.");
        errorText.setLocalTranslation(centered(errorText.getLineWidth(), width), height / 2f - 185, 0);

        refreshSelectors();
    }

    private BitmapText label(BitmapFont font, String text, float x, float y) {
        BitmapText t = new BitmapText(font);
        t.setSize(font.getCharSet().getRenderedSize());
        t.setColor(ColorRGBA.LightGray);
        t.setText(text);
        t.setLocalTranslation(x, y, 0);
        return t;
    }

    private float centered(float lineWidth, int screenWidth) {
        return (screenWidth - lineWidth) / 2f;
    }

    private void refreshSelectors() {
        factionText.setText("<  " + labelFor(factions[factionIndex]) + "  >");
        roleText.setText("<  " + labelFor(roles[roleIndex]) + "  >");
    }

    private String labelFor(CharacterProfile.Faction faction) {
        switch (faction) {
            case TAURI: return "Tau'ri / SGC";
            case FREE_JAFFA: return "Freie Jaffa";
            case TOKRA: return "Tok'ra";
            case LUCIAN: return "Lucian Alliance";
            default: return faction.name();
        }
    }

    private String labelFor(CharacterProfile.Role role) {
        switch (role) {
            case MILITAER: return "Militaer";
            case WISSENSCHAFT: return "Wissenschaft";
            case DIPLOMATIE: return "Diplomatie";
            default: return role.name();
        }
    }

    @Override
    protected void cleanup(Application application) {
    }

    @Override
    protected void onEnable() {
        app.getGuiNode().attachChild(root);
        app.getInputManager().setCursorVisible(true);
        nameField.setFocused(true);
        registerInput();
    }

    @Override
    protected void onDisable() {
        root.removeFromParent();
        errorText.removeFromParent();
        unregisterInput();
    }

    private void registerInput() {
        InputManager im = app.getInputManager();
        im.addMapping("CreateSubmit", new KeyTrigger(KeyInput.KEY_RETURN));
        im.addMapping("CreateBack", new KeyTrigger(KeyInput.KEY_ESCAPE));
        im.addMapping("CreateBackspace", new KeyTrigger(KeyInput.KEY_BACK));
        im.addMapping("FactionPrev", new KeyTrigger(KeyInput.KEY_LEFT));
        im.addMapping("FactionNext", new KeyTrigger(KeyInput.KEY_RIGHT));
        im.addMapping("RolePrev", new KeyTrigger(KeyInput.KEY_UP));
        im.addMapping("RoleNext", new KeyTrigger(KeyInput.KEY_DOWN));
        im.addListener(this, "CreateSubmit", "CreateBack", "CreateBackspace",
                "FactionPrev", "FactionNext", "RolePrev", "RoleNext");
        im.addRawInputListener(this);
    }

    private void unregisterInput() {
        InputManager im = app.getInputManager();
        im.removeListener(this);
        im.removeRawInputListener(this);
        for (String m : new String[] { "CreateSubmit", "CreateBack", "CreateBackspace",
                "FactionPrev", "FactionNext", "RolePrev", "RoleNext" }) {
            if (im.hasMapping(m)) {
                im.deleteMapping(m);
            }
        }
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        if (!isPressed) {
            return;
        }
        switch (name) {
            case "CreateSubmit":
                submit();
                break;
            case "CreateBack":
                cancel();
                break;
            case "CreateBackspace":
                nameField.handleBackspace();
                break;
            case "FactionPrev":
                factionIndex = (factionIndex - 1 + factions.length) % factions.length;
                refreshSelectors();
                break;
            case "FactionNext":
                factionIndex = (factionIndex + 1) % factions.length;
                refreshSelectors();
                break;
            case "RolePrev":
                roleIndex = (roleIndex - 1 + roles.length) % roles.length;
                refreshSelectors();
                break;
            case "RoleNext":
                roleIndex = (roleIndex + 1) % roles.length;
                refreshSelectors();
                break;
            default:
                break;
        }
    }

    private void submit() {
        String name = nameField.getValue().trim();
        if (name.isEmpty()) {
            root.attachChild(errorText);
            return;
        }
        errorText.removeFromParent();
        session.characters.add(new CharacterProfile(name, factions[factionIndex], roles[roleIndex]));
        backToSelect();
    }

    private void cancel() {
        backToSelect();
    }

    private void backToSelect() {
        CharacterSelectState select = getStateManager().getState(CharacterSelectState.class);
        getStateManager().detach(this);
        if (select != null) {
            select.setEnabled(true);
        }
    }

    @Override
    public void update(float tpf) {
        nameField.update(tpf);
    }

    @Override
    public void beginInput() {
    }

    @Override
    public void endInput() {
    }

    @Override
    public void onJoyAxisEvent(JoyAxisEvent evt) {
    }

    @Override
    public void onJoyButtonEvent(JoyButtonEvent evt) {
    }

    @Override
    public void onMouseMotionEvent(MouseMotionEvent evt) {
    }

    @Override
    public void onMouseButtonEvent(MouseButtonEvent evt) {
    }

    @Override
    public void onTouchEvent(TouchEvent evt) {
    }

    @Override
    public void onKeyEvent(KeyInputEvent evt) {
        if (!evt.isPressed()) {
            return;
        }
        char c = evt.getKeyChar();
        if (c >= 32 && c < 127) {
            nameField.handleChar(c);
        }
    }
}
