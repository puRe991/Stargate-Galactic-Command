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

import java.util.List;

/**
 * Shared keyboard- and mouse-navigable vertical list screen, used by
 * {@link ServerSelectState} and {@link CharacterSelectState}. Rebuilds its
 * item list every time it becomes visible so dynamic content (e.g. a
 * freshly-created character) shows up without extra plumbing.
 */
abstract class ListMenuState extends BaseAppState implements ActionListener {

    static final class Item {
        final String label;
        final Runnable action;
        BitmapText text;

        Item(String label, Runnable action) {
            this.label = label;
            this.action = action;
        }
    }

    private static final ColorRGBA COLOR_NORMAL = new ColorRGBA(0.72f, 0.8f, 0.86f, 1f);
    private static final ColorRGBA COLOR_SELECTED = new ColorRGBA(0.4f, 0.85f, 1f, 1f);
    private static final float ITEM_SPACING = 38f;

    protected SimpleApplication app;
    protected BitmapFont font;
    private Node menuRoot;
    private BitmapText selector;
    private List<Item> items;
    private int selectedIndex;
    private Vector2f lastCursor;

    // Distinct per concrete subclass AND per instance: ServerSelectState and
    // CharacterSelectState both extend this class and can be enabled/disabled
    // in quick succession (going back and forth). Sharing literal mapping
    // names between them caused addMapping/deleteMapping calls from one
    // instance to clobber the other's still-registered input while both were
    // momentarily attached, silently swallowing Esc/Up/Down/Enter.
    private final String mp = getClass().getSimpleName() + "-" + System.identityHashCode(this) + "-";

    protected abstract String getTitle();

    protected String getSubtitle() {
        return null;
    }

    protected abstract List<Item> buildItems();

    /** Called on Esc. Default does nothing -- override to navigate back. */
    protected void onBack() {
    }

    @Override
    protected void initialize(Application application) {
        this.app = (SimpleApplication) application;
        this.font = app.getAssetManager().loadFont("Interface/Fonts/Default.fnt");
    }

    @Override
    protected void cleanup(Application application) {
    }

    @Override
    protected void onEnable() {
        rebuild();
        registerInput();
        app.getInputManager().setCursorVisible(true);
    }

    @Override
    protected void onDisable() {
        if (menuRoot != null) {
            menuRoot.removeFromParent();
        }
        unregisterInput();
    }

    private float centered(float lineWidth, int screenWidth) {
        return (screenWidth - lineWidth) / 2f;
    }

    private void rebuild() {
        if (menuRoot != null) {
            menuRoot.removeFromParent();
        }
        menuRoot = new Node(getClass().getSimpleName() + "-root");
        int width = app.getCamera().getWidth();
        int height = app.getCamera().getHeight();

        BitmapText title = new BitmapText(font);
        title.setSize(font.getCharSet().getRenderedSize() * 1.8f);
        title.setColor(ColorRGBA.White);
        title.setText(getTitle());
        title.setLocalTranslation(centered(title.getLineWidth(), width), height - 100, 0);
        menuRoot.attachChild(title);

        String subtitle = getSubtitle();
        if (subtitle != null) {
            BitmapText sub = new BitmapText(font);
            sub.setSize(font.getCharSet().getRenderedSize());
            sub.setColor(COLOR_NORMAL);
            sub.setText(subtitle);
            sub.setLocalTranslation(centered(sub.getLineWidth(), width), height - 135, 0);
            menuRoot.attachChild(sub);
        }

        items = buildItems();
        selectedIndex = 0;
        lastCursor = null;

        float startY = height / 2f + (items.size() - 1) * ITEM_SPACING / 2f;
        for (int i = 0; i < items.size(); i++) {
            Item item = items.get(i);
            BitmapText text = new BitmapText(font);
            text.setSize(font.getCharSet().getRenderedSize() * 1.15f);
            text.setColor(COLOR_NORMAL);
            text.setText(item.label);
            text.setLocalTranslation(centered(text.getLineWidth(), width), startY - i * ITEM_SPACING, 0);
            item.text = text;
            menuRoot.attachChild(text);
        }

        selector = new BitmapText(font);
        selector.setSize(font.getCharSet().getRenderedSize() * 1.15f);
        selector.setColor(COLOR_SELECTED);
        selector.setText(">");
        menuRoot.attachChild(selector);

        app.getGuiNode().attachChild(menuRoot);
        updateHighlight();
    }

    private void registerInput() {
        InputManager im = app.getInputManager();
        im.addMapping(mp + "Up", new KeyTrigger(KeyInput.KEY_UP), new KeyTrigger(KeyInput.KEY_W));
        im.addMapping(mp + "Down", new KeyTrigger(KeyInput.KEY_DOWN), new KeyTrigger(KeyInput.KEY_S));
        im.addMapping(mp + "Select", new KeyTrigger(KeyInput.KEY_RETURN), new KeyTrigger(KeyInput.KEY_SPACE));
        im.addMapping(mp + "Back", new KeyTrigger(KeyInput.KEY_ESCAPE));
        im.addMapping(mp + "Click", new MouseButtonTrigger(MouseInput.BUTTON_LEFT));
        im.addListener(this, mp + "Up", mp + "Down", mp + "Select", mp + "Back", mp + "Click");
    }

    private void unregisterInput() {
        InputManager im = app.getInputManager();
        im.removeListener(this);
        for (String mapping : new String[] { mp + "Up", mp + "Down", mp + "Select", mp + "Back", mp + "Click" }) {
            if (im.hasMapping(mapping)) {
                im.deleteMapping(mapping);
            }
        }
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        if (!isPressed || items.isEmpty()) {
            if (isPressed && (mp + "Back").equals(name)) {
                onBack();
            }
            return;
        }
        if ((mp + "Up").equals(name)) {
            selectedIndex = (selectedIndex - 1 + items.size()) % items.size();
            updateHighlight();
        } else if ((mp + "Down").equals(name)) {
            selectedIndex = (selectedIndex + 1) % items.size();
            updateHighlight();
        } else if ((mp + "Select").equals(name)) {
            items.get(selectedIndex).action.run();
        } else if ((mp + "Back").equals(name)) {
            onBack();
        } else if ((mp + "Click").equals(name)) {
            handleClick();
        }
    }

    @Override
    public void update(float tpf) {
        if (items == null || items.isEmpty()) {
            return;
        }
        Vector2f cursor = app.getInputManager().getCursorPosition();
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
            items.get(clicked).action.run();
        }
    }

    private int itemUnderCursor() {
        Vector2f cursor = app.getInputManager().getCursorPosition();
        for (int i = 0; i < items.size(); i++) {
            BitmapText text = items.get(i).text;
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
        if (items.isEmpty()) {
            selector.setText("");
            return;
        }
        for (int i = 0; i < items.size(); i++) {
            items.get(i).text.setColor(i == selectedIndex ? COLOR_SELECTED : COLOR_NORMAL);
        }
        BitmapText selectedText = items.get(selectedIndex).text;
        selector.setText(">");
        selector.setLocalTranslation(selectedText.getLocalTranslation().x - 26f, selectedText.getLocalTranslation().y, 0);
    }
}
