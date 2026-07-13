package com.stargate.sgw3d;

import com.jme3.asset.AssetManager;
import com.jme3.material.Material;
import com.jme3.material.RenderState.BlendMode;
import com.jme3.math.ColorRGBA;
import com.jme3.math.FastMath;
import com.jme3.scene.Geometry;
import com.jme3.scene.Node;
import com.jme3.scene.Spatial.CullHint;
import com.jme3.scene.shape.Box;
import com.jme3.scene.shape.Cylinder;
import com.jme3.scene.shape.Torus;

import java.util.ArrayList;
import java.util.List;

/**
 * Reusable procedural Stargate: ring, 9 individually switchable chevrons and
 * an event horizon that stays hidden until activated. Built entirely from
 * primitives so no downloaded 3D assets are needed; both the SGC hub and the
 * planet mission scene share this model. The gate faces the +Z axis.
 */
class StargateModel {

    static final int CHEVRON_COUNT = 9;
    static final float RING_RADIUS = 3.2f;

    private static final ColorRGBA CHEVRON_UNLIT = new ColorRGBA(0.09f, 0.14f, 0.17f, 1f);
    private static final ColorRGBA CHEVRON_LIT = new ColorRGBA(0.4f, 0.85f, 1f, 1f);

    private final Node node = new Node("stargate");
    private final List<Material> chevronMaterials = new ArrayList<>();
    private final Geometry horizon;
    private boolean horizonActive;

    StargateModel(AssetManager assetManager) {
        Torus ringMesh = new Torus(32, 16, 0.35f, RING_RADIUS);
        Geometry ring = new Geometry("gate-ring", ringMesh);
        Material ringMat = new Material(assetManager, "Common/MatDefs/Light/Lighting.j3md");
        ringMat.setBoolean("UseMaterialColors", true);
        ringMat.setColor("Diffuse", new ColorRGBA(0.14f, 0.16f, 0.18f, 1f));
        ringMat.setColor("Ambient", new ColorRGBA(0.14f, 0.16f, 0.18f, 1f));
        ring.setMaterial(ringMat);
        node.attachChild(ring);

        for (int i = 0; i < CHEVRON_COUNT; i++) {
            float angle = FastMath.TWO_PI * i / CHEVRON_COUNT;
            Box chevronMesh = new Box(0.18f, 0.28f, 0.12f);
            Geometry chevron = new Geometry("chevron-" + (i + 1), chevronMesh);
            Material chevronMat = new Material(assetManager, "Common/MatDefs/Misc/Unshaded.j3md");
            chevronMat.setColor("Color", CHEVRON_UNLIT.clone());
            chevron.setMaterial(chevronMat);
            // Pushed forward out of the torus tube (radius 0.35) so the studs
            // are visible on the gate's front face instead of buried inside it.
            chevron.setLocalTranslation(FastMath.sin(angle) * RING_RADIUS, FastMath.cos(angle) * RING_RADIUS, 0.4f);
            chevronMaterials.add(chevronMat);
            node.attachChild(chevron);
        }

        Cylinder horizonMesh = new Cylinder(2, 32, RING_RADIUS - 0.55f, 0.06f, true);
        horizon = new Geometry("event-horizon", horizonMesh);
        Material horizonMat = new Material(assetManager, "Common/MatDefs/Misc/Unshaded.j3md");
        horizonMat.setColor("Color", new ColorRGBA(0.4f, 0.85f, 1f, 0.75f));
        horizonMat.getAdditionalRenderState().setBlendMode(BlendMode.Alpha);
        horizon.setMaterial(horizonMat);
        horizon.setCullHint(CullHint.Always);
        node.attachChild(horizon);
    }

    Node getNode() {
        return node;
    }

    /** Lights chevrons 1..count and dims the rest. */
    void setChevronsLit(int count) {
        for (int i = 0; i < chevronMaterials.size(); i++) {
            chevronMaterials.get(i).setColor("Color", (i < count ? CHEVRON_LIT : CHEVRON_UNLIT).clone());
        }
    }

    void activateHorizon() {
        horizonActive = true;
        horizon.setCullHint(CullHint.Inherit);
    }

    boolean isHorizonActive() {
        return horizonActive;
    }

    /** Back to idle: all chevrons dark, wormhole closed. */
    void reset() {
        horizonActive = false;
        horizon.setCullHint(CullHint.Always);
        setChevronsLit(0);
    }
}
