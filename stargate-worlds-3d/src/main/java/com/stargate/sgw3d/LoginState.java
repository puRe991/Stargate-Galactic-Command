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
 * Mock login screen: accepts any non-empty username, no real backend call
 * yet (see stargate-worlds-3d/README.md for the planned backend wiring).
 */
public class LoginState extends BaseAppState implements ActionListener, RawInputListener {

    private final GameSession session;
    private SimpleApplication app;
    private Node root;
    private TextField username;
    private TextField password;
    private TextField focused;
    private BitmapText errorText;

    LoginState(GameSession session) {
        this.session = session;
    }

    @Override
    protected void initialize(Application application) {
        this.app = (SimpleApplication) application;
        BitmapFont font = app.getAssetManager().loadFont("Interface/Fonts/Default.fnt");
        int width = app.getCamera().getWidth();
        int height = app.getCamera().getHeight();

        root = new Node("login");

        BitmapText title = titleText(font, "SGC ZUGANGSKONTROLLE", width, height - 100);
        root.attachChild(title);

        BitmapText hint = bodyText(font, "Prototyp -- lokale Anmeldung, echte Backend-Anbindung folgt",
                new ColorRGBA(0.72f, 0.8f, 0.86f, 1f), width, height - 135);
        root.attachChild(hint);

        BitmapText userLabel = labelText(font, "Benutzername:", width / 2f - 180, height / 2f + 50);
        root.attachChild(userLabel);

        username = new TextField(font, "Benutzername eingeben", false);
        username.getDisplay().setLocalTranslation(width / 2f - 180, height / 2f + 25, 0);
        root.attachChild(username.getDisplay());

        BitmapText passLabel = labelText(font, "Passwort:", width / 2f - 180, height / 2f - 20);
        root.attachChild(passLabel);

        password = new TextField(font, "Passwort eingeben", true);
        password.getDisplay().setLocalTranslation(width / 2f - 180, height / 2f - 45, 0);
        root.attachChild(password.getDisplay());

        BitmapText loginButton = new BitmapText(font);
        loginButton.setSize(font.getCharSet().getRenderedSize() * 1.2f);
        loginButton.setColor(new ColorRGBA(0.4f, 0.85f, 1f, 1f));
        loginButton.setText("[ Einloggen ]");
        loginButton.setLocalTranslation(centered(loginButton.getLineWidth(), width), height / 2f - 110, 0);
        root.attachChild(loginButton);

        BitmapText backHint = bodyText(font, "Tab: Feld wechseln  |  Enter: einloggen  |  Esc: zurueck",
                ColorRGBA.LightGray, width, height / 2f - 150);
        root.attachChild(backHint);

        errorText = new BitmapText(font);
        errorText.setSize(font.getCharSet().getRenderedSize());
        errorText.setColor(new ColorRGBA(1f, 0.5f, 0.5f, 1f));
        errorText.setText("Benutzername erforderlich.");
        errorText.setLocalTranslation(centered(errorText.getLineWidth(), width), height / 2f - 180, 0);
    }

    private BitmapText titleText(BitmapFont font, String text, int width, float y) {
        BitmapText t = new BitmapText(font);
        t.setSize(font.getCharSet().getRenderedSize() * 1.8f);
        t.setColor(ColorRGBA.White);
        t.setText(text);
        t.setLocalTranslation(centered(t.getLineWidth(), width), y, 0);
        return t;
    }

    private BitmapText bodyText(BitmapFont font, String text, ColorRGBA color, int width, float y) {
        BitmapText t = new BitmapText(font);
        t.setSize(font.getCharSet().getRenderedSize());
        t.setColor(color);
        t.setText(text);
        t.setLocalTranslation(centered(t.getLineWidth(), width), y, 0);
        return t;
    }

    private BitmapText labelText(BitmapFont font, String text, float x, float y) {
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

    @Override
    protected void cleanup(Application application) {
    }

    @Override
    protected void onEnable() {
        app.getGuiNode().attachChild(root);
        app.getInputManager().setCursorVisible(true);
        focused = username;
        username.setFocused(true);
        password.setFocused(false);
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
        im.addMapping("LoginSubmit", new KeyTrigger(KeyInput.KEY_RETURN));
        im.addMapping("LoginTab", new KeyTrigger(KeyInput.KEY_TAB));
        im.addMapping("LoginBack", new KeyTrigger(KeyInput.KEY_ESCAPE));
        im.addMapping("LoginBackspace", new KeyTrigger(KeyInput.KEY_BACK));
        im.addListener(this, "LoginSubmit", "LoginTab", "LoginBack", "LoginBackspace");
        im.addRawInputListener(this);
    }

    private void unregisterInput() {
        InputManager im = app.getInputManager();
        im.removeListener(this);
        im.removeRawInputListener(this);
        for (String m : new String[] { "LoginSubmit", "LoginTab", "LoginBack", "LoginBackspace" }) {
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
            case "LoginSubmit":
                submit();
                break;
            case "LoginTab":
                focused = (focused == username) ? password : username;
                username.setFocused(focused == username);
                password.setFocused(focused == password);
                break;
            case "LoginBack":
                returnToMenu();
                break;
            case "LoginBackspace":
                if (focused != null) {
                    focused.handleBackspace();
                }
                break;
            default:
                break;
        }
    }

    private void submit() {
        String name = username.getValue().trim();
        if (name.isEmpty()) {
            root.attachChild(errorText);
            return;
        }
        errorText.removeFromParent();
        session.username = name;
        getStateManager().attach(new ServerSelectState(session));
        setEnabled(false);
    }

    private void returnToMenu() {
        MainMenuState menu = getStateManager().getState(MainMenuState.class);
        getStateManager().detach(this);
        if (menu != null) {
            menu.setEnabled(true);
        }
    }

    @Override
    public void update(float tpf) {
        username.update(tpf);
        password.update(tpf);
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
        if (!evt.isPressed() || focused == null) {
            return;
        }
        char c = evt.getKeyChar();
        if (c >= 32 && c < 127) {
            focused.handleChar(c);
        }
    }
}
