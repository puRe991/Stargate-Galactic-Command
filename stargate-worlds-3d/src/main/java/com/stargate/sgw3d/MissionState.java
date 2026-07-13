package com.stargate.sgw3d;

import com.jme3.app.Application;
import com.jme3.app.SimpleApplication;
import com.jme3.app.state.BaseAppState;
import com.jme3.font.BitmapFont;
import com.jme3.font.BitmapText;
import com.jme3.input.InputManager;
import com.jme3.input.KeyInput;
import com.jme3.input.controls.ActionListener;
import com.jme3.input.controls.KeyTrigger;
import com.jme3.light.AmbientLight;
import com.jme3.light.DirectionalLight;
import com.jme3.material.Material;
import com.jme3.math.ColorRGBA;
import com.jme3.math.Vector3f;
import com.jme3.scene.Geometry;
import com.jme3.scene.Node;
import com.jme3.scene.shape.Box;

/**
 * The vertical-slice gameplay scene: one planet surface, one Stargate, one
 * away-team avatar. A fresh instance is attached each time a character is
 * chosen or created at the end of the Login -> Server -> Character flow,
 * and detaches itself on Esc.
 */
public class MissionState extends BaseAppState implements ActionListener {

    private static final float MOVE_SPEED = 6f;

    private final GameSession session;
    private SimpleApplication app;
    private Node sceneRoot;
    private Node hudRoot;
    private final Node playerNode = new Node("away-team-member");
    private final Vector3f moveDirection = new Vector3f();
    private boolean up, down, left, right;

    MissionState(GameSession session) {
        this.session = session;
    }

    @Override
    protected void initialize(Application application) {
        this.app = (SimpleApplication) application;
        sceneRoot = new Node("mission-scene");
        hudRoot = new Node("mission-hud");

        buildLighting();
        buildGround();
        buildStargate();
        buildCoverObjects();
        buildPlayer();
        buildHud();
    }

    @Override
    protected void cleanup(Application application) {
        // Scene graph nodes are already detached in onDisable(); nothing else to release.
    }

    @Override
    protected void onEnable() {
        app.getRootNode().attachChild(sceneRoot);
        app.getGuiNode().attachChild(hudRoot);
        app.getInputManager().setCursorVisible(false);
        app.getCamera().setLocation(new Vector3f(0, 5, 12));
        app.getCamera().lookAt(new Vector3f(0, 1.5f, 0), Vector3f.UNIT_Y);
        registerInput();
    }

    @Override
    protected void onDisable() {
        sceneRoot.removeFromParent();
        hudRoot.removeFromParent();
        unregisterInput();
    }

    private void buildLighting() {
        AmbientLight ambient = new AmbientLight();
        ambient.setColor(ColorRGBA.White.mult(0.45f));
        sceneRoot.addLight(ambient);

        DirectionalLight sun = new DirectionalLight();
        sun.setDirection(new Vector3f(-0.5f, -1f, -0.4f).normalizeLocal());
        sun.setColor(ColorRGBA.White.mult(1.1f));
        sceneRoot.addLight(sun);
    }

    /** Placeholder for "Beta Site" style rocky/tan terrain from the SGW planet list. */
    private void buildGround() {
        Box groundMesh = new Box(24f, 0.2f, 24f);
        Geometry ground = new Geometry("ground", groundMesh);
        Material groundMat = new Material(app.getAssetManager(), "Common/MatDefs/Light/Lighting.j3md");
        groundMat.setBoolean("UseMaterialColors", true);
        groundMat.setColor("Diffuse", new ColorRGBA(0.55f, 0.42f, 0.28f, 1f));
        groundMat.setColor("Ambient", new ColorRGBA(0.55f, 0.42f, 0.28f, 1f));
        ground.setMaterial(groundMat);
        ground.setLocalTranslation(0, -0.2f, 0);
        sceneRoot.attachChild(ground);
    }

    /** Shared procedural gate model; arrives "hot" -- all chevrons lit, wormhole still open behind the team. */
    private void buildStargate() {
        StargateModel gate = new StargateModel(app.getAssetManager());
        gate.setChevronsLit(StargateModel.CHEVRON_COUNT);
        gate.activateHorizon();
        gate.getNode().setLocalTranslation(0, 3.6f, -8f);
        sceneRoot.attachChild(gate.getNode());
    }

    /** Low crates standing in for Stargate Worlds' cover-based combat concept (visual only in this slice). */
    private void buildCoverObjects() {
        float[][] positions = { { -3f, -3f }, { 3.5f, -1.5f }, { -1.5f, 2.5f } };
        for (int i = 0; i < positions.length; i++) {
            Box coverMesh = new Box(0.6f, 0.5f, 0.6f);
            Geometry cover = new Geometry("cover-" + i, coverMesh);
            Material coverMat = new Material(app.getAssetManager(), "Common/MatDefs/Light/Lighting.j3md");
            coverMat.setBoolean("UseMaterialColors", true);
            coverMat.setColor("Diffuse", new ColorRGBA(0.3f, 0.3f, 0.32f, 1f));
            coverMat.setColor("Ambient", new ColorRGBA(0.3f, 0.3f, 0.32f, 1f));
            cover.setMaterial(coverMat);
            cover.setLocalTranslation(positions[i][0], 0.5f, positions[i][1]);
            sceneRoot.attachChild(cover);
        }
    }

    /** Away-team avatar, tinted with the active character's faction color. */
    private void buildPlayer() {
        Box bodyMesh = new Box(0.35f, 0.9f, 0.25f);
        Geometry body = new Geometry("player-body", bodyMesh);
        Material bodyMat = new Material(app.getAssetManager(), "Common/MatDefs/Light/Lighting.j3md");
        bodyMat.setBoolean("UseMaterialColors", true);
        ColorRGBA tint = FactionColors.of(session.activeCharacter);
        bodyMat.setColor("Diffuse", tint);
        bodyMat.setColor("Ambient", tint);
        body.setMaterial(bodyMat);
        body.setLocalTranslation(0, 0.9f, 0);
        playerNode.attachChild(body);
        playerNode.setLocalTranslation(0, 0, 4f);
        sceneRoot.attachChild(playerNode);
    }

    private void buildHud() {
        BitmapFont font = app.getAssetManager().loadFont("Interface/Fonts/Default.fnt");
        CharacterProfile character = session.activeCharacter;
        String characterLine = character == null
                ? "SGW Vertical Slice -- Beta Site -- SGC Away Team"
                : character.name + " (" + character.factionLabel() + " / " + character.roleLabel() + ") -- Beta Site";

        BitmapText title = new BitmapText(font);
        title.setSize(font.getCharSet().getRenderedSize() * 1.1f);
        title.setColor(ColorRGBA.White);
        title.setText(characterLine);
        title.setLocalTranslation(10, app.getCamera().getHeight() - 10, 0);
        hudRoot.attachChild(title);

        BitmapText hint = new BitmapText(font);
        hint.setSize(font.getCharSet().getRenderedSize());
        hint.setColor(ColorRGBA.LightGray);
        hint.setText("WASD: bewegen  |  ESC: zurueck zum Stargate Center");
        hint.setLocalTranslation(10, app.getCamera().getHeight() - 30, 0);
        hudRoot.attachChild(hint);
    }

    private void registerInput() {
        InputManager im = app.getInputManager();
        im.addMapping("MoveForward", new KeyTrigger(KeyInput.KEY_W));
        im.addMapping("MoveBack", new KeyTrigger(KeyInput.KEY_S));
        im.addMapping("MoveLeft", new KeyTrigger(KeyInput.KEY_A));
        im.addMapping("MoveRight", new KeyTrigger(KeyInput.KEY_D));
        im.addMapping("ReturnToMenu", new KeyTrigger(KeyInput.KEY_ESCAPE));
        im.addListener(this, "MoveForward", "MoveBack", "MoveLeft", "MoveRight", "ReturnToMenu");
    }

    private void unregisterInput() {
        InputManager im = app.getInputManager();
        im.removeListener(this);
        for (String mapping : new String[] { "MoveForward", "MoveBack", "MoveLeft", "MoveRight", "ReturnToMenu" }) {
            if (im.hasMapping(mapping)) {
                im.deleteMapping(mapping);
            }
        }
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        switch (name) {
            case "MoveForward":
                up = isPressed;
                break;
            case "MoveBack":
                down = isPressed;
                break;
            case "MoveLeft":
                left = isPressed;
                break;
            case "MoveRight":
                right = isPressed;
                break;
            case "ReturnToMenu":
                if (isPressed) {
                    returnToMenu();
                }
                break;
            default:
                break;
        }
    }

    private void returnToMenu() {
        SgcHubState sgcHub = getStateManager().getState(SgcHubState.class);
        CharacterSelectState characterSelect = getStateManager().getState(CharacterSelectState.class);
        MainMenuState menu = getStateManager().getState(MainMenuState.class);
        getStateManager().detach(this);
        if (sgcHub != null) {
            sgcHub.setEnabled(true);
        } else if (characterSelect != null) {
            characterSelect.setEnabled(true);
        } else if (menu != null) {
            menu.setEnabled(true);
        }
    }

    @Override
    public void update(float tpf) {
        moveDirection.set(0, 0, 0);
        if (up) {
            moveDirection.z -= 1;
        }
        if (down) {
            moveDirection.z += 1;
        }
        if (left) {
            moveDirection.x -= 1;
        }
        if (right) {
            moveDirection.x += 1;
        }
        if (moveDirection.lengthSquared() > 0) {
            moveDirection.normalizeLocal().multLocal(MOVE_SPEED * tpf);
            playerNode.move(moveDirection);
        }

        Vector3f playerPos = playerNode.getLocalTranslation();
        Vector3f cameraOffset = new Vector3f(0, 4.2f, 9f);
        app.getCamera().setLocation(playerPos.add(cameraOffset));
        app.getCamera().lookAt(playerPos.add(0, 1.5f, 0), Vector3f.UNIT_Y);
    }
}
