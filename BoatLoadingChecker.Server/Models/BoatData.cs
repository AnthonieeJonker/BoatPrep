namespace BoatLoadingChecker.Server.Models
{
    public class BoatData
    {
        public string? ShipName { get; set; }
        public string? ImoNumber { get; set; }
        public float? LengthOverall { get; set; }

        public ParallelBodyData ParallelBody { get; set; } = new();

        public double? FreeboardSummer { get; set; }
        public int? SummerDeadweight { get; set; }

        public ManifoldHeightData ManifoldHeight { get; set; } = new();

        public float? CargoCapacity98 { get; set; }
    }

    public class ParallelBodyData
    {
        public MeasurementSet Forward { get; set; } = new();
        public MeasurementSet Aft { get; set; } = new();
        public MeasurementSet Total { get; set; } = new();
    }

    public class MeasurementSet
    {
        public double? Lightship { get; set; }
        public double? NormalBallast { get; set; }
        public double? SummerDWT { get; set; }
    }
    public class ManifoldHeightData
    {
        public double? NormalBallast { get; set; }
        public float? SummerDWT { get; set; }
    }
}