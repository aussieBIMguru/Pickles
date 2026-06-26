using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DesignScript.Builtin;
using Dynamo.Graph.Nodes.CustomNodes;
using System.Drawing.Drawing2D;
using System.Windows.Controls;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to collection of elements.
    /// </summary>
    public class Pkl_Collect
    {
        internal Pkl_Collect() { }

        /// <summary>
        /// Collects all linked and/or imported CAD objects in a Document.
        /// </summary>
        /// <param name="includeLinked">Include linked CAD instances.</param>
        /// <param name="includeImported">Include imported CAD instances.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="instance">The CAD instance.</returns>
        /// <returns name="isLinked">If the CAD instance is linked (or imported).</returns>
        /// <returns name="type">The type of the CAD instance.</returns>
        /// <returns name="ownerView">The view that owns the CAD instance if it is view specific.</returns>
        /// <search>Revit.Collect.CadInstances</search>
        [NodeCategory("Action")]
        [MultiReturn("instance", "isLinked", "type", "ownerView")]
        public static Dictionary<string, object> CadInstances(bool includeLinked = true, bool includeImported = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Lists we will build for other outputs
            var outInstances = new List<DynElement?>();
            var outLinked = new List<bool>();
            var outLinkTypes = new List<DynElement?>();
            var outOwnerViews = new List<DynElement?>();

            // Default output dictionary
            var output = new Dictionary<string, object>
            {
                { "instance", outInstances },
                { "isLinked", outLinked },
                { "type", outLinkTypes },
                { "ownerView", outOwnerViews }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Get all CAD import instances
            IList<DB.ImportInstance> importInstances = docHelper.Document.Ext_CollectByClass<DB.ImportInstance>()
                .Where(i => (i.IsLinked && includeLinked) || (!i.IsLinked && includeImported))
                .ToList();

            // For each CAD import instance...
            foreach (DB.ImportInstance importInstance in importInstances)
            {
                // Get and append the type and owner View
                outInstances.Add(importInstance.Ext_ToDynElement(true));
                outLinked.Add(importInstance.IsLinked);
                DynElement linkType = importInstance.GetTypeId()
                    .Ext_GetDynamoElement(docHelper.Document, true);
                outLinkTypes.Add(linkType);
                DynElement ownerView = importInstance.OwnerViewId
                    .Ext_GetDynamoElement(docHelper.Document, true);
                outOwnerViews.Add(ownerView);
            }

            // Return the outputs
            return output;
        }

        /// <summary>
        /// Collects all DesignOptions in a Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="options">A list of DesignOptions.</returns>
        /// <search>Revit.Collect.DesignOptions</search>
        [NodeCategory("Action")]
        public static IList<DynElement?> DesignOptions([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement?>();
            }

            // Set and return the outputs
            return docHelper.Document.Ext_CollectByClassToDyn<DB.DesignOption>();
        }

        /// <summary>
        /// Collects all DesignOptionSets in the Document and their primary/secondary options.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="designOptionSets">The DesignOptionSets as Elements.</returns>
        /// <returns name="primaryOptions">The primary option of each set.</returns>
        /// <returns name="secondaryOptions">A list of secondary options for each set.</returns>
        /// <search>Revit.Collect.DesignOptionSets</search>
        [NodeCategory("Action")]
        [MultiReturn("designOptionSets", "primaryOptions", "secondaryOptions")]
        public static Dictionary<string, object> DesignOptionSets([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Output names
            string outputName1 = "designOptionSets";
            string outputName2 = "primaryOptions";
            string outputName3 = "secondaryOptions";

            // Output lists
            var optionSets = new List<DynElement>();
            var primaryOptions = new List<DynElement>();
            var secondaryOptionSets = new List<List<DynElement>>();

            // Default output dictionary
            var output = new Dictionary<string, object>
            {
                { outputName1, optionSets },
                { outputName2, primaryOptions },
                { outputName3, secondaryOptionSets }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Group design options by set
            var designOptionsBySet = docHelper.Document.Ext_CollectByClass<DB.DesignOption>()
                .Where(o => o.Ext_GetDesignOptionSet() != null)
                .GroupBy(o => o.Ext_GetDesignOptionSet().Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            // For each options set in the dictionary...
            foreach (var kvp in designOptionsBySet)
            {
                // Add the key to option sets
                optionSets.Add(kvp.Key.Ext_GetDynamoElement(docHelper.Document, true));

                // List to construct for secondary options per set
                var secondaryOptionsSet = new List<DynElement>();

                // For each design option for the set...
                foreach (DB.DesignOption designOption in kvp.Value)
                {
                    // Add to primary or add to secondary
                    if (designOption.IsPrimary)
                    {
                        primaryOptions.Add(designOption.Ext_ToDynElement(true));
                    }
                    else
                    {
                        secondaryOptionsSet.Add(designOption.Ext_ToDynElement(true));
                    }
                }

                // Add the secondary set to the matrix
                secondaryOptionSets.Add(secondaryOptionsSet);
            }

            // Return the output
            return output;
        }

        /// <summary>
        /// Collects all DimensionTypes in the Document, separated by standard and spot types.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="dimensionTypes">All DimensionTypes in the document.</returns>
        /// <returns name="spotDimensionTypes">All SpotDimensionTypes in the document.</returns>
        /// <search>Revit.Collect.DimensionTypes</search>
        [NodeCategory("Action")]
        [MultiReturn("dimensionTypes", "spotDimensionTypes")]
        public static Dictionary<string, object> DimensionTypes([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the output
            return new Dictionary<string, object>()
            {
                { "dimensionTypes",
                    docHelper.Document.Ext_CollectByClassToDyn<DB.DimensionType>(elementTypes: true) },
                { "spotDimensionTypes",
                    docHelper.Document.Ext_CollectByClassToDyn<DB.SpotDimensionType>(elementTypes: true) }
            };
        }

        /// <summary>
        /// Collects all Families in the Document, separated by user created, internal and modelled in place.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="userCreated">Families created by users.</returns>
        /// <returns name="internal">Families that are internal to Revit.</returns>
        /// <returns name="modelledInPlace">Families that are modelled in place.</returns>
        /// <search>Revit.Collect.Families</search>
        [NodeCategory("Action")]
        [MultiReturn("userCreated", "internal", "modelledInPlace")]
        public static Dictionary<string, object> Families([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the output
            List<DynElement> userCreatedFamilies = new();
            List<DynElement> internalFamilies = new();
            List<DynElement> inplaceFamilies = new();

            var output = new Dictionary<string, object>()
            {
                { "userCreated", userCreatedFamilies },
                { "internal", internalFamilies },
                { "modelledInPlace", inplaceFamilies }
            };

            // Collect and filter families into the outputs
            foreach (DB.Family family in docHelper.Document.Ext_CollectByClass<DB.Family>())
            {
                if (family.IsInPlace) { inplaceFamilies.Add(family.Ext_ToDynElement(true)); }
                else if (family.IsUserCreated) { userCreatedFamilies.Add(family.Ext_ToDynElement(true)); }
                else { internalFamilies.Add(family.Ext_ToDynElement(true)); }
            }
            return output;
        }

        /// <summary>
        /// Collects all FillPatternElements in the Document, and if they are Drafting patterns.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="patternElements">The FillPatternElements.</returns>
        /// <returns name="isDrafting">Is it a Drafting pattern.</returns>
        /// <search>Revit.Collect.FillPatternElements</search>
        [NodeCategory("Action")]
        [MultiReturn("patternElements", "isDrafting")]
        public static Dictionary<string, object> FillPatternElements([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Get the patterns and if they are drafting
            List<DynElement> patternElements = new();
            List<bool> isDrafting = new();

            foreach (var fpe in docHelper.Document.Ext_CollectByClass<DB.FillPatternElement>())
            {
                patternElements.Add(fpe.Ext_ToDynElement(true));
                isDrafting.Add(fpe.GetFillPattern().Target == DB.FillPatternTarget.Drafting);
            }

            // Return the output
            return new Dictionary<string, object>()
            {
                { "patternElements", patternElements },
                { "isDrafting", isDrafting },
            };
        }

        /// <summary>
        /// Collects all GroupTypes as Dynamo GroupTypes.
        /// </summary>
        /// <param name="refresh">Refreshes the contents of the node.</param>
        /// <returns name="groupTypeIds">The GroupTypes.</returns>
        /// <search>Revit.Collect.GroupTypes</search>
        [NodeCategory("Action")]
        public static List<DynGroupType> GroupTypes(bool refresh = false)
        {
            return DB.ParameterUtils.GetAllBuiltInGroups()
                .Select(g => g.Ext_ToDynGroupType())
                .ToList();
        }

        /// <summary>
        /// Collects all Image instances in a Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="instances">The Image instances.</returns>
        /// <returns name="fileNames">The File name of the instances.</returns>
        /// <returns name="ownerViews">The view that owns the Image instance.</returns>
        /// <search>Revit.Collect.ImageInstances</search>
        [NodeCategory("Action")]
        [MultiReturn("instances", "fileNames", "ownerViews")]
        public static Dictionary<string, object> ImageInstances([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);
            DB.Document doc = docHelper.Document;

            // Lists we will build for other outputs
            var outInstances = new List<DynElement?>();
            var outFileNames = new List<string>();
            var outOwnerViews = new List<DynElement?>();

            // Default output dictionary
            var output = new Dictionary<string, object>
            {
                { "instances", outInstances },
                { "fileNames", outFileNames },
                { "ownerViews", outOwnerViews }
            };

            // Get all image instances
            var imageInstances = doc.Ext_CollectByClass<DB.ImageInstance>();

            // For each Image instance...
            foreach (DB.ImageInstance imageInstance in imageInstances)
            {
                outInstances.Add(imageInstance.Ext_ToDynElement(true));
                outFileNames.Add(imageInstance.Name);
                outOwnerViews.Add(imageInstance.OwnerViewId
                    .Ext_GetElement<DB.View>(doc)?
                    .Ext_ToDynElement(true));
            }

            // Return the outputs
            return output;
        }

        /// <summary>
        /// Collects all Keynotes from the current document's loaded Keynote file.
        /// </summary>
        /// <param name="excludeParents">Exclude keynotes that act as parents to others.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="keynoteValues">The values of the Keynotes.</returns>
        /// <returns name="keynoteDescriptions">The descriptions of the Keynotes.</returns>
        /// <returns name="keynoteParents">Keynotes that act as parent values.</returns>
        /// <search>Revit.Collect.Keynotes</search>
        [NodeCategory("Action")]
        [MultiReturn("keynoteValues", "keynoteDescriptions", "keynoteParents")]
        public static Dictionary<string, object> Keynotes(bool excludeParents = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);
            DB.Document doc = docHelper.Document;

            var output = new Dictionary<string, object>
            {
                { "keynoteValues", new List<string>() },
                { "keynoteDescriptions", new List<string>() },
                { "keynoteParents", new List<string>() }
            };

            // Get keynote table, early return if none available
            DB.KeynoteTable table = DB.KeynoteTable.GetKeynoteTable(doc);
            DB.KeyBasedTreeEntries keynoteEntry = table?.GetKeyBasedTreeEntries();
            DB.KeyBasedTreeEntriesIterator keynoteEnum = keynoteEntry?.GetKeyBasedTreeEntriesIterator();

            if (keynoteEnum == null)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("No keynote entries could be retrieved from the current document.");
                return output;
            }

            // Lists we will build for other outputs
            List<string> keyValues = new();
            List<string> keyDescriptions = new();
            List<string> keyParents = new();
            List<string> baseValues = new();
            List<string> baseDescriptions = new();
            List<string> baseParents = new();
            

            // Iterate through the keynote entries
            while (keynoteEnum.MoveNext())
            {
                DB.KeyBasedTreeEntry entry = keynoteEnum.Current;
                keyValues.Add(entry.Key);
                keyParents.Add(entry.ParentKey);
                keyDescriptions.Add(((DB.KeynoteEntry)entry).KeynoteText);
            }

            // Process down to entries without parent rows
            if (excludeParents)
            {
                HashSet<string> parentSet = keyParents
                    .Where(p => p.Ext_HasChars())
                    .ToHashSet();

                // Obtain the entries that are not parents
                for (int i = 0; i < keyValues.Count; i++)
                {
                    string keyValue = keyValues[i];

                    if (!parentSet.Contains(keyValue))
                    {
                        baseValues.Add(keyValues[i]);
                        baseDescriptions.Add(keyDescriptions[i]);
                        baseParents.Add(keyParents[i]);
                    }
                }
            }

            // Return the outputs
            output["keynoteValues"] = excludeParents ? baseValues : keyValues;
            output["keynoteDescriptions"] = excludeParents ? baseDescriptions : keyDescriptions;
            output["keynoteParents"] = excludeParents ? baseParents : keyParents;
            return output;
        }

        /// <summary>
        /// Collects all Levels, sorted by elevation.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="levels">A list of Levels.</returns>
        /// <search>Revit.Collect.Levels</search>
        [NodeCategory("Action")]
        public static IList<DynElement> Levels([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the elements
            return docHelper.Document.Ext_CollectByClass<DB.Level>()
                .OrderBy(l => l.Elevation)
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Collects all Legend Types.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="legendTypes">A list of Legend Types.</returns>
        /// <search>Revit.Collect.LegendTypes</search>
        [NodeCategory("Action")]
        public static IList<DynElement> LegendTypes([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the elements
            return docHelper.Document.Ext_CollectByClass<DB.View>()
                .Where(v => v.ViewType == DB.ViewType.Legend)
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Collects all LinePatternElements in the Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="patternElements">The LinePatternElements.</returns>
        /// <search>Revit.Collect.LinePatternElements</search>
        [NodeCategory("Action")]
        public static IList<DynElement> LinePatternElements([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the elements
            return docHelper.Document.Ext_CollectByClassToDyn<DB.LinePatternElement>();
        }

        /// <summary>
        /// Collects all Lines in the Document, and if they are detail lines.
        /// </summary>
        /// <param name="includeDetail">Include detail lines.</param>
        /// <param name="includeModel">Include model lines.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="lines">The Lines.</returns>
        /// <returns name="isDetailLine">Is it a detail line.</returns>
        /// <search>Revit.Collect.Lines</search>
        [NodeCategory("Action")]
        [MultiReturn("lines", "isDetailLine")]
        public static Dictionary<string, object> Lines(bool includeDetail = true, bool includeModel = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Get the patterns and if they are drafting
            List<DynElement> lines = new();
            List<bool> isDetail = new();

            var output = new Dictionary<string, object>
            {
                { "lines", lines },
                { "isDetailLine", isDetail }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Collect and add the lines by detail/model types
            foreach (var l in docHelper.Document.Ext_CollectByCategory(DB.BuiltInCategory.OST_Lines))
            {
                bool detail = l is DB.DetailCurve;

                if ((detail && includeDetail) || (!detail && includeModel))
                {
                    lines.Add(l.Ext_ToDynElement(true));
                    isDetail.Add(detail);
                }
            }

            // Return the output
            return output;
        }

        /// <summary>
        /// Collects all LineStyles in the document as well as their associated GraphicStyle Element, Id and if they are user made.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="lineStyles">The LineStyles as Revit.Element.Category.</returns>
        /// <returns name="lineStyleIds">The Id's of the LineStyles as integers.</returns>
        /// <returns name="graphicStyles">The GraphicStyle elements of the LineStyles.</returns>
        /// <returns name="userMade">If the LineStyle is made by a user vs system.</returns>
        /// <search>Revit.Collect.LineStyles</search>
        [NodeCategory("Action")]
        [MultiReturn("lineStyles", "lineStyleIds", "graphicStyles", "userMade")]
        public static Dictionary<string, object> LineStyles([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Get the patterns and if they are drafting
            List<DynCategory> lineStyles = new();
            List<long> lineStyleIds = new();
            List<DynElement> graphicStyles = new();
            List<bool> userMade = new();

            var output = new Dictionary<string, object>
            {
                { "lineStyles", lineStyles },
                { "lineStyleIds", lineStyleIds },
                { "graphicStyles", graphicStyles },
                { "userMade", userMade }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }
            
            // Get line subcategories
            DB.Document doc = docHelper.Document;
            DB.Category? linesCat = DB.Category.GetCategory(doc, DB.BuiltInCategory.OST_Lines);

            // Collect and add the lines by detail/model types
            foreach (DB.Category subCategory in linesCat.SubCategories.Cast<DB.Category>())
            {
                long subCategoryInt = subCategory.Id.Value;
                DynElement graphicStyle = subCategory
                    .GetGraphicsStyle(DB.GraphicsStyleType.Projection)
                    .Ext_ToDynElement(true);

                lineStyles.Add(subCategory.Ext_ToDynCategory());
                lineStyleIds.Add(subCategoryInt);
                graphicStyles.Add(graphicStyle);

                bool isUserMade =  subCategory.Parent != null &&
                    subCategory.Parent.Id == linesCat.Id &&
                    !subCategory.Name.StartsWith("<");

                userMade.Add(isUserMade);
            }

            // Return the output
            return output;
        }

        /// <summary>
        /// Collects all Materials.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="materials">A list of Materials.</returns>
        /// <search>Revit.Collect.Materials</search>
        [NodeCategory("Action")]
        public static IList<DynElement> Materials([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the elements
            return docHelper.Document.Ext_CollectByClassToDyn<DB.Material>();
        }

        /// <summary>
        /// Collects all Phases.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="phases">A list of Phases.</returns>
        /// <search>Revit.Collect.Phases</search>
        [NodeCategory("Action")]
        public static IList<DynElement> Phases([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the elements
            return docHelper.Document.Ext_CollectByClassToDyn<DB.Material>();
        }

        /// <summary>
        /// Collects all Regions in the Document, and if they are Filled or Masking regions.
        /// </summary>
        /// <param name="includeFilled">Include filled regions.</param>
        /// <param name="includeMasking">Include masking regions.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="regions">The regions.</returns>
        /// <returns name="isFilledRegion">Is it a filled region.</returns>
        /// <search>Revit.Collect.Regions</search>
        [NodeCategory("Action")]
        [MultiReturn("regions", "isFilledRegion")]
        public static Dictionary<string, object> Regions(bool includeFilled = true, bool includeMasking = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Get the patterns and if they are drafting
            List<DynElement> regions = new();
            List<bool> isFilledRegion = new();

            var output = new Dictionary<string, object>
            {
                { "regions", regions },
                { "isFilledRegion", isFilledRegion }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Collect and add the regions
            foreach (var r in docHelper.Document.Ext_CollectByClass<DB.FilledRegion>())
            {
                var type = docHelper.Document.GetElement(r.GetTypeId()) as DB.FilledRegionType;
                if (type == null) continue;

                bool isFilled = !type.IsMasking;

                if ((isFilled && includeFilled) || (!isFilled && includeMasking))
                {
                    regions.Add(r.Ext_ToDynElement(true));
                    isFilledRegion.Add(isFilled);
                }
            }

            // Return the output
            return output;
        }

        /// <summary>
        /// Collects all Revisions, sorted by sequence number.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="revisions">The Revisions.</returns>
        /// <search>Revit.Collect.Revisions</search>
        [NodeCategory("Action")]
        public static IList<DynElement> Revisions([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the elements
            return docHelper.Document.Ext_CollectByClass<DB.Revision>()
                .OrderBy(r => r.SequenceNumber)
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Collects all SpecTypes as Dynamo ForgeTypes.
        /// </summary>
        /// <param name="refresh">Refreshes the contents of the node.</param>
        /// <returns name="specTypeIds">The SpecTypes.</returns>
        /// <search>Revit.Collect.SpecTypes</search>
        [NodeCategory("Action")]
        public static List<DynSpecType> SpecTypes(bool refresh = false)
        {
            return DB.SpecUtils.GetAllSpecs()
                .Select(g => g.Ext_ToDynSpecType())
                .ToList();
        }

        /// <summary>
        /// Collects all Sheets in a Document, with optional SheetCollection and placeholder filters.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="sheetCollection">Optional SheetCollection to filter sheets by.</param>
        /// <param name="includePlaceholders">Include placeholder sheets.</param>
        /// <returns name="sheets">A list of Sheets.</returns>
        /// <search>Revit.Collect.Sheets</search>
        [NodeCategory("Action")]
        public static IList<DynElement?> Sheets([DefaultArgument("null")] DynElement? sheetCollection = null, bool includePlaceholders = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement?>();
            }

            // Get sheet collection Id
            DB.Element internalSheetCollection = sheetCollection?.InternalElement;
            DB.ElementId sheetCollectionId = internalSheetCollection.Ext_ToSheetCollectionId();

            // Set and return the outputs
            return docHelper.Document.Ext_CollectSheets(sheetCollectionId, includePlaceholders).Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Collects all Warnings.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="warnings">A list of Warnings.</returns>
        /// <search>Revit.Collect.Warnings</search>
        [NodeCategory("Action")]
        public static IList<DynWarning> Warnings([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynWarning>();
            }

            // Return warnings
            return docHelper.Document.GetWarnings()
                .Select(w => w.Ext_ToDynWarning())
                .ToList();
        }

        /// <summary>
        /// Collects all Worksets as DB.Worksets.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="warnings">A list of DB.Worksets.</returns>
        /// <search>Revit.Collect.Worksets</search>
        [NodeCategory("Action")]
        public static IList<DB.Workset> Worksets([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DB.Workset>();
            }

            // Early return if document is not workshared
            if (!docHelper.Document.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return new List<DB.Workset>();
            }

            // Return worksets
            return new FilteredWorksetCollector(docHelper.Document)
                .OfKind(WorksetKind.UserWorkset)
                .ToList();
        }
    }
}