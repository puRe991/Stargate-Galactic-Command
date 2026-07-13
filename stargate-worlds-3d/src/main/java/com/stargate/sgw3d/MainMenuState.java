package com.stargate.sgw3d;

import com.jme3.app.Application;
import com.jme3.app.SimpleApplication;
import com.jme3.app.state.BaseAppState;
import com.jme3.font.BitmapFont;
import com.jme3.font.BitmapText;
import com.jme3.input.InputManager;
import com.jme3.input.KeyInput;
import com.jme3.input.MouseInput;
import com.jme3.input.controls.ActionListener;
import com.jme3.input.controls.KeyTrigger;
import com.jme3.input.controls.MouseButtonTrigger;
import com.jme3.math.ColorRGBA;
import com.jme3.math.Vector2f;
import com.jme3.scene.Node;

import java.util.ArrayList;
import java.util.List;

/**
 * Keyboard- and mouse-navigable main menu, built from plain guiNode text so
 * the prototype doesn't need an extra GUI library (Lemur/Nifty) dependency
 * yet. Up/Down or W/S to move the selection, Enter/Space or a left click to
 * confirm, Esc to leave the Optionen placeholder.
 */
public class MainMenuState extends BaseAppState implements ActionListener {

    private static final ColorRGBA COLOR_NORMAL = new ColorRGBA(0.72f, 0.8f, 0.86f, 1f);
    private static final ColorRGBA COLOR_SELECTED = new ColorRGBA(0.4f, 0.85f, 1f, 1f);
    private static final float ITEM_SPACING = 40f;

    private final GameSession session;
    private SimpleApplication app;
    private Node menuRoot;
    private BitmapText selector;
    private BitmapText optionsInfo;
    private final List<MenuOption> options = new ArrayList<>();
    private int selectedIndex = 0;
    private boolean showingOptions = false;
    private Vector2f lastCursor;

    MainMenuState(GameSession session) {
        this.session = session;
    }

    private static final class MenuOption {
        final String label;
        final Runnable action;
        BitmapText text;

        MenuOption(String label, Runnable action) {
            this.label = label;
            this.action = action;
        }
    }

    @Override
    protected void initialize(Application application) {
        this.app = (SimpleApplication) application;
        BitmapFont font = app.getAssetManager().loadFont("Interface/Fonts/Default.fnt");
        int width = app.getCamera().getWidth();
        int height = app.getCamera().getHeight();

        menuRoot = new Node("main-menu");

        BitmapText title = new BitmapText(font);
        title.setSize(font.getCharSet().getRenderedSize() * 2.1f);
        title.setColor(ColorRGBA.White);
        title.setText("STARGATE GALACTIC COMMAND");
        title.setLocalTranslation(centered(title.getLineWidth(), width), height - 120, 0);
        menuRoot.attachChild(title);

        BitmapText subtitle = new BitmapText(font);
        subtitle.setSize(font.getCharSet().getRenderedSize() * 1.05f);
        subtitle.setColor(COLOR_NORMAL);
        subtitle.setText("SGW Vertical Slice -- Beta Site");
        subtitle.setLocalTranslation(centered(subtitle.getLineWidth(), width), height - 160, 0);
        menuRoot.attachChild(subtitle);

        options.add(new MenuOption("Spiel starten", this::startGame));
        options.add(new MenuOption("Optionen", this::openOptions));
        options.add(new MenuOption("Beenden", this::quit));

        float startY = height / 2f + ITEM_SPACING;
        float maxLabelWidth = 0f;
        for (int i = 0; i < options.size(); i++) {
            MenuOption option = options.get(i);
            BitmapText text = new BitmapText(font);
            text.setSize(font.getCharSet().getRenderedSize() * 1.3f);
            text.setColor(COLOR_NORMAL);
            text.setText(option.label);
            text.setLocalTranslation(centered(text.getLineWidth(), width), startY - i * ITEM_SPACING, 0);
            option.text = text;
            menuRoot.attachChild(text);
            maxLabelWidth = Math.max(maxLabelWidth, text.getLineWidth());
        }

        selector = new BitmapText(font);
        selector.setSize(font.getCharSet().getRenderedSize() * 1.3f);
        selector.setColor(COLOR_SELECTED);
        selector.setText(">");
        menuRoot.attachChild(selector);

        optionsInfo = new BitmapText(font);
        optionsInfo.setSize(font.getCharSet().getRenderedSize());
        optionsInfo.setColor(COLOR_NORMAL);
        optionsInfo.setText("Noch keine Optionen implementiert.  --  Esc: zurueck");
        float belowItems = startY - options.size() * ITEM_SPACING - 40;
        optionsInfo.setLocalTranslation(centered(optionsInfo.getLineWidth(), width), belowItems, 0);

        updateHighlight();
    }

    private float centered(float lineWidth, int screenWidth) {
        return (screenWidth - lineWidth) / 2f;
    }

    @Override
    protected void cleanup(Application application) {
        // Scene graph nodes are already detached in onDisable(); nothing else to release.
    }

    @Override
    protected void onEnable() {
        app.getGuiNode().attachChild(menuRoot);
        showingOptions = false;
        selectedIndex = 0;
        lastCursor = null;
        updateHighlight();
        registerInput();
        app.getInputManager().setCursorVisible(true);
    }

    @Override
    protected void onDisable() {
        menuRoot.removeFromParent();
        optionsInfo.removeFromParent();
        unregisterInput();
    }

    private void registerInput() {
        InputManager im = app.getInputManager();
        im.addMapping("MenuUp", new KeyTrigger(KeyInput.KEY_UP), new KeyTrigger(KeyInput.KEY_W));
        im.addMapping("MenuDown", new KeyTrigger(KeyInput.KEY_DOWN), new KeyTrigger(KeyInput.KEY_S));
        im.addMapping("MenuSelect", new KeyTrigger(KeyInput.KEY_RETURN), new KeyTrigger(KeyInput.KEY_SPACE));
        im.addMapping("MenuBack", new KeyTrigger(KeyInput.KEY_ESCAPE));
        im.addMapping("MenuClick", new MouseButtonTrigger(MouseInput.BUTTON_LEFT));
        im.addListener(this, "MenuUp", "MenuDown", "MenuSelect", "MenuBack", "MenuClick");
    }

    private void unregisterInput() {
        InputManager im = app.getInputManager();
        im.removeListener(this);
        for (String mapping : new String[] { "MenuUp", "MenuDown", "MenuSelect", "MenuBack", "MenuClick" }) {
            if (im.hasMapping(mapping)) {
                im.deleteMapping(mapping);
            }
        }
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        if (!isPressed) {
            return;
        }
        switch (name) {
            case "MenuUp":
                if (!showingOptions) {
                    selectedIndex = (selectedIndex - 1 + options.size()) % options.size();
                    updateHighlight();
                }
                break;
            case "MenuDown":
                if (!showingOptions) {
                    selectedIndex = (selectedIndex + 1) % options.size();
                    updateHighlight();
                }
                break;
            case "MenuSelect":
                if (!showingOptions) {
                    options.get(selectedIndex).action.run();
                }
                break;
            case "MenuBack":
                if (showingOptions) {
                    closeOptions();
                }
                break;
            case "MenuClick":
                if (!showingOptions) {
                    handleClick();
                }
                break;
            default:
                break;
        }
    }

    @Override
    public void update(float tpf) {
        if (showingOptions) {
            return;
        }
        Vector2f cursor = app.getInputManager().getCursorPosition();
        // Only let the mouse steal the selection once it actually moves --
        // otherwise its idle/default position can silently override
        // keyboard navigation the moment the menu appears.
        boolean moved = lastCursor != null && lastCursor.distance(cursor) > 0.5f;
        lastCursor = cursor.clone();
        if (!moved) {
            return;
        }
        int hovered = itemUnderCursor();
        if (hovered >= 0 && hovered != selectedIndex) {
            selectedIndex = hovered;
            updateHighlight();
        }
    }

    private void handleClick() {
        int clicked = itemUnderCursor();
        if (clicked >= 0) {
            options.get(clicked).action.run();
        }
    }

    private int itemUnderCursor() {
        Vector2f cursor = app.getInputManager().getCursorPosition();
        for (int i = 0; i < options.size(); i++) {
            BitmapText text = options.get(i).text;
            float left = text.getLocalTranslation().x;
            float top = text.getLocalTranslation().y;
            float right = left + text.getLineWidth();
            float bottom = top - text.getLineHeight();
            if (cursor.x >= left && cursor.x <= right && cursor.y >= bottom && cursor.y <= top) {
                return i;
            }
        }
        return -1;
    }

    private void updateHighlight() {
        for (int i = 0; i < options.size(); i++) {
            options.get(i).text.setColor(i == selectedIndex ? COLOR_SELECTED : COLOR_NORMAL);
        }
        BitmapText selectedText = options.get(selectedIndex).text;
        selector.setLocalTranslation(selectedText.getLocalTranslation().x - 26f, selectedText.getLocalTranslation().y, 0);
    }

    private void startGame() {
        getStateManager().attach(new LoginState(session));
        setEnabled(false);
    }

    private void openOptions() {
        showingOptions = true;
        app.getGuiNode().attachChild(optionsInfo);
    }

    private void closeOptions() {
        showingOptions = false;
        optionsInfo.removeFromParent();
    }

    private void quit() {
        app.stop();
    }
}
