import { sampleTerrainHeight } from "./terrainGeneration";

const DEFAULT_SEED = 7;

const makeGroundedPosition = (x, z, seed = DEFAULT_SEED) => {
  const terrainHeight = sampleTerrainHeight(x, z, { seed });
  return [x, terrainHeight + 0.02, z];
};

const makeBuilding = (
  id,
  kind,
  x,
  z,
  {
    seed = DEFAULT_SEED,
    rotation = 0,
    width = 5,
    depth = 4,
    wallHeight = 2.8,
    roofHeight = 1.6,
    wallColor = "#e8dcc8",
    roofColor = "#d4a843",
    frameColor = "#5c4033",
    detailColor = "#8b6841",
    openFront = false,
    chimney = false,
    fire = false,
  } = {},
) => ({
  id,
  kind,
  position: makeGroundedPosition(x, z, seed),
  rotation,
  width,
  depth,
  wallHeight,
  roofHeight,
  wallColor,
  roofColor,
  frameColor,
  detailColor,
  openFront,
  chimney,
  fire,
});

const makeProp = (kind, x, z, scale = 1, seed = DEFAULT_SEED) => ({
  kind,
  position: makeGroundedPosition(x, z, seed),
  scale,
});

export const buildVillageLayout = ({ seed = DEFAULT_SEED } = {}) => {
  const buildings = [
    makeBuilding("maren_house", "house", 0, -16, {
      seed,
      width: 5.2,
      depth: 4.6,
      wallHeight: 3.2,
      roofHeight: 1.9,
      chimney: true,
    }),
    makeBuilding("korvin_stall", "stall", 0, -1, {
      seed,
      width: 4.6,
      depth: 3.2,
      wallHeight: 1.8,
      roofHeight: 1.0,
      wallColor: "#8b6841",
      roofColor: "#d4a843",
      detailColor: "#5c4033",
      openFront: true,
    }),
    makeBuilding("hagen_forge", "forge", 14, 0, {
      seed,
      width: 6,
      depth: 4.8,
      wallHeight: 2.6,
      roofHeight: 1.4,
      wallColor: "#7a7a7a",
      roofColor: "#5c4033",
      detailColor: "#8b6914",
      openFront: true,
      fire: true,
    }),
    makeBuilding("lotte_house", "house", -14, 0, {
      seed,
      width: 4.8,
      depth: 4.2,
      wallHeight: 2.9,
      roofHeight: 1.6,
      roofColor: "#d4a843",
      chimney: true,
    }),
    makeBuilding("ren_camp", "shed", 20, 10, {
      seed,
      width: 3.8,
      depth: 3.2,
      wallHeight: 1.7,
      roofHeight: 1.1,
      wallColor: "#8b6841",
      roofColor: "#5c4033",
      openFront: true,
    }),
    makeBuilding("east_house_1", "house", 11, -12, {
      seed,
      width: 4.6,
      depth: 4.0,
      wallHeight: 2.8,
      roofHeight: 1.5,
      chimney: true,
    }),
    makeBuilding("east_house_2", "house", 18, -8, {
      seed,
      width: 4.4,
      depth: 4.0,
      wallHeight: 2.8,
      roofHeight: 1.5,
    }),
    makeBuilding("west_house_1", "house", -11, -11, {
      seed,
      width: 4.4,
      depth: 4.0,
      wallHeight: 2.8,
      roofHeight: 1.5,
    }),
    makeBuilding("west_house_2", "house", -18, -6, {
      seed,
      width: 4.5,
      depth: 4.1,
      wallHeight: 2.8,
      roofHeight: 1.5,
      chimney: true,
    }),
    makeBuilding("north_house_1", "house", -8, 12, {
      seed,
      width: 4.6,
      depth: 4.1,
      wallHeight: 2.9,
      roofHeight: 1.6,
    }),
    makeBuilding("north_house_2", "house", 8, 13, {
      seed,
      width: 4.6,
      depth: 4.1,
      wallHeight: 2.9,
      roofHeight: 1.6,
      chimney: true,
    }),
    makeBuilding("storage_shed", "shed", 16, 15, {
      seed,
      width: 4.2,
      depth: 3.4,
      wallHeight: 2.2,
      roofHeight: 1.2,
      wallColor: "#8b6841",
      roofColor: "#5c4033",
      openFront: true,
    }),
  ];

  const marketSquare = {
    position: makeGroundedPosition(0, 0, seed),
    width: 11,
    depth: 9,
  };

  const fences = [
    makeProp("fence", -5, -18, 1, seed),
    makeProp("fence", -1.5, -18, 1, seed),
    makeProp("fence", 2, -18, 1, seed),
    makeProp("fence", 5.5, -18, 1, seed),
    makeProp("fence", -16, 4, 1, seed),
    makeProp("fence", -16, 8, 1, seed),
    makeProp("fence", 16, 4, 1, seed),
    makeProp("fence", 16, 8, 1, seed),
  ];

  const barrels = [
    makeProp("barrel", -2.4, -2.6, 1, seed),
    makeProp("barrel", 1.9, -2.2, 1, seed),
    makeProp("barrel", 13.2, 2.2, 1, seed),
    makeProp("barrel", 14.6, -1.2, 1, seed),
    makeProp("barrel", -13.2, 1.8, 1, seed),
    makeProp("barrel", -15.2, -1.5, 1, seed),
  ];

  const lanterns = [
    makeProp("lantern", -6, -8, 1, seed),
    makeProp("lantern", 6, -8, 1, seed),
    makeProp("lantern", -6, 8, 1, seed),
    makeProp("lantern", 6, 8, 1, seed),
    makeProp("lantern", 0, 10, 1, seed),
  ];

  const smoke = [
    makeProp("smoke", -0.8, -15.5, 1, seed),
    makeProp("smoke", -13.6, 0.2, 1, seed),
    makeProp("smoke", 11.8, -11.6, 1, seed),
    makeProp("smoke", -17.4, -5.4, 1, seed),
    makeProp("smoke", 8.2, 12.2, 1, seed),
  ];

  return {
    buildings,
    marketSquare,
    fences,
    barrels,
    lanterns,
    smoke,
  };
};
