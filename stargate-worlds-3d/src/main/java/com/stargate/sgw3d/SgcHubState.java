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
import com.jme3.math.FastMath;
import com.jme3.math.Vector3f;
import com.jme3.scene.Geometry;
import com.jme3.scene.Node;
import com.jme3.scene.shape.Box;
import com.jme3.scene.shape.Cylinder;

/**
 * Stargate Command, Ebene 28 -- the starting hub after character selection,
 * laid out after the original Cheyenne Mountain Level 28 floor plan:
 * gate room with ramp and Stargate, control room with observation window,
 * blast door to the tunnel corridor, round hallway with elevators, and the
 * laboratory. Rooms are walkable rectangles with wall collision (axis-wise
 * sliding). Approaching the ramp starts the 9-chevron dial sequence;
 * stepping into the open event horizon travels to {@link MissionState}.
 * The layout is rotated versus the plan so the gate faces the camera.
 */
public class SgcHubState extends BaseAppState implements ActionListener {

    private static final float MOVE_SPEED = 6f;
    private static final float WALL_HEIGHT = 2.6f;
    private static final float GATE_Z = -15f;
    private static final float DIAL_TRIGGER_Z = -9f;
    private static final float ENTER_Z = -14.2f;
    private static final float CHEVRON_INTERVAL = 0.35f;
    private static final float PLAYER_MARGIN = 0.4f;

    /**
     * Walkable floor areas {xmin, xmax, zmin, zmax}. Passage rectangles
     * deliberately overlap into their neighbouring rooms: the walkability
     * check shrinks every rectangle by PLAYER_MARGIN, which would otherwise
     * open an impassable seam exactly at each doorway.
     */
    private static final float[][] WALKABLE = {
            { -6, 6, -16, -4 },      // Gateroom
            { -1, 1, -4.9f, -1.1f }, // Blast-door passage
            { -14, 10, -2, 2 },      // Tunnel corridor
            { -12, -10, 1.1f, 4.9f },// Lab passage
            { -16, -6, 4, 12 },      // Laboratory
            { 8, 10, -6.9f, -1.1f }, // Control-room stairs passage
            { 7, 13, -14, -6 },      // Control room
            { 9, 16, -4, 4 },        // Round hallway (overlaps corridor end)
    };

    private final GameSession session;
    private SimpleApplication app;
    private Node sceneRoot;
    private Node hudRoot;
    private final Node playerNode = new Node("sgc-player");
    private final Vector3f moveDirection = new Vector3f();
    private boolean up, down, left, right;

    private StargateModel gate;
    private BitmapText statusText;
    private boolean dialing;
    private float dialTimer;
    private int chevronsLit;
    private float dialCooldown;

    SgcHubState(GameSession session) {
        this.session = session;
    }

    @Override
    protected void initialize(Application application) {
        this.app = (SimpleApplication) application;
        sceneRoot = new Node("sgc-scene");
        hudRoot = new Node("sgc-hud");

        buildLighting();
        buildLayout();
        buildGate();
        buildPlayer();
        buildHud();
    }

    @Override
    protected void cleanup(Application application) {
    }

    @Override
    protected void onEnable() {
        app.getRootNode().attachChild(sceneRoot);
        app.getGuiNode().attachChild(hudRoot);
        app.getInputManager().setCursorVisible(false);
        playerNode.setLocalTranslation(0, 0, 0.5f);
        gate.reset();
        dialing = false;
        dialTimer = 0;
        chevronsLit = 0;
        dialCooldown = 1.5f;
        status("Ebene 28 -- durch die Blast-Tuer in den Gate-Raum, Annaeherung startet die Waehlsequenz");
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
        ambient.setColor(ColorRGBA.White.mult(0.55f));
        sceneRoot.addLight(ambient);

        DirectionalLight overhead = new DirectionalLight();
        overhead.setDirection(new Vector3f(-0.2f, -1f, -0.3f).normalizeLocal());
        overhead.setColor(ColorRGBA.White.mult(0.9f));
        sceneRoot.addLight(overhead);
    }

    /** Level-28 layout: floors, walls with door gaps, props. Open-topped so the chase camera sees over walls. */
    private void buildLayout() {
        ColorRGBA concrete = new ColorRGBA(0.34f, 0.35f, 0.38f, 1f);
        ColorRGBA floorColor = new ColorRGBA(0.24f, 0.25f, 0.28f, 1f);

        // One big floor slab under all rooms.
        addLitBox("floor", new Box(17f, 0.1f, 15f), new Vector3f(0, -0.1f, -2f), floorColor);

        // Gateroom (x -6..6, z -16..-4)
        addWall(0, -16.15f, 6.15f, concrete);            // north (behind gate)
        addWallZ(-6.15f, -10, 6.15f, concrete);          // west
        addWallZ(6.15f, -10, 6.15f, concrete);           // east (control room behind)
        addWall(-3.5f, -3.85f, 2.5f, concrete);          // south, west of blast door
        addWall(3.5f, -3.85f, 2.5f, concrete);           // south, east of blast door

        // Blast-door passage (x -1..1, z -4..-2) + massive door frame overhead
        addWallZ(-1.15f, -3f, 1f, concrete);
        addWallZ(1.15f, -3f, 1f, concrete);
        addLitBox("blast-door-frame", new Box(1.6f, 0.4f, 0.3f), new Vector3f(0, 2.7f, -3f),
                new ColorRGBA(0.16f, 0.17f, 0.2f, 1f));

        // Tunnel corridor (x -14..10, z -2..2)
        addWall(-7.5f, -2.15f, 6.5f, concrete);          // north, west of blast door
        addWall(4.5f, -2.15f, 3.5f, concrete);           // north, between blast door and control stairs
        addWall(-13f, 2.15f, 1f, concrete);              // south, west of lab passage
        addWall(0, 2.15f, 10f, concrete);                // south, east of lab passage
        addWallZ(-14.15f, 0, 2.15f, concrete);           // west end

        // Lab passage (x -12..-10, z 2..4)
        addWallZ(-12.15f, 3f, 1f, concrete);
        addWallZ(-9.85f, 3f, 1f, concrete);

        // Laboratory (x -16..-6, z 4..12)
        addWall(-14f, 4.15f, 2f, concrete);              // north, west of passage
        addWall(-8f, 4.15f, 2f, concrete);               // north, east of passage
        addWall(-11f, 12.15f, 5.15f, concrete);          // south
        addWallZ(-16.15f, 8f, 4.15f, concrete);          // west
        addWallZ(-5.85f, 8f, 4.15f, concrete);           // east
        addLitBox("lab-table", new Box(1.2f, 0.5f, 0.7f), new Vector3f(-11f, 0.5f, 8f),
                new ColorRGBA(0.4f, 0.4f, 0.44f, 1f));
        addGlowBox("lab-console", new Box(0.06f, 0.5f, 1.1f), new Vector3f(-15.8f, 1.4f, 8f),
                new ColorRGBA(0.5f, 0.9f, 0.6f, 1f));

        // Control room (x 7..13, z -14..-6) with observation window toward the gate room
        addWall(10f, -14.15f, 3.15f, concrete);          // north
        addWallZ(6.85f, -10f, 4.15f, concrete);          // west (shared with gateroom east)
        addWallZ(13.15f, -10f, 4.15f, concrete);         // east
        addWall(7.5f, -5.85f, 0.5f, concrete);           // south, west of stairs
        addWall(11.5f, -5.85f, 1.5f, concrete);          // south, east of stairs
        addGlowBox("observation-window", new Box(0.08f, 0.7f, 2f), new Vector3f(6.3f, 2.4f, -11f),
                new ColorRGBA(0.55f, 0.75f, 0.95f, 1f));
        addLitBox("dial-computer", new Box(0.9f, 0.45f, 0.5f), new Vector3f(10f, 0.45f, -11f),
                new ColorRGBA(0.28f, 0.3f, 0.34f, 1f));

        // Control stairs passage (x 8..10, z -6..-2)
        addWallZ(7.85f, -4f, 2f, concrete);
        addWallZ(10.15f, -4f, 2f, concrete);

        // Round hallway (x 10..16, z -4..4) with central pillar and elevator doors
        addWall(13f, -4.15f, 3.15f, concrete);           // north
        addWall(13f, 4.15f, 3.15f, concrete);            // south
        addWallZ(16.15f, 0, 4.3f, concrete);             // east
        addWallZ(10f, -3f, 1f, concrete);                // west, north of corridor opening
        addWallZ(10f, 3f, 1f, concrete);                 // west, south of corridor opening
        Cylinder pillarMesh = new Cylinder(2, 16, 1.1f, WALL_HEIGHT, true);
        Geometry pillar = new Geometry("round-hallway-pillar", pillarMesh);
        Material pillarMat = new Material(app.getAssetManager(), "Common/MatDefs/Light/Lighting.j3md");
        pillarMat.setBoolean("UseMaterialColors", true);
        pillarMat.setColor("Diffuse", concrete);
        pillarMat.setColor("Ambient", concrete);
        pillar.setMaterial(pillarMat);
        pillar.rotate(FastMath.HALF_PI, 0, 0);
        pillar.setLocalTranslation(13f, WALL_HEIGHT / 2f, 0);
        sceneRoot.attachChild(pillar);
        addGlowBox("elevator-door", new Box(0.06f, 1.1f, 0.8f), new Vector3f(15.9f, 1.2f, 0),
                new ColorRGBA(0.85f, 0.8f, 0.55f, 1f));

        // Metal ramp strip up to the gate.
        addLitBox("ramp", new Box(1.7f, 0.04f, 2.6f), new Vector3f(0, 0.04f, -12f),
                new ColorRGBA(0.18f, 0.19f, 0.22f, 1f));

        // Tunnel lights along the corridor walls.
        float[][] lights = { { -11f, -1.8f }, { -5f, -1.8f }, { 3f, -1.8f }, { -3f, 1.8f }, { 7f, 1.8f } };
        for (int i = 0; i < lights.length; i++) {
            addGlowBox("tunnel-light-" + i, new Box(0.4f, 0.12f, 0.05f),
                    new Vector3f(lights[i][0], 2.6f, lights[i][1]), new ColorRGBA(1f, 0.95f, 0.8f, 1f));
        }
    }

    /** Wall segment running along the X axis (thin in Z). */
    private void addWall(float cx, float cz, float halfX, ColorRGBA color) {
        addLitBox("wall", new Box(halfX, WALL_HEIGHT / 2f, 0.15f), new Vector3f(cx, WALL_HEIGHT / 2f, cz), color);
    }

    /** Wall segment running along the Z axis (thin in X). */
    private void addWallZ(float cx, float cz, float halfZ, ColorRGBA color) {
        addLitBox("wall", new Box(0.15f, WALL_HEIGHT / 2f, halfZ), new Vector3f(cx, WALL_HEIGHT / 2f, cz), color);
    }

    private void addLitBox(String name, Box mesh, Vector3f position, ColorRGBA color) {
        Geometry geometry = new Geometry(name, mesh);
        Material material = new Material(app.getAssetManager(), "Common/MatDefs/Light/Lighting.j3md");
        material.setBoolean("UseMaterialColors", true);
        material.setColor("Diffuse", color);
        material.setColor("Ambient", color);
        geometry.setMaterial(material);
        geometry.setLocalTranslation(position);
        sceneRoot.attachChild(geometry);
    }

    private void addGlowBox(String name, Box mesh, Vector3f position, ColorRGBA color) {
        Geometry geometry = new Geometry(name, mesh);
        Material material = new Material(app.getAssetManager(), "Common/MatDefs/Misc/Unshaded.j3md");
        material.setColor("Color", color);
        geometry.setMaterial(material);
        geometry.setLocalTranslation(position);
        sceneRoot.attachChild(geometry);
    }

    private void buildGate() {
        gate = new StargateModel(app.getAssetManager());
        gate.getNode().setLocalTranslation(0, 3.9f, GATE_Z);
        sceneRoot.attachChild(gate.getNode());
    }

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
        sceneRoot.attachChild(playerNode);
    }

    private void buildHud() {
        BitmapFont font = app.getAssetManager().loadFont("Interface/Fonts/Default.fnt");
        CharacterProfile character = session.activeCharacter;
        String who = character == null ? "SGC" : character.name + " (" + character.factionLabel() + " / " + character.roleLabel() + ")";

        BitmapText title = new BitmapText(font);
        title.setSize(font.getCharSet().getRenderedSize() * 1.1f);
        title.setColor(ColorRGBA.White);
        title.setText(who + " -- Stargate Center, Ebene 28");
        title.setLocalTranslation(10, app.getCamera().getHeight() - 10, 0);
        hudRoot.attachChild(title);

        BitmapText hint = new BitmapText(font);
        hint.setSize(font.getCharSet().getRenderedSize());
        hint.setColor(ColorRGBA.LightGray);
        hint.setText("WASD: bewegen  |  ESC: zurueck zur Charakterauswahl");
        hint.setLocalTranslation(10, app.getCamera().getHeight() - 30, 0);
        hudRoot.attachChild(hint);

        statusText = new BitmapText(font);
        statusText.setSize(font.getCharSet().getRenderedSize());
        statusText.setColor(new ColorRGBA(0.4f, 0.85f, 1f, 1f));
        statusText.setLocalTranslation(10, app.getCamera().getHeight() - 50, 0);
        hudRoot.attachChild(statusText);
    }

    private void status(String text) {
        if (statusText != null) {
            statusText.setText(text);
        }
    }

    private void registerInput() {
        InputManager im = app.getInputManager();
        im.addMapping("SgcForward", new KeyTrigger(KeyInput.KEY_W));
        im.addMapping("SgcBack", new KeyTrigger(KeyInput.KEY_S));
        im.addMapping("SgcLeft", new KeyTrigger(KeyInput.KEY_A));
        im.addMapping("SgcRight", new KeyTrigger(KeyInput.KEY_D));
        im.addMapping("SgcEscape", new KeyTrigger(KeyInput.KEY_ESCAPE));
        im.addListener(this, "SgcForward", "SgcBack", "SgcLeft", "SgcRight", "SgcEscape");
    }

    private void unregisterInput() {
        InputManager im = app.getInputManager();
        im.removeListener(this);
        for (String mapping : new String[] { "SgcForward", "SgcBack", "SgcLeft", "SgcRight", "SgcEscape" }) {
            if (im.hasMapping(mapping)) {
                im.deleteMapping(mapping);
            }
        }
    }

    @Override
    public void onAction(String name, boolean isPressed, float tpf) {
        switch (name) {
            case "SgcForward": up = isPressed; break;
            case "SgcBack": down = isPressed; break;
            case "SgcLeft": left = isPressed; break;
            case "SgcRight": right = isPressed; break;
            case "SgcEscape":
                if (isPressed) {
                    returnToCharacterSelect();
                }
                break;
            default: break;
        }
    }

    private void returnToCharacterSelect() {
        CharacterSelectState select = getStateManager().getState(CharacterSelectState.class);
        MainMenuState menu = getStateManager().getState(MainMenuState.class);
        getStateManager().detach(this);
        if (select != null) {
            select.setEnabled(true);
        } else if (menu != null) {
            menu.setEnabled(true);
        }
    }

    private void travelThroughGate() {
        getStateManager().attach(new MissionState(session));
        setEnabled(false);
    }

    private boolean isWalkable(float x, float z) {
        for (float[] r : WALKABLE) {
            if (x >= r[0] + PLAYER_MARGIN && x <= r[1] - PLAYER_MARGIN
                    && z >= r[2] + PLAYER_MARGIN && z <= r[3] - PLAYER_MARGIN) {
                return true;
            }
        }
        return false;
    }

    @Override
    public void update(float tpf) {
        moveDirection.set(0, 0, 0);
        if (up) moveDirection.z -= 1;
        if (down) moveDirection.z += 1;
        if (left) moveDirection.x -= 1;
        if (right) moveDirection.x += 1;

        Vector3f p = playerNode.getLocalTranslation().clone();
        if (moveDirection.lengthSquared() > 0) {
            moveDirection.normalizeLocal().multLocal(MOVE_SPEED * tpf);
            // Axis-separated collision so the player slides along walls.
            if (isWalkable(p.x + moveDirection.x, p.z)) {
                p.x += moveDirection.x;
            }
            if (isWalkable(p.x, p.z + moveDirection.z)) {
                p.z += moveDirection.z;
            }
            playerNode.setLocalTranslation(p);
        }

        // Steep chase camera; rooms are open-topped and walls low enough
        // that this angle always clears them.
        Vector3f cameraPos = p.add(0, 7f, 5f);
        app.getCamera().setLocation(cameraPos);
        app.getCamera().lookAt(p.add(0, 1f, -2.5f), Vector3f.UNIT_Y);

        if (dialCooldown > 0) {
            dialCooldown -= tpf;
            return;
        }

        if (!dialing && !gate.isHorizonActive() && p.z < DIAL_TRIGGER_Z && Math.abs(p.x) < 6f) {
            dialing = true;
            dialTimer = 0;
        }

        if (dialing && !gate.isHorizonActive()) {
            dialTimer += tpf;
            int target = Math.min((int) (dialTimer / CHEVRON_INTERVAL), StargateModel.CHEVRON_COUNT);
            while (chevronsLit < target) {
                chevronsLit++;
                gate.setChevronsLit(chevronsLit);
                status("Chevron " + chevronsLit + " codiert ...");
            }
            if (chevronsLit >= StargateModel.CHEVRON_COUNT
                    && dialTimer > StargateModel.CHEVRON_COUNT * CHEVRON_INTERVAL + 0.4f) {
                gate.activateHorizon();
                dialing = false;
                status("Wurmloch aktiv -- betrete das Gate zur Mission (Beta Site)");
            }
        }

        if (gate.isHorizonActive() && p.z < ENTER_Z && Math.abs(p.x) < 2.4f) {
            travelThroughGate();
        }
    }
}
