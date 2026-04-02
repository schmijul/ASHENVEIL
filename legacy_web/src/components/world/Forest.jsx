import { useLayoutEffect, useMemo, useRef } from "react";
import {
  BufferAttribute,
  BufferGeometry,
  Color,
  ConeGeometry,
  DodecahedronGeometry,
  IcosahedronGeometry,
  Object3D,
  Euler,
} from "three";
import { useFrame } from "@react-three/fiber";
import { CuboidCollider, CylinderCollider, RigidBody } from "@react-three/rapier";
import {
  getRockCollisionProfile,
  getTreeCollisionProfile,
} from "../../utils/collisionProfiles";
import { buildForestPlacements, valueNoise2D } from "../../utils/terrainGeneration";

const helper = new Object3D();
const _color = new Color();

const setPlacementMatrix = (placement) => {
  helper.position.set(
    placement.position[0],
    placement.position[1],
    placement.position[2],
  );
  helper.rotation.set(0, placement.rotation, 0);
  helper.scale.setScalar(placement.scale);
  helper.updateMatrix();
  return helper.matrix.clone();
};

const applyInstances = (mesh, placements, colorFn) => {
  placements.forEach((placement, index) => {
    mesh.setMatrixAt(index, setPlacementMatrix(placement));
    if (colorFn) {
      colorFn(_color, placement, index);
      mesh.setColorAt(index, _color);
    }
  });
  mesh.instanceMatrix.needsUpdate = true;
  if (mesh.instanceColor) mesh.instanceColor.needsUpdate = true;
};

// Displace geometry vertices with noise for organic shapes
const displaceGeometry = (geometry, amplitude, seed) => {
  const pos = geometry.attributes.position;
  for (let i = 0; i < pos.count; i++) {
    const x = pos.getX(i);
    const y = pos.getY(i);
    const z = pos.getZ(i);
    const dx = valueNoise2D(x * 3.1 + seed, y * 2.3, seed + 7) * amplitude;
    const dz = valueNoise2D(z * 2.7 + seed, y * 3.1, seed + 11) * amplitude;
    pos.setXYZ(i, x + dx, y, z + dz);
  }
  pos.needsUpdate = true;
  geometry.computeVertexNormals();
  return geometry;
};

// Custom grass blade cluster geometry
const buildGrassGeometry = () => {
  const positions = [];
  const colors = [];
  const indices = [];
  const bladeCount = 5;

  for (let b = 0; b < bladeCount; b++) {
    const angle = (b / bladeCount) * Math.PI * 2;
    const spread = 0.08 + b * 0.02;
    const height = 0.28 + b * 0.04;
    const lean = 0.06;

    const bx = Math.cos(angle) * spread;
    const bz = Math.sin(angle) * spread;
    const base = positions.length / 3;

    // Bottom vertex (dark green)
    positions.push(bx - 0.015, 0, bz);
    positions.push(bx + 0.015, 0, bz);
    // Mid vertex
    positions.push(bx - 0.01 + lean * Math.cos(angle), height * 0.55, bz + lean * Math.sin(angle));
    positions.push(bx + 0.01 + lean * Math.cos(angle), height * 0.55, bz + lean * Math.sin(angle));
    // Tip vertex (lighter green)
    positions.push(bx + lean * 1.5 * Math.cos(angle), height, bz + lean * 1.5 * Math.sin(angle));

    // Colors: dark at base, lighter at tip
    colors.push(0.18, 0.38, 0.13, 0.18, 0.38, 0.13); // base
    colors.push(0.28, 0.50, 0.18, 0.28, 0.50, 0.18); // mid
    colors.push(0.47, 0.60, 0.31); // tip (single)

    indices.push(
      base, base + 1, base + 2,
      base + 1, base + 3, base + 2,
      base + 2, base + 3, base + 4,
    );
  }

  const geo = new BufferGeometry();
  geo.setAttribute("position", new BufferAttribute(new Float32Array(positions), 3));
  geo.setAttribute("color", new BufferAttribute(new Float32Array(colors), 3));
  geo.setIndex(indices);
  geo.computeVertexNormals();
  return geo;
};

// Bush cross-plane geometry (4 intersecting quads)
const buildBushGeometry = (radius = 0.45, height = 0.55) => {
  const positions = [];
  const colors = [];
  const indices = [];

  for (let i = 0; i < 4; i++) {
    const angle = (i / 4) * Math.PI;
    const ax = Math.cos(angle) * radius;
    const az = Math.sin(angle) * radius;
    const base = positions.length / 3;

    positions.push(-ax, 0, -az);
    positions.push(ax, 0, az);
    positions.push(-ax * 0.8, height, -az * 0.8);
    positions.push(ax * 0.8, height, az * 0.8);

    const g = 0.28 + i * 0.02;
    colors.push(0.18, g, 0.12, 0.18, g, 0.12, 0.32, g + 0.12, 0.20, 0.32, g + 0.12, 0.20);

    indices.push(base, base + 1, base + 2, base + 1, base + 3, base + 2);
    indices.push(base + 2, base + 1, base, base + 2, base + 3, base + 1);
  }

  const geo = new BufferGeometry();
  geo.setAttribute("position", new BufferAttribute(new Float32Array(positions), 3));
  geo.setAttribute("color", new BufferAttribute(new Float32Array(colors), 3));
  geo.setIndex(indices);
  geo.computeVertexNormals();
  return geo;
};

export default function Forest() {
  const pinesTrunk1Ref = useRef(null);
  const pinesCanopy1Ref = useRef(null);
  const pinesCanopy2Ref = useRef(null);
  const pinesCanopy3Ref = useRef(null);
  const oaksTrunkRef = useRef(null);
  const oaksCanopyRef = useRef(null);
  const grassRef = useRef(null);
  const rocksRef = useRef(null);
  const bushesRef = useRef(null);
  const logsRef = useRef(null);

  const placements = useMemo(
    () => buildForestPlacements({ seed: 7, size: 120 }),
    [],
  );

  // Displaced pine canopy geometries (3 tiers)
  const pineCanopyGeo1 = useMemo(() => displaceGeometry(new ConeGeometry(1.3, 1.2, 10), 0.09, 3), []);
  const pineCanopyGeo2 = useMemo(() => displaceGeometry(new ConeGeometry(1.0, 1.0, 10), 0.07, 11), []);
  const pineCanopyGeo3 = useMemo(() => displaceGeometry(new ConeGeometry(0.6, 0.9, 8), 0.05, 19), []);

  // Displaced oak canopy
  const oakCanopyGeo = useMemo(() => displaceGeometry(new IcosahedronGeometry(1.4, 2), 0.22, 7), []);

  // Displaced rock geometry
  const rockGeo = useMemo(() => displaceGeometry(new DodecahedronGeometry(0.45, 1), 0.08, 13), []);

  const grassGeo = useMemo(() => buildGrassGeometry(), []);
  const bushGeo = useMemo(() => buildBushGeometry(), []);

  // Pine color variation
  const pineColorFn = (color, placement, index) => {
    const v = 0.85 + (index % 7) * 0.022;
    color.setRGB(0.24 * v, 0.39 * v, 0.22 * v);
  };
  // Oak color variation
  const oakColorFn = (color, placement, index) => {
    const v = 0.88 + (index % 5) * 0.025;
    color.setRGB(0.29 * v, 0.44 * v, 0.25 * v);
  };
  // Rock color variation: gray → brown-gray → mossy
  const rockColorFn = (color, placement, index) => {
    const t = (index % 3) / 2;
    if (t < 0.33) color.setRGB(0.35, 0.35, 0.34);
    else if (t < 0.66) color.setRGB(0.40, 0.37, 0.33);
    else color.setRGB(0.29, 0.35, 0.25);
  };

  useLayoutEffect(() => {
    if (pinesTrunk1Ref.current) {
      applyInstances(pinesTrunk1Ref.current, placements.pines.map((p) => ({
        ...p, position: [p.position[0], p.position[1] + 1.1 * p.scale, p.position[2]],
      })), pineColorFn);
    }
    if (pinesCanopy1Ref.current) {
      applyInstances(pinesCanopy1Ref.current, placements.pines.map((p) => ({
        ...p, position: [p.position[0], p.position[1] + 2.0 * p.scale, p.position[2]],
      })), pineColorFn);
    }
    if (pinesCanopy2Ref.current) {
      applyInstances(pinesCanopy2Ref.current, placements.pines.map((p) => ({
        ...p, position: [p.position[0], p.position[1] + 2.9 * p.scale, p.position[2]],
      })), pineColorFn);
    }
    if (pinesCanopy3Ref.current) {
      applyInstances(pinesCanopy3Ref.current, placements.pines.map((p) => ({
        ...p, position: [p.position[0], p.position[1] + 3.7 * p.scale, p.position[2]],
      })), pineColorFn);
    }
    if (oaksTrunkRef.current) {
      applyInstances(oaksTrunkRef.current, placements.oaks.map((p) => ({
        ...p, position: [p.position[0], p.position[1] + 0.9 * p.scale, p.position[2]],
      })), (color) => color.setRGB(0.36, 0.25, 0.20));
    }
    if (oaksCanopyRef.current) {
      applyInstances(oaksCanopyRef.current, placements.oaks.map((p) => ({
        ...p, position: [p.position[0], p.position[1] + 2.2 * p.scale, p.position[2]],
      })), oakColorFn);
    }
    if (grassRef.current) {
      applyInstances(grassRef.current, placements.grass.map((p) => ({
        ...p, position: [p.position[0], p.position[1], p.position[2]],
      })));
    }
    if (rocksRef.current) {
      applyInstances(rocksRef.current, placements.rocks, rockColorFn);
    }
    if (bushesRef.current) {
      applyInstances(bushesRef.current, placements.bushes.map((p) => ({
        ...p, position: [p.position[0], p.position[1], p.position[2]],
      })));
    }
    if (logsRef.current) {
      placements.logs.forEach((p, index) => {
        helper.position.set(p.position[0], p.position[1] + 0.12 * p.scale, p.position[2]);
        helper.rotation.set(Math.PI / 2, 0, p.rotation);
        helper.scale.setScalar(p.scale);
        helper.updateMatrix();
        logsRef.current.setMatrixAt(index, helper.matrix.clone());
      });
      logsRef.current.instanceMatrix.needsUpdate = true;
    }
  }, [placements]);

  // Wind animation for grass via shader time uniform
  const grassMat = useRef(null);
  useFrame(({ clock }) => {
    if (grassMat.current?.userData.timeUniform) {
      grassMat.current.userData.timeUniform.value = clock.getElapsedTime();
    }
  });

  return (
    <group>
      {/* Pine trunks */}
      {placements.pines.length > 0 && (
        <instancedMesh ref={pinesTrunk1Ref} args={[null, null, placements.pines.length]} castShadow>
          <cylinderGeometry args={[0.14, 0.22, 2.2, 10, 2]} />
          <meshStandardMaterial roughness={0.95} metalness={0} vertexColors />
        </instancedMesh>
      )}
      {/* Pine canopy — 3 stacked tiers */}
      {placements.pines.length > 0 && (
        <instancedMesh ref={pinesCanopy1Ref} args={[null, null, placements.pines.length]} castShadow>
          <primitive object={pineCanopyGeo1} attach="geometry" />
          <meshStandardMaterial roughness={0.82} metalness={0} flatShading vertexColors />
        </instancedMesh>
      )}
      {placements.pines.length > 0 && (
        <instancedMesh ref={pinesCanopy2Ref} args={[null, null, placements.pines.length]} castShadow>
          <primitive object={pineCanopyGeo2} attach="geometry" />
          <meshStandardMaterial roughness={0.82} metalness={0} flatShading vertexColors />
        </instancedMesh>
      )}
      {placements.pines.length > 0 && (
        <instancedMesh ref={pinesCanopy3Ref} args={[null, null, placements.pines.length]} castShadow>
          <primitive object={pineCanopyGeo3} attach="geometry" />
          <meshStandardMaterial roughness={0.82} metalness={0} flatShading vertexColors />
        </instancedMesh>
      )}

      {/* Oak trunks */}
      {placements.oaks.length > 0 && (
        <instancedMesh ref={oaksTrunkRef} args={[null, null, placements.oaks.length]} castShadow>
          <cylinderGeometry args={[0.16, 0.28, 1.8, 10, 2]} />
          <meshStandardMaterial roughness={0.95} metalness={0} vertexColors />
        </instancedMesh>
      )}
      {/* Oak canopy — displaced icosahedron */}
      {placements.oaks.length > 0 && (
        <instancedMesh ref={oaksCanopyRef} args={[null, null, placements.oaks.length]} castShadow>
          <primitive object={oakCanopyGeo} attach="geometry" />
          <meshStandardMaterial roughness={0.80} metalness={0} flatShading vertexColors />
        </instancedMesh>
      )}

      {/* Grass blade clusters with wind shader */}
      {placements.grass.length > 0 && (
        <instancedMesh ref={grassRef} args={[null, null, placements.grass.length]}>
          <primitive object={grassGeo} attach="geometry" />
          <meshStandardMaterial
            ref={grassMat}
            vertexColors
            roughness={0.9}
            side={2}
            onBeforeCompile={(shader) => {
              shader.uniforms.uTime = { value: 0 };
              grassMat.current.userData.timeUniform = shader.uniforms.uTime;
              shader.vertexShader = shader.vertexShader.replace(
                "#include <begin_vertex>",
                `#include <begin_vertex>
                // Wind sway — displace upper part of blade based on Y height
                float windStrength = max(0.0, transformed.y / 0.35);
                float wind = sin(uTime * 0.8 + position.x * 0.5 + position.z * 0.3) * 0.06 * windStrength;
                transformed.x += wind;
                transformed.z += wind * 0.5;`,
              );
              shader.vertexShader = "uniform float uTime;\n" + shader.vertexShader;
            }}
          />
        </instancedMesh>
      )}

      {/* Rocks */}
      {placements.rocks.length > 0 && (
        <instancedMesh ref={rocksRef} args={[null, null, placements.rocks.length]} castShadow>
          <primitive object={rockGeo} attach="geometry" />
          <meshStandardMaterial roughness={0.95} metalness={0} flatShading vertexColors />
        </instancedMesh>
      )}

      {/* Bushes / ferns */}
      {placements.bushes.length > 0 && (
        <instancedMesh ref={bushesRef} args={[null, null, placements.bushes.length]} castShadow>
          <primitive object={bushGeo} attach="geometry" />
          <meshStandardMaterial roughness={0.88} metalness={0} vertexColors side={2} />
        </instancedMesh>
      )}

      {/* Fallen logs — cylinders rotated 90° to lie on ground */}
      {placements.logs.length > 0 && (
        <instancedMesh ref={logsRef} args={[null, null, placements.logs.length]} castShadow>
          <cylinderGeometry args={[0.12, 0.16, 2.2, 8]} />
          <meshStandardMaterial color="#4a3828" roughness={0.97} metalness={0} />
        </instancedMesh>
      )}

      {/* Tree collision bodies */}
      {placements.pines.map((placement, index) => {
        const profile = getTreeCollisionProfile(placement, "pine");
        const [x, y, z] = placement.position;
        return (
          <RigidBody key={`pine-collider-${index}`} colliders={false} position={[x, y + profile.halfHeight, z]} type="fixed">
            <CylinderCollider args={[profile.halfHeight, profile.radius]} />
          </RigidBody>
        );
      })}
      {placements.oaks.map((placement, index) => {
        const profile = getTreeCollisionProfile(placement, "oak");
        const [x, y, z] = placement.position;
        return (
          <RigidBody key={`oak-collider-${index}`} colliders={false} position={[x, y + profile.halfHeight, z]} type="fixed">
            <CylinderCollider args={[profile.halfHeight, profile.radius]} />
          </RigidBody>
        );
      })}
      {placements.rocks.map((placement, index) => {
        const profile = getRockCollisionProfile(placement);
        const [x, y, z] = placement.position;
        return (
          <RigidBody key={`rock-collider-${index}`} colliders={false} position={[x, y + profile.halfExtents[1], z]} type="fixed">
            <CuboidCollider args={profile.halfExtents} />
          </RigidBody>
        );
      })}
    </group>
  );
}
