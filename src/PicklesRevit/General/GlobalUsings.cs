// This file provides alias' and availability of namespaces in all files

// General Dynamo usings
global using Dynamo.Graph.Nodes;
global using Autodesk.DesignScript.Runtime;
global using RevitServices.Persistence;
global using DynamoServices;
global using RevitServices.Transactions;
global using Revit.Elements;

// Dynamo element alias'
global using DynElement = global::Revit.Elements.Element;
global using DynRevision = global::Revit.Elements.Revision;
global using DynView = global::Revit.Elements.Views.View;
global using DynSheet = global::Revit.Elements.Views.Sheet;
global using DynFamilySymbol = global::Revit.Elements.FamilyType;
global using DynSpecType = Revit.Elements.SpecType;
global using DynGroupType = Revit.Elements.GroupType;
global using DynForgeType = Revit.Elements.ForgeType;

// Revit database alias
global using DB = Autodesk.Revit.DB;
global using RUI = Autodesk.Revit.UI;
global using Result = Autodesk.Revit.UI.Result;

// Pickle usings
global using Pickles.Enums;
global using Pickles.Extensions;
global using Pickles.Helpers;
global using pklFrm = Pickles.Utilities.Utils_Forms;
global using pklCal = Pickles.Forms.Callers;
global using KeyedObject = Pickles.Forms.KeyedValue<object>;