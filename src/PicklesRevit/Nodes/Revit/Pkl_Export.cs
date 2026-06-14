using Autodesk.Revit.DB;
using System.IO;
using pklEnum = Pickles.Enums.EnumHelpers;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to exporting from Revit.
    /// </summary>
    public class Pkl_Export
    {
        internal Pkl_Export() { }

        /// <summary>
        /// Collects all BaseExportOption names in the provided Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="optionNames">A list of BaseExportOption names.</returns>
        /// <search>Revit.Export.GetCadExportOptionNames</search>
        [NodeCategory("Action")]
        public static IList<string> GetCadExportOptionNames([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<string>();
            }

            // Collect elements and return as output
            return DB.BaseExportOptions.GetPredefinedSetupNames(docHelper.Document);
        }

        /// <summary>
        /// Prepares a PdfExportOptions object for use in PDF exporting.
        /// </summary>
        /// <param name="alwaysUseRaster">Force raster for all sheets.</param>
        /// <param name="exportQuality">Quality of exported Pdf.</param>
        /// <param name="hideCropBoundaries">Hide crop boundaries.</param>
        /// <param name="hideReferencePlanes">Hide reference planes.</param>
        /// <param name="hideScopeBoxes">Hide scope boxes.</param>
        /// <param name="hideUnreferencedViewTags">Hide unreferenced view tags.</param>
        /// <param name="maskCoincidentLines">Mask coincident lines.</param>
        /// <param name="paperFormat">Forat of page to use.</param>
        /// <param name="pageOrientation">Orientation of page to use.</param>
        /// <param name="rasterQuality">Quality of raster images.</param>
        /// <param name="replaceHalfToneWithThinLines">Replace halftone lines with thin lines.</param>
        /// <param name="viewLinksInBlue">View links in blue.</param>
        /// <param name="zoomPercentage">Percentage of zoom if not using fit to page.</param>
        /// <param name="zoomType">Zoom type to use.</param>
        /// <returns name="exportOptions">The PDFExportOptions.</returns>
        /// <search>Revit.Export.PdfExportOptions</search>
        [NodeCategory("Create")]
        public static DB.PDFExportOptions PdfExportOptions(
            bool alwaysUseRaster = true, string exportQuality = "DPI300",
            bool hideCropBoundaries = true, bool hideReferencePlanes = true,
            bool hideScopeBoxes = true, bool hideUnreferencedViewTags = true,
            bool maskCoincidentLines = true, string paperFormat = "Default",
            string pageOrientation = "Auto", string rasterQuality = "High",
            bool replaceHalfToneWithThinLines = false, bool viewLinksInBlue = false,
            int zoomPercentage = 100, string zoomType = "Zoom")
        {
            return new DB.PDFExportOptions()
            {
                AlwaysUseRaster = alwaysUseRaster,
                Combine = true,
                ExportQuality = pklEnum.EnumByName(exportQuality, DB.PDFExportQualityType.DPI300),
                HideCropBoundaries = hideCropBoundaries,
                HideReferencePlane = hideReferencePlanes,
                HideScopeBoxes = hideScopeBoxes,
                HideUnreferencedViewTags = hideUnreferencedViewTags,
                MaskCoincidentLines = maskCoincidentLines,
                PaperFormat = pklEnum.EnumByName(paperFormat, DB.ExportPaperFormat.Default),
                PaperOrientation = pklEnum.EnumByName(pageOrientation, DB.PageOrientationType.Auto),
                RasterQuality = pklEnum.EnumByName(rasterQuality, DB.RasterQualityType.High),
                StopOnError = false,
                ZoomPercentage = zoomPercentage,
                ZoomType = pklEnum.EnumByName(zoomType, DB.ZoomType.Zoom)
            };
        }

        /// <summary>
        /// Exports views and/or sheets to PDF using Revit's built-in driver.
        /// </summary>
        /// <param name="directoryPath">Directory to export to.</param>
        /// <param name="viewsOrSheets">Views and/or sheets to export.</param>
        /// <param name="fileNames">File names to use.</param>
        /// <param name="options">Optional PDFExportOptions.</param>
        /// <returns name="filePaths">The file paths that were exported.</returns>
        /// <returns name="success">Was the export successful.</returns>
        /// <search>Revit.Export.ExportToPdf</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "filePaths", "success" })]
        public static Dictionary<string, object> ExportToPdf(string directoryPath, List<DynView> viewsOrSheets,
            List<string> fileNames, [DefaultArgument("null")] DB.PDFExportOptions options = null)
        {
            // Default outputs
            List<string?> filePaths = new();
            List<bool> success = new();

            Dictionary<string, object> output = new()
            {
                { "filePaths", filePaths },
                { "success", success }
            };

            // Make sure the directory exists or can be created
            if (!Directory.Exists(directoryPath))
            {
                WARNING_TYPE.DIRECTORY_DOES_NOT_EXIST.Ext_Raise();
                return output;
            }

            // Get current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Make sure inputs are likely valid
            if (viewsOrSheets.Count != fileNames.Count
                || viewsOrSheets.Count == 0
                || viewsOrSheets.Ext_ListIsValid(ensureNoNulls: true)
                || fileNames.Ext_ListIsValid(ensureNoNulls: true))
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Catch invalid options
            options ??= new PDFExportOptions()
            {
                AlwaysUseRaster = false,
                Combine = true,
                ExportQuality = DB.PDFExportQualityType.DPI300,
                HideCropBoundaries = true,
                HideReferencePlane = true,
                HideScopeBoxes = true,
                HideUnreferencedViewTags = true,
                MaskCoincidentLines = true,
                PaperFormat = DB.ExportPaperFormat.Default,
                PaperOrientation = DB.PageOrientationType.Auto,
                RasterQuality = DB.RasterQualityType.High,
                StopOnError = false,
                ZoomPercentage = 100,
                ZoomType = DB.ZoomType.Zoom
            };

            // For each view/sheet...
            for (int i = 0; i < viewsOrSheets.Count; i++)
            {
                // Set file name and id list to export
                options.FileName = fileNames[i];
                var exportIds = new List<ElementId>() { viewsOrSheets[i].InternalElement.Id };

                // Export the sheet, make the file path
                bool result = doc.Export(directoryPath, exportIds, options);
                string filePath = result ? Path.Combine(directoryPath, fileNames[i] + ".pdf") : null;

                // Add the outcomes
                filePaths.Add(filePath);
                success.Add(result);
            }

            // Return outputs
            return output;
        }

        /// <summary>
        /// Exports views and/or sheets to DWG.
        /// </summary>
        /// <param name="directoryPath">Directory to export to.</param>
        /// <param name="viewsOrSheets">Views and/or sheets to export.</param>
        /// <param name="fileNames">File names to use.</param>
        /// <param name="useSharedCoordinates">Use shared coordinates or internal.</param>
        /// <param name="mergeViewsAsXrefs">Merge views into one Dwg.</param>
        /// <param name="optionsName">Optional DwgExportOptions name.</param>
        /// <returns name="filePaths">The file paths that were exported.</returns>
        /// <returns name="success">Was the export successful.</returns>
        /// <returns name="optionsFound">Options were found and used.</returns>
        /// <search>Revit.Export.ExportToDwg</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "filePaths", "success", "optionsFound" })]
        public static Dictionary<string, object> ExportToDwg(string directoryPath, List<DynView> viewsOrSheets,
            List<string> fileNames, bool useSharedCoordinates = false, bool mergeViewsAsXrefs = true,
            [DefaultArgument("null")] string optionsName = null)
        {
            // Default outputs
            List<string?> filePaths = new();
            List<bool> success = new();

            Dictionary<string, object> output = new()
            {
                { "filePaths", filePaths },
                { "success", success },
                { "optionsFound", false }
            };

            // Make sure the directory exists or can be created
            if (!Directory.Exists(directoryPath))
            {
                WARNING_TYPE.DIRECTORY_DOES_NOT_EXIST.Ext_Raise();
                return output;
            }

            // Get current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Make sure inputs are likely valid
            if (viewsOrSheets.Count != fileNames.Count
                || viewsOrSheets.Count == 0
                || viewsOrSheets.Ext_ListIsValid(ensureNoNulls: true)
                || fileNames.Ext_ListIsValid(ensureNoNulls: true))
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Get all export option names to find a match
            List<string> optionNames = DB.BaseExportOptions.GetPredefinedSetupNames(doc).ToList();
            DB.DWGExportOptions dwgExportOptions;

            // If we have a match, get the options, flag as found
            if (optionsName.Ext_HasChars() && optionNames.Contains(optionsName))
            {
                dwgExportOptions = DB.DWGExportOptions.GetPredefinedOptions(doc, optionsName);
                output["optionsFound"] = true;
            }
            else
            {
                // Default options otherwise
                dwgExportOptions = new DWGExportOptions();
            }

            // Set the coordinate and merge settings
            dwgExportOptions.SharedCoords = useSharedCoordinates;
            dwgExportOptions.MergedViews = mergeViewsAsXrefs;

            // For each view/sheet...
            for (int i = 0; i < viewsOrSheets.Count; i++)
            {
                // Set file name and id list to export
                var exportIds = new List<ElementId>() { viewsOrSheets[i].InternalElement.Id };

                // Export the sheet, make the file path
                bool result = doc.Export(directoryPath, fileNames[i], exportIds, dwgExportOptions);
                string filePath = result ? Path.Combine(directoryPath, fileNames[i] + ".dwg") : null;

                // Add the outcomes
                filePaths.Add(filePath);
                success.Add(result);
            }

            // Return outputs
            return output;
        }
    }
}
