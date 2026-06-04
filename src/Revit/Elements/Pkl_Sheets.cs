// Autodesk
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using DynamoServices;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using DB = Autodesk.Revit.DB;
// Pickle
using pklGen = Pkl_Utilities.Pkl_General;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Sheets.
    /// </summary>
    public class Pkl_Sheets
    {
        internal Pkl_Sheets() { }

        /// <summary>
        /// Creates Revit sheets if they do not exist by number in the project.
        /// </summary>
        /// <param name="titleBlockType">The FamilyType to use.</param>
        /// <param name="numbers">A list of numbers (strings).</param>
        /// <param name="names">A list of names (strings).</param>
        /// <param name="asPlaceholder">Create sheets as placeholders.</param>
        /// <returns>A list of sheets and outcomes.</returns>
        [MultiReturn("sheets", "success")]
        public static Dictionary<string, object> Create(global::Revit.Elements.FamilyType titleBlockType,
            List<string> numbers, List<string> names, bool asPlaceholder = false)
        {
            // Current document
            var doc = DocumentManager.Instance.CurrentDBDocument;
            
            // Inputs for node
            var outputName1 = "sheets";
            var outputName2 = "success";

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { outputName1, new List<global::Revit.Elements.Element>() },
                { outputName2, new List<bool>() }
            };
            
            // Null validation routine, ensure equal name/number count
            if (titleBlockType is null
                || pklGen.NullOrEmpty(numbers, ensureNoNulls: true)
                || pklGen.NullOrEmpty(names, ensureNoNulls: true)
                || numbers.Count != names.Count)
            {
                LogWarningMessageEvents.OnLogWarningMessage("Invalid inputs were provided.");
                return output;
            }

            // Ensure family type is a title block
            if (titleBlockType.InternalElement is FamilySymbol symbol
                && symbol.Family.FamilyCategory.Id.Value != (int)BuiltInCategory.OST_TitleBlocks)
            {
                LogWarningMessageEvents.OnLogWarningMessage("Titleblock family type was not provided.");
                return output;
            }

            // Get existing sheet numbers in the document
            var exSheetNumbers = new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.ViewSheet))
                .Cast<DB.ViewSheet>()
                .Select(s => s.SheetNumber)
                .Distinct()
                .ToHashSet<string>();

            // Titleblock family type Id
            var ttbTypeId = titleBlockType.InternalElement.Id;

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Final outputs
            var sheets = new List<global::Revit.Elements.Element>();
            var success = new List<bool>();

            // Using a transaction...
            using (DB.Transaction transaction = new DB.Transaction(doc, "Pickle: Sheets.Create"))
            {
                transaction.Start();

                // For each name/number pairing...
                for (int i = 0; i < numbers.Count; i++)
                {
                    // If the number is not used...
                    if (!exSheetNumbers.Contains(numbers[i]))
                    {
                        // Create the sheet, add to number set
                        var sheet = pklGen.CreateSheet(doc,
                                numbers[i],
                                names[i],
                                asPlaceholder,
                                ttbTypeId);
                        exSheetNumbers.Add(numbers[i]);

                        // Add to outputs
                        success.Add(true);
                        sheets.Add(sheet.ToDSType(true));
                    }
                    else
                    {
                        // Add error to outputs
                        sheets.Add(null);
                        success.Add(false);
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Store and return outputs
            output[outputName1] = sheets;
            output[outputName2] = success;
            return output;
        }
    }
}