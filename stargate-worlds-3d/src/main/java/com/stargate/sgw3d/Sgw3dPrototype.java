package com.stargate.sgw3d;

import com.jme3.app.SimpleApplication;
import com.jme3.app.state.ScreenshotAppState;
import com.jme3.font.BitmapText;
import com.jme3.input.KeyInput;
import com.jme3.input.controls.ActionListener;
import com.jme3.input.controls.KeyTrigger;
import com.jme3.light.AmbientLight;
import com.jme3.light.DirectionalLight;
import com.jme3.material.Material;
import com.jme3.material.RenderState.BlendMode;
import com.jme3.math.ColorRGBA;
import com.jme3.math.FastMath;
import com.jme3.math.Vector3f;
import com.jme3.scene.Geometry;
import com.jme3.scene.Node;
import com.jme3.scene.shape.Box;
import com.jme3.scene.shape.Cylinder;
import com.jme3.scene.shape.Torus;
import com.jme3.system.AppSettings;

/**
 * Vertical-slice prototype: one planet surface, one Stargate, one away-team
 * avatar. Proves the Java/jMonkeyEngine rendering path against the existing
 * SGC lore (chevron count, cover-based combat concept from Stargate Worlds
 * previews) without touching the ASP.NET Core backend or the current 2D web
 * client. Move with WASD, look around is fixed to a chase camera.
 */
public class Sgw3dPrototype extends SimpleApplication implements ActionListener {

    private static final float MOVE_SPEED = 6f;
    private static final int CHEVRON_COUNT = 9;

    private final Node playerNode = new Node("away-team-member");
    private final Vector3f moveDirection = new Vector3f();
    private boolean up, down, left, right;

    private int screenshotAtFrame = -1;
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

        // Optional: pass --screenshot-and-exit to render a handful of frames,
        // save a screenshot and quit. Used for headless verification (CI/Xvfb).
        for (String arg : args) {
            if (arg.equals("--screenshot-and-exit")) {
                app.screenshotAtFrame = 20;
            }
        }
        app.start();
    }

    @Override
    public void simpleInitApp() {
        flyCam.setEnabled(false);
        setDisplayStatView(false);
        setDisplayFps(false);

        buildLighting();
        buildGround();
        buildStargate();
        buildCoverObjects();
        buildPlayer();
        buildHud();
        registerInput();

        cam.setLocation(new Vector3f(0, 5, 12));
        cam.lookAt(new Vector3f(0, 1.5f, 0), Vector3f.UNIT_Y);

        // Always available: press F2 to save a numbered screenshot (sgw3d-liveNNNN.png)
        // into the working directory, e.g. to verify WASD movement from an external
        // key-injection tool (xdotool) against the real InputManager binding.
        screenshotAppState = new ScreenshotAppState();
        screenshotAppState.setFilePath(System.getProperty("user.dir") + "/");
        screenshotAppState.setFileName("sgw3d-live");
        screenshotAppState.setIsNumbered(true);
        stateManager.attach(screenshotAppState);
    }

    private void buildLighting() {
        AmbientLight ambient = new AmbientLight();
        ambient.setColor(ColorRGBA.White.mult(0.45f));
        rootNode.addLight(ambient);

        DirectionalLight sun = new DirectionalLight();
        sun.setDirection(new Vector3f(-0.5f, -1f, -0.4f).normalizeLocal());
        sun.setColor(ColorRGBA.White.mult(1.1f));
        rootNode.addLight(sun);
    }

    /** Placeholder for "Beta Site" style rocky/tan terrain from the SGW planet list. */
    private void buildGround() {
        Box groundMesh = new Box(24f, 0.2f, 24f);
        Geometry ground = new Geometry("ground", groundMesh);
        Material groundMat = new Material(assetManager, "Common/MatDefs/Light/Lighting.j3md");
        groundMat.setBoolean("UseMaterialColors", true);
        groundMat.setColor("Diffuse", new ColorRGBA(0.55f, 0.42f, 0.28f, 1f));
        groundMat.setColor("Ambient", new ColorRGBA(0.55f, 0.42f, 0.28f, 1f));
        ground.setMaterial(groundMat);
        ground.setLocalTranslation(0, -0.2f, 0);
        rootNode.attachChild(ground);
    }

    /** Ring + 9 chevrons + event horizon, mirroring the chevron count used in the web client's dial animation. */
    private void buildStargate() {
        Node gate = new Node("stargate");

        Torus ringMesh = new Torus(32, 16, 0.35f, 3.2f);
        Geometry ring = new Geometry("gate-ring", ringMesh);
        Material ringMat = new Material(assetManager, "Common/MatDefs/Light/Lighting.j3md");
        ringMat.setBoolean("UseMaterialColors", true);
        ringMat.setColor("Diffuse", new ColorRGBA(0.14f, 0.16f, 0.18f, 1f));
        ringMat.setColor("Ambient", new ColorRGBA(0.14f, 0.16f, 0.18f, 1f));
        ring.setMaterial(ringMat);
        gate.attachChild(ring);

        for (int i = 0; i < CHEVRON_COUNT; i++) {
            float angle = FastMath.TWO_PI * i / CHEVRON_COUNT;
            Box chevronMesh = new Box(0.18f, 0.28f, 0.12f);
            Geometry chevron = new Geometry("chevron-" + (i + 1), chevronMesh);
            Material chevronMat = new Material(assetManager, "Common/MatDefs/Misc/Unshaded.j3md");
            chevronMat.setColor("Color", new ColorRGBA(0.4f, 0.75f, 0.87f, 1f));
            chevron.setMaterial(chevronMat);
            chevron.setLocalTranslation(FastMath.sin(angle) * 3.2f, FastMath.cos(angle) * 3.2f, 0);
            gate.attachChild(chevron);
        }

        Cylinder horizonMesh = new Cylinder(2, 32, 2.65f, 0.06f, true);
        Geometry horizon = new Geometry("event-horizon", horizonMesh);
        Material horizonMat = new Material(assetManager, "Common/MatDefs/Misc/Unshaded.j3md");
        horizonMat.setColor("Color", new ColorRGBA(0.4f, 0.85f, 1f, 0.75f));
        horizonMat.getAdditionalRenderState().setBlendMode(BlendMode.Alpha);
        horizon.setMaterial(horizonMat);
        gate.attachChild(horizon);

        gate.setLocalTranslation(0, 3.6f, -8f);
        rootNode.attachChild(gate);
    }

    /** Low crates standing in for Stargate Worlds' cover-based combat concept (visual only in this slice). */
    private void buildCoverObjects() {
        float[][] positions = { { -3f, -3f }, { 3.5f, -1.5f }, { -1.5f, 2.5f } };
        for (int i = 0; i < positions.length; i++) {
            Box coverMesh = new Box(0.6f, 0.5f, 0.6f);
            Geometry cover = new Geometry("cover-" + i, coverMesh);
            Material coverMat = new Material(assetManager, "Common/MatDefs/Light/Lighting.j3md");
            coverMat.setBoolean("UseMaterialColors", true);
            coverMat.setColor("Diffuse", new ColorRGBA(0.3f, 0.3f, 0.32f, 1f));
            coverMat.setColor("Ambient", new ColorRGBA(0.3f, 0.3f, 0.32f, 1f));
            cover.setMaterial(coverMat);
            cover.setLocalTranslation(positions[i][0], 0.5f, positions[i][1]);
            rootNode.attachChild(cover);
        }
    }

    /** Stand-in for an SGC away-team member (Tau'ri faction blue). */
    private void buildPlayer() {
        Box bodyMesh = new Box(0.35f, 0.9f, 0.25f);
        Geometry body = new Geometry("player-body", bodyMesh);
        Material bodyMat = new Material(assetManager, "Common/MatDefs/Light/Lighting.j3md");
        bodyMat.setBoolean("UseMaterialColors", true);
        bodyMat.setColor("Diffuse", new ColorRGBA(0.2f, 0.4f, 0.85f, 1f));
        bodyMat.setColor("Ambient", new ColorRGBA(0.2f, 0.4f, 0.85f, 1f));
        body.setMaterial(bodyMat);
        body.setLocalTranslation(0, 0.9f, 0);
        playerNode.attachChild(body);
        playerNode.setLocalTranslation(0, 0, 4f);
        rootNode.attachChild(playerNode);
    }

    private void buildHud() {
        guiFont = assetManager.loadFont("Interface/Fonts/Default.fnt");
        BitmapText title = new BitmapText(guiFont);
        title.setSize(guiFont.getCharSet().getRenderedSize() * 1.1f);
        title.setColor(ColorRGBA.White);
        title.setText("SGW Vertical Slice -- Beta Site -- SGC Away Team");
        title.setLocalTranslation(10, settings.getHeight() - 10, 0);
        guiNode.attachChild(title);

        BitmapText hint = new BitmapText(guiFont);
        hint.setSize(guiFont.getCharSet().getRenderedSize());
        hint.setColor(ColorRGBA.LightGray);
        hint.setText("WASD: move  |  ESC: quit");
        hint.setLocalTranslation(10, settings.getHeight() - 30, 0);
        guiNode.attachChild(hint);
    }

    private void registerInput() {
        inputManager.addMapping("MoveForward", new KeyTrigger(KeyInput.KEY_W));
        inputManager.addMapping("MoveBack", new KeyTrigger(KeyInput.KEY_S));
        inputManager.addMapping("MoveLeft", new KeyTrigger(KeyInput.KEY_A));
        inputManager.addMapping("MoveRight", new KeyTrigger(KeyInput.KEY_D));
        inputManager.addMapping("TakeScreenshot", new KeyTrigger(KeyInput.KEY_F12));
        inputManager.addListener(this, "MoveForward", "MoveBack", "MoveLeft", "MoveRight", "TakeScreenshot");
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        switch (name) {
            case "MoveForward": up = isPressed; break;
            case "MoveBack": down = isPressed; break;
            case "MoveLeft": left = isPressed; break;
            case "MoveRight": right = isPressed; break;
            case "TakeScreenshot":
                if (isPressed && screenshotAppState != null) { screenshotAppState.takeScreenshot(); }
                break;
            default: break;
        }
    }

    @Override
    public void simpleUpdate(float tpf) {
        moveDirection.set(0, 0, 0);
        if (up) moveDirection.z -= 1;
        if (down) moveDirection.z += 1;
        if (left) moveDirection.x -= 1;
        if (right) moveDirection.x += 1;
        if (moveDirection.lengthSquared() > 0) {
            moveDirection.normalizeLocal().multLocal(MOVE_SPEED * tpf);
            playerNode.move(moveDirection);
        }

        Vector3f playerPos = playerNode.getLocalTranslation();
        Vector3f cameraOffset = new Vector3f(0, 4.2f, 9f);
        cam.setLocation(playerPos.add(cameraOffset));
        cam.lookAt(playerPos.add(0, 1.5f, 0), Vector3f.UNIT_Y);

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
