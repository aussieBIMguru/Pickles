using System.IO;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Revit Documents.
    /// </summary>
    public class Pkl_Document
    {
        internal Pkl_Document() { }

        /// <summary>
        /// Opens a Revit Document from a file path in the background.
        /// 
        /// Central files will be opened as a new local file if a directory path for the local is provided.
        /// </summary>
        /// <param name="filePath">The path of the file to open in Revit.</param>
        /// <param name="options">Optional options to configure how the file is opened.</param>
        /// <param name="localDirectoryPath">If a workshared document is provided, create/open locally here instead.</param>
        /// <returns name="document">The Document.</returns>
        /// <search>Revit.Document.Create</search>
        [NodeCategory("Action")]
        public static DynDocument? Open(string filePath, [DefaultArgument("null")] DB.OpenOptions? options = null,
            [DefaultArgument("null")] string? localDirectoryPath = null)
        {
            // Validate Document suitability
            DB.BasicFileInfo info = DB.BasicFileInfo.Extract(filePath);

            if (info.IsSavedInLaterVersion)
            {
                WARNING_TYPE.DOC_HIGHER_VERSION.Ext_Raise();
                return null;
            }

            // Convert to model path
            DB.ModelPath modelPath = DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
            options ??= new DB.OpenOptions();

            // Workshared document causes local creation if directory specified
            if (info.IsWorkshared && localDirectoryPath.Ext_HasChars())
            {
                options.DetachFromCentralOption = DB.DetachFromCentralOption.DoNotDetach;

                string localPath = System.IO.Path.Combine(localDirectoryPath,
                    System.IO.Path.GetFileNameWithoutExtension(filePath) + "_local.rvt");

                // Create new local file
                DB.ModelPath localModelPath = DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(localPath);
                DB.ModelPath centralModelPath = DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(info.CentralPath);
                DB.WorksharingUtils.CreateNewLocal(centralModelPath, localModelPath);
                modelPath = localModelPath;
            }

            // Open the document
            var app = DocumentManager.Instance.CurrentUIApplication.Application;
            return app.OpenDocumentFile(modelPath, options).Ext_ToDynDocument();
        }

        /// <summary>
        /// Constructs OpenOptions for opening a Document.
        /// </summary>
        /// <param name="audit">Should the audit setting be applied.</param>
        /// <param name="detachFromCentral">Should the model be detached from central.</param>
        /// <param name="preserveWorksets">If detaching, will worksets be preserved.</param>
        /// <param name="ignoreExtensions">Should any extensible schemas be removed if encountered.</param>
        /// <param name="allowWrongUser">Should users who do not own a local be able to open it.</param>
        /// <returns name="options">The DB.OpenOptions.</returns>
        /// <search>Revit.Document.OpenOptions</search>
        [NodeCategory("Create")]
        public static DB.OpenOptions OpenOptions(bool audit = false, bool detachFromCentral = false,
            bool preserveWorksets = true, bool ignoreExtensions = false, bool allowWrongUser = false)
        {
            // Create detach options
            DB.DetachFromCentralOption detachOptions = detachFromCentral 
                ? preserveWorksets 
                ? DB.DetachFromCentralOption.DetachAndPreserveWorksets
                : DB.DetachFromCentralOption.DetachAndDiscardWorksets
                : DB.DetachFromCentralOption.DoNotDetach;

            // Return the options
            return new DB.OpenOptions()
            {
                Audit = audit,
                DetachFromCentralOption = detachOptions,
                IgnoreExtensibleStorageSchemaConflict = ignoreExtensions,
                AllowOpeningLocalByWrongUser = allowWrongUser
            };
        }

        /// <summary>
        /// Saves a Document (without closing it).
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <returns name="document">The Document.</returns>
        /// <search>Revit.Document.Action</search>
        [NodeCategory("Action")]
        public static DynDocument? Save(DynDocument document)
        {
            document.Ext_ToDBDocument().Save();
            return document;
        }

        /// <summary>
        /// Saves a Document to a specified path (without closing it).
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="filePath">The path to save the document to.</param>
        /// <param name="overwrite">Permits overwriting of documents.</param>
        /// <returns name="document">The Document.</returns>
        /// <search>Revit.Document.Action</search>
        [NodeCategory("Action")]
        public static DynDocument? SaveAs(DynDocument document, string filePath, bool overwrite = true)
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (directory.Ext_HasChars())
            {
                Directory.CreateDirectory(directory);
            }

            var options = new DB.SaveAsOptions()
            {
                OverwriteExistingFile = overwrite
            };

            document.Ext_ToDBDocument().SaveAs(filePath, options);

            return document;
        }

        /// <summary>
        /// Synchronizes a Document to central (without closing it).
        /// </summary>
        /// <param name="document">The document to sync.</param>
        /// <param name="options">Optional DB.SynchronizeWithCentralOptions to apply.</param>
        /// <returns name="document">The Document.</returns>
        /// <search>Revit.Document.SyncWithCentral</search>
        [NodeCategory("Action")]
        public static DynDocument? SyncWithCentral(DynDocument document,
            [DefaultArgument("null")] DB.SynchronizeWithCentralOptions? options = null)
        {
            // To Revit DB Document
            DB.Document rvtDoc = document.Ext_ToDBDocument();

            // Catch non-syncable document
            if (!rvtDoc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return document;
            }

            // Sync options, sync and return document
            options ??= new DB.SynchronizeWithCentralOptions();
            var trOptions = new DB.TransactWithCentralOptions();
            rvtDoc.SynchronizeWithCentral(trOptions, options);
            return document;
        }

        /// <summary>
        /// Constructs SynchronizeWithCentralOptions for syncing a Document.
        /// </summary>
        /// <param name="comment">Optional comment to append to the sync.</param>
        /// <param name="compact">Compact the model on sync.</param>
        /// <param name="relinquish">Relinquish all elements and worksets.</param>
        /// <param name="saveBefore">Save locally before.</param>
        /// <param name="saveAfter">Save locally after.</param>
        /// <returns name="options">The DB.SynchronizeWithCentralOptions.</returns>
        /// <search>Revit.Document.SyncOptions</search>
        [NodeCategory("Create")]
        public static DB.SynchronizeWithCentralOptions SyncOptions(string comment = "",
            bool compact = false, bool relinquish = true, bool saveBefore = true, bool saveAfter = true)
        {   
            // Construct options
            var options = new DB.SynchronizeWithCentralOptions()
            {
                Comment = comment,
                Compact = compact,
                SaveLocalAfter = saveBefore,
                SaveLocalBefore = saveAfter
            };

            // Construct relinquish options, apply and return
            var relinquishOptions = new DB.RelinquishOptions(relinquish);
            options.SetRelinquishOptions(relinquishOptions);
            return options;
        }

        /// <summary>
        /// Closes a Document.
        /// </summary>
        /// <param name="document">The document to close.</param>
        /// <param name="save">Save changes before closing.</param>
        /// <returns name="closed">Just returns True.</returns>
        /// <search>Revit.Document.Close</search>
        [NodeCategory("Action")]
        public static bool Close(DynDocument document, bool save = false)
        {
            document.Ext_ToDBDocument().Close(save);
            return true;
        }

        /// <summary>
        /// Gets the document related to a link, if not the document provided and if not, the current document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="document">The Document.</returns>
        /// <search>Revit.Document.GetDocument</search>
        [NodeCategory("Action")]
        public static DynDocument? GetDocument([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.Ext_ToDynDocument();
        }

        /// <summary>
        /// Returns if the provided or current document is workshared.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="isWorkshared">If the Document is workshsared.</returns>
        /// <search>Revit.Document.IsWorkshared</search>
        [NodeCategory("Query")]
        public static bool IsWorkshared([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.IsWorkshared;
        }

        /// <summary>
        /// Returns if the provided or current document is a Family Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="isFamilyDocument">If the Document is a Family Document.</returns>
        /// <search>Revit.Document.IsFamilyDocument</search>
        [NodeCategory("Query")]
        public static bool IsFamilyDocument([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.IsFamilyDocument;
        }

        /// <summary>
        /// Returns the title of the Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="title">If the Document is a Family Document.</returns>
        /// <search>Revit.Document.IsFamilyDocument</search>
        [NodeCategory("Query")]
        public static string Title([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.Title;
        }

        /// <summary>
        /// Returns the first document with a matching name in the application, if any.
        /// </summary>
        /// <param name="documentName">The title to match a Document with.</param>
        /// <param name="prioritizeBackground">If background documents should be preferred vs linked/current.</param>
        /// <returns name="document">A Dynamo Document.</returns>
        /// <search>Revit.Document.GetByTitle</search>
        [NodeCategory("Action")]
        public static DynDocument? GetByTitle(string documentName, bool prioritizeBackground = false)
        {
            // Work through documents
            DB.Document backgroundMatch = null;
            DB.Document foregroundMatch = null;

            // For each document...
            foreach (DB.Document doc in DocumentManager.Instance.CurrentDBDocument.Application.Documents)
            {
                // If we found a match...
                if (doc.Title == documentName)
                {
                    // Foreground check
                    if (foregroundMatch is null && (doc.IsLinked || documentName == doc.Title))
                    {
                        foregroundMatch = doc;
                    }
                    // Background check
                    else if (backgroundMatch is null)
                    {
                        backgroundMatch = doc;
                    }
                }
            }

            return (prioritizeBackground ? backgroundMatch ?? foregroundMatch : foregroundMatch ?? backgroundMatch).Ext_ToDynDocument();
        }

        /// <summary>
        /// Returns the current Room BoundaryLocation setting used by a Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to query (current if not provided).</param>
        /// <returns name="boundaryLocation">The BoundaryLocation setting name.</returns>
        /// <search>Revit.Document.GetBoundaryLocation</search>
        [NodeCategory("Query")]
        public static string GetBoundaryLocation([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            var settings = DB.AreaVolumeSettings.GetAreaVolumeSettings(docHelper.Document);
            return settings.GetSpatialElementBoundaryLocation(DB.SpatialElementType.Room).ToString();
        }

        /// <summary>
        /// Gets the document unit type used for a given specification.
        /// </summary>
        /// <param name="specType">The specification to query.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="unitInfo">The unit type as a Pkl_UnitInfo object.</returns>
        /// <returns name="unitName">The display name of the unit type.</returns>
        /// <search>Revit.Document.GetUnitInfo</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "unitType", "unitName" })]
        public static Dictionary<string, object> GetUnitInfo(DynSpecType specType,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            Dictionary<string, object> output = new()
            {
                { "unitType", null },
                { "unitName", null }
            };

            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            try
            {
                DB.ForgeTypeId unitTypeId = docHelper.Document
                    .GetUnits()
                    .GetFormatOptions(specType.Ext_ToSpecTypeId())
                    .GetUnitTypeId();

                output["unitType"] = unitTypeId.Ext_ToDynForgeType();
                output["unitName"] = DB.LabelUtils.GetLabelForUnit(unitTypeId);
            }
            catch
            {
            }

            return output;
        }

        /// <summary>
        /// Gets the local file path of a document, using the Revit cache location for workshared models.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="filePath">The local file path.</returns>
        /// <search>Revit.Document.LocalPath</search>
        [NodeCategory("Query")]
        public static string? LocalPath([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return null;
            }

            // Try to get the local path of the document
            DB.Document doc = docHelper.Document;

            if (doc.WorksharingCentralGUID != Guid.Empty)
            {
                try
                {
                    string guidString = doc.WorksharingCentralGUID.ToString();
                    string revitFolder = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Autodesk",
                        "Revit");

                    return Directory
                        .GetFiles(revitFolder, $"{guidString}.rvt", SearchOption.AllDirectories)
                        .FirstOrDefault()
                        ?? doc.PathName;
                }
                catch { }
            }

            return doc.PathName;
        }

        /// <summary>
        /// Gets the starting view of a document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="view">The starting view.</returns>
        /// <search>Revit.Document.GetStartingView</search>
        [NodeCategory("Query")]
        public static DynElement? GetStartingView([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return null;
            }

            // Get the starting view, if any
            var settings = DB.StartingViewSettings.GetStartingViewSettings(docHelper.Document);
            return settings.ViewId.Ext_GetDynamoElement(docHelper.Document, true);
        }
    }
}
