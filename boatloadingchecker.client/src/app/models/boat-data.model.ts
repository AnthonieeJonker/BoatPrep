export class MeasurementSet {
  lightship?: number;
  normalBallast?: number;
  summerDWT?: number;
}

export class ParallelBodyData {
  forward: MeasurementSet = new MeasurementSet();
  aft: MeasurementSet = new MeasurementSet();
  total: MeasurementSet = new MeasurementSet();
}

export class ManifoldHeightData {
  normalBallast?: number;
  summerDWT?: number;
}

export class BoatData {
  shipName?: string;
  imoNumber?: string;
  lengthOverall?: number;
  parallelBody: ParallelBodyData = new ParallelBodyData();
  freeboardSummer?: number;
  summerDeadweight?: number;
  manifoldHeight: ManifoldHeightData = new ManifoldHeightData();
  cargoCapacity98?: number;
}
