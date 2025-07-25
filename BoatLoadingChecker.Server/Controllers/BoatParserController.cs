﻿using BoatLoadingChecker.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using Xceed.Words.NET;

namespace BoatLoadingChecker.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoatDataController : ControllerBase
    {
        [HttpPost("parse")]
        public async Task<ActionResult<BoatData>> UploadAndParse(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string text;
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Determine file type
            if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                using var pdf = PdfDocument.Open(memoryStream);
                text = string.Join("\n", pdf.GetPages().Select(p => p.Text));
            }
            else if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                using var doc = DocX.Load(memoryStream);
                text = doc.Text;
            }
            else
            {
                return BadRequest("Unsupported file type. Please upload a .pdf or .docx file.");
            }

            var data = ParseBoatData(text);
            return Ok(data);
        }

        //  Helper method
        private BoatData ParseBoatData(string text)
        {
            var data = new BoatData();

            // Ship name
            var nameMatch = Regex.Match(
                text,
                @"1\.2\s*Vessel[’'`]?\s*s* name\s*\(IMO number\):\s*([^\n(]+)",
                RegexOptions.IgnoreCase
            );
            if (nameMatch.Success)
                data.ShipName = nameMatch.Groups[1].Value.Trim();

            // IMO number
            var imoMatch = Regex.Match(
                text,
                @"\(\s*(\d{7,})\s*\)",
                RegexOptions.IgnoreCase
            );
            if (imoMatch.Success)
                data.ImoNumber = imoMatch.Groups[1].Value.Trim();

            // Length overall
            var loaMatch = Regex.Match(
                text,
                @"(?:1\.27|Length overall|LOA).*?[:\s]+([\d,.]+)\s*(?:Metres|M)",
                RegexOptions.IgnoreCase
            );
            if (loaMatch.Success)
            {
                var rawValue = loaMatch.Groups[1].Value.Replace(",", "."); // normalize decimal separator
                if (float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedLoa))
                {
                    data.LengthOverall = (float)Math.Round(parsedLoa); // round to whole number
                }
            }

            // Parallel body
            ExtractParallelBodyData(text, data);

            // Freeboard Summer
            var freeboardMatch = Regex.Match(text, @"Summer:\s*([\d,.]+)\s*Metres.*?([\d,.]+)\s*Metres.*?([\d,]+)", RegexOptions.IgnoreCase);
            if (freeboardMatch.Success)
            {
                data.FreeboardSummer = double.TryParse(
                    freeboardMatch.Groups[1].Value.Replace(",", "."),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var freeboardVal
                ) ? Math.Round(freeboardVal, 2) : 0;
                data.SummerDeadweight = ParseInt(freeboardMatch.Groups[3].Value);
            }

            // Manifold height
            var manifoldMatch = Regex.Match(text, @"8\.24.*?Manifold height.*?[:\s]+([\d,.]+)\s*Metres", RegexOptions.IgnoreCase);
            if (manifoldMatch.Success)
            {
                data.ManifoldHeight.NormalBallast = double.TryParse(manifoldMatch.Groups[1].Value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out var freeboardVal) ? Math.Round(freeboardVal, 2) : 0;
                data.ManifoldHeight.SummerDWT = ParseFloat(manifoldMatch.Groups[2].Value);
            }

            // Cargo capacity
            var cargoMatch = Regex.Match(text, @"(?:8\.2a|Cargo Capacity).*?98[%\s]+.*?([\d,.]+)\s*(?:Cu\.|Cubic)\s*Metres", RegexOptions.IgnoreCase);
            if (cargoMatch.Success)
                data.CargoCapacity98 = ParseFloat(cargoMatch.Groups[1].Value);

            return data;
        }

        //  Parsing helpers
        private float ParseFloat(string input)
        {
            return float.TryParse(input.Replace(",", "."), out var result) ? result : 0;
        }

        private double ParseDouble(string input)
        {
            return double.TryParse(input.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? Math.Round(result, 2)
                : 0;
        }

        private int ParseInt(string input)
        {
            return int.TryParse(input.Replace(",", ""), out var result) ? result : 0;
        }

        private void ExtractParallelBodyData(string text, BoatData data)
        {
            var match = Regex.Match(
                text,
                @"Forward to mid-point manifold:\s*([\d,.]+)\s*Metres\s*([\d,.]+)\s*Metres\s*([\d,.]+)\s*Metres.*?" +
                @"Aft to mid-point manifold:\s*([\d,.]+)\s*Metres\s*([\d,.]+)\s*Metres\s*([\d,.]+)\s*Metres.*?" +
                @"Parallel body length:\s*([\d,.]+)\s*Metres\s*([\d,.]+)\s*Metres\s*([\d,.]+)\s*Metres",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            if (match.Success)
            {
                data.ParallelBody.Forward.Lightship = ParseDouble(match.Groups[1].Value);
                data.ParallelBody.Forward.NormalBallast = ParseDouble(match.Groups[2].Value);
                data.ParallelBody.Forward.SummerDWT = ParseDouble(match.Groups[3].Value);

                data.ParallelBody.Aft.Lightship = ParseDouble(match.Groups[4].Value);
                data.ParallelBody.Aft.NormalBallast = ParseDouble(match.Groups[5].Value);
                data.ParallelBody.Aft.SummerDWT = ParseDouble(match.Groups[6].Value);

                data.ParallelBody.Total.Lightship = ParseDouble(match.Groups[7].Value);
                data.ParallelBody.Total.NormalBallast = ParseDouble(match.Groups[8].Value);
                data.ParallelBody.Total.SummerDWT = ParseDouble(match.Groups[9].Value);
            }
        }
    }
}
