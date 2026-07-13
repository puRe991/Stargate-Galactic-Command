package com.stargate.sgw3d;

import com.jme3.font.BitmapFont;
import com.jme3.font.BitmapText;
import com.jme3.math.ColorRGBA;

/**
 * Minimal single-line text field rendered as a BitmapText. Characters are
 * fed in externally (from a RawInputListener) since jME has no built-in
 * text input widget without an extra GUI library.
 */
class TextField {

    private static final int MAX_LENGTH = 24;
    private static final ColorRGBA PLACEHOLDER_COLOR = new ColorRGBA(0.5f, 0.55f, 0.6f, 1f);

    private final StringBuilder value = new StringBuilder();
    private final BitmapText display;
    private final String placeholder;
    private final boolean masked;
    private boolean focused;
    private float blinkTimer;
    private boolean cursorOn = true;

    TextField(BitmapFont font, String placeholder, boolean masked) {
        this.placeholder = placeholder;
        this.masked = masked;
        display = new BitmapText(font);
        display.setSize(font.getCharSet().getRenderedSize() * 1.1f);
        refresh();
    }

    BitmapText getDisplay() {
        return display;
    }

    String getValue() {
        return value.toString();
    }

    void setFocused(boolean focused) {
        this.focused = focused;
        blinkTimer = 0;
        cursorOn = true;
        refresh();
    }

    void handleChar(char c) {
        if (!focused || c < 32 || c == 127) {
            return;
        }
        if (value.length() < MAX_LENGTH) {
            value.append(c);
            refresh();
        }
    }

    void handleBackspace() {
        if (!focused || value.length() == 0) {
            return;
        }
        value.deleteCharAt(value.length() - 1);
        refresh();
    }

    void update(float tpf) {
        if (!focused) {
            return;
        }
        blinkTimer += tpf;
        if (blinkTimer >= 0.5f) {
            blinkTimer = 0;
            cursorOn = !cursorOn;
            refresh();
        }
    }

    private void refresh() {
        if (value.length() == 0 && !focused) {
            display.setColor(PLACEHOLDER_COLOR);
            display.setText(placeholder);
            return;
        }
        display.setColor(ColorRGBA.White);
        String shown = masked ? "*".repeat(value.length()) : value.toString();
        display.setText(shown + (focused && cursorOn ? "_" : ""));
    }
}
