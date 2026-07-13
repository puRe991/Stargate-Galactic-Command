package com.stargate.sgw3d;

import com.jme3.app.SimpleApplication;
import com.jme3.app.state.ScreenshotAppState;
import com.jme3.input.KeyInput;
import com.jme3.input.controls.ActionListener;
import com.jme3.input.controls.KeyTrigger;
import com.jme3.math.ColorRGBA;
import com.jme3.system.AppSettings;

/**
 * App shell: boots the window, wires the global screenshot hotkey (F12,
 * used for headless/CI verification) and hands off to the main menu.
 * Gameplay lives in {@link MissionState}, menu navigation in
 * {@link MainMenuState} -- both are jME AppStates attached/detached as the
 * player moves between screens.
 */
public class Sgw3dPrototype extends SimpleApplication implements ActionListener {

    private int screenshotAtFrame = -1;
    private boolean autoStartMission = false;
    private int frameCount = 0;
    private ScreenshotAppState screenshotAppState;

    public static void main(String[] args) {
        Sgw3dPrototype app = new Sgw3dPrototype();
        AppSettings settings = new AppSettings(true);
        settings.setResolution(1280, 720);
        settings.setTitle("Stargate Galactic Command - SGW Vertical Slice");
        settings.setVSync(false);
        settings.setAudioRenderer(null);
        app.setSettings(settings);
        app.setShowSettings(false);
        app.setPauseOnLostFocus(false);

        // Headless/CI verification flags: render a handful of frames, save a
        // screenshot and quit, without needing a human at the keyboard.
        for (String arg : args) {
            if (arg.equals("--screenshot-and-exit")) {
                app.screenshotAtFrame = 20;
            }
            if (arg.equals("--screenshot-mission-and-exit")) {
                app.screenshotAtFrame = 20;
                app.autoStartMission = true;
            }
        }
        app.start();
    }

    @Override
    public void simpleInitApp() {
        flyCam.setEnabled(false);
        setDisplayStatView(false);
        setDisplayFps(false);
        viewPort.setBackgroundColor(new ColorRGBA(0.02f, 0.04f, 0.07f, 1f));

        // SimpleApplication binds Esc to immediate app-exit by default; both
        // MainMenuState (close Optionen) and MissionState (back to menu)
        // need Esc for their own navigation instead.
        inputManager.deleteMapping(INPUT_MAPPING_EXIT);

        // Always available regardless of screen: F12 saves a numbered
        // screenshot (sgw3d-liveNNNN.png) into the working directory.
        screenshotAppState = new ScreenshotAppState();
        screenshotAppState.setFilePath(System.getProperty("user.dir") + "/");
        screenshotAppState.setFileName("sgw3d-live");
        screenshotAppState.setIsNumbered(true);
        stateManager.attach(screenshotAppState);

        inputManager.addMapping("TakeScreenshot", new KeyTrigger(KeyInput.KEY_F12));
        inputManager.addListener(this, "TakeScreenshot");

        GameSession session = new GameSession();
        if (autoStartMission) {
            session.selectedServer = "CI-Testwelt";
            session.activeCharacter = new CharacterProfile("CI-Test",
                    CharacterProfile.Faction.TAURI, CharacterProfile.Role.MILITAER);
            stateManager.attach(new MissionState(session));
        } else {
            stateManager.attach(new MainMenuState(session));
        }
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        if ("TakeScreenshot".equals(name) && isPressed && screenshotAppState != null) {
            screenshotAppState.takeScreenshot();
        }
    }

    @Override
    public void simpleUpdate(float tpf) {
        if (screenshotAtFrame > 0) {
            frameCount++;
            if (frameCount == screenshotAtFrame) {
                screenshotAppState.takeScreenshot();
            } else if (frameCount == screenshotAtFrame + 5) {
                stop();
            }
        }
    }
}
