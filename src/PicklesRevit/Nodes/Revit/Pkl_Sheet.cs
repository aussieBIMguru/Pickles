using System.Text;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Sheets.
    /// </summary>
    public class Pkl_Sheet
    {
        internal Pkl_Sheet() { }

        /// <summary>
        /// Creates Revit sheets if they do not exist by number in the project.
        /// </summary>
        /// <param name="titleBlockType">The FamilyType to use.</param>
        /// <param name="numbers">A list of numbers (strings).</param>
        /// <param name="names">A list of names (strings).</param>
        /// <param name="asPlaceholder">Create sheets as placeholders.</param>
        /// <returns name="sheets">Created sheets.</returns>
        /// <returns name="success">If the sheet was created successfully.</returns>
        /// <search>Revit.Sheet.Create</search>
        [NodeCategory("Create")]
        [MultiReturn("sheets", "success")]
        public static Dictionary<string, object> Create(DynFamilySymbol titleBlockType,
            List<string> numbers, List<string> names, bool asPlaceholder = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null);

            // Final outputs
            var outSheets = new List<DynElement>();
            var outSuccess = new List<bool>();

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "sheets", outSheets },
                { "success", outSuccess }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Null validation routine, ensure equal name/number count
            if (titleBlockType == null
                || numbers.Ext_ListIsValid(ensureNoNulls: true)
                || names.Ext_ListIsValid(ensureNoNulls: true)
                || numbers.Count != names.Count)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Ensure family type is a title block
            if (titleBlockType.InternalElement is DB.FamilySymbol symbol
                && symbol.Family.FamilyCategory.Id.Value != (int)DB.BuiltInCategory.OST_TitleBlocks)
            {
                WARNING_TYPE.WRONG_CATEGORY_INPUTS.Ext_Raise();
                return output;
            }

            // Get existing sheet numbers in the document
            HashSet<string> exSheetNumbers = new DB.FilteredElementCollector(docHelper.Document)
                .OfClass(typeof(DB.ViewSheet))
                .Cast<DB.ViewSheet>()
                .Select(s => s.SheetNumber)
                .Distinct()
                .ToHashSet();

            // Titleblock family type Id
            DB.ElementId ttbTypeId = titleBlockType.InternalElement.Id;

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(docHelper.Document, "Pickle: Sheets.Create"))
            {
                transaction.Start();

                // For each name/number pairing...
                for (int i = 0; i < numbers.Count; i++)
                {
                    // If the number is not used...
                    if (!exSheetNumbers.Contains(numbers[i]))
                    {
                        // Create the sheet, add to number set
                        DB.ViewSheet sheet = docHelper.Document.Ext_CreateSheet(
                            numbers[i], names[i], asPlaceholder, ttbTypeId);
                        exSheetNumbers.Add(numbers[i]);

                        // Add to outputs
                        outSuccess.Add(true);
                        outSheets.Add(sheet.Ext_ToDynElement(true));
                    }
                    else
                    {
                        // Add error to outputs
                        outSheets.Add(null);
                        outSuccess.Add(false);
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return outputs
            return output;
        }

        /// <summary>
        /// Adds a Revision to Sheet(s).
        /// </summary>
        /// <param name="sheets">Sheets to add the Revision to.</param>
        /// <param name="revision">The Revision to add.</param>
        /// <returns name="sheets">The sheets.</returns>
        /// <returns name="success">If the revision was applied.</returns>
        /// <search>Revit.Sheet.AddRevision</search>
        [NodeCategory("Action")]
        [MultiReturn("sheets", "success")]
        public static Dictionary<string, object> AddRevision(List<DynSheet> sheets, DynRevision revision)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null);

            // Final outputs
            var outSuccess = new List<bool>();

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "sheets", sheets },
                { "success", outSuccess }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(docHelper.Document, "Pickle: Sheet.AddRevision"))
            {
                transaction.Start();

                // Revit revision
                DB.Revision internalRevision = revision.InternalElement as DB.Revision;

                // Add the Revision to each sheet
                foreach (DynSheet sheet in sheets)
                {
                    DB.ViewSheet internalSheet = sheet.InternalElement as DB.ViewSheet;
                    bool success = internalSheet.Ext_AddRevision(internalRevision);
                    outSuccess.Add(success);
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return outputs
            return output;
        }

        /// <summary>
        /// Adds a Revision to Sheet(s).
        /// </summary>
        /// <param name="sheets">Sheets to remove the Revision from.</param>
        /// <param name="revision">The Revision to remove.</param>
        /// <returns name="text">The sheets.</returns>
        /// <returns name="success">If the revision was removed.</returns>
        /// <search>Revit.Sheet.RemoveRevision</search>
        [NodeCategory("Action")]
        [MultiReturn("sheets", "success")]
        public static Dictionary<string, object> RemoveRevision(List<DynSheet> sheets, DynRevision revision)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null);

            // Final outputs
            var outSuccess = new List<bool>();

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "sheets", sheets },
                { "success", outSuccess }
            };

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(docHelper.Document, "Pickle: Sheet.AddRevision"))
            {
                transaction.Start();

                // Revit revision
                DB.Revision internalRevision = revision.InternalElement as DB.Revision;

                // Remove the Revision for each sheet
                foreach (DynSheet sheet in sheets)
                {
                    DB.ViewSheet interalSheet = sheet.InternalElement as DB.ViewSheet;
                    bool success = interalSheet.Ext_RemoveRevision(internalRevision);
                    outSuccess.Add(success);
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return outputs
            return output;
        }

        /// <summary>
        /// Given a number, returns the related sheet.
        /// </summary>
        /// <param name="numbers">Numbers to find sheets for.</param>
        /// <param name="sheetCollection">Numbers to find sheets for.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="sheets">The sheets that were found.</returns>
        /// <returns name="success">If a sheet was found.</returns>
        /// <search>Revit.Sheet.GetByNumber</search>
        [NodeCategory("Action")]
        [MultiReturn("sheets", "success")]
        public static Dictionary<string, object> GetByNumber(List<string> numbers, [DefaultArgument("null")] DynElement sheetCollection = null,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null);

            // Final outputs
            var outSheets = new List<DynElement?>();
            var outSuccess = new List<bool>();

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "sheets", outSheets },
                { "success", outSuccess }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Get sheet collection Id
            DB.Element internalSheetCollection = sheetCollection?.InternalElement;
            DB.ElementId sheetCollectionId = internalSheetCollection.Ext_ToSheetCollectionId();

            // Sheet dictionary by number
            var sheetDict = docHelper.Document.Ext_CollectSheetsByNumber(sheetCollectionId);

            // For each number, get its sheet if it exists in the document
            foreach (string number in numbers)
            {
                if (sheetDict.TryGetValue(number, out DB.ViewSheet sheet))
                {
                    outSheets.Add(sheet.Ext_ToDynElement(true));
                    outSuccess.Add(true);
                }
                else
                {
                    outSheets.Add(null);
                    outSuccess.Add(false);
                }
            }

            // Return outputs
            return output;
        }

        /// <summary>
        /// Processes sheet properties into formatted Document titles based on rule parts.
        /// 
        /// Rule logic:
        /// XxxParam|Err|ParamName
        /// - Xxx = Prj, Source value from Project Information
        /// - Xxx = Sht, Source value from Sheet
        /// - Err = Value to insert if error occurs
        /// 
        /// Any parts that do not have two | characters will be fixed text.
        /// </summary>
        /// <param name="sheets">Sheets to generate titles for.</param>
        /// <param name="ruleParts">Sheet/Project parmeter references or separators.</param>
        /// <returns name="titles">The formatted document titles.</returns>
        /// <search>Revit.Sheet.FormattedTitle</search>
        [NodeCategory("Action")]
        public static List<string> FormattedTitle(List<DynSheet> sheets, [DefaultArgument("null")] List<string> ruleParts = null)
        {
            // Final outputs
            var titles = new List<string>();

            // Default rules
            ruleParts ??= new List<string>()
            {
                "PrjParam|ERR|Project Number",
                "-",
                "ShtParam|ERR|Sheet Number",
                "_[",
                "ShtParam|-|Current Revision",
                "] - ",
                "ShtParam|ERR|Sheet Name"
            };

            // For each sheet...
            foreach (DynSheet sheet in sheets)
            {
                // Make sure we have a valid sheet
                if (sheet.InternalElement == null)
                {
                    titles.Add(null);
                    continue;
                }

                // Internal sheet, begin stringbuilder
                DB.ViewSheet internalSheet = sheet.InternalElement as DB.ViewSheet;
                StringBuilder sb = new StringBuilder();
                
                // For each part of the rule system...
                foreach (string rulePart in ruleParts)
                {
                    // Split it by the separator
                    string[] subParts = rulePart.Split("|");

                    // 3 parts = parameter based approach
                    if (subParts.Length == 3)
                    {
                        // Get parameter name and default parameter/value
                        string parameterName = subParts[2];
                        string value = "";

                        // Get sheet or project parameter
                        if (subParts[0].StartsWith("ShtParam"))
                        {
                            value = internalSheet.Ext_GetParameterValue<string>(parameterName);
                        }
                        else if (subParts[0].StartsWith("PrjParam"))
                        {
                            value = internalSheet?.Document.ProjectInformation.Ext_GetParameterValue<string>(parameterName);
                        }

                        // Catch error case, append to builder
                        value = value.Ext_DeNull(ifNull: subParts[1], replaceEmpty: true);
                        sb.Append(value);
                    }
                    // Otherwise, fixed text case
                    else 
                    {
                        sb.Append(rulePart ?? "");
                    }
                }

                titles.Add(sb.ToString());
            }

            // Return outputs
            return titles;
        }

        /// <summary>
        /// Returns if a sheet is a placeholder.
        /// </summary>
        /// <param name="sheet">Sheets to check.</param>
        /// <returns name="isPlaceholder">If the sheet is a placeholder.</returns>
        /// <search>Revit.Sheet.IsPlaceholder</search>
        [NodeCategory("Query")]
        public static bool? IsPlaceholder(DynSheet sheet)
        {
            // Final outputs
            if (sheet.InternalElement is DB.ViewSheet internalSheet)
            {
                return internalSheet.IsPlaceholder;
            }

            // Return outputs
            return null;
        }

        /// <summary>
        /// Returns all Revisions on the sheet, sorted by sequence Id.
        /// </summary>
        /// <param name="sheet">Sheets to get revisions of.</param>
        /// <returns name="revisions">Revisions on the Sheet.</returns>
        /// <search>Revit.Sheet.GetRevisions</search>
        [NodeCategory("Query")]
        public static List<DynElement> GetRevisions(DynSheet sheet)
        {
            // Final outputs
            var revisions = new List<DynElement>();

            if (sheet.InternalElement is DB.ViewSheet internalSheet)
            {
                revisions = internalSheet.GetAllRevisionIds()
                    .Select(i => i.Ext_GetElement<DB.Revision>(internalSheet.Document))
                    .OrderBy(r => r.SequenceNumber)
                    .Ext_ToDynamoElements(true)
                    .ToList();
            }

            // Return outputs
            return revisions;
        }
    }
}